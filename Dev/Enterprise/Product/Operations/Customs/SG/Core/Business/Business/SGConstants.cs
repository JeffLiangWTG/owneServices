namespace Enterprise.Customs.SG.V4.Business
{
	public static class SGConstants
	{
		public const string STGCWebURL = "https://www.customs.gov.sg/businesses/strategic-goods-control/overview";

		public static class TradeNetVersion
		{
			public const string Four = "SG4";
			public const string FourPointOne = "4.1";
			public const string NTP = "NTP";

			public static class AssociationAssignedCodes
			{
				public const string Four = "040";
				public const string FourPointOne = "041";
			}
		}

		public static class Weight
		{
			public const string Kilograms = "KGM";
			public const string Tonnes = "TNE";
		}

		public static class NumericFormatting
		{
			public const int DecimalPlacesNone = 0;
			public const int DecimalPlacesForAmountValues = 2;
			public const int DecimalPlacesForMeasurementValues = 4;
			public const int DecimalPlacesForExchangeRateValues = 6;
			public const int DecimalPlacesForPercentageValues = 3;
			public const int DecimalPlacesForTaxRateValues = 4;
			public const int DecimalPlacesForUnitPriceAmountValues = 4;
			public const int DecimalPlacesForNetRegisteredTonnage = 2;

			public static class TradeNet4Point1
			{
				public const int DecimalPlacesForMeasurementValues = 3;
				public const int DecimalPlacesForNetRegisteredTonnage = 3;
				public const int DecimalPlacesForGSTPercentage = 0;
			}
		}

		public static class TransportCodes
		{
			public const int Sea = 1;
			public const int Rail = 2;
			public const int Road = 3;
			public const int Air = 4;
			public const int Mail = 5;
			//public const int Multimodal = 6;  Not yet used
			public const int Pipeline = 7; // Enterprise Mode "Other"
		}

		public const string Unbranded = "UNBRANDED";
		public const string OneLot = "1 LOT";
		public const string HSCodeAll9 = "99999999";
		public const string LPA = "LPA";
		public const string SeaStore = "SEASTORE";
		public const string Permit = "Permit";
		public const string Refund = "Refund";
		public const string TN4_1Permit = "DocPrintPermitTN41";
		public const int MaxEntryLines = 50;

		public static class MaxLinesValidation
		{
			public const string MaxLineLimitForDeclaration = "The maximum number of lines on a TradeNet declaration is limited by Singapore Customs to 50 lines.\r\nPlease create a separate declaration, (or declarations), for any additional lines required.\r\nAlternatively, consider merging invoice lines if appropriate.";
			public const string MaxEntryLimitForDeclaration = "The maximum number of lines on a TradeNet declaration is limited by Singapore Customs to 50 lines.\r\nYou have chosen to merge this entry but currently the merged lines exceed the 50 entry lines limit. You currently will be unable to send this declaration.\r\nPlease create a separate declaration, (or declarations), for the additional lines required.";
			public const string CannotSendExceedsMaxLines = "The message cannot be sent.\r\n\r\nThe maximum number of lines on a TradeNet declaration is limited by Singapore Customs to 50 lines.\r\nThe merged lines on this declaration exceed the 50 entry lines limit.\r\n\r\nPlease create a separate declaration, (or declarations), for the additional lines required.";
			public const string CheckEntryLineLimitForDeclaration = "The maximum number of lines on a TradeNet declaration is limited by Singapore Customs to 50 lines.\r\nYou have chosen to merge these lines. If the merged lines exceed 50 entry lines, you will be unable to send this declaration.\r\n\r\nPlease save/merge this entry so the system can determine the number of entry lines currently created.";
		}

		public static class UpdateIndicators
		{
			public const string AME = "AME";
			public const string AMR = "AMR";
			public const string FRF = "FRF";
			public const string PRG = "PRG";
			public const string PRS = "PRS";
			public const string CNL = "CNL";
		}

		public static class Attributes
		{
			public static class Names
			{
				public const string ISIMPORTCONTROL = "ISIMPORTCONTROL";
				public const string ISTRANSHIPMENTCONTROL = "ISTRANSHIPMENTCONTROL";
				public const string ISEXPORTCONTROL = "ISEXPORTCONTROL";
				public const string CommodityType = "COMMODITYTYPE";
				public const string COOTemplateCode = "COOTemplate";
			}
			public static class Values
			{
				public const string Yes = "Y";
				public const string No = "N";
			}
		}
	}
}
