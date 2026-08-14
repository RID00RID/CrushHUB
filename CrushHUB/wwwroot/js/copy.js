// Кнопки «Копировать»: data-copy указывает на id элемента, чей текст кладём в буфер.
document.querySelectorAll('[data-copy]').forEach(function (button) {
    button.addEventListener('click', function () {
        var source = document.getElementById(button.dataset.copy);

        if (!source || !navigator.clipboard) {
            return;
        }

        navigator.clipboard.writeText(source.textContent.trim()).then(function () {
            var label = button.textContent;

            button.textContent = 'Скопировано';
            setTimeout(function () { button.textContent = label; }, 1500);
        });
    });
});
