using System.Collections.Generic;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff
{
	public interface IFileManager
	{
		IReadOnlyCollection<IFileDetails> GetFiles();
	}
}
