using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GambianMuslimCommunity.Data;
using GambianMuslimCommunity.Models;

namespace GambianMuslimCommunity.Controllers
{
    public class MembershipController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembershipController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Membership/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Membership/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Member member)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingMember = await _context.Members
                    .FirstOrDefaultAsync(m => m.Email.ToLower() == member.Email.ToLower());
                
                if (existingMember != null)
                {
                    ModelState.AddModelError("Email", "A member with this email address already exists.");
                    return View(member);
                }

                // Set default values
                member.RegistrationDate = DateTime.UtcNow;
                member.MembershipStatus = "Pending";
                member.IsActive = true;

                try
                {
                    _context.Members.Add(member);
                    await _context.SaveChangesAsync();

                    // Log the registration
                    var activityLog = new MemberActivityLog
                    {
                        MemberId = member.Id,
                        Action = "Registration",
                        Description = "Member registered for community membership",
                        ActivityDate = DateTime.UtcNow,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                    };
                    
                    _context.MemberActivityLogs.Add(activityLog);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Assalamu Alaikum! Thank you for your membership application. Your registration has been submitted successfully and is pending approval from our community administrators. You will be notified via email once your application is reviewed. Barakallahu feek!";
                    return RedirectToAction(nameof(Success));
                }
                catch (Exception ex)
                {
                    // Log error (you might want to use a proper logging framework)
                    ModelState.AddModelError("", "An error occurred while processing your registration. Please try again or contact our administrators.");
                    return View(member);
                }
            }

            return View(member);
        }

        // GET: /Membership/Success
        public IActionResult Success()
        {
            return View();
        }

        // GET: /Membership/Directory
        public async Task<IActionResult> Directory()
        {
            var activeMembers = await _context.Members
                .Where(m => m.MembershipStatus == "Active" && m.IsActive)
                .OrderBy(m => m.FirstName)
                .ThenBy(m => m.LastName)
                .Select(m => new
                {
                    m.Id,
                    m.FirstName,
                    m.LastName,
                    m.City,
                    m.State,
                    m.Country,
                    m.Profession,
                    m.Nationality,
                    m.ApprovedDate
                })
                .ToListAsync();

            return View(activeMembers);
        }

        // GET: /Membership/Status
        public async Task<IActionResult> Status(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return View();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Email.ToLower() == email.ToLower());

            if (member == null)
            {
                ViewBag.ErrorMessage = "No member found with this email address.";
                return View();
            }

            return View(member);
        }

        // POST: /Membership/Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Status(string email, string dummy)
        {
            return await Status(email);
        }
    }
}