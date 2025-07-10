namespace CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.MedlemsLand
{
	partial class MedlemLandListe
	{
		public Medlemsland[] Member
		{
			get => medlem;
			set => medlem = value;
		}
	}

	partial class Medlemsland
	{
		public string CountryCode
		{
			get => landkode;
			set => landkode = value;
		}

		public string CountryName
		{
			get => landnavn;
			set => landnavn = value;
		}

		public Landgruppe[] CountryGroups
		{
			get => landgrupper;
			set => landgrupper = value;
		}
	}

	partial class Landgruppe
	{
		public string CountryGroupCode
		{
			get => landgruppekode;
			set => landgruppekode = value;
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
