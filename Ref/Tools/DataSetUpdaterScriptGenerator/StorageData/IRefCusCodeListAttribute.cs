using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusCodeListAttribute : IDataSetStorage
	{
		Guid ZZE_PK { get; set; }
		Guid ZZE_ZZD_CodeList { get; set; }
		string ZZE_ZXE_NKName { get; set; }
		string ZZE_Value { get; set; }
		Nullable<DateTime> ZZE_StartDate { get; set; }
		Nullable<DateTime> ZZE_EndDate { get; set; }

		IEnumerable<IRefCusCodeOrAttributeTransportMode> RefCusCodeOrAttributeTransportMode { get; }
	}
}
