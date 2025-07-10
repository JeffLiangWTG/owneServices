namespace Enterprise.Customs.ZA.Manifest.Business
{
	public static class ValidationConstants
	{
		public static string PlaceOfEntryIsCompulsoryForRoadTranshipmentManifest => ResString.GetMultilingualString("{cb3b8ae8-7689-4240-8ead-0ae251746c3c}", "‘Place of Entry(ZA)’ is compulsory for Road Transhipment manifest");
		public static string BillNumberRequiresTheSame => ResString.GetMultilingualString("{1FE4FF25-18E4-496D-BBA5-565EC95F8DA7}", "BOL and Bill number needs to be the same for ALM Manifest type");

		public static string MustBeLoggedInUnderZaToSendZaMessages => ResString.GetMultilingualString("{B96BDAB4-133D-4759-B2C1-73309369C4DC}", "To create a message for South Africa you must link the current logged in company to the ZA Branch in the Registry, Customs>South Africa>Default Branch for Manifest Submission. This is so that the correct OrgProxy is selected for determining the Manifest EDI Profile.");
	}
}
