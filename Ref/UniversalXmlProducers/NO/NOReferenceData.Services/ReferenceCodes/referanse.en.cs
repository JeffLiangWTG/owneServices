namespace CargoWise.RefDbRepo.NOReferenceData.Services.ReferenceCodes
{
	partial class referanserListe
	{
		public Referanse[] Reference
		{
			get => referanse;
			set => referanse = value;
		}
	}

	partial class Referanse
	{
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

		public string Code
		{
			get => kode;
			set => kode = value;
		}

		public string Description
		{
			get => beskrivelse;
			set => beskrivelse = value;
		}
	}
}
