// Admin Core JavaScript - Essential functionality
document.addEventListener('DOMContentLoaded', function() {
    console.log('Admin Core JS loaded');
    
    // 1. Fix Sidebar Toggle
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebarClose = document.getElementById('sidebarClose');
    const sidebar = document.getElementById('sidebar-wrapper');
    const overlay = document.querySelector('.sidebar-overlay');
    
    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function(e) {
            e.preventDefault();
            sidebar.classList.toggle('active');
            
            if (overlay) {
                overlay.style.display = sidebar.classList.contains('active') ? 'block' : 'none';
            }
            
            // Update icon
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
    
    if (sidebarClose && sidebar) {
        sidebarClose.addEventListener('click', function() {
            sidebar.classList.remove('active');
            if (overlay) overlay.style.display = 'none';
        });
    }
    
    if (overlay && sidebar) {
        overlay.addEventListener('click', function() {
            sidebar.classList.remove('active');
            this.style.display = 'none';
        });
    }
    
    // 2. Fix Theme Toggle
    const themeToggle = document.querySelector('.theme-toggle');
    if (themeToggle) {
        themeToggle.addEventListener('click', function() {
            const html = document.documentElement;
            const currentTheme = html.getAttribute('data-bs-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            html.setAttribute('data-bs-theme', newTheme);
            
            // Save to localStorage
            localStorage.setItem('theme', newTheme);
            
            // Update icon
            const icon = this.querySelector('i');
            if (icon) {
                if (newTheme === 'dark') {
                    icon.classList.remove('fa-moon');
                    icon.classList.add('fa-sun');
                } else {
                    icon.classList.remove('fa-sun');
                    icon.classList.add('fa-moon');
                }
            }
        });
        
        // Load saved theme
        const savedTheme = localStorage.getItem('theme') || 'light';
        document.documentElement.setAttribute('data-bs-theme', savedTheme);
        
        const icon = themeToggle.querySelector('i');
        if (icon && savedTheme === 'dark') {
            icon.classList.remove('fa-moon');
            icon.classList.add('fa-sun');
        }
    }
    
    // 3. Fix Dropdowns
    const dropdowns = document.querySelectorAll('.dropdown-toggle');
    dropdowns.forEach(dropdown => {
        dropdown.addEventListener('click', function(e) {
            e.preventDefault();
            // Let Bootstrap handle this
        });
    });
    
    // 4. Fix Collapse for Orders
    const orderCollapse = document.getElementById('orderCollapse');
    if (orderCollapse) {
        // Bootstrap will handle this automatically
    }
    
    // 5. Fix all sidebar links
    const sidebarLinks = document.querySelectorAll('.sidebar-link');
    sidebarLinks.forEach(link => {
        // Remove any conflicting event listeners
        link.removeEventListener('click', null);
        
        // For collapse toggle links, let Bootstrap handle
        if (link.hasAttribute('data-bs-toggle') && link.getAttribute('data-bs-toggle') === 'collapse') {
            return;
        }
        
        // For regular links
        link.addEventListener('click', function(e) {
            if (this.href && !this.href.includes('#')) {
                console.log('Navigating to:', this.href);
                // Navigation will happen automatically via href
            }
        });
    });
    
    // 6. Auto-close sidebar on mobile when clicking a link
    const allLinks = document.querySelectorAll('a');
    allLinks.forEach(link => {
        link.addEventListener('click', function() {
            if (window.innerWidth < 992) { // Mobile
                if (sidebar) sidebar.classList.remove('active');
                if (overlay) overlay.style.display = 'none';
            }
        });
    });
    
    console.log('All core functionality initialized');
});

// Make functions available globally
window.adminCore = {
    toggleSidebar: function() {
        const sidebar = document.getElementById('sidebar-wrapper');
        const overlay = document.querySelector('.sidebar-overlay');
        if (sidebar) {
            sidebar.classList.toggle('active');
            if (overlay) {
                overlay.style.display = sidebar.classList.contains('active') ? 'block' : 'none';
            }
        }
    },
    
    setTheme: function(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem('theme', theme);
    }
};