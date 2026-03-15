using System.Net;
using System.Net.Mail;
using GambianMuslimCommunity.Models;

namespace GambianMuslimCommunity.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendDonationReceiptAsync(MasjidDonation donation, MasjidProject project)
        {
            // Only send if donor provided an email
            if (string.IsNullOrWhiteSpace(donation.Email))
                return true;

            try
            {
                var smtp   = _config["Smtp:Host"]     ?? "smtp.gmail.com";
                var port   = int.Parse(_config["Smtp:Port"] ?? "587");
                var user   = _config["Smtp:Username"] ?? "";
                var pass   = _config["Smtp:Password"] ?? "";
                var from   = _config["Smtp:FromEmail"] ?? "noreply@gambianmuslimcommunity.org";
                var fromName = _config["Smtp:FromName"] ?? "Gambian Muslim Community";

                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                {
                    _logger.LogWarning("SMTP credentials not configured — skipping receipt email.");
                    return true; // treat as non-fatal
                }

                var receiptNumber = $"GMC-{donation.Id:D6}-{donation.DonationDate:yyyyMMdd}";
                var html = BuildReceiptHtml(donation, project, receiptNumber);

                using var message = new MailMessage();
                message.From = new MailAddress(from, fromName);
                message.To.Add(new MailAddress(donation.Email, donation.DonorName));
                message.Subject = $"Donation Receipt — {receiptNumber} | Gambian Muslim Community";
                message.IsBodyHtml = true;
                message.Body = html;

                using var client = new SmtpClient(smtp, port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(user, pass),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("Receipt email sent to {Email} for donation {Receipt}", donation.Email, receiptNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send receipt email to {Email}", donation.Email);
                return false; // non-fatal — payment is still recorded
            }
        }

        private static string BuildReceiptHtml(MasjidDonation donation, MasjidProject project, string receiptNumber)
        {
            var donorName    = donation.IsAnonymous ? "Anonymous Donor" : (donation.DonorName ?? "Valued Donor");
            var donationDate = donation.DonationDate.ToString("MMMM d, yyyy 'at' h:mm tt");
            var amount       = donation.Amount.ToString("C");
            var projectTitle = project?.Title ?? "Masjid Building Project";
            var txId         = string.IsNullOrEmpty(donation.TransactionId) ? "—" : donation.TransactionId;

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8"" />
<meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
<title>Donation Receipt</title>
</head>
<body style=""margin:0;padding:0;background:#f4f1eb;font-family:'Helvetica Neue',Helvetica,Arial,sans-serif;"">

  <!-- Wrapper -->
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f4f1eb;padding:40px 0;"">
    <tr><td align=""center"">
      <table width=""620"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 8px 32px rgba(0,0,0,.12);"">

        <!-- Header -->
        <tr>
          <td style=""background:linear-gradient(135deg,#1a3d1e 0%,#2c5530 100%);padding:36px 40px;text-align:center;"">
            <p style=""margin:0 0 8px;font-size:11px;letter-spacing:4px;text-transform:uppercase;color:rgba(212,175,55,.8);"">بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ</p>
            <h1 style=""margin:0;font-size:26px;color:#ffffff;font-weight:700;"">Gambian Muslim Community</h1>
            <p style=""margin:4px 0 0;font-size:13px;color:rgba(255,255,255,.6);letter-spacing:2px;text-transform:uppercase;"">Minnesota</p>
            <div style=""margin:20px auto 0;width:48px;height:2px;background:#d4af37;""></div>
          </td>
        </tr>

        <!-- Gold accent bar -->
        <tr>
          <td style=""background:linear-gradient(90deg,#d4af37,#b8941f);padding:3px 0;""></td>
        </tr>

        <!-- Thank you block -->
        <tr>
          <td style=""padding:36px 40px 24px;text-align:center;"">
            <div style=""width:72px;height:72px;border-radius:50%;background:linear-gradient(135deg,#d4af37,#b8941f);margin:0 auto 16px;display:flex;align-items:center;justify-content:center;"">
              <span style=""font-size:32px;"">&#10003;</span>
            </div>
            <h2 style=""margin:0 0 10px;font-size:22px;color:#1a3d1e;"">Jazakallahu Khayran!</h2>
            <p style=""margin:0;font-size:15px;color:#555;line-height:1.6;"">
              May Allah (SWT) reward you abundantly for your generous contribution.<br/>
              Your donation has been successfully received and recorded.
            </p>
          </td>
        </tr>

        <!-- Amount box -->
        <tr>
          <td style=""padding:0 40px 28px;"">
            <div style=""background:linear-gradient(135deg,#f5f3ee,#eae6dc);border:2px solid #d4af37;border-radius:12px;padding:24px;text-align:center;"">
              <p style=""margin:0 0 4px;font-size:12px;color:#888;text-transform:uppercase;letter-spacing:2px;"">Donation Amount</p>
              <p style=""margin:0;font-size:42px;font-weight:700;color:#1a3d1e;"">{amount}</p>
              <p style=""margin:6px 0 0;font-size:13px;color:#888;"">via {donation.PaymentMethod}</p>
            </div>
          </td>
        </tr>

        <!-- Receipt details table -->
        <tr>
          <td style=""padding:0 40px 32px;"">
            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border:1px solid #e8e4dc;border-radius:10px;overflow:hidden;"">
              <tr style=""background:#f9f7f4;"">
                <td colspan=""2"" style=""padding:12px 18px;font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:2px;color:#888;"">Receipt Details</td>
              </tr>
              {Row("Receipt No.", receiptNumber)}
              {Row("Donor", donorName)}
              {Row("Project", projectTitle)}
              {Row("Date", donationDate)}
              {Row("Transaction ID", txId)}
              {Row("Payment Method", donation.PaymentMethod ?? "PayPal")}
              {Row("Status", "<span style=\"color:#28a745;font-weight:700;\">&#10003; Completed</span>")}
            </table>
          </td>
        </tr>

        <!-- Hadith -->
        <tr>
          <td style=""padding:0 40px 32px;"">
            <div style=""background:linear-gradient(135deg,#1a3d1e,#2c5530);border-radius:12px;padding:24px 28px;text-align:center;"">
              <p style=""margin:0 0 6px;font-size:15px;font-style:italic;color:rgba(255,255,255,.85);line-height:1.7;"">
                &#8220;Whoever builds a mosque for Allah, Allah will build for him a house like it in Paradise.&#8221;
              </p>
              <p style=""margin:0;font-size:12px;color:#d4af37;"">— Prophet Muhammad ﷺ (Bukhari &amp; Muslim)</p>
            </div>
          </td>
        </tr>

        <!-- Next steps -->
        <tr>
          <td style=""padding:0 40px 32px;"">
            <p style=""margin:0 0 14px;font-size:13px;font-weight:700;color:#1a3d1e;text-transform:uppercase;letter-spacing:1px;"">What's Next</p>
            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
              {Step("1", "Keep this email as your official donation receipt.", "#d4af37")}
              {Step("2", "You can track your contribution history at any time using your email address.", "#2c5530")}
              {Step("3", "Share this project with family and friends — every donation brings us closer to our goal.", "#b8941f")}
            </table>
          </td>
        </tr>

        <!-- Footer -->
        <tr>
          <td style=""background:#f9f7f4;border-top:1px solid #e8e4dc;padding:24px 40px;text-align:center;"">
            <p style=""margin:0 0 6px;font-size:13px;color:#555;"">
              Questions? Contact us at
              <a href=""mailto:info@gambianmuslimcommunity.org"" style=""color:#2c5530;text-decoration:none;"">info@gambianmuslimcommunity.org</a>
            </p>
            <p style=""margin:0;font-size:11px;color:#aaa;"">
              &copy; {DateTime.Now.Year} Gambian Muslim Community in Minnesota &nbsp;|&nbsp;
              This is an automated receipt — please keep it for your records.
            </p>
          </td>
        </tr>

      </table>
    </td></tr>
  </table>
</body>
</html>";
        }

        private static string Row(string label, string value) => $@"
              <tr style=""border-top:1px solid #e8e4dc;"">
                <td style=""padding:11px 18px;font-size:13px;color:#888;width:40%;"">{label}</td>
                <td style=""padding:11px 18px;font-size:13px;color:#333;font-weight:600;"">{value}</td>
              </tr>";

        private static string Step(string num, string text, string color) => $@"
              <tr>
                <td style=""padding:6px 0;vertical-align:top;"">
                  <table cellpadding=""0"" cellspacing=""0"">
                    <tr>
                      <td style=""width:28px;height:28px;border-radius:50%;background:{color};color:#fff;font-size:12px;font-weight:700;text-align:center;vertical-align:middle;padding:6px 0;"">{num}</td>
                      <td style=""padding-left:12px;font-size:13px;color:#555;line-height:1.5;"">{text}</td>
                    </tr>
                  </table>
                </td>
              </tr>";
    }
}
