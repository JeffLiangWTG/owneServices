using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCarrierCode : IDataSetStorage
	{
		Guid ZZ4_PK { get; set; }
		string ZZ4_Code { get; set; }
		string ZZ4_Description { get; set; }
		string ZZ4_ZZZ_NKDataGrouping { get; set; }
		bool ZZ4_IsSea { get; set; }
		bool ZZ4_IsRoad { get; set; }
		bool ZZ4_IsRail { get; set; }
		bool ZZ4_IsAir { get; set; }

		IEnumerable<IRefCarrierCodeAttribute> RefCarrierCodeAttributes { get; }
		IEnumerable<IRefCarrierVesselPivot> RefCarrierVesselPivots { get; }
		IEnumerable<IRefCarrierCodeLanguage> RefCarrierCodeLanguages { get; }
	}
}
