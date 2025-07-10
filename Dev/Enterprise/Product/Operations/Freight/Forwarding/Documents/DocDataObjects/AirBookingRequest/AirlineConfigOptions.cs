using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class AirlineConfigOptions
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public AirlineConfigOptionValue UldBooking { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public AirlineConfigOptionValue LooseBooking { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public AirlineConfigOptionValue AllotmentBooking { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public AirlineConfigOptionValue CommodityCode { get; set; }

		public bool RequiredTermsAgreement { get; set; }
	}
}
