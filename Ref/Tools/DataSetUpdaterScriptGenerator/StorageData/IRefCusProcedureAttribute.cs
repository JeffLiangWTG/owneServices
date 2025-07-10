using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusProcedureAttribute : IDataSetStorage
	{
		Guid ZXB_PK { get; set; }
		Guid ZXB_ZZ6_ProcedureCode { get; set; }
		string ZXB_Name { get; set; }
		string ZXB_Value { get; set; }
	}
}
