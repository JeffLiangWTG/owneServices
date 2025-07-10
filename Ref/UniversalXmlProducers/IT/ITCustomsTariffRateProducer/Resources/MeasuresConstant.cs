using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources
{
	public static class MeasuresConstant
	{
		public const string DateUpdateOn = "Dati aggiornati al:";
		public const string ValueAddedTax = "Imposta Valore Aggiunto";
		public const string Excise = "Accise";
		public const string VatTradeGroup = "ERGA OMNES";
		public const string AdditionalCode = "CADD";

		static DateTime _today = DateTime.Today;
		public static readonly DateTime DefaultStartDate = new DateTime(_today.Year, _today.Month, _today.Day, 00, 00, 00);

		public const string DefaultTariffType = "IMP";
		public const string EuropeanUnionTradeCode = "EUN";
		public const string IsUnique = "0";
		public const string ItalianTradeCode = "IT";

		public const string CurrencyToken = "EURO/";

		public static readonly string[] DescriptionsToIgnore =
		{
			"Controlli fitosanitari imballaggi.",
		};
	}
}
