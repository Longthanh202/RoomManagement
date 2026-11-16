(function () {
    'use strict';

    var body = document.body;

    function openModal(id) {
        var m = document.getElementById(id);
        if (!m) return;
        m.classList.add('is-open');
        m.setAttribute('aria-hidden', 'false');
        body.classList.add('rc-modal-open');
        var first = m.querySelector('input, textarea, select');
        if (first) setTimeout(function () { first.focus(); }, 50);
    }

    function closeModal(m) {
        if (!m) return;
        m.classList.remove('is-open');
        m.setAttribute('aria-hidden', 'true');
        if (!document.querySelector('.rc-modal-overlay.is-open')) {
            body.classList.remove('rc-modal-open');
        }
    }

    document.addEventListener('click', function (e) {
        var opener = e.target.closest('[data-modal-open]');
        if (opener) {
            e.preventDefault();
            openModal(opener.getAttribute('data-modal-open'));
            return;
        }
        var closer = e.target.closest('[data-modal-close]');
        if (closer) {
            closeModal(closer.closest('.rc-modal-overlay'));
            return;
        }
        if (e.target.classList.contains('rc-modal-overlay')) closeModal(e.target);
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeModal(document.querySelector('.rc-modal-overlay.is-open'));
    });

    // Ngày đi không được trước ngày đến
    var arrival = document.getElementById('guest-arrival');
    var departure = document.getElementById('guest-departure');
    if (arrival && departure) {
        arrival.addEventListener('change', function () {
            departure.min = arrival.value || departure.min;
            if (departure.value && arrival.value && departure.value < arrival.value) {
                departure.value = '';
            }
        });
        departure.form.addEventListener('submit', function (e) {
            if (arrival.value && departure.value && departure.value < arrival.value) {
                e.preventDefault();
                departure.setCustomValidity('Ngày đi phải sau hoặc bằng ngày đến.');
                departure.reportValidity();
                departure.setCustomValidity('');
            }
        });
    }

    // Nếu server trả lỗi validate, tự mở lại modal chứa lỗi
    var err = document.querySelector('.rc-modal .validation-summary-errors, .rc-modal .field-validation-error:not(:empty)');
    if (err) {
        var overlay = err.closest('.rc-modal-overlay');
        if (overlay) openModal(overlay.id);
    }
})();