(function () {
    var theme = null;
    try { theme = localStorage.getItem('snippetz.theme'); } catch (e) { }
    if (theme !== 'light' && theme !== 'dark') {
        theme = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }
    document.documentElement.setAttribute('data-theme', theme);
    var link = document.getElementById('hljs-theme');
    if (link) {
        link.href = theme === 'dark'
            ? 'lib/highlight.js/styles/atom-one-dark.min.css'
            : 'lib/highlight.js/styles/github.min.css';
    }
})();
