using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusRateTypeLanguage : IDataSetStorage
	{
		Guid ZXT_PK { get; set; }
		string ZXT_ZX6_NKLanguage { get; set; }
		Guid ZXT_ZZR_RateType { get; set; }
		string ZXT_Description { get; set; }
	}
}
