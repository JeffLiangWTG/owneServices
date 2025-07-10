using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusRateType : IDataSetStorage
	{
		Guid ZZR_PK { get; set; }
		string ZZR_RateType { get; set; }
		string ZZR_Description { get; set; }
		bool ZZR_IsPayable { get; set; }
		string ZZR_CustomsValueFormula { get; set; }
		string ZZR_ZZZ_NKDataGrouping { get; set; }
		bool ZZR_IsExport { get; set; }
		string ZZR_RX_NKFormulaCurrency { get; set; }

		IEnumerable<IRefCusRateCode> RefCusRateCode { get; }
		IEnumerable<IRefCusRateTypeLanguage> RefCusRateTypeLanguage { get; }
	}
}

