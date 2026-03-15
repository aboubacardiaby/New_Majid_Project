using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GambianMuslimCommunity.Data;
using GambianMuslimCommunity.Models;
using System.Security.Claims;

namespace GambianMuslimCommunity.Controllers
{
    [Authorize]
    public class AdminMembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminMembersController> _logger;

        public AdminMembersController(ApplicationDbContext context, ILogger<AdminMembersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /AdminMembers
        public async Task<IActionResult> Index(string status = "all", string search = "", int page = 1)
        {
            var membersQuery = _context.Members
                .Include(m => m.ApprovedBy)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                membersQuery = membersQuery.Where(m => m.MembershipStatus.ToLower() == status.ToLower());
            }

            // Search functionality
            if (!string.IsNullOrEmpty(search))
            {
                membersQuery = membersQuery.Where(m => 
                    m.FirstName.Contains(search) ||
                    m.LastName.Contains(search) ||
                    m.Email.Contains(search) ||
                    m.PhoneNumber.Contains(search) ||
                    m.City.Contains(search) ||
                    m.Country.Contains(search));
            }

            var totalMembers = await membersQuery.CountAsync();
            var pageSize = 20;
            var totalPages = (int)Math.Ceiling(totalMembers / (double)pageSize);
            
            var members = await membersQuery
                .OrderByDescending(m => m.RegistrationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MemberListViewModel
                {
                    Id = m.Id,
                    FullName = m.FullName,
                    Email = m.Email,
                    PhoneNumber = m.PhoneNumber,
                    City = m.City,
                    Country = m.Country,
                    Profession = m.Profession,
                    Nationality = m.Nationality,
                    MembershipStatus = m.MembershipStatus,
                    RegistrationDate = m.RegistrationDate,
                    ApprovedDate = m.ApprovedDate,
                    ApprovedBy = m.ApprovedBy != null ? m.ApprovedBy.FullName : null,
                    IsActive = m.IsActive
                })
                .ToListAsync();

            // Get stats for dashboard cards
            var stats = new MembershipStatsViewModel
            {
                TotalMembers = await _context.Members.CountAsync(),
                PendingMembers = await _context.Members.CountAsync(m => m.MembershipStatus == "Pending"),
                ActiveMembers = await _context.Members.CountAsync(m => m.MembershipStatus == "Active"),
                SuspendedMembers = await _context.Members.CountAsync(m => m.MembershipStatus == "Suspended"),
                RejectedMembers = await _context.Members.CountAsync(m => m.MembershipStatus == "Rejected")
            };

            var viewModel = new MembersManagementViewModel
            {
                Members = members,
                Stats = stats,
                CurrentStatus = status,
                SearchTerm = search,
                CurrentPage = page,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };

            return View(viewModel);
        }

