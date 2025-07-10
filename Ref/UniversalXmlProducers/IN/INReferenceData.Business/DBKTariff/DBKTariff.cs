namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class DBKTariff
	{
		public DBKTariff() { }

		public string TariffCode { get; set; }

		public string TariffDescription { get; set; }

		public decimal Rate { get; set; }

		public string RateString { get; set; }

		public string Unit { get; set; }

		public decimal SpecificRate { get; set; }

		public string RateCode { get; set; }
	}
}
