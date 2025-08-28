using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using TradeMe.Api.Tests.Configuration;

namespace TradeMe.Api.Tests.Client
{
    /// <summary>
    /// A client for interacting with the Trade Me API.
    /// Provides methods to get listings, and manage the authenticated user's watchlist.
    /// Handles OAuth 1.0 authentication and logs request/response details for debugging.
    /// </summary>
    public class TradeMeApiClient
    {
        private readonly RestClient _client;
        private readonly TradeMeConfig _config;


        /// <summary>
        /// Initializes a new instance of <see cref="TradeMeApiClient"/> with the specified configuration.
        /// </summary>
        /// <param name="config">TradeMe API configuration including keys, tokens, and base URL.</param>
        
        public TradeMeApiClient(TradeMeConfig config)
        {
            _config = config;
            _client = new RestClient(new RestClientOptions(_config.BaseUrl));
        }

        /// <summary>
        /// Adds OAuth 1.0 parameters to a RestRequest for authentication.
        /// </summary>
        /// <param name="request">The RestRequest to which OAuth headers will be added.</param>

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

        /// <summary>
        /// Logs the details of a REST request, including URL, method, and headers.
        /// Authorization headers are partially masked for security.
        /// </summary>
        /// <param name="request">The request to log.</param>
        /// <param name="methodName">The calling method name for reference.</param>

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

        /// <summary>
        /// Logs the response of a REST request, including status code, description, any errors, and content.
        /// Useful for debugging API calls and verifying responses.
        /// </summary>
        /// <param name="response">The response to log.</param>
        /// <param name="methodName">The calling method name for reference.</param>

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

        /// <summary>
        /// Retrieves the details of a specific listing by its ID.
        /// Sends a GET request to the Trade Me API listings endpoint and applies OAuth authentication.
        /// </summary>
        /// <param name="listingId">The ID of the listing to retrieve.</param>
        /// <param name="format">Response format (default is "json").</param>
        /// <returns>A task containing the API response with the listing details.</returns>

        public async Task<RestResponse> GetListing(string listingId, string format = "json")
        {
            var request = new RestRequest($"listings/{listingId}.{format}");
            AddOAuthParameters(request);
            LogRequest(request, nameof(GetListing));
            var response = await _client.ExecuteAsync(request);
            LogResponse(response, nameof(GetListing));
            return response;
        }


        /// <summary>
        /// Adds a specific listing to the authenticated user's watchlist.
        /// Sends a POST request to the watchlist endpoint and includes OAuth headers.
        /// </summary>
        /// <param name="listingId">The ID of the listing to add.</param>
        /// <param name="format">Response format (default is "json").</param>
        /// <returns>A task containing the API response confirming the addition.</returns>

        public async Task<RestResponse> AddToWatchList(string listingId, string format = "json")
        {
            var request = new RestRequest($"mytrademe/watchList/{listingId}.{format}", Method.Post);
            AddOAuthParameters(request);
            LogRequest(request, nameof(AddToWatchList));
            var response = await _client.ExecuteAsync(request);
            LogResponse(response, nameof(AddToWatchList));
            return response;
        }

        /// <summary>
        /// Retrieves the authenticated user's watchlist.
        /// If a filter is provided, only matching items are returned; otherwise, all items are returned.
        /// Includes OAuth authentication and logs the request/response for debugging.
        /// </summary>
        /// <param name="filter">Optional filter to narrow down watchlist results.</param>
        /// <param name="format">Response format (default is "json").</param>
        /// <returns>A task containing the API response with the watchlist items.</returns>

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
        /// <summary>
        /// Removes a specific listing from the authenticated user's watchlist.
        /// Sends a DELETE request to the watchlist endpoint and applies OAuth authentication.
        /// Useful for cleaning up or managing watchlist items programmatically.
        /// </summary>
        /// <param name="listingId">The ID of the listing to remove.</param>
        /// <param name="format">Response format (default is "json").</param>
        /// <returns>A task containing the API response confirming the removal.</returns>
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
