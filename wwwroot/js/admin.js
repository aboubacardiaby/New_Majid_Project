/**
 * Admin Panel JavaScript
 * Gambian Muslim Community Website
 */

$(document).ready(function() {
    // Initialize admin functionality
    initializeAdmin();
});

function initializeAdmin() {
    // Initialize tooltips
    initializeTooltips();
    
    // Initialize modals
    initializeModals();
    
    // Initialize data tables
    initializeDataTables();
    
    // Initialize form handlers
    initializeFormHandlers();
    
    // Initialize dashboard updates
    initializeDashboard();
    
    // Initialize sidebar
    initializeSidebar();
}

// Tooltip initialization
function initializeTooltips() {
    if (typeof bootstrap !== 'undefined') {
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
}

// Modal initialization
function initializeModals() {
    if (typeof bootstrap !== 'undefined') {
        // Initialize modals
        var modalElements = document.querySelectorAll('.modal');
        modalElements.forEach(function(modalEl) {
            new bootstrap.Modal(modalEl);
        });
    }
}

// Data tables initialization (if needed)
function initializeDataTables() {
    // Add DataTables initialization if needed
    if (typeof $.fn.DataTable !== 'undefined') {
        $('.admin-table').DataTable({
            responsive: true,
            pageLength: 25,
            order: [[0, 'desc']],
            language: {
                search: "Search:",
                lengthMenu: "Show _MENU_ entries",
                info: "Showing _START_ to _END_ of _TOTAL_ entries",
                paginate: {
                    first: "First",
                    last: "Last",
                    next: "Next",
                    previous: "Previous"
                }
            }
        });
    }
}

// Form handlers
function initializeFormHandlers() {
    // Handle form submissions with loading states
    $('form').on('submit', function() {
        var submitBtn = $(this).find('button[type="submit"]');
        if (submitBtn.length) {
            var originalText = submitBtn.html();
            submitBtn.html('<i class="fas fa-spinner fa-spin me-2"></i>Processing...')
                     .prop('disabled', true);
            
            // Re-enable after 10 seconds as failsafe
            setTimeout(function() {
                submitBtn.html(originalText).prop('disabled', false);
            }, 10000);
        }
    });
    
    // Handle AJAX form submissions
    $(document).on('click', '[data-ajax="true"]', function(e) {
        e.preventDefault();
        var $this = $(this);
        var url = $this.data('url') || $this.attr('href');
        var method = $this.data('method') || 'GET';
        var confirmMessage = $this.data('confirm');
        
        if (confirmMessage && !confirm(confirmMessage)) {
            return false;
        }
        
        // Show loading state
        var originalText = $this.html();
        $this.html('<i class="fas fa-spinner fa-spin"></i>').prop('disabled', true);
        
        $.ajax({
            url: url,
            method: method,
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    showNotification('Success', response.message || 'Operation completed successfully', 'success');
                    
                    // Reload page if requested
                    if (response.reload) {
                        setTimeout(function() {
                            location.reload();
                        }, 1500);
                    }
                } else {
                    showNotification('Error', response.message || 'An error occurred', 'error');
                }
            },
            error: function() {
                showNotification('Error', 'A network error occurred. Please try again.', 'error');
            },
            complete: function() {
                // Restore button state
                $this.html(originalText).prop('disabled', false);
            }
        });
    });
}

// Dashboard specific functionality
function initializeDashboard() {
    // Animate statistics cards
    animateStatsCards();
    
    // Auto-refresh dashboard data
    if (window.location.pathname.includes('/Admin/Dashboard')) {
        // Refresh every 5 minutes
        setInterval(refreshDashboardStats, 300000);
    }
}

// Animate statistics cards
function animateStatsCards() {
    $('.stats-value').each(function() {
        var $this = $(this);
        var countTo = parseInt($this.text().replace(/[^0-9]/g, ''));
        
        if (countTo > 0) {
            $({ countNum: 0 }).animate({
                countNum: countTo
            }, {
                duration: 2000,
                easing: 'swing',
                step: function() {
                    var prefix = $this.text().replace(/[0-9]/g, '').charAt(0) || '';
                    $this.text(prefix + Math.floor(this.countNum).toLocaleString());
                },
                complete: function() {
                    var prefix = $this.text().replace(/[0-9]/g, '').charAt(0) || '';
                    $this.text(prefix + countTo.toLocaleString());
                }
            });
        }
    });
}

// Refresh dashboard statistics
function refreshDashboardStats() {
    $.get('/Admin/GetDashboardStats', function(data) {
        if (data.success) {
            updateDashboardStats(data.stats);
        }
    }).fail(function() {
        console.log('Failed to refresh dashboard stats');
    });
}

