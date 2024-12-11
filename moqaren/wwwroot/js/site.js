// Main initialization
document.addEventListener("DOMContentLoaded", function () {
    initializeLoginModal();
    initializeFormValidations();
    initializePriceComparison();
    initializeCRUDHandlers();
});

// Login Modal Handling
function initializeLoginModal() {
    const modal = document.getElementById('loginModal');
    if (!modal) return;

    const userType = getCookie('userType');
    const userName = document.querySelector('.user-name');

    if (!userType && !userName) {
        modal.classList.add('show');
    }
}

// Form Validations
function initializeFormValidations() {
    // Login Form Validation
    const loginForm = document.querySelector(".login-form");
    if (loginForm) {
        loginForm.addEventListener("submit", function (event) {
            if (!validateLoginForm(event)) {
                event.preventDefault();
            }
        });
    }

    // Register Form Validation
    const registerForm = document.querySelector(".registration-form");
    if (registerForm) {
        registerForm.addEventListener("submit", function (event) {
            if (!validateRegistrationForm(event)) {
                event.preventDefault();
            }
        });
    }

    // Signup Form Validation
    const signupForm = document.querySelector(".signup-form");
    if (signupForm) {
        signupForm.addEventListener("submit", function (event) {
            if (!validateSignupForm(event)) {
                event.preventDefault();
            }
        });
    }
}

// Price Comparison Functionality
function initializePriceComparison() {
    initializePriceSorting();
    initializePriceTracking();
    initializePriceRefresh();
}

// CRUD Handlers
function initializeCRUDHandlers() {
    // Handle TempData messages
    const tempDataSuccess = document.querySelector('[data-success-message]');
    const tempDataError = document.querySelector('[data-error-message]');

    if (tempDataSuccess) {
        showNotification(tempDataSuccess.dataset.successMessage, 'success');
    }
    if (tempDataError) {
        showNotification(tempDataError.dataset.errorMessage, 'error');
    }

    // Initialize modals if they exist
    initializeProductModals();
}

// Form Validation Functions
function validateLoginForm(event) {
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

    return valid;
}

function validateRegistrationForm(event) {
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

    return valid;
}

function validateSignupForm(event) {
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

    return valid;
}

// Price Comparison Functions
function initializePriceSorting() {
    const sortSelect = document.getElementById('sort-prices');
    if (sortSelect) {
        sortSelect.addEventListener('change', function () {
            const rows = Array.from(document.querySelectorAll('.product-row'));
            const tbody = document.querySelector('.price-comparison tbody');

            rows.sort((a, b) => {
                const pricesA = Array.from(a.querySelectorAll('.price'))
                    .map(td => parseFloat(td.textContent.replace(/[^0-9.]/g, '')));
                const pricesB = Array.from(b.querySelectorAll('.price'))
                    .map(td => parseFloat(td.textContent.replace(/[^0-9.]/g, '')));

                const minA = Math.min(...pricesA);
                const minB = Math.min(...pricesB);

                return this.value === 'low' ? minA - minB : minB - minA;
            });

            rows.forEach(row => row.remove());
            rows.forEach(row => tbody.insertBefore(row, tbody.querySelector('.totals-row')));
        });
    }
}

function initializePriceTracking() {
    document.querySelectorAll('.track-price').forEach(button => {
        button.addEventListener('click', function () {
            const productId = this.dataset.productId;
            showNotification('Price tracking will be implemented soon!', 'info');
        });
    });
}

function initializePriceRefresh() {
    const refreshButton = document.getElementById('refresh-all');
    if (refreshButton) {
        refreshButton.addEventListener('click', function () {
            document.querySelectorAll('.price').forEach(priceCell => {
                priceCell.classList.add('price-updated');
                // Price refresh logic will be added here
            });
            showNotification('Prices updated successfully', 'success');
        });
    }
}

// Modal Functions
function initializeProductModals() {
    // Edit Product Modal
    window.editProduct = function (product) {
        document.getElementById('edit-productId').value = product.productID;
        document.getElementById('edit-name').value = product.name;
        document.getElementById('edit-brand').value = product.brand || '';
        document.getElementById('edit-model').value = product.model || '';
        document.getElementById('edit-categoryId').value = product.categoryID;
        document.getElementById('edit-description').value = product.description || '';

        const editModal = new bootstrap.Modal(document.getElementById('editModal'));
        editModal.show();
    };

    // Delete Product Modal
    window.deleteProduct = function (productId) {
        document.getElementById('delete-productId').value = productId;
        const deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
        deleteModal.show();
    };
}

