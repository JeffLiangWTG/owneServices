using System;
using System.IO;
using CargoWise.RefDbRepo.PLReferenceData.Services;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates.UpdateStrategy;

sealed class DefaultUpdateStrategy(DateTime fromDate, ISettingsIndexer settingsIndexer = null) : IUpdateStrategy
{
	public bool ShouldWaitForResponses => true;

	public string SaveRequestsFilePath => Path.Combine(
		AppConfigHelper.GetAppSettingsValue("DownloadsPLPath", settingsIndexer),
		AppConfigHelper.GetAppSettingsValue("UpdateRequestsFileName", settingsIndexer));

	public DateTime FromDate => fromDate.Date;

	public DateTime ToDate => DateTime.Today;

	public void OnSuccessfulUpdate() { }
}


