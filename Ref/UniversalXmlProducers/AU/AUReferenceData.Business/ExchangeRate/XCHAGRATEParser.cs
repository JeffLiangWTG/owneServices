using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business.ExchangeRate
{
	public class XCHAGRATEParser : ExchangeRateParser<RefExchangeRateZZ>
	{
		protected override string WebPageUrl => Services.ApplicationConfig.ExchangeRateUrl ?? "https://www.ccf.customs.gov.au/reference/production/main/";

		protected override string FileNamePrefix => Services.ApplicationConfig.ExchangeRateFileNamePrefix ?? "XCHGRATE-P1-EDMAIN";

		protected override IEnumerable<PropertyMapping<RefExchangeRateZZ>> Mappings => new List<PropertyMapping<RefExchangeRateZZ>>
		{
			new PropertyMapping<RefExchangeRateZZ>(x => x.ZZN_RX_NKExCurrency, 1, 3),
			new PropertyMapping<RefExchangeRateZZ>(x => x.ZZN_Rate, 47, 12),
			new PropertyMapping<RefExchangeRateZZ>(x => x.ZZN_StartDate, 60, 8, Constants.RefData_Common.MinimumDateTime),
			new PropertyMapping<RefExchangeRateZZ>(x => x.ZZN_EndDate, 69, 8, DateTime.Today)
		};

		protected override Func<RefExchangeRateZZ, bool> Filter => x => x.ZZN_StartDate > DateTime.Today.AddDays(-14);

		protected override string XMLWriterDataSource => "AU Customs Exchange Rates";

		public override string OutputFileName => "AU Customs Exchange Rates.xml";
	}
}
