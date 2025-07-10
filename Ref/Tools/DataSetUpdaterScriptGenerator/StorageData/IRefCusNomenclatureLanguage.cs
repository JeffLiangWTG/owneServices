using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusNomenclatureLanguage : IDataSetStorage
	{
		Guid ZX8_PK { get; set; }
		string ZX8_ZX6_NKLanguage { get; set; }
		Guid ZX8_ZZ5_NomenclatureGroup { get; set; }
		string ZX8_Description { get; set; }
	}
}
