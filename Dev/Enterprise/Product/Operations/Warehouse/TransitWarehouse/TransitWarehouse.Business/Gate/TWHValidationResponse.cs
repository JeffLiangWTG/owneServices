using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Integration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TWHValidationResponse : ITWHValidationResponse, IGateBookingValidationResponse
	{
		[JsonProperty("result")]
		[JsonConverter(typeof(StringEnumConverter))]
		public ValidationResult? Result { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("messageCode")]
		[JsonConverter(typeof(StringEnumConverter))]
		public ValidationErrorCode? MessageCode { get; set; }

		[JsonProperty("referenceNumberType")]
		public string ReferenceNumberType { get; set; }

		[JsonProperty("grossWeightValue", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? GrossWeightValue { get; set; }

		[JsonProperty("grossWeightUnit")]
		public string GrossWeightUnit { get; set; }

		[JsonProperty("grossVolumeValue", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? GrossVolumeValue { get; set; }

		[JsonProperty("grossVolumeUnit")]
		public string GrossVolumeUnit { get; set; }

		[JsonProperty("quantityValue", NullValueHandling = NullValueHandling.Ignore)]
		public int? QuantityValue { get; set; }
	}
}
