using GambianMuslimCommunity.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PayPal.Api;

namespace GambianMuslimCommunity.Services
{
    public class PayPalService : IPayPalService
    {
        private readonly PayPalSettings _settings;
        private readonly ILogger<PayPalService> _logger;
        private readonly IMemoryCache _cache;
        private const string TokenCacheKey = "PayPal_AccessToken";

        public PayPalService(IOptions<PayPalSettings> settings, ILogger<PayPalService> logger, IMemoryCache cache)
        {
            _settings = settings.Value;
            _logger = logger;
            _cache = cache;
        }

        public APIContext GetAPIContext()
        {
            var config = new Dictionary<string, string>
            {
                { "mode", _settings.Mode },
                { "clientId", _settings.ClientId },
                { "clientSecret", _settings.ClientSecret }
            };

            var accessToken = _cache.GetOrCreate(TokenCacheKey, entry =>
            {
                // PayPal tokens are valid for ~9 hours; refresh at 8 hours to be safe
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8);
                _logger.LogInformation("Fetching new PayPal OAuth token");
                return new OAuthTokenCredential(_settings.ClientId, _settings.ClientSecret, config).GetAccessToken();
            });

            return new APIContext(accessToken)
            {
                Config = config
            };
        }

        public Payment CreatePayment(decimal amount, string currency, string description, string returnUrl, string cancelUrl)
        {
            try
            {
                var apiContext = GetAPIContext();

                var payment = new Payment
                {
                    intent = "sale",
                    payer = new Payer { payment_method = "paypal" },
                    transactions = new List<Transaction>
                    {
                        new Transaction
                        {
                            description = description,
                            invoice_number = Guid.NewGuid().ToString(),
                            amount = new Amount
                            {
                                currency = currency,
                                total = amount.ToString("F2")
                            }
                        }
                    },
                    redirect_urls = new RedirectUrls
                    {
                        cancel_url = cancelUrl,
                        return_url = returnUrl
                    }
                };

                var createdPayment = payment.Create(apiContext);
                _logger.LogInformation("PayPal payment created: {PaymentId}", createdPayment.id);
                return createdPayment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal payment");
                throw;
            }
        }

        public Payment ExecutePayment(string paymentId, string payerId)
        {
            try
            {
                var apiContext = GetAPIContext();
                var paymentExecution = new PaymentExecution { payer_id = payerId };
                var payment = new Payment { id = paymentId };
                
                var executedPayment = payment.Execute(apiContext, paymentExecution);
                _logger.LogInformation("PayPal payment executed: {PaymentId}", executedPayment.id);
                return executedPayment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing PayPal payment: {PaymentId}", paymentId);
                throw;
            }
        }

        public bool ValidatePayment(Payment payment, decimal expectedAmount)
        {
            if (payment == null || payment.state != "approved")
                return false;

            var transaction = payment.transactions?.FirstOrDefault();
            if (transaction == null)
                return false;

            if (decimal.TryParse(transaction.amount?.total, out decimal actualAmount))
            {
                return Math.Abs(actualAmount - expectedAmount) < 0.01m; // Allow for minor rounding differences
            }

            return false;
        }
    }
}