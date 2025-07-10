using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTaxOrFeeType : IDataSetStorage
	{
		Guid ZX0_PK { get; set; }
		string ZX0_TaxOrFeeType { get; set; }
		string ZX0_Description { get; set; }
	}
}
