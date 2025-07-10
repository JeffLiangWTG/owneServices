using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusRuling : IDataSetStorage
	{
		Guid ZZX_PK { get; set; }
		string ZZX_RN_NKCountryCode { get; set; }
		string ZZX_RulingNumber { get; set; }
		string ZZX_Description { get; set; }
		string ZZX_RulingType { get; set; }
		DateTime ZZX_StartDate { get; set; }
		DateTime ZZX_EndDate { get; set; }

		IEnumerable<IRefCusRulingConfig> RefCusRulingConfig { get; }
	}
}
