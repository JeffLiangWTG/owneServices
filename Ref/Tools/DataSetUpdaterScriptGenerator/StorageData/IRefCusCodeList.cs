using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusCodeList : IDataSetStorage
	{
		Guid ZZD_PK { get; set; }
		string ZZD_Code { get; set; }
		string ZZD_Description { get; set; }
		DateTime ZZD_StartDate { get; set; }
		DateTime ZZD_EndDate { get; set; }
		string ZZD_ZZZ_NKDataGrouping { get; set; }
		string ZZD_ZZK_NKCodeType { get; set; }

		IEnumerable<IRefCusCodeListAttribute> RefCusCodeListAttributes { get; }
		IEnumerable<IRefCusCodeOrAttributeTransportMode> RefCusCodeOrAttributeTransportMode { get; }
		IEnumerable<IRefCusCodeListLanguage> RefCusCodeListLanguage { get; }
	}
}
