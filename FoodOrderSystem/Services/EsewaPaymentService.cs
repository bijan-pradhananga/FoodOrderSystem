using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace FoodOrderSystem.Services
{
    public class EsewaPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public EsewaPaymentService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public string GetPaymentUrl(string amount, string orderId)
        {
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["amt"] = amount;
            queryParams["pdc"] = "0";
            queryParams["psc"] = "0";
            queryParams["txAmt"] = "0";
            queryParams["tAmt"] = amount;
            queryParams["pid"] = orderId;
            queryParams["scd"] = _configuration["eSewa:MerchantId"];
            queryParams["su"] = _configuration["eSewa:SuccessUrl"];
            queryParams["fu"] = _configuration["eSewa:FailureUrl"];

            var paymentUrl = $"{_configuration["eSewa:PaymentUrl"]}?{queryParams}";
            return paymentUrl;
        }

        public async Task<bool> VerifyPayment(string amount, string orderId, string refId)
        {
            var verificationUrl = "https://uat.esewa.com.np/epay/transrec";
            var parameters = new Dictionary<string, string>
            {
                {"amt", amount},
                {"scd", _configuration["eSewa:MerchantId"]},
                {"pid", orderId},
                {"rid", refId}
            };

            var response = await _httpClient.PostAsync(verificationUrl, new FormUrlEncodedContent(parameters));
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody.Contains("<response_code>Success</response_code>");
        }
    }
}
