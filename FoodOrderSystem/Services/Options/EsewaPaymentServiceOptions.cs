namespace FoodOrderSystem.Services.Options
{
    public class EsewaPaymentServiceOptions
    {
        public string BaseUrl { get; set; }       // eSewa API base URL
        public string MerchantCode { get; set; }   // Your merchant code
        public string SuccessUrl { get; set; }     // URL to redirect on success
        public string FailureUrl { get; set; }     // URL to redirect on failure
    }
}
