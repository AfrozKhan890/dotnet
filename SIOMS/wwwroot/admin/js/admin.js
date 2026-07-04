document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Sidebar toggle
    const sidebarToggle = document.getElementById('sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function(e) {
            e.preventDefault();
            document.getElementById('sidebar-wrapper').classList.toggle('active');
            document.getElementById('page-content-wrapper').classList.toggle('active');
            
            // Toggle icon
            const icon = this.querySelector('i');
            if (icon.classList.contains('fa-bars')) {
                icon.classList.remove('fa-bars');
                icon.classList.add('fa-times');
            } else {
                icon.classList.remove('fa-times');
                icon.classList.add('fa-bars');
            }
        });
    }

    // Theme toggle
    const themeToggle = document.querySelector('.theme-toggle');
    if (themeToggle) {
        themeToggle.addEventListener('click', function() {
            const currentTheme = document.documentElement.getAttribute('data-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            
            document.documentElement.setAttribute('data-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            
            // Update icon
            const icon = this.querySelector('i');
            icon.classList.toggle('fa-moon');
            icon.classList.toggle('fa-sun');
        });

        // Load saved theme
        const savedTheme = localStorage.getItem('theme') || 'light';
        document.documentElement.setAttribute('data-theme', savedTheme);
        
        // Update icon based on saved theme
        const icon = themeToggle.querySelector('i');
        if (savedTheme === 'dark') {
            icon.classList.remove('fa-moon');
            icon.classList.add('fa-sun');
        }
    }

    // Update current time
    function updateDateTime() {
        const now = new Date();
        const dateString = now.toLocaleDateString('en-US', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
        const timeString = now.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit'
        });
        
        const dateElement = document.getElementById('currentDate');
        const timeElement = document.getElementById('currentTime');
        
        if (dateElement) dateElement.textContent = dateString;
        if (timeElement) timeElement.textContent = timeString;
    }
    
    // Update every minute
    updateDateTime();
    setInterval(updateDateTime, 60000);

    // Check for unread alerts
    function checkUnreadAlerts() {
        fetch('/Admin/Alert/GetUnresolvedCount')
            .then(response => response.json())
            .then(data => {
                const alertCount = data.count || 0;
                const alertBadges = document.querySelectorAll('#alertCount, #topAlertCount');
                
                alertBadges.forEach(badge => {
                    if (alertCount > 0) {
                        badge.textContent = alertCount;
                        badge.classList.remove('d-none');
                    } else {
                        badge.classList.add('d-none');
                    }
                });
            })
            .catch(error => console.error('Error fetching alerts:', error));
    }
    
    // Check alerts every 30 seconds
    checkUnreadAlerts();
    setInterval(checkUnreadAlerts, 30000);

    // Auto-refresh dashboard data
    const refreshDashboard = document.getElementById('refreshDashboard');
    if (refreshDashboard) {
        refreshDashboard.addEventListener('click', function() {
            const btn = this;
            const originalHTML = btn.innerHTML;
            
            btn.innerHTML = '<i class="fas fa-sync-alt fa-spin me-2"></i> Refreshing...';
            btn.disabled = true;
            
            setTimeout(() => {
                location.reload();
            }, 1000);
        });
    }

    // Confirm before delete
    const deleteButtons = document.querySelectorAll('.btn-delete-confirm');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            if (!confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
                e.preventDefault();
                e.stopPropagation();
                return false;
            }
        });
    });

    // Form validation
    const forms = document.querySelectorAll('.needs-validation');
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });

    // Auto-dismiss alerts
    const alerts = document.querySelectorAll('.alert-auto-dismiss');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.5s ease';
            alert.style.opacity = '0';
            setTimeout(() => {
                alert.remove();
            }, 500);
        }, 5000);
    });

    // Update user info in navbar
    function updateUserInfo() {
        fetch('/Admin/Admin/CheckSession')
            .then(response => response.json())
            .then(data => {
                if (data.isAuthenticated) {
                    const usernameElements = document.querySelectorAll('#sidebarUsername, #navbarUsername');
                    usernameElements.forEach(el => {
                        el.textContent = data.username || 'Admin';
                    });
                }
            })
            .catch(error => console.error('Error fetching user info:', error));
    }
    
    updateUserInfo();

    // Handle bulk actions
    const bulkActionSelect = document.getElementById('bulkActionSelect');
    const bulkActionBtn = document.getElementById('bulkActionBtn');
    const selectAllCheckbox = document.getElementById('selectAll');
    
    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function() {
            const checkboxes = document.querySelectorAll('.row-checkbox');
            checkboxes.forEach(checkbox => {
                checkbox.checked = this.checked;
            });
        });
    }

    // Responsive table handling
    function handleResponsiveTables() {
        const tables = document.querySelectorAll('.table-responsive');
        
        tables.forEach(table => {
            const wrapper = table.parentElement;
            if (window.innerWidth < 768) {
                wrapper.classList.add('table-responsive-sm');
            } else {
                wrapper.classList.remove('table-responsive-sm');
            }
        });
    }
    
    window.addEventListener('resize', handleResponsiveTables);
    handleResponsiveTables();

    // Initialize charts if Chart.js is loaded
    if (typeof Chart !== 'undefined') {
        initializeCharts();
    }

    // Print functionality
    const printButtons = document.querySelectorAll('.btn-print');
    printButtons.forEach(button => {
        button.addEventListener('click', function() {
            window.print();
        });
    });
});

// Initialize dashboard charts
function initializeCharts() {
    // Sales Chart
    const salesCtx = document.getElementById('salesChart');
    if (salesCtx) {
        new Chart(salesCtx, {
            type: 'line',
            data: {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
                datasets: [{
                    label: 'Sales',
                    data: [12000, 19000, 15000, 25000, 22000, 30000, 28000, 35000, 30000, 40000, 38000, 45000],
                    borderColor: '#4361ee',
                    backgroundColor: 'rgba(67, 97, 238, 0.1)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'top',
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            drawBorder: false
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    }

    // Stock Chart
    const stockCtx = document.getElementById('stockChart');
    if (stockCtx) {
        new Chart(stockCtx, {
            type: 'bar',
            data: {
                labels: ['Electronics', 'Clothing', 'Books', 'Home', 'Sports', 'Toys'],
                datasets: [{
                    label: 'Current Stock',
                    data: [120, 190, 150, 250, 220, 180],
                    backgroundColor: [
                        'rgba(67, 97, 238, 0.8)',
                        'rgba(76, 201, 240, 0.8)',
                        'rgba(247, 37, 133, 0.8)',
                        'rgba(255, 158, 0, 0.8)',
                        'rgba(114, 9, 183, 0.8)',
                        'rgba(230, 57, 70, 0.8)'
                    ],
                    borderColor: [
                        '#4361ee',
                        '#4cc9f0',
                        '#f72585',
                        '#ff9e00',
                        '#7209b7',
                        '#e63946'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'top',
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            drawBorder: false
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    }
}

// Export functions for other scripts
window.adminDashboard = {
    refreshData: function() {
        checkUnreadAlerts();
        updateUserInfo();
    },
    
    showNotification: function(message, type = 'info') {
        const container = document.querySelector('.notification-container');
        if (!container) {
            const div = document.createElement('div');
            div.className = 'notification-container position-fixed top-0 end-0 p-3';
            div.style.zIndex = '9999';
            document.body.appendChild(div);
        }
        
        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible fade show`;
        alert.role = 'alert';
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;
        
        document.querySelector('.notification-container').appendChild(alert);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            alert.remove();
        }, 5000);
    },
    
    confirmAction: function(message) {
        return confirm(message || 'Are you sure?');
    }
};