// Update dashboard statistics
function updateDashboardStats(stats) {
    $('.stats-card').each(function() {
        var $card = $(this);
        var statType = $card.data('stat-type');
        
        if (stats[statType] !== undefined) {
            var $value = $card.find('.stats-value');
            var newValue = stats[statType];
            
            // Animate the change
            $value.fadeOut(200, function() {
                $value.text(newValue).fadeIn(200);
            });
        }
    });
}

// Sidebar functionality
function initializeSidebar() {
    // Mobile sidebar toggle
    $('#sidebarToggle').on('click', function() {
        $('.sidebar').toggleClass('collapsed');
        $('.main-content').toggleClass('expanded');
    });
    
    // Auto-collapse sidebar on mobile
    if ($(window).width() < 768) {
        $('.sidebar').addClass('collapsed');
        $('.main-content').addClass('expanded');
    }
    
    // Handle window resize
    $(window).on('resize', function() {
        if ($(window).width() >= 768) {
            $('.sidebar').removeClass('collapsed');
            $('.main-content').removeClass('expanded');
        }
    });
}

// Notification system
function showNotification(title, message, type) {
    var alertClass = 'alert-info';
    var icon = 'fas fa-info-circle';
    
    switch (type) {
        case 'success':
            alertClass = 'alert-success';
            icon = 'fas fa-check-circle';
            break;
        case 'error':
        case 'danger':
            alertClass = 'alert-danger';
            icon = 'fas fa-exclamation-triangle';
            break;
        case 'warning':
            alertClass = 'alert-warning';
            icon = 'fas fa-exclamation-circle';
            break;
    }
    
    var notificationHtml = `
        <div class="alert ${alertClass} alert-dismissible fade show notification-toast" role="alert" style="position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
            <i class="${icon} me-2"></i>
            <strong>${title}:</strong> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    $('body').append(notificationHtml);
    
    // Auto-dismiss after 5 seconds
    setTimeout(function() {
        $('.notification-toast').alert('close');
    }, 5000);
}

// Utility functions
function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(amount);
}

function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

function formatDateTime(dateString) {
    return new Date(dateString).toLocaleString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// Export functions for global use
window.AdminPanel = {
    showNotification: showNotification,
    formatCurrency: formatCurrency,
    formatDate: formatDate,
    formatDateTime: formatDateTime,
    refreshDashboardStats: refreshDashboardStats
};

// Message management functions
function markMessageAsRead(messageId) {
    $.post('/Admin/MarkMessageRead', {
        id: messageId,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    }, function(response) {
        if (response.success) {
            $(`[data-message-id="${messageId}"]`).removeClass('unread').addClass('read');
            showNotification('Success', 'Message marked as read', 'success');
        } else {
            showNotification('Error', 'Failed to mark message as read', 'error');
        }
    });
}

function deleteMessage(messageId) {
    if (confirm('Are you sure you want to delete this message?')) {
        $.post('/Admin/DeleteMessage', {
            id: messageId,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        }, function(response) {
            if (response.success) {
                $(`[data-message-id="${messageId}"]`).fadeOut(300, function() {
                    $(this).remove();
                });
                showNotification('Success', 'Message deleted successfully', 'success');
            } else {
                showNotification('Error', 'Failed to delete message', 'error');
            }
        });
    }
}

// Settings management
function updateSettings(formData) {
    $.post('/Admin/UpdateSettings', formData, function(response) {
        if (response.success) {
            showNotification('Success', 'Settings updated successfully', 'success');
        } else {
            showNotification('Error', response.message || 'Failed to update settings', 'error');
        }
    });
}

// Real-time features (if WebSockets are implemented)
function initializeRealTime() {
    // Placeholder for real-time notifications
    // Could implement SignalR for real-time updates
}

// Error handling
window.addEventListener('error', function(e) {
    console.error('JavaScript error:', e.error);
    // Could send error reports to server
});

// Keyboard shortcuts
$(document).on('keydown', function(e) {
    // Ctrl+Shift+D for Dashboard
    if (e.ctrlKey && e.shiftKey && e.keyCode === 68) {
        window.location.href = '/Admin/Dashboard';
    }
    
    // Ctrl+Shift+S for Settings
    if (e.ctrlKey && e.shiftKey && e.keyCode === 83) {
        window.location.href = '/Admin/Settings';
    }
});

// Performance monitoring
function logPerformance(action, startTime) {
    var endTime = performance.now();
    var duration = endTime - startTime;
    console.log(`${action} took ${duration.toFixed(2)} milliseconds`);
}

// Initialize performance monitoring
var pageLoadStart = performance.now();
$(window).on('load', function() {
    logPerformance('Page load', pageLoadStart);
});