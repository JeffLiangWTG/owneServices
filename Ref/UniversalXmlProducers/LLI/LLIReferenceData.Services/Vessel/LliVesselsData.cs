using System.Collections.Generic;
using CargoWise.RefDbRepo.LLIReferenceData.Services.Entities;

namespace CargoWise.RefDbRepo.LLIReferenceData.Services.Vessel;

public record LliVesselsData(
	IReadOnlyList<Entities.Vessel> Vessels,
	IReadOnlyList<VesselBasicCharacteristic> VesselsBasicCharacteristics,
	IReadOnlyList<VesselAdvancedCharacteristic> VesselsAdvancedCharacteristics);
