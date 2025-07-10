using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CargoWise.RefDbRepo.LLIReferenceData.Services.Entities;

record VesselAdvancedCharacteristicsResponse(
	bool IsSuccess,
	VesselAdvancedCharacteristicData Data,
	List<string> Errors);

public record VesselAdvancedCharacteristicData(
	[property: JsonPropertyName("currentPage")] int CurrentPage,
	[property: JsonPropertyName("totalPages")] int TotalPages,
	[property: JsonPropertyName("totalRecords")] int TotalRecords,
	[property: JsonPropertyName("items")] List<VesselAdvancedCharacteristic> Items
);

public record VesselAdvancedCharacteristic(
	[property: JsonPropertyName("vesselAdvancedChars")] VesselAdvancedChars VesselAdvancedChars,
	[property: JsonPropertyName("vesselDimensions")] VesselDimensions VesselDimensions,
	[property: JsonPropertyName("vesselDesignSuperstructure")] VesselDesignSuperStructure VesselDesignSuperStructure,
	[property: JsonPropertyName("vesselCapacitites")] VesselCapacities VesselCapacities
);

public record VesselAdvancedChars(
	[property: JsonPropertyName("vesselId")] string VesselId,
	[property: JsonPropertyName("vesselImo")] string VesselImo,
	[property: JsonPropertyName("vesselName")] string VesselName,
	[property: JsonPropertyName("hullId")] string HullId,
	[property: JsonPropertyName("vesselMmsi")] string VesselMmsi
);

public record VesselDimensions(
	[property: JsonPropertyName("breadthExtreme")] double? BreadthExtreme,
	[property: JsonPropertyName("breadthMoulded")] double? BreadthMoulded,
	[property: JsonPropertyName("depth")] double? Depth,
	[property: JsonPropertyName("draft")] double? Draft,
	[property: JsonPropertyName("freeboard")] double? Freeboard,
	[property: JsonPropertyName("lbp")] double? Lbp,
	[property: JsonPropertyName("loa")] double? Loa,
	[property: JsonPropertyName("lrg")] double? Lrg,
	[property: JsonPropertyName("tpcmi")] double? Tpcmi,
	[property: JsonPropertyName("manifoldToBow")] double? ManifoldToBow,
	[property: JsonPropertyName("parallelBodyLength")] double? ParallelBodyLength,
	[property: JsonPropertyName("manifoldDeckToCentre")] double? ManifoldDeckToCentre,
	[property: JsonPropertyName("ktm")] double? Ktm,
	[property: JsonPropertyName("formulaDwt")] double? FormulaDwt
);

public record VesselDesignSuperStructure(
	[property: JsonPropertyName("bulbousBow")] bool? BulbousBow,
	[property: JsonPropertyName("hullType")] string HullType,
	[property: JsonPropertyName("hullDesign")] string HullDesign,
	[property: JsonPropertyName("materialOfBuild")] string MaterialOfBuild,
	[property: JsonPropertyName("numberOfDecks")] int? NumberOfDecks,
	[property: JsonPropertyName("heliDeck")] bool? HeliDeck,
	[property: JsonPropertyName("watertightCompartments")] int? WatertightCompartments,
	[property: JsonPropertyName("bulkheads")] int? Bulkheads,
	[property: JsonPropertyName("thrusters")] string Thrusters,
	[property: JsonPropertyName("enginePosition")] string EnginePosition,
	[property: JsonPropertyName("totalTanks")] int? TotalTanks,
	[property: JsonPropertyName("wingTankers")] int? WingTankers,
	[property: JsonPropertyName("centreTanks")] int? CentreTanks,
	[property: JsonPropertyName("strengthenedHeavyCargo")] bool? StrengthenedHeavyCargo,
	[property: JsonPropertyName("oreCargo")] bool? OreCargo,
	[property: JsonPropertyName("fuelCapacity")] double? FuelCapacity,
	[property: JsonPropertyName("fuelConsumption")] string FuelConsumption,
	[property: JsonPropertyName("propulsionType")] string PropulsionType,
	[property: JsonPropertyName("propulsionText")] string PropulsionText,
	[property: JsonPropertyName("speed")] double? Speed,
	[property: JsonPropertyName("speedType")] string SpeedType,
	[property: JsonPropertyName("crew")] int? Crew,
	[property: JsonPropertyName("passengers")] int? Passengers,
	[property: JsonPropertyName("berths")] int? Berths,
	[property: JsonPropertyName("cabins")] int? Cabins,
	[property: JsonPropertyName("laneLength")] double? LaneLength,
	[property: JsonPropertyName("laneWidth")] double? LaneWidth,
	[property: JsonPropertyName("reeferPlugs")] int? ReeferPlugs,
	[property: JsonPropertyName("conversionText")] string ConversionText,
	[property: JsonPropertyName("conversionDate")] string ConversionDate,
	[property: JsonPropertyName("hatchType")] string HatchType,
	[property: JsonPropertyName("gearless")] bool? Gearless,
	[property: JsonPropertyName("co2")] bool? Co2
);

public record VesselCapacities(
	[property: JsonPropertyName("bale")] double? Bale,
	[property: JsonPropertyName("ballast")] double? Ballast,
	[property: JsonPropertyName("bollardPull")] double? BollardPull,
	[property: JsonPropertyName("cars")] int? Cars,
	[property: JsonPropertyName("trailers")] int? Trailers,
	[property: JsonPropertyName("gas")] double? Gas,
	[property: JsonPropertyName("grain")] double? Grain,
	[property: JsonPropertyName("liquid")] double? Liquid,
	[property: JsonPropertyName("liquidBarrels")] double? LiquidBarrels,
	[property: JsonPropertyName("oreTonnes")] double? OreTonnes,
	[property: JsonPropertyName("pumpCubic")] double? PumpCubic,
	[property: JsonPropertyName("pumpTonnes")] double? PumpTonnes,
	[property: JsonPropertyName("pumpDescription")] string PumpDescription,
	[property: JsonPropertyName("refrigerated")] double? Refrigerated,
	[property: JsonPropertyName("deckTank")] double? DeckTank,
	[property: JsonPropertyName("slopTank")] double? SlopTank,
	[property: JsonPropertyName("teu")] double? Teu,
	[property: JsonPropertyName("teuDeck")] double? TeuDeck,
	[property: JsonPropertyName("teuHold")] double? TeuHold,
	[property: JsonPropertyName("teuOperation")] double? TeuOperation,
	[property: JsonPropertyName("refrigeratedTeu")] double? RefrigeratedTeu,
	[property: JsonPropertyName("teu14t")] double? Teu14t
);
