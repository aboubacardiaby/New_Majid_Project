document.addEventListener('DOMContentLoaded', function() {
    // Mobile Navigation
    const hamburger = document.getElementById('hamburger');
    const navMenu = document.getElementById('nav-menu');
    const navLinks = document.querySelectorAll('.nav-menu a');

    if (hamburger && navMenu) {
        hamburger.addEventListener('click', function() {
            hamburger.classList.toggle('active');
            navMenu.classList.toggle('active');
        });

        // Close mobile menu when clicking on a link or logout button
        navLinks.forEach(link => {
            link.addEventListener('click', function() {
                hamburger.classList.remove('active');
                navMenu.classList.remove('active');
            });
        });

        // Close mobile menu when clicking logout button
        const logoutButton = document.querySelector('.nav-logout-btn');
        if (logoutButton) {
            logoutButton.addEventListener('click', function() {
                hamburger.classList.remove('active');
                navMenu.classList.remove('active');
            });
        }
    }

    // Header scroll effect
    window.addEventListener('scroll', function() {
        const header = document.querySelector('header');
        if (header) {
            if (window.scrollY > 100) {
                header.style.background = 'rgba(255, 255, 255, 0.98)';
            } else {
                header.style.background = 'rgba(255, 255, 255, 0.95)';
            }
        }
    });

    // Contact Form Enhancement
    const contactForm = document.querySelector('.contact-form form');
    if (contactForm) {
        contactForm.addEventListener('submit', function(e) {
            // Add loading state
            const submitButton = this.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Sending...';
                submitButton.disabled = true;
            }

            // Reset button after form submission (in case of validation errors)
            setTimeout(() => {
                if (submitButton) {
                    submitButton.innerHTML = '<i class="fas fa-paper-plane"></i> Send Message';
                    submitButton.disabled = false;
                }
            }, 3000);
        });
    }

    // Prayer Times Auto-Update
    function updatePrayerTimes() {
        const now = new Date();
        const currentTime = now.getHours() * 60 + now.getMinutes();

        // Prayer times in minutes from midnight (these should ideally come from server)
        const prayerTimes = {
            'Fajr': 345,    // 5:45 AM
            'Dhuhr': 735,   // 12:15 PM  
            'Asr': 870,     // 2:30 PM (14:30)
            'Maghrib': 1005, // 4:45 PM (16:45)
            'Isha': 1095    // 6:15 PM (18:15)
        };

        let nextPrayer = null;
        let nextPrayerTime = null;

        // Find next prayer
        for (const [prayer, time] of Object.entries(prayerTimes)) {
            if (currentTime < time) {
                nextPrayer = prayer;
                nextPrayerTime = time;
                break;
            }
        }

        // If no prayer left today, next is Fajr tomorrow
        if (!nextPrayer) {
            nextPrayer = 'Fajr';
            nextPrayerTime = prayerTimes['Fajr'] + 1440; // Next day
        }

        // Update prayer cards
        const prayerCards = document.querySelectorAll('.prayer-card, .prayer-card-large');
        prayerCards.forEach(card => {
            const prayerName = card.querySelector('h3');
            if (prayerName && prayerName.textContent.trim() === nextPrayer && nextPrayerTime < 1440) {
                card.classList.add('next-prayer');
            } else {
                card.classList.remove('next-prayer');
            }
        });
    }

    // Update prayer times immediately and then every minute
    updatePrayerTimes();
    setInterval(updatePrayerTimes, 60000);

    // Intersection Observer for animations
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);

    // Animate elements on scroll
    const animatedElements = document.querySelectorAll('.service-card, .event-card, .prayer-card, .program-card');
    animatedElements.forEach(el => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';
        el.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(el);
    });

    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                const headerOffset = 80;
                const elementPosition = target.getBoundingClientRect().top;
                const offsetPosition = elementPosition + window.pageYOffset - headerOffset;

                window.scrollTo({
                    top: offsetPosition,
                    behavior: 'smooth'
                });
            }
        });
    });

    // Enhanced hover effects for cards
    const eventCards = document.querySelectorAll('.event-card, .event-card-detailed');
    eventCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.boxShadow = '0 8px 25px rgba(44, 85, 48, 0.15)';
            this.style.transition = 'all 0.3s ease';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.boxShadow = '0 4px 6px rgba(0, 0, 0, 0.1)';
        });
    });

    const serviceCards = document.querySelectorAll('.service-card, .service-card-detailed');
    serviceCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            const icon = this.querySelector('i');
            if (icon) {
                icon.style.transform = 'scale(1.1)';
                icon.style.color = 'var(--primary-color)';
                icon.style.transition = 'all 0.3s ease';
            }
        });
        
        card.addEventListener('mouseleave', function() {
            const icon = this.querySelector('i');
            if (icon) {
                icon.style.transform = 'scale(1)';
                icon.style.color = 'var(--secondary-color)';
            }
        });
    });

    // Hero section animation
    window.addEventListener('load', function() {
        const heroContent = document.querySelector('.hero-content');
        if (heroContent) {
            heroContent.style.opacity = '0';
            heroContent.style.transform = 'translateY(30px)';
            heroContent.style.transition = 'opacity 1s ease, transform 1s ease';
            
            setTimeout(() => {
                heroContent.style.opacity = '1';
                heroContent.style.transform = 'translateY(0)';
            }, 300);
        }
    });

    // Form validation enhancement
    const formInputs = document.querySelectorAll('.form-control');
    formInputs.forEach(input => {
        input.addEventListener('focus', function() {
            this.style.borderColor = 'var(--primary-color)';
            this.style.boxShadow = '0 0 0 3px rgba(44, 85, 48, 0.1)';
        });

        input.addEventListener('blur', function() {
            if (!this.value) {
                this.style.borderColor = 'var(--border-color)';
                this.style.boxShadow = 'none';
            }
        });

        input.addEventListener('input', function() {
            // Remove error styling when user starts typing
            const errorSpan = this.parentElement.querySelector('.text-danger');
            if (errorSpan && this.value) {
                this.style.borderColor = 'var(--success-color)';
            }
        });
    });

    // Alert auto-dismiss
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.opacity = '0';
            alert.style.transform = 'translateY(-10px)';
            alert.style.transition = 'all 0.3s ease';
            setTimeout(() => {
                alert.style.display = 'none';
            }, 300);
        }, 5000);
    });

    // Prayer time countdown (for detailed prayer times page)
    const prayerTimeCountdown = document.querySelector('.prayer-time-countdown');
    if (prayerTimeCountdown) {
        function updateCountdown() {
            const now = new Date();
            const nextPrayerElement = document.querySelector('.prayer-card-large.next-prayer .prayer-time-large');
            
            if (nextPrayerElement) {
                const nextPrayerTimeText = nextPrayerElement.textContent.trim();
                const nextPrayerTime = parseTime(nextPrayerTimeText);
                
                if (nextPrayerTime) {
                    const diff = nextPrayerTime - now;
                    
                    if (diff > 0) {
                        const hours = Math.floor(diff / (1000 * 60 * 60));
                        const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
                        const seconds = Math.floor((diff % (1000 * 60)) / 1000);
                        
                        prayerTimeCountdown.innerHTML = `Next prayer in: ${hours}h ${minutes}m ${seconds}s`;
                    } else {
                        prayerTimeCountdown.innerHTML = 'Prayer time has arrived!';
                    }
                }
            }
        }

        setInterval(updateCountdown, 1000);
    }

    // Helper function to parse time string
    function parseTime(timeString) {
        const now = new Date();
        const timeRegex = /(\d{1,2}):(\d{2})\s*(AM|PM)/i;
        const match = timeString.match(timeRegex);
        
        if (match) {
            let hours = parseInt(match[1]);
            const minutes = parseInt(match[2]);
            const ampm = match[3].toUpperCase();
            
            if (ampm === 'PM' && hours !== 12) {
                hours += 12;
            } else if (ampm === 'AM' && hours === 12) {
                hours = 0;
            }
            
            const prayerTime = new Date(now);
            prayerTime.setHours(hours, minutes, 0, 0);
            
            // If prayer time has passed today, set it for tomorrow
            if (prayerTime < now) {
                prayerTime.setDate(prayerTime.getDate() + 1);
            }
            
            return prayerTime;
        }
        
        return null;
    }

    // Back to top button
    const backToTopButton = document.createElement('button');
    backToTopButton.innerHTML = '<i class="fas fa-arrow-up"></i>';
    backToTopButton.className = 'back-to-top';
    backToTopButton.style.cssText = `
        position: fixed;
        bottom: 20px;
        right: 20px;
        background: var(--gradient-primary);
        color: white;
        border: none;
        border-radius: 50%;
        width: 50px;
        height: 50px;
        cursor: pointer;
        opacity: 0;
        visibility: hidden;
        transition: all 0.3s ease;
        z-index: 1000;
        box-shadow: var(--shadow);
    `;

    document.body.appendChild(backToTopButton);

    window.addEventListener('scroll', function() {
        if (window.pageYOffset > 300) {
            backToTopButton.style.opacity = '1';
            backToTopButton.style.visibility = 'visible';
        } else {
            backToTopButton.style.opacity = '0';
            backToTopButton.style.visibility = 'hidden';
        }
    });

    backToTopButton.addEventListener('click', function() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });

    // Console greeting
    console.log('%c🕌 Gambian Muslim Community Website', 'color: #2c5530; font-size: 16px; font-weight: bold;');
    console.log('%cAssalamu Alaikum! Welcome to our community website.', 'color: #d4af37; font-size: 14px;');
});