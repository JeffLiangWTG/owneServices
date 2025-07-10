using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffTypeLanguage : IDataSetStorage
	{
		Guid ZXK_PK { get; set; }
		Guid ZXK_ZZI_TariffType { get; set; }
		string ZXK_ZX6_NKLanguage { get; set; }
		string ZXK_Description { get; set; }
	}
}
