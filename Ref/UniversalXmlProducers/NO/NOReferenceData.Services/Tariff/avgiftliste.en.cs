namespace CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste
{
	partial class AvgiftListe
	{
		public Vare[] Goods
		{
			get => vare;
			set => vare = value;
		}
	}

	partial class Vare
	{
		public AvgiftSats[] DutyRates
		{
			get => avgiftsatser;
			set => avgiftsatser = value;
		}
	}

	partial class AvgiftSats
	{
		public string CountryGroup
		{
			get => landgruppe;
			set => landgruppe = value;
		}

		public AvgiftType[] DutyTypes
		{
			get => avgiftstyper;
			set => avgiftstyper = value;
		}
	}

	partial class AvgiftType
	{
		public string DutyType
		{
			get => avgiftstype;
			set => avgiftstype = value;
		}

		public string DutyTypeDescription
		{
			get => avgiftstypebeskrivelse;
			set => avgiftstypebeskrivelse = value;
		}

		public AvgiftsGruppe[] DutyGroups
		{
			get => avgiftsgrupper;
			set => avgiftsgrupper = value;
		}
	}

	partial class AvgiftsGruppe
	{
		public short DutyGroup
		{
			get => avgiftsgruppe;
			set => avgiftsgruppe = value;
		}

		public string DutyGroupDescription
		{
			get => avgiftsgruppebeskrivelse;
			set => avgiftsgruppebeskrivelse = value;
		}

		public string Rate
		{
			get => sats;
			set => sats = value;
		}

		public string RateIsGivenInFractionOfNOK
		{
			get => oresats;
			set => oresats = value;
		}

		public string Unit
		{
			get => enhet;
			set => enhet = value;
		}

		public string UnitDescription
		{
			get => enhetbeskrivelse;
			set => enhetbeskrivelse = value;
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
