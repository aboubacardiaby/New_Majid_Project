document.addEventListener('DOMContentLoaded', function() {
    const hamburger = document.querySelector('.hamburger');
    const navMenu = document.querySelector('.nav-menu');
    const navLinks = document.querySelectorAll('.nav-menu a');

    hamburger.addEventListener('click', function() {
        hamburger.classList.toggle('active');
        navMenu.classList.toggle('active');
    });

    navLinks.forEach(link => {
        link.addEventListener('click', function() {
            hamburger.classList.remove('active');
            navMenu.classList.remove('active');
        });
    });

    window.addEventListener('scroll', function() {
        const header = document.querySelector('header');
        if (window.scrollY > 100) {
            header.style.background = 'rgba(255, 255, 255, 0.98)';
        } else {
            header.style.background = 'rgba(255, 255, 255, 0.95)';
        }
    });

    const contactForm = document.querySelector('.contact-form form');
    if (contactForm) {
        contactForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const name = document.getElementById('name').value;
            const email = document.getElementById('email').value;
            const subject = document.getElementById('subject').value;
            const message = document.getElementById('message').value;

            if (name && email && message) {
                alert('Thank you for your message! We will get back to you soon, Insha\'Allah.');
                contactForm.reset();
            } else {
                alert('Please fill in all required fields.');
            }
        });
    }

    function updatePrayerTimes() {
        const now = new Date();
        const today = now.toDateString();
        const currentTime = now.getHours() * 60 + now.getMinutes();

        const prayerTimes = {
            'Fajr': 345,    // 5:45 AM
            'Dhuhr': 735,   // 12:15 PM  
            'Asr': 150,     // 2:30 PM
            'Maghrib': 285, // 4:45 PM
            'Isha': 375     // 6:15 PM
        };

        let nextPrayer = null;
        let nextPrayerTime = null;

        for (const [prayer, time] of Object.entries(prayerTimes)) {
            if (currentTime < time) {
                nextPrayer = prayer;
                nextPrayerTime = time;
                break;
            }
        }

        if (!nextPrayer) {
            nextPrayer = 'Fajr';
            nextPrayerTime = prayerTimes['Fajr'] + 1440; // Next day
        }

        const prayerCards = document.querySelectorAll('.prayer-card');
        prayerCards.forEach(card => {
            const prayerName = card.querySelector('h3').textContent;
            if (prayerName === nextPrayer && nextPrayerTime < 1440) {
                card.style.background = 'rgba(212, 175, 55, 0.3)';
                card.style.border = '2px solid var(--secondary-color)';
            }
        });
    }

    updatePrayerTimes();
    setInterval(updatePrayerTimes, 60000);

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

    const animatedElements = document.querySelectorAll('.service-card, .event-card, .prayer-card');
    animatedElements.forEach(el => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';
        el.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(el);
    });

    const currentYear = new Date().getFullYear();
    const footerYear = document.querySelector('.footer-bottom p');
    if (footerYear) {
        footerYear.textContent = footerYear.textContent.replace('2024', currentYear);
    }

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

    const eventCards = document.querySelectorAll('.event-card');
    eventCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.boxShadow = '0 8px 25px rgba(44, 85, 48, 0.15)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.boxShadow = '0 4px 6px rgba(0, 0, 0, 0.1)';
        });
    });

    const serviceCards = document.querySelectorAll('.service-card');
    serviceCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            const icon = this.querySelector('i');
            icon.style.transform = 'scale(1.1)';
            icon.style.color = 'var(--primary-color)';
        });
        
        card.addEventListener('mouseleave', function() {
            const icon = this.querySelector('i');
            icon.style.transform = 'scale(1)';
            icon.style.color = 'var(--secondary-color)';
        });
    });

    window.addEventListener('load', function() {
        const heroContent = document.querySelector('.hero-content');
        heroContent.style.opacity = '0';
        heroContent.style.transform = 'translateY(30px)';
        heroContent.style.transition = 'opacity 1s ease, transform 1s ease';
        
        setTimeout(() => {
            heroContent.style.opacity = '1';
            heroContent.style.transform = 'translateY(0)';
        }, 300);
    });
});