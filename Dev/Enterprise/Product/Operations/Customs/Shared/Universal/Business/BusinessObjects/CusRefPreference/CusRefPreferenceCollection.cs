using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class CusRefPreferenceCollection : ActiveBusinessObjectCollection<CusRefPreference>
	{
		public CusRefPreferenceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CusRefPreferenceCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Constant")]
		public static class FilterConstants
		{
			public const string CountryCode = "Country Code";
			public const string Preference = "Preference";
			public const string Description = "Description";
		}
	}
}
