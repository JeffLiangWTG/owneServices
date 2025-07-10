using System.Collections.Generic;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class DossierDataDTO
	{
		[JsonProperty("OrgaosAnuentes")]
		public List<ApprovingBodyDTO> ApprovingBodies { get; set; }

		[JsonProperty("TiposDocumento")]
		public List<DocumentTypeDTO> DocumentTypes { get; set; }

		[JsonProperty("TiposDossie")]
		public List<DossierTypeDTO> DossierTypes { get; set; }

		public string OperationType { get; set; }
	}
}
