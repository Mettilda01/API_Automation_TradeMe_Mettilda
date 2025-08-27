using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using TradeMe.Api.Tests.Configuration;

namespace TradeMe.Api.Tests.Client
{
    public class TradeMeApiClient
    {
        private readonly RestClient _client;
        private readonly TradeMeConfig _config;

        public TradeMeApiClient(TradeMeConfig config)
        {
            _config = config;
            _client = new RestClient(new RestClientOptions(_config.BaseUrl));
        }

        private void AddOAuthParameters(RestRequest request)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var nonce = Guid.NewGuid().ToString("N");
            var signature = $"{Uri.EscapeDataString(_config.ConsumerSecret)}&{Uri.EscapeDataString(_config.TokenSecret)}";
            
            var authHeader = $"OAuth " +
                           $"oauth_consumer_key=\"{Uri.EscapeDataString(_config.ConsumerKey)}\", " +
                           $"oauth_nonce=\"{Uri.EscapeDataString(nonce)}\", " +
                           $"oauth_signature=\"{Uri.EscapeDataString(signature)}\", " +
                           $"oauth_signature_method=\"PLAINTEXT\", " +
                           $"oauth_timestamp=\"{timestamp}\", " +
                           $"oauth_token=\"{Uri.EscapeDataString(_config.AccessToken)}\", " +
                           $"oauth_version=\"1.0\"";

            request.AddHeader("Authorization", authHeader);
        }

        private void LogRequest(RestRequest request, string methodName)
        {
            var fullUrl = $"{_config.BaseUrl.TrimEnd('/')}/{request.Resource.TrimStart('/')}";
            Console.WriteLine($"\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {methodName}");
            Console.WriteLine($"Making {request.Method} request to: {fullUrl}");
            Console.WriteLine("Headers:");
            foreach (var header in request.Parameters.Where(p => p.Type == ParameterType.HttpHeader))
            {
                if (header.Name == "Authorization")
                {
                    var auth = header.Value?.ToString() ?? "";
                    Console.WriteLine($"  {header.Name}: OAuth {auth.Split("OAuth ").LastOrDefault()?.Split(',').FirstOrDefault() ?? "[EMPTY]"}...");
                }
                else
                {
                    Console.WriteLine($"  {header.Name}: {header.Value}");
                }
            }
            Console.WriteLine();
        }

        private void LogResponse(RestResponse response, string methodName)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {methodName} Response");
            Console.WriteLine($"Status Code: {response.StatusCode} ({(int)response.StatusCode})");
            Console.WriteLine($"Status Description: {response.StatusDescription}");
            if (response.ErrorException != null)
            {
                Console.WriteLine($"Error: {response.ErrorException.Message}");
            }
            if (!string.IsNullOrEmpty(response.Content))
            {
                Console.WriteLine("Response Content:");
                Console.WriteLine(response.Content);
            }
            Console.WriteLine();
        }

        public async Task<RestResponse> GetListing(string listingId, string format = "json")
        {
            var request = new RestRequest($"listings/{listingId}.{format}");
            AddOAuthParameters(request);
            LogRequest(request, nameof(GetListing));
            var response = await _client.ExecuteAsync(request);
            LogResponse(response, nameof(GetListing));
            return response;
        }

        public async Task<RestResponse> AddToWatchList(string listingId, string format = "json")
        {
            var request = new RestRequest($"mytrademe/watchList/{listingId}.{format}", Method.Post);
            AddOAuthParameters(request);
            LogRequest(request, nameof(AddToWatchList));
            var response = await _client.ExecuteAsync(request);
            LogResponse(response, nameof(AddToWatchList));
            return response;
        }

        public async Task<RestResponse> GetWatchList(string filter = "", string format = "json")
        {
            string resource = string.IsNullOrWhiteSpace(filter)
                ? $"mytrademe/watchList.{format}"
                : $"mytrademe/watchList/{filter}.{format}";

            var request = new RestRequest(resource);
            //var request = new RestRequest($"mytrademe/watchlist/{filter}.{format}");
            AddOAuthParameters(request);
            LogRequest(request, nameof(GetWatchList));
            var response = await _client.ExecuteAsync(request);
            LogResponse(response, nameof(GetWatchList));
            return response;
        }

        public async Task<RestResponse> RemoveFromWatchList(string listingId, string format = "json")
        {
            var request = new RestRequest($"mytrademe/watchList/{listingId}.{format}", Method.Delete);
            AddOAuthParameters(request);
            LogRequest(request, nameof(RemoveFromWatchList));
            var response = await _client.ExecuteAsync(request);
            LogResponse(response, nameof(RemoveFromWatchList));
            return response;
        }
    }
}
