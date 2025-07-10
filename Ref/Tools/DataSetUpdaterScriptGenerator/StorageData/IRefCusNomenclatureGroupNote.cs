using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusNomenclatureGroupNote : IDataSetStorage
	{
		Guid ZZL_PK { get; set; }
		Guid ZZL_ZZ5_NomenclatureGroup { get; set; }
		string ZZL_NoteType { get; set; }
		string ZZL_Note { get; set; }
		string ZZL_ZZZ_NKDataGrouping { get; set; }
		string ZZL_ZX6_NKLanguage { get; set; }
	}
}
