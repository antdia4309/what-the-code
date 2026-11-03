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

    // On phones the dock covers the bottom of the screen. After a block is added,
    // scroll just enough that it sits between the pinned board and the dock.
    revealBlock(id) {
        const dock = document.querySelector('.dock');
        if (!dock || getComputedStyle(dock).position !== 'fixed') return;
        const block = document.querySelector(`[data-block="${id}"]`);
        if (!block) return;

        const board = document.querySelector('.board-panel');
        const visibleTop = board ? board.getBoundingClientRect().bottom : 0;
        const visibleBottom = dock.getBoundingClientRect().top;
        const r = block.getBoundingClientRect();
        const wanted = r.top + Math.min(r.height, 150); // a new loop shows its drop area too

        if (wanted > visibleBottom - 12) {
            window.scrollBy({ top: wanted - visibleBottom + 20, behavior: 'smooth' });
        } else if (r.top < visibleTop + 8) {
            window.scrollBy({ top: r.top - visibleTop - 16, behavior: 'smooth' });
        }
    }
};
