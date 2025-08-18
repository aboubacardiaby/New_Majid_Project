using PayPal.Api;

namespace GambianMuslimCommunity.Services
{
    public interface IPayPalService
    {
        APIContext GetAPIContext();
        Payment CreatePayment(decimal amount, string currency, string description, string returnUrl, string cancelUrl);
        Payment ExecutePayment(string paymentId, string payerId);
        bool ValidatePayment(Payment payment, decimal expectedAmount);
    }
}