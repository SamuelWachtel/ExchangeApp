using CsvHelper;
using CsvHelper.Configuration;
using Serilog;
using System.Configuration;
using System.Globalization;

namespace ExchangeRatesUpdater
{
    internal class CsvFileDownloadAndFormat
    {
        string fileUrl = ConfigurationManager.AppSettings["downloadFrom"];
        string filePath = @"C:\Users\samuel.wachtel\source\repos\Autorestart\Resources\downloaded_file.csv";///ConfigurationManager.AppSettings["exchangeCurrencyDataPath"];

        ///to be removed
        string inputFile = @"C:\Users\samuel.wachtel\source\repos\Autorestart\Resources\dt.csv";
        string outputFile = @"C:\Users\samuel.wachtel\source\repos\Autorestart\Resources\dt2.csv";

        internal async Task ExcelDownLoader()
        {
            Log.Information($"Downloading CSV file from {fileUrl}");
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
                    var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);

                    if (response.IsSuccessStatusCode)
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var fileStream = File.Create(filePath))
                            {
                                await stream.CopyToAsync(fileStream);
                                Log.Information($"CSV file downloaded successfully to: {filePath}");
                            }
                        }
                    }
                    else
                    {
                        Log.Information($"Failed to download CSV file: {response.ReasonPhrase}");
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Information($"Failed to download CSV file: {ex.Message}");
            }
        }

        public async Task ExcelTransformer()
        {
            Log.Information($"Transforming CSV file {inputFile/*filePath*/}");
            try
            {
                List<string> lines = new List<string>();

                Dictionary<string, string> replacements = new Dictionary<string, string>
        {
            { "IND RUPEE","Indian_rupee"},
            {"KES / RWF","Rwandan_franc"},
            {"US DOLLAR","US_dollar"},
            {"JPY (100)","Japanese_yen"},
            {"CHINESE YUAN","Chinese_yuan_renminbi"},
            {"SINGAPORE DOLLAR","Singapore_dollar" },
            {"SW KRONER","Swedish_krona" },
            {"AE DIRHAM","UAE_dirham" },
            {"KES / TSHS","Tanzanian_shilling" },
            {"SA RAND","South_African_rand" },
            {"S FRANC","Swiss_franc" },
            {"STG POUND","British_pound" },
            {"HONGKONG DOLLAR","Hong_Kong_SAR_dollar" },
            {"CAN $","Canadian_dollar" },
            {"DAN KRONER","Danish_krone" },
            {"KES / BIF", "Burundi_franc" },
            {"KES / USHS","Uganda_new_shilling" },
            {"AUSTRALIAN $","Australian_dollar" },
            {"EURO","EU_euro" },
            {"SAUDI RIYAL","Saudi_riyal" },
            {"NOR KRONER","Norwegian_krone" }
        };

                using (var reader = new StreamReader(inputFile/*filePath*/))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    string[] expectedHeaders = { "Date", "Currency", "EXCHANGE RATE" };

                    string headerLine = reader.ReadLine();
                    if (string.IsNullOrEmpty(headerLine))
                    {
                        throw new InvalidOperationException("Header line is empty or null.");
                    }

                    string[] headers = headerLine.Split(',').Select(header => header.Trim()).ToArray();
                    headers = headers.Select(h => h.Replace("\"", "")).ToArray();

                    foreach (var header in expectedHeaders)
                    {
                        if (!headers.Any(h => h.Equals(header, StringComparison.OrdinalIgnoreCase)))
                        {
                            throw new InvalidOperationException($"Header '{header}' not found.");
                        }
                    }

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Replace("\"", "");
                        var values = line.Split(',');

                        var date = values[0];
                        var currency = values[1];
                        var rate = values[2];

                        foreach (var replacement in replacements)
                        {
                            currency = currency.Replace(replacement.Key, replacement.Value);
                        }

                        lines.Add($"{date},{currency},{rate}");
                    }
                }
                await File.WriteAllLinesAsync(outputFile/*filePath*/, lines);
            }
            catch (Exception ex)
            {
                Log.Information($"Error: {ex.Message}");
            }
            Log.Information($"CSV file transformed successfully to {outputFile/*filePath*/}");
        }

        public class CurrencyRecord
        {
            public string CurrencyName { get; set; }
        }

        public sealed class CurrencyRecordMap : ClassMap<CurrencyRecord>
        {
            public CurrencyRecordMap()
            {
                Map(m => m.CurrencyName);
            }
        }
    }
}

