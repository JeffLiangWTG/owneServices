using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public interface IRefCarrierCodeLanguage : IDataSetStorage
	{
		Guid ZCL_PK { get; set; }
		Guid ZCL_ZZ4_CarrierCode { get; set; }
		string ZCL_ZX6_NKLanguage { get; set; }
		string ZCL_Description { get; set; }
	}
}