        // GET: /AdminMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.ApprovedBy)
                .Include(m => m.ActivityLogs)
                    .ThenInclude(a => a.AdminUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: /AdminMembers/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            if (member.MembershipStatus != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending members can be approved.";
                return RedirectToAction(nameof(Index));
            }

            var adminId = GetCurrentAdminId();
            
            member.MembershipStatus = "Active";
            member.ApprovedDate = DateTime.UtcNow;
            member.ApprovedById = adminId;

            // Log the activity
            var activityLog = new MemberActivityLog
            {
                MemberId = member.Id,
                Action = "Approved",
                Description = $"Membership approved by admin",
                ActivityDate = DateTime.UtcNow,
                AdminUserId = adminId,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.MemberActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Member {member.FullName} has been approved successfully.";
            
            // TODO: Send email notification to member
            _logger.LogInformation("Member {MemberId} ({MemberEmail}) approved by admin {AdminId}", 
                member.Id, member.Email, adminId);

            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminMembers/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason = "")
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            if (member.MembershipStatus != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending members can be rejected.";
                return RedirectToAction(nameof(Index));
            }

            var adminId = GetCurrentAdminId();
            
            member.MembershipStatus = "Rejected";
            
            if (!string.IsNullOrEmpty(reason))
            {
                member.Notes = string.IsNullOrEmpty(member.Notes) 
                    ? $"Rejection reason: {reason}" 
                    : $"{member.Notes}\n\nRejection reason: {reason}";
            }

            // Log the activity
            var activityLog = new MemberActivityLog
            {
                MemberId = member.Id,
                Action = "Rejected",
                Description = $"Membership rejected by admin. Reason: {reason}",
                ActivityDate = DateTime.UtcNow,
                AdminUserId = adminId,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.MemberActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Member {member.FullName} has been rejected.";
            
            // TODO: Send email notification to member
            _logger.LogInformation("Member {MemberId} ({MemberEmail}) rejected by admin {AdminId}. Reason: {Reason}", 
                member.Id, member.Email, adminId, reason);

            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminMembers/Suspend/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(int id, string reason = "")
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            if (member.MembershipStatus != "Active")
            {
                TempData["ErrorMessage"] = "Only active members can be suspended.";
                return RedirectToAction(nameof(Index));
            }

            var adminId = GetCurrentAdminId();
            
            member.MembershipStatus = "Suspended";
            
            if (!string.IsNullOrEmpty(reason))
            {
                member.Notes = string.IsNullOrEmpty(member.Notes) 
                    ? $"Suspension reason: {reason}" 
                    : $"{member.Notes}\n\nSuspension reason: {reason}";
            }

            // Log the activity
            var activityLog = new MemberActivityLog
            {
                MemberId = member.Id,
                Action = "Suspended",
                Description = $"Membership suspended by admin. Reason: {reason}",
                ActivityDate = DateTime.UtcNow,
                AdminUserId = adminId,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.MemberActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Member {member.FullName} has been suspended.";
            
            // TODO: Send email notification to member
            _logger.LogInformation("Member {MemberId} ({MemberEmail}) suspended by admin {AdminId}. Reason: {Reason}", 
                member.Id, member.Email, adminId, reason);

            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminMembers/Reactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            if (member.MembershipStatus == "Active")
            {
                TempData["ErrorMessage"] = "Member is already active.";
                return RedirectToAction(nameof(Index));
            }

            var adminId = GetCurrentAdminId();
            
            member.MembershipStatus = "Active";
            member.ApprovedDate = DateTime.UtcNow;
            member.ApprovedById = adminId;
            member.IsActive = true;

            // Log the activity
            var activityLog = new MemberActivityLog
            {
                MemberId = member.Id,
                Action = "Reactivated",
                Description = "Membership reactivated by admin",
                ActivityDate = DateTime.UtcNow,
                AdminUserId = adminId,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.MemberActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Member {member.FullName} has been reactivated.";
            
            // TODO: Send email notification to member
            _logger.LogInformation("Member {MemberId} ({MemberEmail}) reactivated by admin {AdminId}", 
                member.Id, member.Email, adminId);

            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: /AdminMembers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Member member)
        {
            if (id != member.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMember = await _context.Members.FindAsync(id);
                    if (existingMember == null)
                    {
                        return NotFound();
                    }

                    // Update allowed properties
                    existingMember.FirstName = member.FirstName;
                    existingMember.LastName = member.LastName;
                    existingMember.Email = member.Email;
                    existingMember.PhoneNumber = member.PhoneNumber;
                    existingMember.Address = member.Address;
                    existingMember.City = member.City;
                    existingMember.State = member.State;
                    existingMember.PostalCode = member.PostalCode;
                    existingMember.Country = member.Country;
                    existingMember.DateOfBirth = member.DateOfBirth;
                    existingMember.Gender = member.Gender;
                    existingMember.Profession = member.Profession;
                    existingMember.Nationality = member.Nationality;
                    existingMember.MaritalStatus = member.MaritalStatus;
                    existingMember.EmergencyContactName = member.EmergencyContactName;
                    existingMember.EmergencyContactPhone = member.EmergencyContactPhone;
                    existingMember.EmergencyContactRelationship = member.EmergencyContactRelationship;
                    existingMember.Notes = member.Notes;
                    existingMember.ReceiveEmailNotifications = member.ReceiveEmailNotifications;
                    existingMember.ReceiveSmsNotifications = member.ReceiveSmsNotifications;
                    existingMember.PreferredLanguage = member.PreferredLanguage;
                    existingMember.IsActive = member.IsActive;

                    // Log the activity
                    var adminId = GetCurrentAdminId();
                    var activityLog = new MemberActivityLog
                    {
                        MemberId = member.Id,
                        Action = "Updated",
                        Description = "Member information updated by admin",
                        ActivityDate = DateTime.UtcNow,
                        AdminUserId = adminId,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                    };

                    _context.MemberActivityLogs.Add(activityLog);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Member information updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(member);
        }

        // POST: /AdminMembers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            var adminId = GetCurrentAdminId();
            var memberName = member.FullName;
            var memberEmail = member.Email;
            
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Member {memberName} has been deleted.";
            
            _logger.LogInformation("Member {MemberId} ({MemberEmail}) deleted by admin {AdminId}", 
                id, memberEmail, adminId);

            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminMembers/Export
        public async Task<IActionResult> Export(string format = "csv")
        {
            var members = await _context.Members
                .Include(m => m.ApprovedBy)
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName)
                .ToListAsync();

            if (format.ToLower() == "csv")
            {
                return ExportToCsv(members);
            }

            // Default to CSV if format not supported
            return ExportToCsv(members);
        }

        private FileResult ExportToCsv(List<Member> members)
        {
            var csv = new System.Text.StringBuilder();
            
            // Header
            csv.AppendLine("First Name,Last Name,Email,Phone Number,Address,City,State,Postal Code,Country," +
                          "Date of Birth,Gender,Profession,Nationality,Marital Status,Emergency Contact Name," +
                          "Emergency Contact Phone,Emergency Contact Relationship,Membership Status," +
                          "Registration Date,Approved Date,Approved By,Preferred Language,Notes");

            // Data rows
            foreach (var member in members)
            {
                csv.AppendLine($"\"{member.FirstName}\",\"{member.LastName}\",\"{member.Email}\"," +
                              $"\"{member.PhoneNumber}\",\"{member.Address}\",\"{member.City}\"," +
                              $"\"{member.State}\",\"{member.PostalCode}\",\"{member.Country}\"," +
                              $"\"{member.DateOfBirth:yyyy-MM-dd}\",\"{member.Gender}\",\"{member.Profession}\"," +
                              $"\"{member.Nationality}\",\"{member.MaritalStatus}\",\"{member.EmergencyContactName}\"," +
                              $"\"{member.EmergencyContactPhone}\",\"{member.EmergencyContactRelationship}\"," +
                              $"\"{member.MembershipStatus}\",\"{member.RegistrationDate:yyyy-MM-dd HH:mm:ss}\"," +
                              $"\"{member.ApprovedDate:yyyy-MM-dd HH:mm:ss}\",\"{member.ApprovedBy?.FullName ?? ""}\"," +
                              $"\"{member.PreferredLanguage}\",\"{member.Notes?.Replace("\"", "\"\"") ?? ""}\"");
            }

            var fileName = $"Members_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.Id == id);
        }

        private int? GetCurrentAdminId()
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim != null && int.TryParse(adminIdClaim.Value, out int adminId))
            {
                return adminId;
            }
            return null; // Return null if no valid admin ID found
        }
    }

    // View Models
    public class MemberListViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Profession { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string MembershipStatus { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedBy { get; set; }
        public bool IsActive { get; set; }
    }

    public class MembershipStatsViewModel
    {
        public int TotalMembers { get; set; }
        public int PendingMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int SuspendedMembers { get; set; }
        public int RejectedMembers { get; set; }
    }

    public class MembersManagementViewModel
    {
        public List<MemberListViewModel> Members { get; set; } = new();
        public MembershipStatsViewModel Stats { get; set; } = new();
        public string CurrentStatus { get; set; } = "all";
        public string SearchTerm { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}