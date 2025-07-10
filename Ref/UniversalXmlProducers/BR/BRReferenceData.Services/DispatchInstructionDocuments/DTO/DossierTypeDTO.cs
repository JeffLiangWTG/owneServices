using System.Collections.Generic;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class DossierTypeDTO
	{
		[JsonProperty("IdTipoDossie")]
		public int DossierTypeId { get; set; }

		[JsonProperty("NomeTipoDossie")]
		public string DossierTypeName { get; set; }

		[JsonProperty("TipoRepresentacao")]
		public string RepresentationType { get; set; }

		[JsonProperty("TiposDocumento")]
		public List<DocumentTypeDTO> DocumentTypes { get; set; }
	}
}
