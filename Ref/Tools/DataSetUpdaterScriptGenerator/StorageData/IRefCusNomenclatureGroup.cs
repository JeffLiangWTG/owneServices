using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusNomenclatureGroup : IDataSetStorage
	{
		Guid ZZ5_PK { get; set; }
		string ZZ5_Value { get; set; }
		string ZZ5_Description { get; set; }
		DateTime ZZ5_StartDate { get; set; }
		DateTime ZZ5_EndDate { get; set; }
		string ZZ5_CompositeKey { get; set; }
		string ZZ5_ZZZ_NKDataGrouping { get; set; }
		string ZZ5_ZZ9_NKNomenclatureGroupType { get; set; }

		IEnumerable<IRefCusNomenclatureGroupNote> RefCusNomenclatureGroupNotes { get; }
		IEnumerable<IRefCusCondition> RefCusCondition { get; }
		IEnumerable<IRefCusNomenclatureLanguage> RefCusNomenclatureLanguage { get; }
	}
}
