namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class Tariff4PGA
	{
		public string TariffCode { get; set; }

		public bool IsMandatory { get; set; }

		public string PGACode { get; set; }

		public Tariff4PGA(string tariffCode, bool isMandatory, string _PGACode)
		{
			TariffCode = tariffCode;
			IsMandatory = isMandatory;
			PGACode = _PGACode;
		}
	}
}
