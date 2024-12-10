// Modal and form handling
document.addEventListener("DOMContentLoaded", function () {
    // Modal initialization
    const modal = document.getElementById('loginModal');

    // Check both cookie and session status
    const userType = getCookie('userType');
    const userName = document.querySelector('.user-name'); // Check if user is logged in through the DOM

    // Only show modal if there's no userType cookie AND no active session
    if (!userType && !userName) {
        modal.classList.add('show');
        console.log('Showing modal'); // For debugging
    }

    // Login Form Validation
    const loginForm = document.querySelector(".login-form");
    if (loginForm) {
        loginForm.addEventListener("submit", function (event) {
            const email = document.getElementById("email");
            const password = document.getElementById("password");
            let valid = true;

            clearErrors();

            if (!validateEmail(email.value.trim())) {
                showError(email, "Please enter a valid email address.");
                valid = false;
            }

            if (password.value.trim().length < 6) {
                showError(password, "Password must be at least 6 characters long.");
                valid = false;
            }

            if (!valid) {
                event.preventDefault();
            }
        });
    }

    // Register Form Validation
    const registerForm = document.querySelector(".registration-form");
    if (registerForm) {
        registerForm.addEventListener("submit", function (event) {
            const email = document.getElementById("email");
            const phone = document.getElementById("phone");
            const city = document.getElementById("city");
            let valid = true;

            clearErrors();

            if (!validateEmail(email.value.trim())) {
                showError(email, "Please enter a valid email address.");
                valid = false;
            }

            if (phone.value.trim() && !/^\d{10}$/.test(phone.value.trim())) {
                showError(phone, "Please enter a valid 10-digit phone number.");
                valid = false;
            }

            if (!city.value) {
                showError(city, "Please select a city.");
                valid = false;
            }

            if (!valid) {
                event.preventDefault();
            }
        });
    }

    // Signup Form Validation
    const signupForm = document.querySelector(".signup-form");
    if (signupForm) {
        signupForm.addEventListener("submit", function (event) {
            const firstname = document.getElementById("firstname");
            const lastname = document.getElementById("lastname");
            const email = document.getElementById("email");
            const password = document.getElementById("password");
            const confirmPassword = document.getElementById("confirm-password");
            const terms = document.getElementById("terms");
            let valid = true;

            clearErrors();

            if (!firstname.value.trim()) {
                showError(firstname, "First name is required.");
                valid = false;
            }

            if (!lastname.value.trim()) {
                showError(lastname, "Last name is required.");
                valid = false;
            }

            if (!validateEmail(email.value.trim())) {
                showError(email, "Please enter a valid email address.");
                valid = false;
            }

            if (password.value.trim().length < 6) {
                showError(password, "Password must be at least 6 characters long.");
                valid = false;
            }

            if (password.value.trim() !== confirmPassword.value.trim()) {
                showError(confirmPassword, "Passwords do not match.");
                valid = false;
            }

            if (!terms.checked) {
                showError(terms, "You must agree to the terms and conditions.");
                valid = false;
            }

            if (!valid) {
                event.preventDefault();
            }
        });
    }
});

// Guest session handling
function continueAsGuest() {
    const modal = document.getElementById('loginModal');

    fetch('/Home/SetGuestSession', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Set cookie client-side as well to ensure immediate effect
                document.cookie = "userType=guest;path=/;max-age=86400;secure;samesite=strict";
                modal.classList.remove('show');
                location.reload();
            }
        })
        .catch(error => {
            console.error('Error:', error);
        });
}

// Utility functions
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}

function validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

function showError(input, message) {
    const errorElement = document.createElement("small");
    errorElement.className = "error-message";
    errorElement.style.color = "red";
    errorElement.textContent = message;
    input.parentElement.appendChild(errorElement);
}

function clearErrors() {
    const errorMessages = document.querySelectorAll(".error-message");
    errorMessages.forEach(error => error.remove());
}