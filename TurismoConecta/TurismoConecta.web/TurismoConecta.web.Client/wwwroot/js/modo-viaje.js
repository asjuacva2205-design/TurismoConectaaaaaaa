/* Modo viaje — TurismoConecta
 * Mapa Leaflet + ruta por carretera (OSRM) + GPS en vivo + simulación + voz.
 * La lógica de "pueblo a tu derecha/izquierda" vive en C# (ModoViaje.razor); aquí solo hay mapa, GPS y voz.
 */
window.TurismoConectaViaje = {
    mapa: null,
    ref: null,
    paradas: [],
    pueblos: [],
    capaParadas: null,
    capaPueblos: null,
    lineaRuta: null,
    marcadorUsuario: null,
    watchId: null,
    primerFix: true,
    rutaPuntos: [],
    rutaDesdeUsuario: false,
    simTimer: null,
    simulando: false,

    // ───────────── Inicio / fin ─────────────
    inicializar: function(paradas, pueblos, ref) {
        this.destruir();

        this.ref = ref;
        this.paradas = paradas || [];
        this.pueblos = pueblos || [];

        this.mapa = L.map('mapa-viaje').setView([5.6, -73.4], 9);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(this.mapa);

        this.capaPueblos = L.layerGroup().addTo(this.mapa);
        this.capaParadas = L.layerGroup().addTo(this.mapa);

        // Todos los pueblos registrados (los que el Cóndor irá nombrando al pasar cerca).
        this.pueblos.forEach((p) => {
            L.circleMarker([p.latitud, p.longitud], {
                radius: 5, color: '#2F5D50', weight: 2, fillColor: '#ffffff', fillOpacity: 1
            }).bindTooltip(this.esc(p.nombre)).addTo(this.capaPueblos);
        });

        // Paradas del itinerario, numeradas.
        this.paradas.forEach((p, i) => {
            const icono = L.divIcon({
                className: 'parada-numero-icono',
                html: `<span>${i + 1}</span>`,
                iconSize: [28, 28],
                iconAnchor: [14, 14]
            });
            L.marker([p.latitud, p.longitud], { icon: icono })
                .bindPopup(`<strong>${this.esc(p.nombre)}</strong>`)
                .addTo(this.capaParadas);
        });

        setTimeout(() => { if (this.mapa) this.mapa.invalidateSize(); }, 250);

        this.calcularRuta(null);
        this.iniciarGps();
    },

    destruir: function() {
        this.detenerSimulacion();
        if (this.watchId !== null) {
            navigator.geolocation.clearWatch(this.watchId);
            this.watchId = null;
        }
        if ('speechSynthesis' in window) window.speechSynthesis.cancel();
        if (this.mapa) {
            this.mapa.remove();
            this.mapa = null;
        }
        this.marcadorUsuario = null;
        this.lineaRuta = null;
        this.rutaPuntos = [];
        this.rutaDesdeUsuario = false;
        this.primerFix = true;
        this.ref = null;
    },

    // ───────────── Ruta por carretera ─────────────
    // OSRM público (router.project-osrm.org): sin llave, pensado para demos/uso liviano.
    // Si falla, se dibuja una línea recta punteada y se avisa que no es por carretera.
    calcularRuta: async function(origen) {
        const puntos = [];
        if (origen) puntos.push(origen);
        this.paradas.forEach((p) => puntos.push([p.latitud, p.longitud]));
        this.rutaDesdeUsuario = !!origen;

        if (puntos.length < 2 || !this.mapa) return;

        const coords = puntos.map((p) => `${p[1]},${p[0]}`).join(';');
        const url = `https://router.project-osrm.org/route/v1/driving/${coords}?overview=full&geometries=geojson`;

        let latlngs, km, min, porCarretera = true;

        try {
            const ctrl = new AbortController();
            const t = setTimeout(() => ctrl.abort(), 12000);
            const resp = await fetch(url, { signal: ctrl.signal });
            clearTimeout(t);
            if (!resp.ok) throw new Error('OSRM HTTP ' + resp.status);

            const data = await resp.json();
            if (data.code !== 'Ok' || !data.routes || !data.routes.length) throw new Error('OSRM ' + data.code);

            const r = data.routes[0];
            latlngs = r.geometry.coordinates.map((c) => [c[1], c[0]]);
            km = r.distance / 1000;
            min = r.duration / 60;
        } catch (e) {
            console.warn('No se pudo calcular la ruta por carretera:', e);
            porCarretera = false;
            latlngs = puntos;
            km = this.longitudKm(puntos);
            min = (km / 50) * 60; // estimado a 50 km/h
        }

        if (!this.mapa) return; // la página se cerró mientras esperábamos

        this.rutaPuntos = latlngs;
        if (this.lineaRuta) this.mapa.removeLayer(this.lineaRuta);

        this.lineaRuta = L.polyline(latlngs, {
            color: '#B5442E',
            weight: 5,
            opacity: 0.85,
            dashArray: porCarretera ? null : '8 8'
        }).addTo(this.mapa);

        this.mapa.fitBounds(this.lineaRuta.getBounds(), { padding: [40, 40] });

        if (this.ref) this.ref.invokeMethodAsync('RutaCalculada', km, min, porCarretera);
    },

    recalcularDesdeMiUbicacion: function() {
        if (this.marcadorUsuario) {
            const p = this.marcadorUsuario.getLatLng();
            this.calcularRuta([p.lat, p.lng]);
        }
    },

    // ───────────── GPS ─────────────
    iniciarGps: function() {
        if (!navigator.geolocation) {
            if (this.ref) this.ref.invokeMethodAsync('SinGps');
            return;
        }

        this.watchId = navigator.geolocation.watchPosition(
            (pos) => this.alRecibirPosicion(pos),
            (err) => { if (this.ref) this.ref.invokeMethodAsync('ErrorGps', err.code); },
            { enableHighAccuracy: true, maximumAge: 2000, timeout: 20000 }
        );
    },

    alRecibirPosicion: function(pos) {
        if (this.simulando || !this.ref) return;

        const c = pos.coords;
        if (c.accuracy && c.accuracy > 150) return; // lectura demasiado imprecisa

        const lat = c.latitude;
        const lng = c.longitude;
        this.ponerUsuario(lat, lng);

        if (this.primerFix) {
            this.primerFix = false;
            this.calcularRuta([lat, lng]); // mejor camino desde donde estás ahora
        }

        // El rumbo del dispositivo solo es confiable en movimiento.
        const rumbo = (c.heading !== null && !isNaN(c.heading) && c.speed !== null && c.speed > 1.5) ? c.heading : null;
        this.ref.invokeMethodAsync('ActualizarPosicion', lat, lng, rumbo);
    },

    ponerUsuario: function(lat, lng) {
        if (!this.mapa) return;
        if (!this.marcadorUsuario) {
            const icono = L.divIcon({ className: 'usuario-punto-vivo', html: '<span></span>', iconSize: [18, 18], iconAnchor: [9, 9] });
            this.marcadorUsuario = L.marker([lat, lng], { icon: icono, zIndexOffset: 1000 }).addTo(this.mapa);
        } else {
            this.marcadorUsuario.setLatLng([lat, lng]);
        }
    },

    // ───────────── Simulación (para probar/demostrar sin manejar) ─────────────
    simular: async function(pasoMetros, msPorPaso) {
        if (!this.mapa) return;
        this.detenerSimulacion();

        // La simulación recorre la ruta entre paradas (no desde tu ubicación real).
        if (this.rutaDesdeUsuario || this.rutaPuntos.length < 2) {
            await this.calcularRuta(null);
        }
        if (!this.mapa || this.rutaPuntos.length < 2) return;

        const puntos = this.muestrear(this.rutaPuntos, pasoMetros);
        this.simulando = true;
        let i = 0;

        const avanzar = () => {
            if (!this.simulando || !this.mapa || !this.ref) return;

            if (i >= puntos.length) {
                this.simulando = false;
                this.ref.invokeMethodAsync('SimulacionTerminada');
                return;
            }

            const [lat, lng] = puntos[i++];
            this.ponerUsuario(lat, lng);
            this.mapa.panTo([lat, lng], { animate: true, duration: 0.8 });
            this.ref.invokeMethodAsync('ActualizarPosicion', lat, lng, null);
            this.simTimer = setTimeout(avanzar, msPorPaso);
        };

        avanzar();
    },

    detenerSimulacion: function() {
        this.simulando = false;
        if (this.simTimer) {
            clearTimeout(this.simTimer);
            this.simTimer = null;
        }
    },

    // ───────────── Voz ─────────────
    hablar: function(texto, interrumpir) {
        if (!('speechSynthesis' in window) || !texto) return;
        if (interrumpir) window.speechSynthesis.cancel();

        const u = new SpeechSynthesisUtterance(texto);
        u.lang = 'es-CO';
        u.rate = 1;

        const voces = window.speechSynthesis.getVoices();
        const voz = voces.find((v) => v.lang === 'es-CO') ||
            voces.find((v) => v.lang && v.lang.toLowerCase().startsWith('es'));
        if (voz) u.voice = voz;

        window.speechSynthesis.speak(u);
    },

    silenciar: function() {
        if ('speechSynthesis' in window) window.speechSynthesis.cancel();
    },

    // ───────────── Utilidades ─────────────
    muestrear: function(puntos, pasoMetros) {
        const salida = [puntos[0]];
        let acumulado = 0;
        for (let i = 1; i < puntos.length; i++) {
            acumulado += L.latLng(puntos[i - 1]).distanceTo(L.latLng(puntos[i]));
            if (acumulado >= pasoMetros) {
                salida.push(puntos[i]);
                acumulado = 0;
            }
        }
        const ultimo = puntos[puntos.length - 1];
        if (salida[salida.length - 1] !== ultimo) salida.push(ultimo);
        return salida;
    },

    longitudKm: function(puntos) {
        let m = 0;
        for (let i = 1; i < puntos.length; i++) {
            m += L.latLng(puntos[i - 1]).distanceTo(L.latLng(puntos[i]));
        }
        return m / 1000;
    },

    esc: function(t) {
        return String(t).replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    }
};
