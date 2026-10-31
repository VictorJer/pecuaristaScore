(() => {
    const form = document.getElementById('simulacao');
    if (!form) return;
    const sections = [...form.querySelectorAll('[data-etapa]')];
    const back = document.getElementById('voltar');
    const next = document.getElementById('avancar');
    const generate = document.getElementById('gerar');
    const progress = document.getElementById('progresso');
    let step = form.dataset.resumo === 'true' ? 3 : 0;
    const errorStep = sections.findIndex(s => s.querySelector('.input-validation-error'));
    if (errorStep >= 0) step = errorStep;
    const posse = document.getElementById('Posse');
    const rent = document.getElementById('ArrendamentoMensal');
    function conditional() {
        const active = posse.value === '1';
        document.getElementById('campo-arrendamento').hidden = !active;
        rent.disabled = !active;
        rent.required = active;
        document.getElementById('Piquetes').required = document.getElementById('Rotacao').checked;
    }
    function show(focus = true) {
        sections.forEach((s, i) => { s.hidden = i !== step; });
        progress.hidden = false;
        progress.querySelectorAll('li').forEach((li, i) => {
            if (i === step) li.setAttribute('aria-current', 'step');
            else li.removeAttribute('aria-current');
            li.classList.toggle('fw-bold', i === step);
        });
        document.getElementById('etapa-atual').textContent = `Etapa ${step + 1} de 4: ${sections[step].dataset.etapa}`;
        back.hidden = step === 0;
        next.hidden = step === 3;
        generate.hidden = step !== 3;
        if (focus) {
            const heading = sections[step].querySelector('legend, h2');
            heading.tabIndex = -1;
            heading.focus();
        }
    }
    // Validação de apresentação; o servidor valida todas as entradas novamente.
    const positive = ['Area', 'Animais', 'PesoCompra', 'PrecoCompra', 'Dias'];
    function valid(section) {
        for (const input of section.querySelectorAll('input:not([type=hidden]), select')) {
            input.setCustomValidity('');
            if (input.disabled) continue;
            const numeric = positive.includes(input.id) || input.id === 'ArrendamentoMensal' || input.id === 'Piquetes';
            if (numeric && (positive.includes(input.id) || input.required || input.value.trim())) {
                const raw = input.value.trim();
                const integer = ['Animais', 'Dias', 'Piquetes'].includes(input.id);
                const pattern = integer ? /^\d+$/ : /^\d+(,\d+)?$/;
                const value = Number(raw.replace(',', '.'));
                if (!pattern.test(raw) || !Number.isFinite(value) || (input.id === 'ArrendamentoMensal' ? value < 0 : value <= 0))
                    input.setCustomValidity(integer ? 'Informe um número inteiro positivo.' : 'Informe um valor válido, usando vírgula para decimais.');
            }
            if (!input.checkValidity()) { input.reportValidity(); return false; }
        }
        return true;
    }
    form.noValidate = true;
    next.addEventListener('click', () => {
        if (!valid(sections[step])) return;
        step++; show();
        if (step === 3) form.requestSubmit(document.getElementById('atualizar-resumo'));
    });
    back.addEventListener('click', () => { step--; show(); });
    form.addEventListener('input', event => {
        if (event.target.setCustomValidity) event.target.setCustomValidity('');
        const summary = document.getElementById('resumo-calculado');
        if (summary) summary.hidden = true;
        document.getElementById('resumo-aviso').textContent = 'Dados alterados. Atualize o resumo para conferir a ocupação.';
    });
    form.addEventListener('change', conditional);
    form.addEventListener('submit', event => {
        if (step < 3) { event.preventDefault(); next.click(); return; }
        for (let i = 0; i < 3; i++) {
            // Exibe a etapa antes de acionar a validação nativa e seu foco.
            step = i; show(false);
            if (!valid(sections[i])) { event.preventDefault(); return; }
        }
        step = 3; show(false);
    });
    window.addEventListener('pageshow', conditional);
    conditional(); show(errorStep >= 0 || step === 3);
})();
