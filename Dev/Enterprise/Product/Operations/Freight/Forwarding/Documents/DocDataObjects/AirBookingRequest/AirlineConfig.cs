using System.Collections.Generic;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class AirlineConfig
	{
		public string Prefix { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public AirlineConfigOptions Options { get; set; }
		public IReadOnlyCollection<AirlineConfigCommodity> Commodities { get; set; }
		public IReadOnlyCollection<AirlineConfigProduct> Products { get; set; }
	}
}
