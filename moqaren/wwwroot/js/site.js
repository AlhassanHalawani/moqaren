document.addEventListener("DOMContentLoaded", function () {
    document.addEventListener("DOMContentLoaded", function () {
        // Login Form Validation
        const loginForm = document.querySelector(".login-form");
        if (loginForm) {
            // Add event listener for form submission
            loginForm.addEventListener("submit", function (event) {
                // Get email and password input elements
                const email = document.getElementById("email");
                const password = document.getElementById("password");
                let valid = true; // Flag to track validation status

                clearErrors(); // Clear any existing error messages

                // Validate email input
                if (!validateEmail(email.value.trim())) {
                    showError(email, "Please enter a valid email address.");
                    valid = false;
                }

                // Validate password input (at least 6 characters)
                if (password.value.trim().length < 6) {
                    showError(password, "Password must be at least 6 characters long.");
                    valid = false;
                }

                // Prevent form submission if there are validation errors
                if (!valid) {
                    event.preventDefault();
                }
            });
        }

        // Register Form Validation
        const registerForm = document.querySelector(".registration-form");
        if (registerForm) {
            // Add event listener for form submission
            registerForm.addEventListener("submit", function (event) {
                // Get email, phone, and city input elements
                const email = document.getElementById("email");
                const phone = document.getElementById("phone");
                const city = document.getElementById("city");
                let valid = true; // Flag to track validation status

                clearErrors(); // Clear any existing error messages

                // Validate email input
                if (!validateEmail(email.value.trim())) {
                    showError(email, "Please enter a valid email address.");
                    valid = false;
                }

                // Validate phone number (10 digits if provided)
                if (phone.value.trim() && !/^\d{10}$/.test(phone.value.trim())) {
                    showError(phone, "Please enter a valid 10-digit phone number.");
                    valid = false;
                }

                // Validate city selection
                if (!city.value) {
                    showError(city, "Please select a city.");
                    valid = false;
                }

                // Prevent form submission if there are validation errors
                if (!valid) {
                    event.preventDefault();
                }
            });
        }

        // Signup Form Validation
        const signupForm = document.querySelector(".signup-form");
        if (signupForm) {
            // Add event listener for form submission
            signupForm.addEventListener("submit", function (event) {
                // Get all input elements for validation
                const firstname = document.getElementById("firstname");
                const lastname = document.getElementById("lastname");
                const email = document.getElementById("email");
                const password = document.getElementById("password");
                const confirmPassword = document.getElementById("confirm-password");
                const terms = document.getElementById("terms");
                let valid = true; // Flag to track validation status

                clearErrors(); // Clear any existing error messages

                // Validate first name input
                if (!firstname.value.trim()) {
                    showError(firstname, "First name is required.");
                    valid = false;
                }

                // Validate last name input
                if (!lastname.value.trim()) {
                    showError(lastname, "Last name is required.");
                    valid = false;
                }

                // Validate email input
                if (!validateEmail(email.value.trim())) {
                    showError(email, "Please enter a valid email address.");
                    valid = false;
                }

                // Validate password input (at least 6 characters)
                if (password.value.trim().length < 6) {
                    showError(password, "Password must be at least 6 characters long.");
                    valid = false;
                }

                // Validate confirm password (must match password)
                if (password.value.trim() !== confirmPassword.value.trim()) {
                    showError(confirmPassword, "Passwords do not match.");
                    valid = false;
                }

                // Validate terms and conditions checkbox
                if (!terms.checked) {
                    showError(terms, "You must agree to the terms and conditions.");
                    valid = false;
                }

                // Prevent form submission if there are validation errors
                if (!valid) {
                    event.preventDefault();
                }
            });
        }

        // Helper function to validate email using a regular expression
        function validateEmail(email) {
            const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return re.test(email);
        }

        // Function to display an error message next to an input element
        function showError(input, message) {
            const errorElement = document.createElement("small");
            errorElement.className = "error-message";
            errorElement.style.color = "red";
            errorElement.textContent = message;
            input.parentElement.appendChild(errorElement);
        }

        // Function to clear all error messages
        function clearErrors() {
            const errorMessages = document.querySelectorAll(".error-message");
            errorMessages.forEach(error => error.remove());
        }
    });
});