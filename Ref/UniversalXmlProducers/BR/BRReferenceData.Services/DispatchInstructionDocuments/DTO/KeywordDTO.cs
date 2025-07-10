using System.Collections.Generic;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class KeywordDTO
	{
		[JsonProperty("casasDecimais")]
		public int DecimalPlaces { get; set; }

		[JsonProperty("dominios")]
		public List<DomainDTO> Domains { get; set; }

		[JsonProperty("idPalavraChave")]
		public int KeywordId { get; set; }

		[JsonProperty("mascara")]
		public string Mask { get; set; }

		[JsonProperty("nomePalavraChave")]
		public string KeywordName { get; set; }

		[JsonProperty("obrigatoria")]
		public bool IsMandatory { get; set; }

		[JsonProperty("tamanhoCampo")]
		public int FieldSize { get; set; }

		[JsonProperty("tipoDado")]
		public string DataType { get; set; }
	}
}
