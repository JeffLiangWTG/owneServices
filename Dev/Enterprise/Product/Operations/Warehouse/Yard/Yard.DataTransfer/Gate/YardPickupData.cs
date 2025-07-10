using System;
using System.Collections.Generic;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Yard.Integration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Enterprise.Warehouse.Yard.DataTransfer
{
	public class YardPickupData : IYardValidationData, IGateBookingValidationResponse
	{
		[JsonProperty("result")]
		[JsonConverter(typeof(StringEnumConverter))]
		public ResultEnum? Result { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("messageCode")]
		[JsonConverter(typeof(StringEnumConverter))]
		public ResponseCode? MessageCode { get; set; }

		[JsonProperty("releaseDetails")]
		public List<ReleaseDetail> ReleaseDetails { get; set; }

		public sealed class ReleaseDetail
		{
			[JsonProperty("containerDetails")]
			public List<ContainerDetails> Containers { get; set; }

			[JsonProperty("shippingLineCode")]
			public string ClientCode { get; set; }

			[JsonProperty("vesselId")]
			public string VesselId { get; set; }

			[JsonProperty("vesselName")]
			public string VesselName { get; set; }

			[JsonProperty("voyageNumber")]
			public string VoyageNumber { get; set; }

			[JsonProperty("expiryDateUtc")]
			public DateTime? ExpiryDateUtc { get; set; }

			[JsonProperty("availableDateUtc")]
			public DateTime? AvailableDateUtc { get; set; }
		}

		public sealed class ContainerDetails
		{
			[JsonProperty("isoCode")]
			public string IsoCode { get; set; }

			[JsonProperty("totalQuantity")]
			public int TotalQuantity { get; set; }

			[JsonProperty("availableQuantity")]
			public int AvailableQuantity { get; set; }

			[JsonProperty("readyDateUtc")]
			public DateTime? ReadyDateUtc { get; set; }

			[JsonProperty("code")]
			public string Code { get; set; }

			[JsonProperty("description")]
			public string Description { get; set; }
		}

		public enum ResultEnum
		{
			Accept,
			Reject
		}

		public enum ResponseCode
		{
			RNF,
			RNE,
			RNU
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message strings")]
		public static Dictionary<ResponseCode, string> ErrorMessages => new ()
		{
			{ ResponseCode.RNF, "Release number not found" },
			{ ResponseCode.RNE, "Release number expired or not due" },
			{ ResponseCode.RNU, "No Units available to pick up" }
		};
	}
}
