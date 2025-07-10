using System;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates.UpdateStrategy;

interface IUpdateStrategy
{
	bool ShouldWaitForResponses { get; }
	string SaveRequestsFilePath { get; }
	DateTime FromDate { get; }
	DateTime ToDate { get; }
	void OnSuccessfulUpdate();
}
