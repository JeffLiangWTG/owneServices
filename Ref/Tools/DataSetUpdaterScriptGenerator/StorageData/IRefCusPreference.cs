using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusPreference : IDataSetStorage
	{
		Guid ZZS_PK { get; set; }
		string ZZS_Preference { get; set; }
		string ZZS_Description { get; set; }
		string ZZS_ZZZ_NKDataGrouping { get; set; }

		IEnumerable<IRefCusRate> RefCusRate { get; }
		IEnumerable<IRefCusCondition> RefCusCondition { get; }
		IEnumerable<IRefCusPreferenceLanguage> RefCusPreferenceLanguage { get; }
	}
}
