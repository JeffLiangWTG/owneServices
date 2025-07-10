using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public interface IUniversalReferenceDataFileGenerator
	{
		bool GenerateFiles(DateTime publicationDate, ref Errors error);

		IEnumerable<string> InputFiles { get; }
	}
}
