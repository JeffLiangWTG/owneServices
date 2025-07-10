namespace CargoWise.RefDbRepo.NOReferenceData.Services.ExchangeRates
{
	partial class omregningKursListe
	{
		public Omregningskurs[] ConversionRate
		{
			get => omregningskurs;
			set => omregningskurs = value;
		}
	}

	partial class Omregningskurs
	{
		public string CurrencyCode
		{
			get => valutakode;
			set => valutakode = value;
		}

		public string CurrencyDescription
		{
			get => valutabeskrivelse;
			set => valutabeskrivelse = value;
		}
		
		public string CurrencyRate
		{
			get => valutakurs;
			set => valutakurs = value;
		}
		
		public short Multiplier
		{
			get => omregningsenhet;
			set => omregningsenhet = value;
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
