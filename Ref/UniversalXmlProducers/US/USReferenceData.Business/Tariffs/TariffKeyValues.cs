namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class TariffKeyValues
	{
		private string type;

		private string code;

		private string description;

		private string uOM1;

		private string uOM2;

		public string Code { get => code; set => code = value; }
		public string UOM2 { get => uOM2; set => uOM2 = value; }
		public string Description { get => description; set => description = value; }
		public string Type { get => type; set => type = value; }
		public string UOM1 { get => uOM1; set => uOM1 = value; }
	}
}
