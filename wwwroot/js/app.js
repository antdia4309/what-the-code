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
    }
};
