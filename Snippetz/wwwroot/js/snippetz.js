window.snippetz = {
    // The highlight.js stylesheet follows the light/dark scheme. The component library owns the
    // theme itself and raises 'nf:themechange' on document whenever it flips.
    applyHighlightTheme() {
        var link = document.getElementById('hljs-theme');
        if (!link) {
            return;
        }
        var dark = document.documentElement.getAttribute('data-theme') === 'dark';
        link.href = dark
            ? 'lib/highlight.js/styles/atom-one-dark.min.css'
            : 'lib/highlight.js/styles/github.min.css';
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

window.snippetz.applyHighlightTheme();
document.addEventListener('nf:themechange', window.snippetz.applyHighlightTheme);
