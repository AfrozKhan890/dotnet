/* ========================================
   SIOMS ADMIN CORE JS
   ======================================== */

$(document).ready(function() {
    'use strict';

    // ===== SIDEBAR TOGGLE FOR MOBILE =====
    $('#sidebarToggle').on('click', function(e) {
        e.preventDefault();
        $('#mainSidebar').toggleClass('show');
        $('#sidebarOverlay').toggleClass('show');
        $('body').toggleClass('sidebar-open');
    });

    $('#sidebarOverlay').on('click', function() {
        $('#mainSidebar').removeClass('show');
        $('#sidebarOverlay').removeClass('show');
        $('body').removeClass('sidebar-open');
    });

    // ===== AUTO CLOSE SIDEBAR ON MOBILE WHEN LINK CLICKED =====
    $('.nav-sidebar .nav-link').on('click', function() {
        if ($(window).width() < 992) {
            $('#mainSidebar').removeClass('show');
            $('#sidebarOverlay').removeClass('show');
            $('body').removeClass('sidebar-open');
        }
    });

    // ===== OVERLAY SCROLLBARS =====
    if (typeof OverlayScrollbars !== 'undefined') {
        try {
            OverlayScrollbars(document.querySelectorAll('body'), {
                className: 'os-theme-dark',
                sizeAutoCapable: true,
                paddingAbsolute: false,
                scrollbars: {
                    autoHide: 'leave',
                    autoHideDelay: 200,
                    autoHideInteractive: false,
                    clickScrolling: true,
                    dragScrolling: true,
                    overflowBehavior: {
                        x: 'visible-hidden',
                        y: 'scroll'
                    }
                }
            });
        } catch (e) {
            console.log('OverlayScrollbars initialization skipped');
        }
    }

    // ===== AUTO DISMISS ALERTS =====
    $('.alert').each(function() {
        var $alert = $(this);
        setTimeout(function() {
            $alert.fadeOut('slow', function() {
                $(this).remove();
            });
        }, 5000);
    });

    // ===== WINDOW RESIZE HANDLER =====
    $(window).on('resize', function() {
        if ($(window).width() >= 992) {
            $('#mainSidebar').removeClass('show');
            $('#sidebarOverlay').removeClass('show');
            $('body').removeClass('sidebar-open');
        }
    });

    // ===== TOOLTIP INITIALIZATION =====
    if ($.fn.tooltip) {
        $('[data-toggle="tooltip"]').tooltip();
    }

    // ===== POPOVER INITIALIZATION =====
    if ($.fn.popover) {
        $('[data-toggle="popover"]').popover();
    }

    // ===== CONFIRM DELETE =====
    $('.delete-confirm').on('click', function(e) {
        e.preventDefault();
        var href = $(this).attr('href');
        var message = $(this).data('message') || 'Are you sure you want to delete this item?';
        
        if (confirm(message)) {
            window.location.href = href;
        }
    });

    // ===== DATATABLE DEFAULTS =====
    if ($.fn.DataTable) {
        $.extend(true, $.fn.DataTable.defaults, {
            language: {
                search: 'Search:',
                lengthMenu: 'Show _MENU_ entries',
                info: 'Showing _START_ to _END_ of _TOTAL_ entries',
                infoEmpty: 'No entries found',
                infoFiltered: '(filtered from _MAX_ total entries)',
                zeroRecords: 'No matching records found',
                paginate: {
                    first: 'First',
                    last: 'Last',
                    next: 'Next',
                    previous: 'Previous'
                }
            },
            responsive: true,
            autoWidth: false
        });
    }

    // ===== SELECT2 DEFAULTS =====
    if ($.fn.select2) {
        $.fn.select2.defaults.set('theme', 'bootstrap4');
        $.fn.select2.defaults.set('width', '100%');
    }

    // ===== SUMMERNOTE DEFAULTS =====
    if ($.fn.summernote) {
        $.fn.summernote.defaults = {
            height: 200,
            toolbar: [
                ['style', ['style']],
                ['font', ['bold', 'underline', 'clear']],
                ['color', ['color']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['table', ['table']],
                ['insert', ['link']],
                ['view', ['fullscreen', 'codeview', 'help']]
            ]
        };
    }
});

// ===== LOADING SPINNER =====
function showLoading() {
    var spinner = `
        <div id="loadingSpinner" style="
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(255,255,255,0.8);
            z-index: 9999;
            display: flex;
            align-items: center;
            justify-content: center;
        ">
            <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;">
                <span class="sr-only">Loading...</span>
            </div>
        </div>
    `;
    $('body').append(spinner);
}

function hideLoading() {
    $('#loadingSpinner').remove();
}

// ===== AJAX SETUP =====
$.ajaxSetup({
    beforeSend: function() {
        // showLoading();
    },
    complete: function() {
        // hideLoading();
    }
});

// ===== TOAST NOTIFICATION =====
function showToast(message, type) {
    type = type || 'success';
    var icon = {
        'success': 'fa-check-circle',
        'danger': 'fa-exclamation-circle',
        'warning': 'fa-exclamation-triangle',
        'info': 'fa-info-circle'
    };
    
    var toast = `
        <div class="alert alert-${type} alert-dismissible fade show position-fixed" 
             style="top: 20px; right: 20px; z-index: 9999; min-width: 300px; box-shadow: 0 4px 20px rgba(0,0,0,0.15);">
            <i class="fas ${icon[type] || icon.info} mr-2"></i>
            ${message}
            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>
    `;
    
    $('body').append(toast);
    
    setTimeout(function() {
        $('.alert.position-fixed').fadeOut('slow', function() {
            $(this).remove();
        });
    }, 5000);
}