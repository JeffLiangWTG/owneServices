namespace CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tollsats
{
	partial class vareListe
	{
		public AvtaleSats[] Goods
		{
			get => vare;
			set => vare = value;
		}
	}

	partial class AvtaleSats
	{
		public Avtale[] Rates
		{
			get => avtalesatser;
			set => avtalesatser = value;
		}

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
	}

	partial class Avtale
	{
		public string CountryGroup
		{
			get => landgruppe;
			set => landgruppe = value;
		}

		public Sats[] Rate
		{
			get => sats;
			set => sats = value;
		}
	}

	partial class Sats
	{
		public string RateValue
		{
			get => satsVerdi;
			set => satsVerdi = value;
		}

		public string RateUnit
		{
			get => satsEnhet;
			set => satsEnhet = value;
		}

		public string RateUnitDescription
		{
			get => satsEnhetBeskrivelse;
			set => satsEnhetBeskrivelse = value;
		}

		public string DateStart
		{
			get => fomdato;
			set => fomdato = value;
		}

		public string DateEnd
		{
			get => tomdato;
			set => tomdato = value;
		}
	}
}
