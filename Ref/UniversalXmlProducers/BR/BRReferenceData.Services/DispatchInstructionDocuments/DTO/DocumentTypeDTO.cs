using System.Collections.Generic;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class DocumentTypeDTO
	{
		[JsonProperty("IdTipoDocumento")]
		public int DocumentTypeId { get; set; }

		[JsonProperty("NomeTipoDocumento")]
		public string DocumentTypeName { get; set; }

		[JsonProperty("palavrasChave")]
		public List<KeywordDTO> Keywords { get; set; }
	}
}