// Utility Functions
function showNotification(message, type = 'success') {
    const existingNotifications = document.querySelectorAll('.notification');
    existingNotifications.forEach(notification => notification.remove());

    const notification = document.createElement('div');
    notification.className = `notification ${type}`;

    const content = document.createElement('div');
    content.className = 'notification-content';

    const icon = document.createElement('span');
    icon.className = 'notification-icon';
    icon.innerHTML = type === 'success' ? '✓' : type === 'error' ? '⚠' : 'ℹ';
    content.appendChild(icon);

    const messageElement = document.createElement('p');
    messageElement.className = 'notification-message';
    messageElement.textContent = message;
    content.appendChild(messageElement);

    notification.appendChild(content);

    const closeButton = document.createElement('button');
    closeButton.className = 'notification-close';
    closeButton.innerHTML = '×';
    closeButton.onclick = () => {
        notification.style.animation = 'fadeOut 0.3s ease-out forwards';
        setTimeout(() => notification.remove(), 300);
    };
    notification.appendChild(closeButton);

    document.body.appendChild(notification);

    setTimeout(() => {
        if (notification.parentElement) {
            notification.style.animation = 'fadeOut 0.3s ease-out forwards';
            setTimeout(() => notification.remove(), 300);
        }
    }, 5000);
}

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
                document.cookie = "userType=guest;path=/;max-age=86400;secure;samesite=strict";
                modal.classList.remove('show');
                location.reload();
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('Error setting guest session', 'error');
        });
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

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}


// Image Preview Functionality
function initializeImagePreviews() {
    const previewImage = (input, previewElement) => {
        if (input.files && input.files[0]) {
            const reader = new FileReader();
            reader.onload = function (e) {
                previewElement.src = e.target.result;
                previewElement.classList.remove('d-none');
            };
            reader.readAsDataURL(input.files[0]);
        }
    };

    // Add Product Image Preview
    document.querySelector('input[name="image"]').addEventListener('change', function (e) {
        const preview = document.getElementById('image-preview');
        validateAndPreviewImage(this, preview);
    });

    // Edit Product Image Preview
    document.getElementById('edit-image').addEventListener('change', function (e) {
        const preview = document.getElementById('edit-image-preview');
        validateAndPreviewImage(this, preview);
    });
}

// Image Validation
function validateAndPreviewImage(input, previewElement) {
    const file = input.files[0];
    const maxSize = input.dataset.maxSize || 5242880; // 5MB default

    if (file) {
        if (file.size > maxSize) {
            showNotification('File size must be less than 5MB', 'error');
            input.value = '';
            return;
        }

        if (!file.type.match('image.*')) {
            showNotification('Please select a valid image file', 'error');
            input.value = '';
            return;
        }

        previewImage(input, previewElement);
    }
}

// Product Search Functionality
function initializeSearch() {
    const searchInput = document.getElementById('productSearch');
    const table = document.getElementById('productsTable');

    searchInput.addEventListener('keyup', function () {
        const searchText = this.value.toLowerCase();
        const rows = table.querySelectorAll('tbody tr');

        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            row.style.display = text.includes(searchText) ? '' : 'none';
        });
    });
}

// Edit Product
function editProduct(product) {
    document.getElementById('edit-productId').value = product.productID;
    document.getElementById('edit-name').value = product.name;
    document.getElementById('edit-brand').value = product.brand || '';
    document.getElementById('edit-model').value = product.model || '';
    document.getElementById('edit-categoryId').value = product.categoryID;
    document.getElementById('edit-description').value = product.description || '';

    // Reset image preview
    document.getElementById('edit-image').value = '';
    document.getElementById('edit-image-preview').classList.add('d-none');

    const editModal = new bootstrap.Modal(document.getElementById('editModal'));
    editModal.show();
}

// Delete Product
function deleteProduct(productId) {
    document.getElementById('delete-productId').value = productId;
    const deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
    deleteModal.show();
}

// Form Validation
function initializeFormValidation() {
    document.querySelectorAll('form.needs-validation').forEach(form => {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
}

// Character Counter for Textareas
function initializeCharacterCounters() {
    document.querySelectorAll('textarea[maxlength]').forEach(textarea => {
        const counter = document.createElement('small');
        counter.className = 'text-muted d-block text-end';
        textarea.parentNode.appendChild(counter);

        const updateCounter = () => {
            const remaining = parseInt(textarea.getAttribute('maxlength')) - textarea.value.length;
            counter.textContent = `${remaining} characters remaining`;
        };

        textarea.addEventListener('input', updateCounter);
        updateCounter(); // Initial count
    });
}

// Handle Server-Side Messages
function handleServerMessages() {
    const successMsg = document.querySelector('[data-success-message]');
    const errorMsg = document.querySelector('[data-error-message]');

    if (successMsg) {
        showNotification(successMsg.dataset.successMessage, 'success');
    }
    if (errorMsg) {
        showNotification(errorMsg.dataset.errorMessage, 'error');
    }
}

// Table Row Hover Effect
function initializeTableHover() {
    const rows = document.querySelectorAll('#productsTable tbody tr');
    rows.forEach(row => {
        row.addEventListener('mouseenter', function () {
            this.style.cursor = 'pointer';
            this.classList.add('table-active');
        });
        row.addEventListener('mouseleave', function () {
            this.classList.remove('table-active');
        });
    });
}

// Modal Reset Handler
function initializeModalReset() {
    const editModal = document.getElementById('editModal');
    if (editModal) {
        editModal.addEventListener('hidden.bs.modal', function () {
            const form = this.querySelector('form');
            form.reset();
            form.classList.remove('was-validated');
            document.getElementById('edit-image-preview').classList.add('d-none');
        });
    }
}

// Initialization
document.addEventListener('DOMContentLoaded', function () {
    initializeImagePreviews();
    initializeSearch();
    initializeFormValidation();
    initializeCharacterCounters();
    handleServerMessages();
    initializeTableHover();
    initializeModalReset();

    // Enable tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});
