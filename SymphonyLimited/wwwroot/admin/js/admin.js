// Sidebar Toggle with Overlay
document.getElementById('sidebarToggle').addEventListener('click', function() {
    const sidebar = document.getElementById('sidebar-wrapper');
    const overlay = document.querySelector('.sidebar-overlay') || document.createElement('div');

    if (!document.querySelector('.sidebar-overlay')) {
        overlay.className = 'sidebar-overlay';
        document.body.appendChild(overlay);
    }

    sidebar.classList.toggle('active');
    document.body.classList.toggle('sidebar-open');

    // Close sidebar when clicking overlay
    overlay.addEventListener('click', function() {
        sidebar.classList.remove('active');
        document.body.classList.remove('sidebar-open');
    });
});

// Auto-close sidebar on larger screens
function handleResize() {
    const sidebar = document.getElementById('sidebar-wrapper');
    const overlay = document.querySelector('.sidebar-overlay');

    if (window.innerWidth >= 992) {
        sidebar.classList.add('active');
        document.body.classList.remove('sidebar-open');
        if (overlay) overlay.remove();
    } else {
        sidebar.classList.remove('active');
        document.body.classList.remove('sidebar-open');
        if (overlay) overlay.remove();
    }
}

// Initialize
handleResize();
window.addEventListener('resize', handleResize);

// Theme Toggle (unchanged)
document.querySelector('.theme-toggle').addEventListener('click', function() {
    const html = document.documentElement;
    const currentTheme = html.getAttribute('data-bs-theme');
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    html.setAttribute('data-bs-theme', newTheme);

    const icon = this.querySelector('i');
    icon.classList.toggle('fa-moon');
    icon.classList.toggle('fa-sun');

    localStorage.setItem('theme', newTheme);
});

// Initialize theme from localStorage
if (localStorage.getItem('theme') === 'dark') {
    document.documentElement.setAttribute('data-bs-theme', 'dark');
    const icon = document.querySelector('.theme-toggle i');
    if (icon) icon.classList.replace('fa-moon', 'fa-sun');
}