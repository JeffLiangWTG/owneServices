
namespace Enterprise.Customs.US.ISF.Business
{
	public static class ISFConstants
	{
		public static class AddInfoConstants
		{
			public const string EntryType = "EntryType";
			public const string ISFShipmentType = "ISFShipmentType";
			public const string CarrierSCAC = "UI_NKCarrierSCAC";
			public const string ActionReason = "ActionReason";
			public const string SendEquipment = "SendEquipment";

			public const string ImporterIDType = "ImporterIDType";
			public const string ImporterID = "ImporterID";
			public const string ImporterName = "ImporterName";
			public const string ImporterDOB = "ImporterDOB";
			public const string ImporterIssueCountry = "RN_NKImporterIssueCountry";

			public const string ConsigneeIDType = "ConsigneeIDType";
			public const string ConsigneeID = "ConsigneeID";
			public const string ConsigneeName = "ConsigneeName";
			public const string ConsigneeDOB = "ConsigneeDOB";
			public const string ConsigneeIssueCountry = "RN_NKConsigneeIssueCountry";

			public const string ISFBondHolder = "ISFBondHolder";
			public const string ISFBondActivityCode = "ISFBondActivityCode";
			public const string ISFBondType = "ISFBondType";
			public const string ISFSuretyCode = "ISFSuretyCode";
			public const string ISFBondRefNo = "ISFBondRefNo";

			public const string ISFShipmentSubType = "ISFShipmentSubType";
		}

		public static class DateConstants
		{
			public const string ISFLastAccepted = "ISFLastAccepted";
		}

		public static class EntryNumberConstants
		{
			public const string ISF = "ISF";
			public const string ENS = "ENS";
		}

		public static class BillAddInfoConstants
		{
			public const string BillStatus = "BillStatus";
			public const string MatchedDate = "MatchedDate";
			public const string FirstMatched = "FirstMatched";
		}

		public static class CustomizedFieldConstants
		{
			public const string CustomAttribOne = "CustomAttrib1";
			public const string CustomAttribTwo = "CustomAttrib2";
		}

		public static class ContainerConstants
		{
			public const string USContainerType = "USContainerType";
			public const string ISOSizeTypeCode = "ISOSizeTypeCode";
		}
	}
}
