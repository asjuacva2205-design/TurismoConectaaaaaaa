window.TurismoConectaMapa = {
    mapa: null,
    municipios: [],
    marcadores: [],
    referenciaDotNet: null,
    mapaInicializado: false,
    enfocado: false,
    _manejadorScroll: null,

    // ─────────────────────────────────────────────
    // CREACIÓN DEL MAPA
    // ─────────────────────────────────────────────
    inicializar: function (municipios, referenciaDotNet) {
        this.municipios = municipios || [];
        this.referenciaDotNet = referenciaDotNet;

        if (this.mapaInicializado) {
            return;
        }

        const contenedor = document.getElementById("mapa-colombia");
        if (!contenedor) {
            return;
        }

        if (typeof L === "undefined") {
            console.warn("Leaflet no está cargado todavía en la página.");
            return;
        }

        this.mapa = L.map(contenedor, {
            zoomControl: true,
            scrollWheelZoom: true,
            attributionControl: true
        });

        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            maxZoom: 19,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }).addTo(this.mapa);

        this.mapa.setView([4.5709, -74.2973], 5);

        this.mapa.on("zoomend", () => {
            const zoom = this.mapa.getZoom();
            if (zoom < 6.5) {
                this.actualizarZona("Colombia");
            } else if (zoom < 9) {
                this.actualizarZona("Boyacá");
            } else {
                this.actualizarZona("Municipios de Boyacá");
            }
        });

        this.mostrarMunicipios();
        this.resaltarBoyaca();
        this.mapaInicializado = true;

        setTimeout(() => { if (this.mapa) this.mapa.invalidateSize(); }, 250);
        setTimeout(() => { if (this.mapa) this.mapa.invalidateSize(); }, 1000);
    },

    // ─────────────────────────────────────────────
    // NAVEGACIÓN A LA FICHA (sin depender de C#)
    // ─────────────────────────────────────────────

    /** URL de la ficha del municipio, marcando que venimos de inicio. */
    urlMunicipio: function (idMunicipio) {
        return `/municipios/${Number(idMunicipio)}?desde=inicio`;
    },

    /**
     * Navega a la ficha usando el enrutador de Blazor (sin recargar la página).
     * Si Blazor no está disponible, hace una navegación normal del navegador.
     */
    irAMunicipio: function (evento, url) {
        if (evento) {
            evento.preventDefault();
            evento.stopPropagation();
        }

        try {
            if (window.Blazor && typeof window.Blazor.navigateTo === "function") {
                window.Blazor.navigateTo(url);
            } else {
                window.location.href = url;
            }
        } catch (error) {
            console.warn("Blazor.navigateTo falló, se usa navegación normal:", error);
            window.location.href = url;
        }

        return false;
    },

    // ─────────────────────────────────────────────
    // MARCADORES Y POPUPS
    // ─────────────────────────────────────────────
    escaparHtml: function (texto) {
        return String(texto ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    },

    crearContenidoPopup: function (id, nombre, subtitulo) {
        const url = this.urlMunicipio(id);
        const lineaSubtitulo = subtitulo
            ? `<span class="popup-subtitulo">${this.escaparHtml(subtitulo)}</span>`
            : "";

        return `
            <div class="popup-municipio">
                <strong>${this.escaparHtml(nombre)}</strong>
                ${lineaSubtitulo}
                <a href="${url}"
                   class="popup-ir"
                   onclick="return window.TurismoConectaMapa.irAMunicipio(event, '${url}')">
                    Ver municipio →
                </a>
            </div>`;
    },

    mostrarMunicipios: function () {
        if (!this.mapa) {
            return;
        }

        this.marcadores.forEach((marcador) => this.mapa.removeLayer(marcador));
        this.marcadores = [];

        this.municipios.forEach((municipio) => {
            const marcador = L.marker([municipio.latitud, municipio.longitud], {
                title: municipio.nombre
            });

            marcador.idMunicipio = municipio.id;

            marcador
                .addTo(this.mapa)
                .bindPopup(this.crearContenidoPopup(municipio.id, municipio.nombre, null));

            this.marcadores.push(marcador);
        });
    },

    actualizarMarcadores: function (municipios) {
        if (!this.mapa) {
            return;
        }
        this.municipios = municipios || [];
        this.mostrarMunicipios();
    },

    // ─────────────────────────────────────────────
    // BÚSQUEDA TIPO "MAPS"
    // ─────────────────────────────────────────────
    enfocarMunicipio: function (datos, desplazar) {
        if (!this.mapa || !datos) {
            return;
        }

        this.enfocado = true;

        if (desplazar) {
            const contenedor = document.getElementById("mapa-colombia");
            if (contenedor) {
                contenedor.scrollIntoView({ behavior: "smooth", block: "center" });
            }
        }

        const destino = L.latLng(datos.latitud, datos.longitud);
        const contenido = this.crearContenidoPopup(datos.id, datos.nombre, datos.subtitulo);
        const marcador = this.marcadores.find((m) => m.idMunicipio === datos.id);

        const abrirPopup = () => {
            if (!this.mapa) {
                return;
            }
            if (marcador) {
                marcador.setPopupContent(contenido).openPopup();
            } else {
                L.popup().setLatLng(destino).setContent(contenido).openOn(this.mapa);
            }
            this.actualizarZona(datos.nombre);
        };

        this.mapa.stop();
        this.mapa.closePopup();

        const yaEstaAhi = this.mapa.getZoom() === 12 &&
            this.mapa.getCenter().distanceTo(destino) < 50;
        if (yaEstaAhi) {
            abrirPopup();
            return;
        }

        this.mapa.once("moveend", abrirPopup);
        this.mapa.flyTo(destino, 12, { animate: true, duration: 1.6 });
    },

    restablecerVista: function () {
        this.enfocado = false;
        if (!this.mapa) {
            return;
        }
        this.mapa.stop();
        this.mapa.closePopup();
        this.mapa.flyTo([5.65, -73.30], 8.5, { animate: true, duration: 1.5 });
        this.actualizarZona("Boyacá");
    },

    // ─────────────────────────────────────────────
    // LIMPIEZA AL SALIR DE LA PÁGINA
    // ─────────────────────────────────────────────
    destruir: function () {
        if (this._manejadorScroll) {
            window.removeEventListener("scroll", this._manejadorScroll);
            this._manejadorScroll = null;
        }

        if (this.mapa) {
            try {
                this.mapa.stop();
                this.mapa.off();
                this.mapa.remove();
            } catch (error) {
                console.warn("No se pudo destruir el mapa limpiamente:", error);
            }
        }

        this.mapa = null;
        this.marcadores = [];
        this.mapaInicializado = false;
        this.enfocado = false;
        this.referenciaDotNet = null;
    },

    // ─────────────────────────────────────────────
    // POLÍGONO DE BOYACÁ
    // ─────────────────────────────────────────────
    resaltarBoyaca: function () {
        const url =
            'https://gist.githubusercontent.com/john-guerra/43c7656821069d00dcbc/raw/' +
            'be6a6e239cd5b5b803c6e7c2ec405b793a9064dd/Colombia.geo.json';

        fetch(url)
            .then(function (r) { return r.json(); })
            .then(function (data) {
                const boyacaGeoJson = {
                    type: 'FeatureCollection',
                    features: data.features.filter(function (f) {
                        const nombre = (f.properties.NOMBRE_DPT || f.properties.name || '').toUpperCase();
                        return nombre.includes('BOYAC');
                    })
                };

                if (boyacaGeoJson.features.length === 0) {
                    console.warn('No se encontró el departamento de Boyacá en el GeoJSON.');
                    return;
                }

                if (!window.TurismoConectaMapa.mapa) {
                    return;
                }

                L.geoJSON(boyacaGeoJson, {
                    style: {
                        color: '#D9A441',
                        weight: 2.5,
                        fillColor: '#D9A441',
                        fillOpacity: 0.18,
                        dashArray: null
                    },
                    onEachFeature: function (feature, layer) {
                        layer.bindTooltip('Boyacá', {
                            permanent: false,
                            direction: 'center',
                            className: 'boyaca-tooltip'
                        });
                    }
                }).addTo(window.TurismoConectaMapa.mapa);
            })
            .catch(function (err) {
                console.warn('No se pudo cargar el GeoJSON de Boyacá:', err);
            });
    },

    // ─────────────────────────────────────────────
    // PUENTE HACIA C# (solo para el título de zona)
    // ─────────────────────────────────────────────
    actualizarZona: function (zona) {
        if (!this.referenciaDotNet) {
            return;
        }
        this.referenciaDotNet
            .invokeMethodAsync("ActualizarZonaMapa", zona)
            .catch(function (error) {
                console.warn("No se pudo actualizar la zona del mapa:", error);
            });
    },

    /** Se conserva por compatibilidad: ahora navega sin pasar por C#. */
    abrirMunicipio: function (idMunicipio) {
        this.irAMunicipio(null, this.urlMunicipio(idMunicipio));
    },

    acercarABoyaca: function () {
        if (!this.mapa) {
            return;
        }
        this.enfocado = false;
        this.mapa.flyTo([5.65, -73.30], 8.5, { animate: true, duration: 2 });
        this.actualizarZona("Boyacá");
    }
};

