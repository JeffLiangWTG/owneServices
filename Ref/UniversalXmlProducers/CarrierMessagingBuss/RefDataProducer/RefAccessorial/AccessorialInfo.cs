using System;
using System.Text.Json.Serialization;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer
{
	public class AccessorialInfo
	{
		[JsonPropertyName("code")]
		public string Code { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("lastModifiedDateTime")]
		public DateTime LastModifiedDateTime { get; set; }
	}
}
