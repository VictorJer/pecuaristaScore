(() => {
    const posse = document.getElementById("Posse");
    const campo = document.getElementById("campo-arrendamento");
    const valor = document.getElementById("ArrendamentoMensal");

    if (!posse || !campo || !valor) return;

    function atualizarArrendamento() {
        const arrendado = posse.value === "1";
        campo.hidden = !arrendado;
        valor.disabled = !arrendado;
        valor.required = arrendado;
    }

    posse.addEventListener("change", atualizarArrendamento);
    window.addEventListener("pageshow", atualizarArrendamento);
    atualizarArrendamento();
})();
