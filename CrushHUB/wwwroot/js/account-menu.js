// Выпадающее меню аккаунта: клик по аватару открывает, клик по подложке закрывает.
document.querySelectorAll('[data-account-menu]').forEach(function (menu) {
    var toggle = menu.querySelector('[data-account-menu-toggle]');
    var overlay = menu.querySelector('[data-account-menu-close]');

    if (toggle) {
        toggle.addEventListener('click', function () {
            menu.classList.toggle('account-menu--open');
        });
    }

    if (overlay) {
        overlay.addEventListener('click', function () {
            menu.classList.remove('account-menu--open');
        });
    }
});
