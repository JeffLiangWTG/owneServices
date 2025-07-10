using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusRateCodeLanguage : IDataSetStorage
	{
		Guid ZXC_PK { get; set; }
		string ZXC_ZX6_NKLanguage { get; set; }
		Guid ZXC_ZY1_RateCode { get; set; }
		string ZXC_Description { get; set; }
	}
}
