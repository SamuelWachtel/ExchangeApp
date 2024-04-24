using ExchangeRatesUpdater.Contracts;
using TinyCsvParser.Mapping;

namespace ExchangeRatesUpdater
{
    public class Currency
    {
        public string? Code { get; set; }
        public CurrencyIso CurrencyCode { get; set; }
        public decimal Value { get; set; }
        public DateTime? Inserted { get; set; }
        public DateTime LastUpdate { get; set; }

        public decimal Multiplier = 1;
    }
}

namespace ExchangeRatesUpdater
{
    public class CsvCurrencyMapping : CsvMapping<Currency>
    {
        public CsvCurrencyMapping()
        : base()
        {
            MapProperty(0, x => x.Inserted);
            MapProperty(1, x => x.Code);
            MapProperty(2, x => x.Value);
        }
    }
}
