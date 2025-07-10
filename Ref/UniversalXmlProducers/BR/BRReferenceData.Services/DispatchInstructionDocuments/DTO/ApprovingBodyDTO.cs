using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class ApprovingBodyDTO
	{
		[JsonProperty("Descricao")]
		public string Description { get; set; }

		[JsonProperty("Sigla")]
		public string Abbreviation { get; set; }
	}
}
