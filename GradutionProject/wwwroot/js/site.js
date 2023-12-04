$(document).ready(function () {
    // Email validation using jQuery
    $('#email').on('input', function () {
        var email = $(this).val();
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        var isValid = emailRegex.test(email);

        if (!isValid) {
            $(this).next('.text-danger').text('Invalid email address');
        } else {
            $(this).next('.text-danger').text('');
        }
    });
});