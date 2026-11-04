// Small browser helpers called from Blazor.
window.wtc = {
    // On phones the level strip scrolls sideways; keep the current level in view.
    showCurrentLevel() {
        const strip = document.querySelector('.level-path');
        const current = strip && strip.querySelector('.level-stop.is-current');
        if (!strip || !current || strip.scrollWidth <= strip.clientWidth) return;
        const s = strip.getBoundingClientRect();
        const c = current.getBoundingClientRect();
        const left = strip.scrollLeft + (c.left - s.left) - (s.width - c.width) / 2;
        strip.scrollTo({ left: Math.max(0, left), behavior: 'smooth' });
    },

    // On phones the board is pinned at the top and the dock covers the bottom.
    // Scroll just enough that an element sits in the window between them.
    // maxHeight limits how much of a tall element (a loop with blocks inside) we insist on showing.
    reveal(el, maxHeight) {
        const dock = document.querySelector('.dock');
        if (!el || !dock || getComputedStyle(dock).position !== 'fixed') return;

        const board = document.querySelector('.board-panel');
        const visibleTop = board ? board.getBoundingClientRect().bottom : 0;
        const visibleBottom = dock.getBoundingClientRect().top;
        const r = el.getBoundingClientRect();
        const wanted = r.top + Math.min(r.height, maxHeight);

        if (wanted > visibleBottom - 12) {
            window.scrollBy({ top: wanted - visibleBottom + 20, behavior: 'smooth' });
        } else if (r.top < visibleTop + 8) {
            window.scrollBy({ top: r.top - visibleTop - 16, behavior: 'smooth' });
        }
    },

    // After a block is added, or while stepping through a program.
    revealBlock(id, maxHeight) {
        this.reveal(document.querySelector(`[data-block="${id}"]`), maxHeight || 150);
    },

    // ---------- Code editor ----------

    // Adds editor comforts to the code textarea: Tab indents, Enter keeps indentation,
    // and a closing } snaps back a level. Safe to call repeatedly.
    enhanceEditor(id) {
        const ta = document.getElementById(id);
        if (!ta || ta.dataset.enhanced) return;
        ta.dataset.enhanced = '1';
        ta.addEventListener('focus', () => { ta.dataset.touched = '1'; });

        ta.addEventListener('keydown', e => {
            const { selectionStart: start, selectionEnd: end, value } = ta;
            const lineStart = value.lastIndexOf('\n', start - 1) + 1;
            const indent = value.slice(lineStart, start).match(/^ */)[0];

            if (e.key === 'Tab' && !e.shiftKey) {
                e.preventDefault();
                wtc._replace(ta, start, end, '    ', 4);
            } else if (e.key === 'Enter') {
                e.preventDefault();
                const opens = value.slice(lineStart, start).trimEnd().endsWith('{');
                const extra = opens ? '    ' : '';
                wtc._replace(ta, start, end, '\n' + indent + extra, 1 + indent.length + extra.length);
            } else if (e.key === '}' && value.slice(lineStart, start).trim() === '' && indent.length >= 4) {
                e.preventDefault();
                ta.setSelectionRange(start - 4, end);
                wtc._replace(ta, start - 4, end, '}', 1);
            }
        });
    },

    // Inserts a snippet at the cursor (or at the end if the editor was never touched),
    // indented to match, with the cursor left where $0 was.
    insertSnippet(id, snippet) {
        const ta = document.getElementById(id);
        if (!ta) return;
        const value = ta.value;
        let pos = ta.dataset.touched ? ta.selectionStart : value.length;

        const lineStart = value.lastIndexOf('\n', pos - 1) + 1;
        const before = value.slice(lineStart, pos);
        let indent = before.match(/^ */)[0];
        let prefix = '';

        // Mid-line: start the snippet on a fresh line below.
        if (before.trim() !== '') {
            const lineEnd = value.indexOf('\n', pos);
            pos = lineEnd === -1 ? value.length : lineEnd;
            prefix = '\n';
            if (before.trimEnd().endsWith('{')) indent += '    ';
        } else if (pos !== lineStart) {
            // Cursor sits in the indentation: keep it, insert from there.
            indent = before;
            ta.setSelectionRange(lineStart, pos);
            pos = lineStart;
        }

        // Nested for loops need different counter names (i, j, k, m).
        snippet = snippet.replace(/\bi\b/g, wtc._freeLoopVar(value.slice(0, pos)));

        const body = snippet.split('\n').map((l, n) => (n === 0 ? '' : indent) + l).join('\n');
        const text = prefix + indent + body;
        const caret = text.indexOf('$0');
        const clean = text.replace('$0', '');
        const end = (prefix === '' && pos === lineStart) ? pos + before.length : pos;
        wtc._replace(ta, pos, end, clean, caret === -1 ? clean.length : caret);
        ta.dataset.touched = '1';
    },

    // Picks a loop counter name not used by any for loop the cursor is inside.
    _freeLoopVar(textBefore) {
        const stack = [];
        let pending = null;
        const re = /for\s*\(\s*int\s+(\w+)|[{}]/g;
        let m;
        while ((m = re.exec(textBefore))) {
            if (m[1]) pending = m[1];
            else if (m[0] === '{') { stack.push(pending); pending = null; }
            else stack.pop();
        }
        return ['i', 'j', 'k', 'm'].find(v => !stack.includes(v)) || 'i';
    },

    // Replaces a range, puts the caret at an offset into the new text, and tells Blazor.
    _replace(ta, start, end, text, caretOffset) {
        ta.setRangeText(text, start, end, 'end');
        const caret = start + caretOffset;
        ta.setSelectionRange(caret, caret);
        ta.dispatchEvent(new Event('input', { bubbles: true }));
    },

    // While a typed program runs, keep the running line in view on phones.
    revealSelector(selector) {
        this.reveal(document.querySelector(selector), 40);
    }
};
