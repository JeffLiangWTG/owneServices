using System;
using System.Collections.Generic;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Yard.Integration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Enterprise.Warehouse.Yard.DataTransfer
{
	public class YardDropoffData : IYardValidationData, IGateBookingValidationResponse
	{
		[JsonProperty("result")]
		[JsonConverter(typeof(StringEnumConverter))]
		public ResultEnum? Result { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("messageCode")]
		[JsonConverter(typeof(StringEnumConverter))]
		public ResponseCode? MessageCode { get; set; }

		[JsonProperty("containerNumber")]
		public string ContainerNumber { get; set; }

		[JsonProperty("isoCode")]
		public string IsoCode { get; set; }

		[JsonProperty("owner")]
		public string Owner { get; set; }

		[JsonProperty("seal")]
		public string Seal { get; set; }

		[JsonProperty("voyage")]
		public string Voyage { get; set; }

		[JsonProperty("vessel")]
		public string Vessel { get; set; }

		[JsonProperty("referenceNumber")]
		public string ReferenceNumber { get; set; }

		[JsonProperty("isStoringOrderAvailable")]
		public bool IsStoringOrderAvailable { get; set; }

		[JsonProperty("availableDateUtc")]
		public DateTime? AvailableDateUtc { get; set; }

		[JsonProperty("containerCode")]
		public string ContainerCode { get; set; }

		[JsonProperty("containerDescription")]
		public string ContainerDescription { get; set; }

		[JsonProperty("expiryDateUtc")]
		public DateTime? ExpiryDateUtc { get; set; }

		public enum ResultEnum
		{
			Accept,
			Reject,
			Unavailable
		}

		public enum ResponseCode
		{
			CNF,
			FNF,
			CGI
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message strings")]
		public static Dictionary<ResponseCode, string> ErrorMessages => new ()
		{
			{ ResponseCode.CNF, "Container number not found" },
			{ ResponseCode.FNF, "Facility not found" },
			{ ResponseCode.CGI, "Container has been gated-in" },
		};
	}
}
