namespace CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.LandGruppe
{
	partial class LandgruppeListe
	{
		public Landgruppe[] CountryGroup
		{
			get => landgruppe;
			set => landgruppe = value;
		}
	}

	partial class Landgruppe
	{
		public string CountryGroupCode
		{
			get => landgruppekode;
			set => landgruppekode = value;
		}

		public string CountryGroupName
		{
			get => landgruppenavn;
			set => landgruppenavn = value;
		}

		public string PreferenceCode
		{
			get => preferansekode;
			set => preferansekode = value;
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
