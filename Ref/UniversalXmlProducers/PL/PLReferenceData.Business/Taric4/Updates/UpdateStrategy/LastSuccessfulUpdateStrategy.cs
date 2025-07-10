using System;
using System.IO;
using System.Linq;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates.UpdateStrategy;

sealed class LastSuccessfulUpdateStrategy(string filePath) : IUpdateStrategy
{
	public bool ShouldWaitForResponses => true;

	public string SaveRequestsFilePath => string.Empty;

	public DateTime FromDate => File.Exists(filePath)
		&& DateTime.TryParse(File.ReadLines(filePath).First(), out var result)
			? result.Date
			: DateTime.UtcNow.AddDays(-1).Date;

	public DateTime ToDate => DateTime.Today;

	public void OnSuccessfulUpdate() => File.WriteAllText(filePath, $"{DateTime.UtcNow}");
}


