namespace Enterprise.Customs.ZA.Module
{
	public class DeclarationFilterConstants : Customs.Module.DeclarationFilterConstants
	{
		public const string PreviousMRN = "Previous MRN";
		public const string CPCAndPPC = "CPC/PPC";
		public const string UniqueConsignmentReference = "Unique Consignment Reference (UCR)";

		public new class NumberFilterTypes : Customs.Module.DeclarationFilterConstants.NumberFilterTypes
		{
			public const string VIN = "VIN";
			public const string CaseNumber = "Case Number";
		}

		public static class StatusFilterTypes
		{
			public const string SupportingDocumentStatus = "Supporting Document Status";
			public const string ReleasePrinterIndicator = "Customs Printed Release Required";
		}

		public new class DateFilterTypes : Customs.Module.DeclarationFilterConstants.DateFilterTypes
		{
			public const string AcquitByDate = "Acquit By Date";
			public const string AcquittedDate = "Acquitted Date";
		}
	}
}
