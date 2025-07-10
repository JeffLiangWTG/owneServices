using System.Collections.Generic;
using Newtonsoft.Json;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class AirlineConfigCommodity
	{
		public string Code { get; set; }
		public string Description { get; set; }
		[JsonConverter(typeof(JsonCsvToCollectionConverter))]
		public IReadOnlyCollection<string> SpecialHandlingCodes { get; set; }
	}
}
