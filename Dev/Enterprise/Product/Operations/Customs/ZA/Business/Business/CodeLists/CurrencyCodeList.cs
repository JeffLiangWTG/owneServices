using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class CurrencyCodeList : CodeDescriptionPairList
	{
		public CurrencyCodeList()
		{
			AddPair("AUD", "Australia (Dollar)");
			AddPair("BWP", "Botswana (Pula)");
			AddPair("CAD", "Canada (Dollar)");
			AddPair("DKK", "Denmark (Krone)");
			AddPair("EUR", "European Unit (Euro)");
			AddPair("HKD", "Hong Kong (Dollar)");
			AddPair("JPY", "Japan (Yen)");
			AddPair("MWK", "Malawi (Kwacha)");
			AddPair("NZD", "New Zealand (Dollar)");
			AddPair("NOK", "Norway (Krone)");
			AddPair("PTE", "Portugal (Escudo)");
			AddPair("ESP", "Spain (Peseta)");
			AddPair("SEK", "Sweden (Krone)");
			AddPair("CHF", "Switzerland (Franc)");
			AddPair("GBP", "United Kingdom (Pound)");
			AddPair("USD", "United States (Dollar)");
			AddPair("ZWD", "Zimbabwe (Dollar)");
		}
	}
}
