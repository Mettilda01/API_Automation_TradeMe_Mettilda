# TradeMe.Api.Tests

This repository contains **automated API tests** for the [Trade Me Sandbox environment](https://www.tmsandbox.co.nz).  
The tests validate the **Watchlist** and **Listing APIs**, ensuring that adding, retrieving, filtering, and removing items work as expected.

---

##  Project Setup

### Prerequisites
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (or later)
- [.NET 6 / 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet)
- [NUnit](https://nunit.org/) test framework
- Valid [Trade Me Sandbox API credentials](https://developer.trademe.co.nz/api-overview/authentication/)

---

##  Project Structure
TradeMe.Api.Tests/
│
├── Client/ # Trade Me API client (OAuth + RestSharp)
├── Configuration/ # Config models
├── TestData/ # Constants for test listings & filters
├── Tests/ # NUnit automated test cases
│
├── TradeMeConfig.json # API keys and tokens for Sandbox
└── TradeMe.Api.Tests.sln


## WatchlistTests
Setup & Cleanup
•	Initializes API client and adds a listing before tests.
•	Removes test listing after all tests to reset state.
Tests
•	Retrieve a specific listing (GetListing)
•	Retrieve full watchlist (GetWatchList)
•	Add to watchlist (AddToWatchList)
•	Remove from watchlist (RemoveFromWatchList)
•	Get filtered watchlist (GetWatchList("All"))

WatchlistFilterTests

Setup & Cleanup
•	Adds predefined listings before tests, cleans them up afterward.
Helper
•	ValidateWatchlistFilter() → checks status, response content, and expected listing presence.
Tests
•	"All" filter returns expected listings.


##  Configuration

Update `TradeMeConfig.json` with your Sandbox API credentials:

```json
{
  "BaseUrl": "https://api.tmsandbox.co.nz/v1",
  "ConsumerKey": "your_consumer_key",
  "ConsumerSecret": "your_consumer_secret",
  "AccessToken": "your_access_token",
  "TokenSecret": "your_token_secret"
}




