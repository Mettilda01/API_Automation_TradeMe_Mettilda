using System.Net;
using System.Text.Json;
using TradeMe.Api.Tests.Client;
using TradeMe.Api.Tests.Configuration;
using TradeMe.Api.Tests.TestData;

namespace TradeMe.Api.Tests.Tests
{
    /// <summary>
    /// This class contains automated API tests focused on the watchlist filtering functionality,
    /// It validates that the API correctly handles different filter requests.
    /// </summary>
    public class WatchlistFilterTests
    {
        private readonly string[] _testListings;
        private TradeMeApiClient _client; // The new private field for the API client.


        public WatchlistFilterTests()
        {
            // Initialize test listings that can be tested in sandbox
            _testListings = new[] { TestDataConstants.Listings.ValidListingId};
        }

        /// <summary>
        /// Summary:
        /// This setup method runs once before all tests in this class.
        /// It's used to read the configuration, initialize the API client, and add the specified
        /// test listings to the watchlist to ensure a pre-configured state for testing.
        /// </summary>

        [OneTimeSetUp]
        public async Task SetupWatchlist()
        {
            // Read the configuration from the JSON file
            var configJson = await File.ReadAllTextAsync("TradeMeConfig.json");
            var config = JsonSerializer.Deserialize<TradeMeConfig>(configJson);
            _client = new TradeMeApiClient(config);

            foreach (var listingId in _testListings)
            {
                
                var response = await _client.AddToWatchList(listingId);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), 
                    $"Failed to add listing {listingId} to watchlist");
            }
        }

        /// <summary>
        /// Summary:
        /// This cleanup method runs once after all tests have completed.
        /// It's used to remove the test listings from the watchlist to ensure a clean state
        /// for future test runs.
        /// </summary>

        [OneTimeTearDown]
        public async Task CleanupWatchlist()
        {
            // Add a null check to ensure the client was successfully initialized.
            if (_client != null)
            {
                foreach (var listingId in _testListings)
                {
                    await _client.RemoveFromWatchList(listingId);
                }
            }
        }

        /// <summary>
        /// Summary:
        /// Helper method to validate a watchlist response based on a given filter.
        /// This method checks the response status, content, and verifies that the expected listing is present.
        /// </summary>
        private async Task ValidateWatchlistFilter(string filter, string expectedListingId)
        {
          

            var response = await _client.GetWatchList(filter);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Null);

            var jsonDoc = JsonDocument.Parse(response.Content!);
            var listings = jsonDoc.RootElement.GetProperty("List").EnumerateArray();

            if (expectedListingId != null)
            {
                Assert.That(listings.Any(l => l.GetProperty("ListingId").GetInt64().ToString() == expectedListingId),
                    $"Expected listing {expectedListingId} was not found in the filtered watchlist");
            }
        }
        /// <summary>
        /// Test Case: Verifies that the watchlist can be successfully retrieved using the "All" filter.
        /// This test confirms that the watchlist contains the listing added during the setup phase.
        /// This directly addresses the fourth acceptance criteria: "A user should be able to filter their watchlist."
        /// </summary>


        [Test]
        public async Task TestGetWatchlist_All_ShouldReturnAllWatchlistListings()
        {
            await ValidateWatchlistFilter(
                TestDataConstants.WatchlistFilters.All,
                _testListings.First());
        }
    }
}
