using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class DomainDTO
	{
		[JsonProperty("codigo")]
		public string Code { get; set; }

		[JsonProperty("descricao")]
		public string Description { get; set; }
	}
}
