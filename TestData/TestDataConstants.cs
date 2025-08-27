namespace TradeMe.Api.Tests.TestData
{
    /// <summary>
    /// Constants used throughout the test project
    /// </summary>
    public static class TestDataConstants
    {
        public static class Listings
        {
            // General test listing - Used for basic watchlist operations
            public const string ValidListingId = "2149712754";

            // Note: The following test listings require specific states that can't be reliably created in sandbox.
            // Tests using these are marked with [Ignore] attribute. Consider mocking these tests in the future.
            public static class WatchlistFilter
            {
                public const string ClosingTodayListing = "0";  // Placeholder - needs a listing closing today
                public const string LeadingBidsListing = "0";   // Placeholder - needs a listing where we have leading bid
                public const string ReserveMetListing = "0";    // Placeholder - needs a listing with reserve met
                public const string ReserveNotMetListing = "0"; // Placeholder - needs a listing with reserve not met
                public const string OpenHomesListing = "0";     // Placeholder - needs a listing with open homes
            }
        }

        public static class WatchlistFilters
        {
            // These correspond to the filter types supported by the TradeMe API
            public const string ClosingToday = "ClosingToday";
            public const string LeadingBids = "LeadingBids";
            public const string ReserveMet = "ReserveMet";
            public const string ReserveNotMet = "ReserveNotMet";
            public const string OpenHomes = "OpenHomes";
            public const string All = "All";
        }
    }
}
