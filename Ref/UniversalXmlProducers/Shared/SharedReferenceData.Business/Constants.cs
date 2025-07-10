using System;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business
{
	public static class Constants
	{
		public static class ProgramFunctions
		{
			public const string Tariff = "TARIFF";
			public const string ManualJsonParser = "MANUALJSONPARSER";
		}

		public static class DefaultValues
		{
			public static readonly DateTime MaxDateTime = new DateTime(2079, 06, 06, 23, 59, 00);
			public static readonly DateTime MinDateTime = new DateTime(1900, 01, 01, 00, 00, 00);
		}

		public static class SchemaType
		{
			public const string RefCusCodeType = "RefCusCodeType";
			public const string RefCusCodeList = "RefCusCodeList";
			public const string RefCusCodeListAttributeName = "RefCusCodeListAttributeName";
		}

		public const string TariffProcessed = "TariffProcessed";
	}
}
