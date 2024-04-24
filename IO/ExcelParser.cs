using System.Text;
using TinyCsvParser;

namespace ExchangeRatesUpdater
{

    public class ExcelParser
    {
        public IEnumerable<Currency> ParseExchangeRates(Stream stream)
        {
            var csvParserOptions = new CsvParserOptions(false, ',');
            var csvMapper = new CsvCurrencyMapping();
            var csvParser = new CsvParser<Currency>(csvParserOptions, csvMapper);

            var result = csvParser
                .ReadFromStream(stream, Encoding.UTF8)
                .ToList();
            return result.Select(x => x.Result);
        }
    }
    public class CurrencyParser
    {
        public IEnumerable<CurrencyConfiguration> ParseCurrencyConfiguration(Stream stream)
        {
            var csvParserOptions = new CsvParserOptions(false, ',');
            var csvMapper = new CsvCurrencyConfigurationMapping();
            var csvParser = new CsvParser<CurrencyConfiguration>(csvParserOptions, csvMapper);

            var result = csvParser
                .ReadFromStream(stream, Encoding.UTF8)
                .ToList();
            return result.Select(x => x.Result);
        }
    }
}