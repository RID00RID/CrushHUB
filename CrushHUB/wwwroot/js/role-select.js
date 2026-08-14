// Смена роли отправляется сразу при выборе — отдельной кнопки «Сохранить» в макете нет.
document.querySelectorAll('.role-select').forEach(function (select) {
    select.addEventListener('change', function () {
        select.form.submit();
    });
});
