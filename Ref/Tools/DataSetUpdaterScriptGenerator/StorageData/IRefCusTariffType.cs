using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffType : IDataSetStorage
	{
		Guid ZZI_PK { get; set; }
		string ZZI_TariffType { get; set; }
		string ZZI_Description { get; set; }
		string ZZI_ZZZ_NKDataGrouping { get; set; }
		string ZZI_ZZ9_NKNomenclatureGroupType { get; set; }

		IEnumerable<IRefCusTariff> RefCusTariffs { get; }
		IEnumerable<IRefCusTariffRelationship> RefCusTariffRelationships { get; }
		IEnumerable<IRefCusProfileType> RefCusProfileTypes { get; }
	}
}

