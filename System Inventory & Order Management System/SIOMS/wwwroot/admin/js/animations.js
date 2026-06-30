/* ========================================
   SIOMS ANIMATIONS JS
   ======================================== */

$(document).ready(function() {
    'use strict';

    // ===== STAGGERED ANIMATIONS =====
    $('.stagger-children').each(function() {
        var $container = $(this);
        var children = $container.children();
        var delay = 0;
        var step = 0.08;
        
        children.each(function() {
            var $child = $(this);
            $child.css({
                'opacity': '0',
                'animation': 'fadeInUp 0.5s ease forwards',
                'animation-delay': delay + 's'
            });
            delay += step;
        });
        
        // Mark as animated
        $container.addClass('animated');
    });

    // ===== SCROLL REVEAL =====
    var scrollRevealElements = $('.scroll-reveal');
    
    function checkScrollReveal() {
        var windowHeight = $(window).height();
        var scrollTop = $(window).scrollTop();
        
        scrollRevealElements.each(function() {
            var $el = $(this);
            var elementTop = $el.offset().top;
            var revealAt = elementTop - windowHeight + 100;
            
            if (scrollTop > revealAt && !$el.hasClass('revealed')) {
                $el.addClass('revealed');
                $el.css('animation', 'fadeInUp 0.5s ease forwards');
                
                // Remove opacity:0 after animation
                setTimeout(function() {
                    $el.css('opacity', '1');
                }, 500);
            }
        });
    }
    
    // Initial check
    checkScrollReveal();
    
    // Check on scroll
    $(window).on('scroll', function() {
        checkScrollReveal();
    });

    // ===== COUNTER ANIMATION =====
    $('.counter').each(function() {
        var $el = $(this);
        var target = parseInt($el.data('target')) || parseInt($el.text());
        var duration = $el.data('duration') || 1000;
        var start = 0;
        var step = Math.ceil(target / 60);
        var interval = duration / 60;
        
        $el.text(0);
        
        var counterInterval = setInterval(function() {
            start += step;
            if (start >= target) {
                start = target;
                clearInterval(counterInterval);
            }
            $el.text(start.toLocaleString());
        }, interval);
    });

    // ===== TYPING EFFECT =====
    $('.typing-effect').each(function() {
        var $el = $(this);
        var text = $el.data('text') || $el.text();
        var speed = $el.data('speed') || 50;
        var delay = $el.data('delay') || 0;
        var cursor = $el.data('cursor') !== undefined ? $el.data('cursor') : true;
        
        $el.text('');
        $el.css('border-right', cursor ? '2px solid #667eea' : 'none');
        
        setTimeout(function() {
            var i = 0;
            var typingInterval = setInterval(function() {
                if (i < text.length) {
                    $el.text($el.text() + text.charAt(i));
                    i++;
                } else {
                    clearInterval(typingInterval);
                    $el.css('border-right', 'none');
                }
            }, speed);
        }, delay);
    });

    // ===== PARALLAX EFFECT =====
    $('.parallax').on('mousemove', function(e) {
        var $el = $(this);
        var x = e.clientX / window.innerWidth - 0.5;
        var y = e.clientY / window.innerHeight - 0.5;
        var speed = $el.data('speed') || 20;
        
        $el.css({
            'transform': 'translate(' + (x * speed) + 'px, ' + (y * speed) + 'px)'
        });
    });

    // ===== RIPPLE EFFECT =====
    $('.ripple-effect').on('click', function(e) {
        var $el = $(this);
        var rect = $el[0].getBoundingClientRect();
        var x = e.clientX - rect.left;
        var y = e.clientY - rect.top;
        
        var ripple = $('<span class="ripple"></span>');
        ripple.css({
            'position': 'absolute',
            'border-radius': '50%',
            'background': 'rgba(255,255,255,0.3)',
            'width': '100px',
            'height': '100px',
            'left': x - 50 + 'px',
            'top': y - 50 + 'px',
            'transform': 'scale(0)',
            'animation': 'ripple 0.6s ease',
            'pointer-events': 'none'
        });
        
        $el.append(ripple);
        
        setTimeout(function() {
            ripple.remove();
        }, 600);
    });
});

// Add ripple animation to styles if not already there
if (!document.getElementById('ripple-style')) {
    var style = document.createElement('style');
    style.id = 'ripple-style';
    style.textContent = `
        @keyframes ripple {
            from {
                transform: scale(0);
                opacity: 0.5;
            }
            to {
                transform: scale(2);
                opacity: 0;
            }
        }
    `;
    document.head.appendChild(style);
}