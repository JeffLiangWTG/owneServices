using System;

namespace CargoWise.RefDbRepo.DEReferenceData.Business
{
	public static class Constants
	{
		public static class ProgramFunctions
		{
			public const string CodeLists = "CODELISTS";
			public const string ExchangeRates = "EXCHANGERATES";
			public const string Tariffs = "TARIFFS";
			public const string DeTariffs = "DETARIFFS";
		}

		public static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);

		public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);

		public const string GermanTimeZoneID = "Central European Standard Time";
	}
}
