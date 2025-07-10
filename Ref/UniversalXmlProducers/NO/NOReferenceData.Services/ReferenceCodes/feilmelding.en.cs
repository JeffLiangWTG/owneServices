namespace CargoWise.RefDbRepo.NOReferenceData.Services.ErrorCodes
{
	partial class FeilmeldingListe
	{
		public Feilmelding[] ErrorMessage
		{
			get => feilmelding;
			set => feilmelding = value;
		}
	}

	partial class Feilmelding
	{
		public string MessageNumber
		{
			get => meldingNummer;
			set => meldingNummer = value;
		}

		public string MessageText
		{
			get => meldingTekst;
			set => meldingTekst = value;
		}
	}
}
