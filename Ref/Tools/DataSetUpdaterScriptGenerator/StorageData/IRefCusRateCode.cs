using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusRateCode : IDataSetStorage
	{
		Guid ZY1_PK { get; set; }
		string ZY1_RateCode { get; set; }
		Guid ZY1_ZZR_RateType { get; set; }
		string ZY1_Description { get; set; }
		bool ZY1_InternalUse { get; set; }
		string ZY1_ZZZ_NKDataGrouping { get; set; }

		IEnumerable<IRefCusRate> RefCusRate { get; }
		IEnumerable<IRefCusRateCodeLanguage> RefCusRateCodeLanguages { get; }
	}
}

