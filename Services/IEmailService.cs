using GambianMuslimCommunity.Models;

namespace GambianMuslimCommunity.Services
{
    public interface IEmailService
    {
        Task<bool> SendDonationReceiptAsync(MasjidDonation donation, MasjidProject project);
    }
}
