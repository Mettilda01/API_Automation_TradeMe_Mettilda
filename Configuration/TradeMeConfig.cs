namespace TradeMe.Api.Tests.Configuration
{
    public class TradeMeConfig
    {
        public string ConsumerKey { get; set; } = string.Empty;
        public string ConsumerSecret { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string TokenSecret { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.tmsandbox.co.nz/v1";
    }
}
