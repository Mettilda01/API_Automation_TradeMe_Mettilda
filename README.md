**TradeMe.Api.Tests**

This repository contains automated API tests for the Trade Me Sandbox environment.
The tests validate the Watchlist and Listing APIs, ensuring that adding, retrieving, filtering, and removing items work as expected.
These tests also cover user-specific access, ensuring that only a user’s own watchlist is visible.

**Project Setup**

_Prerequisites_

•	Visual Studio 2022 or later

•	NET 6 / 7 SDK

•	NUnit test framework

•	Valid Trade Me Sandbox API credentials

**Project Structure**

    '''TradeMe.Api.Tests/
    │
    ├── Client/          # Trade Me API client (OAuth + RestSharp)
    ├── Configuration/   # Configuration models
    ├── TestData/        # Constants for test listings & filters
    ├── Tests/           # NUnit automated test cases
    │
    ├── TradeMeConfig.json # API keys and tokens for Sandbox
    └── TradeMe.Api.Tests.sln

    
TradeMe.Api.Tests/



**Configuration**

Update TradeMeConfig.json with your Sandbox API credentials:

    '''{"BaseUrl": "https://api.tmsandbox.co.nz/v1",
    "ConsumerKey": "your_consumer_key", 
    "ConsumerSecret": "your_consumer_secret",
    "AccessToken": "your_access_token",
    "TokenSecret": "your_token_secret"
    }
  
  
**Test Coverage**

**_WatchlistTests_**

_Setup & Cleanup_

•	Initializes the API client and adds a test listing before running tests.

•	Removes the test listing after all tests to reset the watchlist for future runs.

_Tests_

•	Retrieve a specific listing → GetListing

•	Retrieve full watchlist → GetWatchList("All")

•	Add listing to watchlist → AddToWatchList

•	Remove listing from watchlist → RemoveFromWatchList

•	Get filtered watchlist → GetWatchList("All")

•	Negative test → ensures other users cannot access your watchlist


**WatchlistFilterTests**

_Setup & Cleanup_

•	Adds predefined listings to the watchlist before tests.

•	Cleans up these listings afterward to maintain a clean state.

_Helper_

•	ValidateWatchlistFilter() → checks response status, ensures content is present, and verifies that expected listings appear.

_Tests_

•	Filter "All" → confirms all expected listings are returned

•	Filter "Current" / "Won" / "Lost" / "Deleted" → ensures filtering works correctly

•	Ensures the API behaves as expected if no listings match the filter

**Acceptance Criteria Covered**

1.	Add & retrieve a listing – Users can add a listing to their watchlist and retrieve it.	
2.	Review & remove listings – Users can view and remove items from their watchlist.	
3.	User-specific access – Users can only view their own watchlist.	
4.	Filter watchlist – Users can filter the watchlist by "All", "Current", "Won", "Lost", or "Deleted".


