(() => {
    const media = window.matchMedia('(prefers-color-scheme: dark)');
    let preference = 'sistema';
    try { preference = localStorage.getItem('tema') || 'sistema'; } catch { }
    function apply() {
        document.documentElement.dataset.bsTheme = preference === 'sistema' ? (media.matches ? 'dark' : 'light') : preference;
    }
    apply();
    media.addEventListener('change', apply);
    document.addEventListener('DOMContentLoaded', () => {
        const select = document.getElementById('tema');
        select.value = preference;
        select.addEventListener('change', () => {
            preference = select.value;
            try { localStorage.setItem('tema', preference); } catch { }
            apply();
        });
    });
})();
