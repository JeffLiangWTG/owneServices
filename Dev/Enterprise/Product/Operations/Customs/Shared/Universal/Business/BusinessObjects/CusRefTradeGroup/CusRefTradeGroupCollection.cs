using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class CusRefTradeGroupCollection : ActiveBusinessObjectCollection<CusRefTradeGroup>
	{
		public CusRefTradeGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Constant")]
		public static class FilterConstants
		{
			public const string CountryCode = "Country Code";
			public const string TradeGroup = "Trade Group";
			public const string Description = "Description";
			public const string StartDate = "Start Date";
			public const string EndDate = "End Date";
		}
	}
}
