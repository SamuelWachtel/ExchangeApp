using ExchangeRatesUpdater;
using ExchangeRatesUpdater.Contracts;
using Network;
using Serilog;
using System.Configuration;

internal class ExchangeRateUpdater
{

    public static async Task ExecuteScheduledTask(string accessToken)
    {
        CsvFileDownloadAndFormat csvFileDownloadAndFormat = new CsvFileDownloadAndFormat();
        await csvFileDownloadAndFormat.ExcelDownLoader();
        await Task.Delay(3000);
        await csvFileDownloadAndFormat.ExcelTransformer();

        string chosenCountry = ConfigurationManager.AppSettings["chosenCountry"];
        Log.Information($"Chosen country: {chosenCountry}");

        string url = ConfigurationManager.AppSettings[chosenCountry.ToUpper() == "KENYA" ? "urlKenya" : chosenCountry.ToUpper() == "OMAN" ? "urlOman" : throw new ArgumentException("Invalid country")];
        Log.Information($"Configured URL: {url}");



        string currencyCodesDataPath = ConfigurationManager.AppSettings["currencyCodesDataPath"];
        string exchangeCurrencyDataPath = ConfigurationManager.AppSettings["exchangeCurrencyDataPath"];

        Log.Information($"Reading exchange rates data from: {exchangeCurrencyDataPath}");
        Log.Information($"Reading currency properties from: {currencyCodesDataPath}");

        Log.Information($"Number of records for each country set to: {ConfigurationManager.AppSettings["numberOfRecordsForEachCurrency"]}");

        Log.Information($"Showing results from year: {ConfigurationManager.AppSettings["fromYear"]}");
        Log.Information($"Showing results to year: {ConfigurationManager.AppSettings["toYear"]}");

        Log.Information($"Showing results with max value: {ConfigurationManager.AppSettings["currencyValueMax"]}");
        Log.Information($"Showing results with min value: {ConfigurationManager.AppSettings["currencyValueMin"]}");

        string username = ConfigurationManager.AppSettings["username"];
        Log.Information($"Username: {username}");

        string password = ConfigurationManager.AppSettings["password"];
        Log.Information($"Password: {new string('*', password.Length)}");

        var repetetion = ConfigurationManager.AppSettings["configuredRepetition"];

        Log.Information($"Repetition set after every {repetetion} hour/s");

        string displayedCurrency = ConfigurationManager.AppSettings["requestedCurrencyIsoCode"];
        List<string> requestedCurrenciesLsit = new List<string>();
        if (displayedCurrency != "all")
        {
            var requestedCurrencies = displayedCurrency.Split(';').Select(x => x.Trim().Replace(" ", "").ToUpper()).Distinct().ToArray();
            var distinctCurrencies = requestedCurrencies.Distinct();
            var invalidCurrencies = new List<string>();
            requestedCurrencies = requestedCurrencies.Where(currency => Enum.IsDefined(typeof(CurrencyIso), currency)).ToArray();
            requestedCurrenciesLsit.AddRange(requestedCurrencies);
            invalidCurrencies.AddRange(distinctCurrencies.Except(requestedCurrencies));

            Log.Information($"Following currencies are not valid: {string.Join(", ", invalidCurrencies)}\n" +
                            $"Currencies to execute: {string.Join(", ", requestedCurrencies)}");
        }
        else
        {
            requestedCurrenciesLsit.AddRange(Enum.GetNames(typeof(CurrencyIso)));
            Log.Information($"No currency was specified, all currencies will be executed");
        }
        var index = 1;
        foreach (var requestedCurrency in requestedCurrenciesLsit)
        {
            var iso = (CurrencyIso)Enum.Parse(typeof(CurrencyIso), requestedCurrency);

            try
            {
                using (var streamCodes = File.OpenRead(currencyCodesDataPath))
                using (var stream = File.OpenRead(exchangeCurrencyDataPath))
                {
                    var currencyCodes = new CurrencyParser().ParseCurrencyConfiguration(streamCodes);
                    var items = new ExcelParser()
                        .ParseExchangeRates(stream)
                        .Where(HasValidDateRange)
                        .Where(HasValidValue)
                        .Select(item => new Currency()
                        {
                            Code = item.Code,
                            CurrencyCode = ParseCurrencyCode(item.Code, currencyCodes),
                            Inserted = item.Inserted,
                            Multiplier = item.Multiplier,
                            Value = item.Value,
                        })
                        .Where(item => item.CurrencyCode != CurrencyIso.Unknown)
                        .Where(item => iso == item.CurrencyCode)
                        .OrderByDescending(item => item.Inserted)
                        .Take(int.Parse(ConfigurationManager.AppSettings["numberOfRecordsForEachCurrency"]))
                        .ToArray();
                    Log.Information($"Value {HasValidValue} Date: {HasValidDateRange}");
                    foreach (var item in items)
                    {
                        try
                        {
                            Log.Information($"Result #{index}, Currency: {item.CurrencyCode}, Rate: {item.Value}, Validity: {item.Inserted}");
                            index++;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Error displaying currency: {item.CurrencyCode} - {ex.Message}");
                        }
                        var client = new CBSClient(url, username, password);
                        await client.SetCurrencies(items, accessToken);
                    }
                }
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Error executing scheduled task: {ErrorMessage}", ex.Message);
            }
            CBSClient.NumberOfFailureUpdates++;
        }
        Log.Information($"Successful updates: {CBSClient.NumberOfSuccessfulUpdates}\nFailed updates: {CBSClient.NumberOfFailureUpdates - CBSClient.NumberOfSuccessfulUpdates - 1}");
    }

    private static CurrencyIso ParseCurrencyCode(string currencyCode, IEnumerable<CurrencyConfiguration> currencyCodes)
    {
        var currency = currencyCodes.FirstOrDefault(x => string.Equals(x.Name, currencyCode, StringComparison.OrdinalIgnoreCase));
        return currency != default ? Enum.Parse<CurrencyIso>(currency.Code) : CurrencyIso.Unknown;
    }
    private static bool HasValidDateRange(Currency currency)
    {
        string toBeParsedFromYear = ConfigurationManager.AppSettings["fromYear"];
        int fromYear = int.Parse(toBeParsedFromYear);
        int setFromYear = DateTime.Now.Year - fromYear;

        string toBeParsedToYear = ConfigurationManager.AppSettings["toYear"];
        int toYear = int.Parse(toBeParsedToYear);
        int setToYear = DateTime.Now.Year - toYear;

        return currency.Inserted >= DateTime.Now.AddYears(-setFromYear)
                && currency.Inserted <= DateTime.Now.AddYears(-setToYear);
    }

    private static bool HasValidValue(Currency currency)
    {
        string toBeParsedMinValue = ConfigurationManager.AppSettings["currencyValueMin"];
        string toBeParsedMaxValue = ConfigurationManager.AppSettings["currencyValueMax"];
        decimal minValue = decimal.Parse(toBeParsedMinValue);
        decimal maxValue = decimal.Parse(toBeParsedMaxValue);
        return currency.Value >= minValue && currency.Value < maxValue;
    }
}