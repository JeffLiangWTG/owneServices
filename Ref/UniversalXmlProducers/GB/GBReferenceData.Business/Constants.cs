using System;

namespace CargoWise.RefDbRepo.GBReferenceData.Business
{
	public static class Constants
	{
		public static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);
		public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);

		public static class ProgramFunctions
		{
			public const string CDSStandingData = "CDSSTANDINGDATA";
			public const string ChiefHarmonisedDeclarationCode = "CHIEFHDC";
			public const string CDSTariffData = "CDSTARIFFDATA";
			public const string GvmsReferenceData = "GVMSREFDATA";
			public const string CDSPortData = "CDSPORTDATA";
			public const string ExchangeRates = "EXCHANGERATES";
			public const string CDSProcedureData = "CDSPROCEDUREDATA";
			public const string UKOfficeCodes = "UKOFFICECODES";
		}

		public static class DefaultValues
		{
			public const string CDSDataGrouping = "CDS";
			public const string GBDataGrouping = "GB";
			public const string EUNDataGrouping = "EUN";
		}

		public static class GvmsDefaults
		{
			public const string GVMS = "GVMS";
			public const string XmlWriterDataSourceGvmsCarrier = "GVMS Carrier Code";
			public const string XmlWriterDataSourceGvmsCusCodePorts = "GVMS Cus Code Ports";
			public const string XmlWriterDataSourceGvmsCusCodeRoutesAndErrors = "GVMS Cus Code Routes and Errors";
			public const string XmlWriterDataSourceGvmsLocoMap = "GVMS Loco Map";
			public const string XmlWriterDataSourceGvmsInspectionLocations = "GVMS Inspection Locations";
			public const string XmlWriterDataSourceGvmsInspectionTypes = "GVMS Inspection Types";

			public static class Codes
			{
				public const string ErrorCode = "ERRCD";
				public const string PORT = "PORT";
				public const string GvmsRoutes = "GvmRt";
				public const string GVMSIL = "GVMIL";
				public const string GVMSIT = "GVMIT";
			}

			public static class LocoMap
			{
				public const string LocalCountryGB = "GB";
				public const string GvmsSystemUsage = "GVM";
			}
		}

		public static class AttributeNames
		{
			public const string Nationality = "Nationality";
			public const string Category = "Category";
			public const string GvmsPortId = "GvmsPortId";
			public const string GvmsAddress = "Address";
			public const string GvmsType = "Type";
			public const string Carrier = "Carrier";
			public const string Direction = "Direction";
			public const string ArrivalPortId = "ArrivalPortId";
			public const string DeparturePortId = "DeparturePortId";
			public const string LocationTypeCode = "FACTY";
			public const string RORO = "RORO";
			public const string Role = "ROLE";
		}

		public static class ExchangeRateValues
		{
			public const string CustomsRateType = "CUS";
		}

		public static class UKOfficeCodeDefaults
		{
			public const string CodeType = "CUSOF";
			public const string CodeTypeDescription = "UK Customs Office Codes";
			public const int DescriptionMaxLength = 2000;
			public const string HeaderTag = "Region";

			public static class RoleValues
			{
				public const string EXT = "EXT";
				public const string EXP = "EXP";
			}
		}
	}
}