// ─────────────────────────────────────────────
// ZOOM AUTOMÁTICO CON EL SCROLL
// ─────────────────────────────────────────────
window.TurismoConectaMapa.activarScroll = function () {
    const TCM = window.TurismoConectaMapa;

    if (TCM._manejadorScroll) {
        window.removeEventListener("scroll", TCM._manejadorScroll);
    }

    let enBoyaca = false;
    let animando = false;

    const actualizarZoom = () => {
        if (!TCM.mapa || animando || TCM.enfocado) {
            return;
        }

        const scrollY = window.scrollY || window.pageYOffset;

        if (scrollY > 80 && !enBoyaca) {
            enBoyaca = true;
            animando = true;
            TCM.mapa.flyTo([5.65, -73.30], 8.5, { animate: true, duration: 1.8 });
            TCM.actualizarZona("Boyacá");
            setTimeout(() => { animando = false; }, 1900);
        }
        else if (scrollY < 40 && enBoyaca) {
            enBoyaca = false;
            animando = true;
            TCM.mapa.flyTo([4.5709, -74.2973], 5.0, { animate: true, duration: 1.5 });
            TCM.actualizarZona("Colombia");
            setTimeout(() => { animando = false; }, 1600);
        }
    };

    TCM._manejadorScroll = actualizarZoom;
    window.addEventListener("scroll", actualizarZoom, { passive: true });

    const indicador = document.querySelector(".hero-mapa-indicador");
    if (indicador) {
        indicador.style.cursor = "pointer";
        indicador.addEventListener("click", () => TCM.acercarABoyaca());
    }
};

// ─────────────────────────────────────────────
// SCROLL A UNA SECCIÓN (#donde-ir) AL VOLVER DESDE OTRA PÁGINA
// ─────────────────────────────────────────────
window.irAAncla = function (id) {
    const elemento = document.getElementById(id);
    if (elemento) {
        elemento.scrollIntoView({ behavior: "smooth", block: "start" });
    }
};
