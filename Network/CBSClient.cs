using ExchangeRatesUpdater;
using Serilog;
using System.Net.Http.Json;
using TinyCsvParser.Mapping;

namespace Network
{
    public class CBSClient
    {
        readonly string CBSAPIUrl;
        readonly string username;
        readonly string password;
        public static int NumberOfFailureUpdates = 0;
        public static int NumberOfSuccessfulUpdates = 0;

        public CBSClient(string CBSAPIUrl, string username, string password)
        {
            this.CBSAPIUrl = CBSAPIUrl;
            this.username = username;
            this.password = password;
        }

        public async Task SetCurrencies(IEnumerable<Currency> currency, string accessToken)
        {
            var apiCurrencies = currency
                .Select(c => new APICurrency
                {
                    CurrencyCode = c.CurrencyCode.ToString(),
                    Code = c.Code,
                    Multiplier = c.Multiplier,
                    Rate = c.Value,
                    Validity = c.Inserted
                }).ToArray();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("accept", "application/json");
                var request = new HttpRequestMessage(HttpMethod.Put, CBSAPIUrl);
                var json = JsonContent.Create(new { data = apiCurrencies });
                request.Content = json;

                request.Headers.Add("Authorization", "Bearer " + accessToken);
                Log.Information($"Sending request to CBS API: {CBSAPIUrl}");

                var result = await client.SendAsync(request);
                var response = await result.Content.ReadAsStringAsync();

                Log.Information($"Status code: {result.StatusCode}, Response message: {response}");

                foreach (var item in apiCurrencies) 
                {
                    if (result.IsSuccessStatusCode)
                    {
                        NumberOfSuccessfulUpdates++;
                        Log.Information("Number of successfull updates was raised by 1");
                    }
                    else
                    {
                        NumberOfFailureUpdates++;
                        Log.Information("Number of failed updates was raised by 1");
                    }
                }
            }
        }
    }

    public class APICurrency
    {
        public string? Code { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal? Multiplier { get; set; }
        public decimal? Rate { get; set; }
        public DateTime? Validity { get; set; }
        public class CsvAPICurrencyMapping : CsvMapping<APICurrency>
        {
            public CsvAPICurrencyMapping()
                : base()
            {
                MapProperty(0, x => x.Validity);
                MapProperty(1, x => x.CurrencyCode);
                MapProperty(2, x => x.Rate);
            }
        }
    }
}