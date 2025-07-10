namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String constants")]

	public static class Constants
	{
		public const string MessageTarget = "AirlineMessaging";
		public const string MessageDepartment = "Airline";
		public const int MaxReasonLength = 512;

		public static class AddInfoCollectionTypes
		{
			public const string ConsignorCompanyIdAndNumber = "ConsignorCompanyIdAndNumber";
			public const string ConsigneeCompanyIdAndNumber = "ConsigneeCompanyIdAndNumber";
			public const string AlsoNotifyCompanyIdAndNumber = "AlsoNotifyCompanyIdAndNumber";
		}
	}
}
