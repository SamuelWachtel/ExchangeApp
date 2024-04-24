using TinyCsvParser.Mapping;

namespace ExchangeRatesUpdater
{
    public class CurrencyConfiguration
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
    }

    public class CsvCurrencyConfigurationMapping : CsvMapping<CurrencyConfiguration>
    {
        public CsvCurrencyConfigurationMapping()
        : base()
        {
            MapProperty(0, x => x.Name);
            MapProperty(1, x => x.Code);
        }
    }
}
