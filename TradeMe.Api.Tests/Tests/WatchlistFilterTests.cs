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
        /// Parameterized test: verifies the watchlist works with different filters.
        /// Acceptance criteria: "A user should be able to filter their watchlist."
        /// English explanation:
        ///  - For each filter (All, Current, Won, Lost, Deleted):
        ///      - Call the API with the filter
        ///      - Confirm response is OK
        ///      - Confirm our test listing appears for filters where it should appear
        /// </summary>
        [TestCase(TestDataConstants.WatchlistFilters.All)]
        [TestCase(TestDataConstants.WatchlistFilters.Current)]
        [TestCase(TestDataConstants.WatchlistFilters.Won)]
        [TestCase(TestDataConstants.WatchlistFilters.Lost)]
        [TestCase(TestDataConstants.WatchlistFilters.Deleted)]
        public async Task TestGetWatchlist_WithFilter_ShouldReturnExpectedListings(string filter)
        {
            // Only expect the test listing in "All" or "Current" filters
            string? expectedListingId = filter == TestDataConstants.WatchlistFilters.All
                                       || filter == TestDataConstants.WatchlistFilters.Current
                                       ? _testListings.First()
                                       : null;

            await ValidateWatchlistFilter(filter, expectedListingId);
        }

        /// <summary>
        /// Negative test: ensures users cannot see other people's watchlists.
        /// Acceptance criteria: "A user should only be able to view their own watchlist."
        /// English explanation:
        ///  - Create a client with a different user's access token
        ///  - Try to get their watchlist
        ///  - Confirm either:
        ///      - API returns Unauthorized
        ///      - Or the watchlist is empty
        /// </summary>
        [Test]
        public async Task TestCannotAccessOtherUsersWatchlist_ShouldReturnUnauthorizedOrEmpty()
        {
            var otherUserClient = new TradeMeApiClient(new TradeMeConfig
            {
                BaseUrl = "https://api.tmsandbox.co.nz/v1",
                ConsumerKey = "<VALID_CONSUMER_KEY>",
                ConsumerSecret = "<VALID_CONSUMER_SECRET>",
                AccessToken = "<DIFFERENT_USER_ACCESS_TOKEN>",
                TokenSecret = "<DIFFERENT_USER_TOKEN_SECRET>"
            });

            var response = await otherUserClient.GetWatchList("All");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var jsonDoc = JsonDocument.Parse(response.Content!);
                var listings = jsonDoc.RootElement.GetProperty("List").EnumerateArray();
                Assert.That(listings.Count(), Is.EqualTo(0),
                    "Other user's watchlist should be empty.");
            }
            else
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized),
                    "Expected Unauthorized when accessing another user's watchlist.");
            }
        }
    }
}
    

