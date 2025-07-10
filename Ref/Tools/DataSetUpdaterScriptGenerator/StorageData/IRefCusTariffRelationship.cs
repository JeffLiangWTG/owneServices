using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffRelationship : IDataSetStorage
	{
		Guid ZZH_PK { get; set; }
		Guid ZZH_ZZ1_Tariff { get; set; }
		Guid ZZH_ZZI_TariffType { get; set; }
		string ZZH_TariffCode { get; set; }
	}
}
