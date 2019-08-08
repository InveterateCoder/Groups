document.addEventListener('DOMContentLoaded', () => {
    let reg_panel = document.getElementById('reg_panel');
    let mt = (window.innerHeight - reg_panel.offsetHeight) / 2;
    mt = mt < 0 ? 0 : mt;
    reg_panel.style.marginTop = mt + 'px';
    window.addEventListener('resize', () => {
        mt = (window.innerHeight - reg_panel.offsetHeight) / 2;
        mt = mt < 0 ? 0 : mt;
        reg_panel.style.marginTop = mt + 'px';
    });
});
function num(e) {
    if (isNaN(e.key))
        e.preventDefault();
}
function pasteCancel(e) {
    if (isNaN(e.target.value))
        e.target.value = null;
}