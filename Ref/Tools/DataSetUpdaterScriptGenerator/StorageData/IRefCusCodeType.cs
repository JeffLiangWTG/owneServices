using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusCodeType : IDataSetStorage
	{
		Guid ZZK_PK { get; set; }
		string ZZK_CodeType { get; set; }
		string ZZK_Description { get; set; }
		bool ZZK_IsReadonly { get; set; }
		byte ZZK_MaxLength { get; set; }
		string ZZK_ZZZ_NKDataGrouping { get; set; }

		IEnumerable<IRefCusCodeTypeLanguage> RefCusCodeTypeLanguage { get; }
	}
}
