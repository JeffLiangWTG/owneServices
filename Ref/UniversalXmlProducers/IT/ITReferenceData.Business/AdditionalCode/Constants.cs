namespace CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode
{
	public static class Constants
	{
		public static class RefCusCodeListCodeTypes
		{
			public const string AdditionalCode = "ADDCD";
		}

		public static class Regex
		{
			public const string AdditionalCodeType = @"<OPTION.*value=\""(?<Type>.+)\"">";
			public const string AdditionalCodeLink = @"<a href=\""javascript:linkToPostKey\('CaddServlet',(?<UC>.*),(?<SC>.*),'(?<ST>.*)','(?<Label>.*)','(?<AdditionalCodeSequentialNumber>.*)','(?<AdditionalCodeType>.*)','(?<ValidityStartDate>.*)','(?<SidCad>.*)'\)\"">";
		}
	}
}
