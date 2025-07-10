using System;
using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public abstract class ExcelDataCollectorProgram : DataCollectorProgram
	{
		protected override void PrepareDataSource(ref Errors error, out DateTime publicationDate)
		{
			publicationDate = DateTime.Now;
		}
	}
}
