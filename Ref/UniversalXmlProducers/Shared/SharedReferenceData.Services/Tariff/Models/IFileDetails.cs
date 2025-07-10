using System;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public interface IFileDetails
	{
		string Filename { get; }
		DateTime ExecutionDate { get; }
	}
}
