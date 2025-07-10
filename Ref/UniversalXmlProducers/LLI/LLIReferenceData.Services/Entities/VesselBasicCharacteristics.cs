using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CargoWise.RefDbRepo.LLIReferenceData.Services.Entities;

record VesselBasicCharacteristicsResponse(
	bool IsSuccess,
	VesselBasicCharacteristicData Data,
	List<string> Errors);

record VesselBasicCharacteristicData(
	[property: JsonPropertyName("currentPage")] int CurrentPage,
	[property: JsonPropertyName("totalPages")] int TotalPages,
	[property: JsonPropertyName("totalRecords")] int TotalRecords,
	[property: JsonPropertyName("vessels")] List<VesselBasicCharacteristic> Vessels
);

public record VesselBasicCharacteristic(
	[property: JsonPropertyName("vesselId")] string VesselId,
	[property: JsonPropertyName("imo")] string Imo,
	[property: JsonPropertyName("vesselName")] string VesselName,
	[property: JsonPropertyName("built")] int? Built,
	[property: JsonPropertyName("flag")] string Flag,
	[property: JsonPropertyName("callsign")] string Callsign,
	[property: JsonPropertyName("mmsi")] string Mmsi,
	[property: JsonPropertyName("portOfRegistry")] string PortOfRegistry,
	[property: JsonPropertyName("grossTonnage")] int? GrossTonnage,
	[property: JsonPropertyName("netTonnage")] int? NetTonnage,
	[property: JsonPropertyName("dwtTonnage")] int? DwtTonnage,
	[property: JsonPropertyName("genType")] string GenType,
	[property: JsonPropertyName("subType")] string SubType,
	[property: JsonPropertyName("vesselType")] string VesselType,
	[property: JsonPropertyName("status")] string Status,
	[property: JsonPropertyName("lastUpdated")] DateTime LastUpdated
);
