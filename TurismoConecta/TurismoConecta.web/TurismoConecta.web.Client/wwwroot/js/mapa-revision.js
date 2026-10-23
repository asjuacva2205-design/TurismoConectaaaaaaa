// Mapas de establecimientos:
//  - mostrar / destruir  → mini mapa de solo lectura en el panel de revisión (BandejaPendientes)
//  - iniciarSelector ... → mapa donde el comerciante marca su ubicación (CrearEstablecimiento)
window.mapaRevision = (function () {
    const CENTRO_BOYACA = [5.45, -73.36];
    const TESELAS = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
    const ATRIBUCION = '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>';

    // ───────── Utilidades compartidas ─────────
    function escaparHtml(texto) {
        const div = document.createElement('div');
        div.textContent = texto ?? '';
        return div.innerHTML;
    }

    function iconoPin() {
        return L.divIcon({
            className: '',
            iconSize: [40, 40],
            iconAnchor: [20, 40],
            html:
                '<div style="width:40px;height:40px;border-radius:50% 50% 50% 0;transform:rotate(-45deg);' +
                'background:#0d2818;border:3px solid #c9a961;box-shadow:0 6px 14px rgba(0,0,0,.35);' +
                'display:grid;place-items:center;">' +
                '<i class="ph-fill ph-storefront" style="transform:rotate(45deg);color:#F7F1E3;font-size:18px;"></i>' +
                '</div>'
        });
    }

    function crearMapa(contenedor, centro, zoom) {
        const m = L.map(contenedor, { scrollWheelZoom: false }).setView(centro, zoom);
        L.tileLayer(TESELAS, { maxZoom: 19, attribution: ATRIBUCION }).addTo(m);
        // Si el contenedor aparece con animación, recalcular tamaño al terminar
        setTimeout(() => m.invalidateSize(), 400);
        return m;
    }

    // ═════════ 1. MINI MAPA DE REVISIÓN (solo lectura) ═════════
    let mapa = null;

    function destruir() {
        if (mapa) {
            mapa.remove();
            mapa = null;
        }
    }

    function mostrar(idElemento, lat, lng, titulo) {
        destruir();
        const contenedor = document.getElementById(idElemento);
        if (!contenedor || typeof L === 'undefined') return;

        mapa = crearMapa(contenedor, [lat, lng], 16);
        L.marker([lat, lng], { icon: iconoPin() })
            .addTo(mapa)
            .bindTooltip(escaparHtml(titulo), { direction: 'top', offset: [0, -38] });
    }

    // ═════════ 2. SELECTOR DE UBICACIÓN (el comerciante marca el punto) ═════════
    let selector = null;
    let pinSelector = null;
    let referenciaDotNet = null;

    function avisarABlazor() {
        if (!pinSelector || !referenciaDotNet) return;
        const p = pinSelector.getLatLng();
        referenciaDotNet.invokeMethodAsync('AlMarcarUbicacion', p.lat, p.lng);
    }

    function ponerPin(lat, lng) {
        if (pinSelector) {
            pinSelector.setLatLng([lat, lng]);
            return;
        }
        pinSelector = L.marker([lat, lng], { icon: iconoPin(), draggable: true }).addTo(selector);
        pinSelector.on('dragend', avisarABlazor);   // al soltar el pin arrastrado
    }

    function iniciarSelector(idElemento, dotNetRef, lat, lng, centroLat, centroLng) {
        destruirSelector();
        const contenedor = document.getElementById(idElemento);
        if (!contenedor || typeof L === 'undefined') return;

        referenciaDotNet = dotNetRef;

        const tienePin = lat !== null && lng !== null;
        const tieneCentro = centroLat !== null && centroLng !== null;

        const centro = tienePin ? [lat, lng] : tieneCentro ? [centroLat, centroLng] : CENTRO_BOYACA;
        const zoom = tienePin ? 16 : tieneCentro ? 14 : 8;

        selector = crearMapa(contenedor, centro, zoom);
        if (tienePin) ponerPin(lat, lng);

        selector.on('click', (e) => {
            ponerPin(e.latlng.lat, e.latlng.lng);
            avisarABlazor();
        });
    }

    // Cuando el comerciante cambia de municipio en el formulario
    function centrarSelector(lat, lng) {
        if (!selector || lat === null || lng === null) return;
        selector.flyTo([lat, lng], 14, { duration: 0.8 });
    }

    function quitarPinSelector() {
        if (pinSelector) {
            pinSelector.remove();
            pinSelector = null;
        }
    }

    function destruirSelector() {
        if (selector) {
            selector.remove();
            selector = null;
        }
        pinSelector = null;
        referenciaDotNet = null;
    }

    return { mostrar, destruir, iniciarSelector, centrarSelector, quitarPinSelector, destruirSelector };
})();
