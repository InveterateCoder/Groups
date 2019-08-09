var app;
document.addEventListener('DOMContentLoaded', () => {
    app = new app_class();
    window.addEventListener('resize', () => {
        app.on_resize();
    });
});
class reg_panel_class {
    constructor() {
        this.panel = document.getElementById('reg_panel');
        this.move();
    }
    move() {
        let mt = (window.innerHeight - this.panel.offsetHeight) / 2;
        mt = mt < 0 ? 0 : mt;
        this.panel.style.marginTop = mt + 'px';
    }
    num(e) {
        if (isNaN(e.key))
            e.preventDefault();
    }
    pasteFilter(e) {
        if (isNaN(e.target.value))
            e.target.value = null;
    }
}
class app_class {
    constructor() {
        this.reg_panel = new reg_panel_class();
    }
    on_resize() {
        this.reg_panel.move();
    }
}