namespace CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Varenummer
{
	partial class VarenummerListe
	{
		public varenummer[] ItemNumber
		{
			get => varenummer;
			set => varenummer = value;
		}
	}

	partial class varenummer
	{
		public string Unit
		{
			get => enhet;
			set => enhet = value;
		}

		public string UnitDescription
		{
			get => enhetBeskrivelse;
			set => enhetBeskrivelse = value;
		}

		public string OtherUnit
		{
			get => annenEnhet;
			set => annenEnhet = value;
		}

		public string OtherUnitDescription
		{
			get => annenEnhetBeskrivelse;
			set => annenEnhetBeskrivelse = value;
		}
	}
}
