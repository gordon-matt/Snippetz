window.snippetz = {
    setTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        try { localStorage.setItem('snippetz.theme', theme); } catch (e) { }
        var link = document.getElementById('hljs-theme');
        if (link) {
            link.href = theme === 'dark'
                ? 'lib/highlight.js/styles/atom-one-dark.min.css'
                : 'lib/highlight.js/styles/github.min.css';
        }
    },
    getTheme() {
        return document.documentElement.getAttribute('data-theme') || 'light';
    },
    highlight(element) {
        if (!window.hljs) {
            return;
        }
        var scope = element && element.querySelectorAll ? element : document;
        scope.querySelectorAll('pre code').forEach(function (block) {
            block.removeAttribute('data-highlighted');
            window.hljs.highlightElement(block);
        });
    },
    copy(text) {
        if (navigator.clipboard && window.isSecureContext) {
            return navigator.clipboard.writeText(text);
        }
        var area = document.createElement('textarea');
        area.value = text;
        area.style.position = 'fixed';
        area.style.left = '-9999px';
        document.body.appendChild(area);
        area.select();
        document.execCommand('copy');
        document.body.removeChild(area);
        return Promise.resolve();
    }
};
