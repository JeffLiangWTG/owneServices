namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class Condicao
	{
		public string operador { get; set; }
		public string valor { get; set; }
		public string composicao { get; set; }
		public Condicao condicao { get; set; }
	}
}
