using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusProcedureLanguage : IDataSetStorage
	{
		Guid ZXV_PK { get; set; }
		Guid ZXV_ZZ6_Procedure { get; set; }
		string ZXV_ZX6_NKLanguage { get; set; }
		string ZXV_Description { get; set; }
	}
}
