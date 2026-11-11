// Rail de Maravillas: avanza solo (autoplay) y con el scroll mientras la sección está "pegada"
window.RailMaravillas = (() => {
    const instancias = new Map();
    const sinMovimiento = window.matchMedia('(prefers-reduced-motion: reduce)');

    function iniciar(escena, dotnet, intervaloMs) {
        destruir(escena);

        const estado = { encima: false, acumulado: 0, ultimoY: window.scrollY, temporizador: null };

        // La sección está cruzando el centro de la pantalla
        const enPantalla = () => {
            const r = escena.getBoundingClientRect();
            const mitad = window.innerHeight / 2;
            return r.top < mitad && r.bottom > mitad;
        };

        // Autoplay: solo si se ve, no hay mouse encima y la pestaña está activa
        const alTemporizador = () => {
            if (estado.encima || document.hidden || sinMovimiento.matches || !enPantalla()) return;
            dotnet.invokeMethodAsync('AlTemporizador');
        };

        const reiniciar = () => {
            clearInterval(estado.temporizador);
            estado.temporizador = setInterval(alTemporizador, intervaloMs);
        };

        // Scroll: mientras está pegada, cada 30% de pantalla recorrida = 1 carta (hacia adelante o atrás)
        const alScroll = () => {
            const y = window.scrollY;
            const delta = y - estado.ultimoY;
            estado.ultimoY = y;

            const r = escena.getBoundingClientRect();
            const pegada = r.top <= 0 && r.bottom >= window.innerHeight;
            if (!pegada) { estado.acumulado = 0; return; }

            estado.acumulado += delta;
            const paso = window.innerHeight * 0.3;
            let movio = false;

            while (Math.abs(estado.acumulado) >= paso) {
                const direccion = Math.sign(estado.acumulado);
                estado.acumulado -= direccion * paso;
                dotnet.invokeMethodAsync('AlScroll', direccion);
                movio = true;
            }

            if (movio) reiniciar();
        };

        // Pausa mientras el mouse está sobre las cartas o los botones de abajo
        const alPasar = (e) => { estado.encima = !!e.target.closest('.rail-card, .rail-pie'); };
        const alSalir = () => { estado.encima = false; };

        window.addEventListener('scroll', alScroll, { passive: true });
        escena.addEventListener('pointerover', alPasar);
        escena.addEventListener('pointerleave', alSalir);
        reiniciar();

        instancias.set(escena, { estado, reiniciar, alScroll, alPasar, alSalir });
    }

    // Tras una acción manual (flecha, clic), el siguiente paso automático espera el intervalo completo
    function reiniciar(escena) {
        instancias.get(escena)?.reiniciar();
    }

    function destruir(escena) {
        const instancia = instancias.get(escena);
        if (!instancia) return;
        clearInterval(instancia.estado.temporizador);
        window.removeEventListener('scroll', instancia.alScroll);
        escena.removeEventListener('pointerover', instancia.alPasar);
        escena.removeEventListener('pointerleave', instancia.alSalir);
        instancias.delete(escena);
    }

    return { iniciar, reiniciar, destruir };
})();
