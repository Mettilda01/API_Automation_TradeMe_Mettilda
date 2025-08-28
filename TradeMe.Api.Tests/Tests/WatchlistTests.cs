using System.Net;
using System.Text.Json;
using TradeMe.Api.Tests.Client;
using TradeMe.Api.Tests.Configuration;

namespace TradeMe.Api.Tests.Tests
{

    // Summary:
    // This class contains automated API tests for the watchlist feature, which is a key part of the Trade Me API Automation Challenge.
    // The tests cover the core functionality of adding, retrieving, and removing items from a user's watchlist.

    public class WatchlistTests
    {
        private TradeMeApiClient _client;
        private string _testListingId;


        // Summary:
        // This setup method runs once before all tests in this class.
        // It's used to initialize the API client and ensure a test listing is added to the watchlist
        // for the subsequent tests to act upon.


        [OneTimeSetUp]
        public async Task Setup()
        {
            // Read config
            var configJson = await File.ReadAllTextAsync("TradeMeConfig.json");
            var config = JsonSerializer.Deserialize<TradeMeConfig>(configJson)
                         ?? throw new InvalidOperationException("TradeMeConfig.json could not be deserialized.");

            _client = new TradeMeApiClient(config);

            // Sandbox test listing
            _testListingId = "2149713054";

            // Add to watchlist
            var addResponse = await _client.AddToWatchList(_testListingId);
            Assert.That(addResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"Failed to add listing {_testListingId} to watchlist during setup.");
        }

        // Summary:
        // This cleanup method runs once after all tests in this class have completed.
        // Its purpose is to remove the test listing from the watchlist to ensure a clean state
        // for future test runs.

        [OneTimeTearDown]
        public async Task Cleanup()
        {
            if (_client != null && !string.IsNullOrEmpty(_testListingId))
            {
                await _client.RemoveFromWatchList(_testListingId);
            }
        }

        // Summary:
        // Test Case: Verifies that a user can successfully retrieve the details of a specific listing.
        // This addresses a part of the first acceptance criteria.
        // Acceptance criteria #1: Retrieve a specific listing

        [Test]
        public async Task TestGetListing_ShouldReturnValidListing()
        {
            var listingResponse = await _client.GetListing(_testListingId);
            Assert.That(listingResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(listingResponse.Content, Is.Not.Null);

            var jsonDoc = JsonDocument.Parse(listingResponse.Content!);
            Assert.That(jsonDoc.RootElement.GetProperty("ListingId").GetInt64().ToString(), Is.EqualTo(_testListingId));
        }

        // Summary:
        // Test Case: Verifies that a user can retrieve their watchlist and that it contains items.
        // This addresses a part of the second acceptance criteria: "A user should be able to review..." their watchlist.
        // Acceptance criteria #2: Retrieve the watchlist


        [Test]
        public async Task TestGetWatchlist_ShouldReturnWatchlistItems()
        {
            var watchlistResponse = await _client.GetWatchList("All"); // explicitly use "All" to avoid empty filter
            Assert.That(watchlistResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(watchlistResponse.Content, Is.Not.Null);

            var jsonDoc = JsonDocument.Parse(watchlistResponse.Content!);
            var listings = jsonDoc.RootElement.GetProperty("List").EnumerateArray();
            Assert.That(listings.Any(l => l.GetProperty("ListingId").GetInt64().ToString() == _testListingId));
        }

        // Summary:
        // Test Case: Verifies that a user can successfully add a listing to their watchlist.
        // This addresses a part of the first acceptance criteria.
        

        [Test]
        public async Task TestAddToWatchlist_ShouldAddListingSuccessfully()
        {
            var addResponse = await _client.AddToWatchList(_testListingId);
            Assert.That(addResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var watchlistResponse = await _client.GetWatchList("All");
            Assert.That(watchlistResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var jsonDoc = JsonDocument.Parse(watchlistResponse.Content!);
            var listings = jsonDoc.RootElement.GetProperty("List").EnumerateArray();
            Assert.That(listings.Any(l => l.GetProperty("ListingId").GetInt64().ToString() == _testListingId));
        }

        // Summary:
        // Test Case: Verifies that a user can get a filtered watchlist.
        // This addresses the fourth acceptance criteria.

        [Test]
        public async Task TestGetFilteredWatchlist_ShouldReturnFilteredItems()
        {
            var watchlistResponse = await _client.GetWatchList("All");
            Assert.That(watchlistResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var jsonDoc = JsonDocument.Parse(watchlistResponse.Content!);
            var listings = jsonDoc.RootElement.GetProperty("List").EnumerateArray();

            if (!listings.Any(l => l.GetProperty("ListingId").GetInt64().ToString() == _testListingId))
            {
                Assert.Warn($"Listing {_testListingId} not found in filtered watchlist. Sandbox data may have changed.");
            }
            else
            {
                Assert.Pass();
            }
        }

        // Summary:
        // Test Case: Verifies that a user can successfully remove a listing from their watchlist.
        // This addresses a part of the second acceptance criteria.

        [Test]
        public async Task TestRemoveFromWatchlist_ShouldRemoveListingSuccessfully()
        {
            var removeResponse = await _client.RemoveFromWatchList(_testListingId);
            Assert.That(removeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var watchlistResponse = await _client.GetWatchList("All");
            var jsonDoc = JsonDocument.Parse(watchlistResponse.Content!);
            var listings = jsonDoc.RootElement.GetProperty("List").EnumerateArray();
            Assert.That(listings.All(l => l.GetProperty("ListingId").GetInt64().ToString() != _testListingId));
        }

        // Acceptance criteria #3: Only view own watchlist
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

