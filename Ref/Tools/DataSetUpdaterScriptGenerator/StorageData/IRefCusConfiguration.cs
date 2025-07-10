using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusConfiguration : IDataSetStorage
	{
		Guid ZZJ_PK { get; set; }
		string ZZJ_RN_NKCustomsCountry { get; set; }
		string ZZJ_TariffDataSource { get; set; }
		bool ZZJ_IsGenericCountry { get; set; }
		bool ZZJ_AllowRiskManagement { get; set; }
		bool ZZJ_IsTransitDeclarationCounty { get; set; }
		bool ZZJ_TurnOnASYDCUDAManifest { get; set; }
		bool ZZJ_TurnOnASYCUDACustoms { get; set; }
		string ZZJ_ZZZ_NKDefaultDataGrouping { get; set; }
		string ZZJ_ZZZ_NKAlternateTariffOnlyDataGrouping { get; set; }
		DateTime ZZJ_StartDate { get; set; }
		Nullable<DateTime> ZZJ_EndDate { get; set; }
	}
}
