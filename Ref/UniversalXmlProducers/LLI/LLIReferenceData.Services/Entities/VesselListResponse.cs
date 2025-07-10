using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.LLIReferenceData.Services.Entities;

record VesselListResponse(bool IsSuccess, VesselListData Data);

record VesselListData(
	int CurrentPage,
	int TotalPages,
	int TotalRecords,
	List<Vessel> Vessels,
	List<string> Errors);

public record Vessel(
	string VesselId,
	string VesselImo,
	string VesselType,
	string VesselName,
	string VesselTypeCategoryLongName,
	string VesselStatus,
	DateTime LastUpdated);
