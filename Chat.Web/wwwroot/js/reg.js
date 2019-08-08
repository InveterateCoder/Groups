document.addEventListener('DOMContentLoaded', () => {
    let reg_panel = document.getElementById('reg_panel');
    reg_panel.style.marginTop = (window.innerHeight - reg_panel.offsetHeight) / 2 + 'px';
    window.addEventListener('resize', () => {
        reg_panel.style.marginTop = (window.innerHeight - reg_panel.offsetHeight) / 2 + 'px';
    });
});
function register() {

}