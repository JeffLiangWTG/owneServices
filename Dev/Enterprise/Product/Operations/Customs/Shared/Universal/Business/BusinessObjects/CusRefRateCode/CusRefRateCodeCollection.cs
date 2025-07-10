using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class CusRefRateCodeCollection : ActiveBusinessObjectCollection<CusRefRateCode>
	{
		public CusRefRateCodeCollection(BusinessObjectFactory factory) : base(factory) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Constant")]
		public static class FilterConstants
		{
			public const string CountryCode = "Country Code";
			public const string RateCode = "Rate Code";
			public const string Description = "Description";
			public const string RateType = "Rate Type";
		}
	}
}
