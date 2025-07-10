using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusCondition : IDataSetStorage
	{
		Guid ZX1_PK { get; set; }
		Guid ZX1_ZX2_ConditionType { get; set; }
		Nullable<Guid> ZX1_ZZ1_Tariff { get; set; }
		Nullable<Guid> ZX1_ZZ5_Nomenclature { get; set; }
		DateTime ZX1_StartDate { get; set; }
		DateTime ZX1_EndDate { get; set; }
		string ZX1_Source { get; set; }
		string ZX1_Comment { get; set; }
		bool ZX1_IsImport { get; set; }
		bool ZX1_IsExport { get; set; }
		bool ZX1_ConditionValueTrueMeansStop { get; set; }
		string ZX1_ZZZ_NKDataGrouping { get; set; }
		Nullable<Guid> ZX1_ZZS_Preference { get; set; }
		byte ZX1_LogicalANDWithinGroup { get; set; }
		string ZX1_ZY7_NKConditionCode { get; set; }
		string ZX1_AdditionalComment { get; set; }
		string ZX1_Severity { get; set; }

		IEnumerable<IRefCusApplicability> RefCusApplicability { get; }
		IEnumerable<IRefCusConditionValue> RefCusConditionValue { get; }
	}
}
