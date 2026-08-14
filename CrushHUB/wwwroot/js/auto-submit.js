// Поля с data-auto-submit отправляют свою форму сразу при изменении — как в макете, без кнопки.
document.querySelectorAll('[data-auto-submit]').forEach(function (field) {
    field.addEventListener('change', function () {
        field.form.submit();
    });
});
