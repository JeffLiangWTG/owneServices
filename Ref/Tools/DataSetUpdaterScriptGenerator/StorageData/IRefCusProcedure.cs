using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusProcedure : IDataSetStorage
	{
		Guid ZZ6_PK { get; set; }
		string ZZ6_Category { get; set; }
		string ZZ6_ProcedureCode { get; set; }
		string ZZ6_PreviousProcedureCode { get; set; }
		string ZZ6_Concession { get; set; }
		string ZZ6_Description { get; set; }
		string ZZ6_ZZZ_NKDataGrouping { get; set; }
		string ZZ6_ShipmentType { get; set; }
		bool ZZ6_CalculateDuty { get; set; }
		string ZZ6_Group { get; set; }
		bool ZZ6_LandedCost { get; set; }
		string ZZ6_IntoWarehouse { get; set; }
		string ZZ6_OutOfWarehouse { get; set; }
		DateTime ZZ6_StartDate { get; set; }
		DateTime ZZ6_EndDate { get; set; }
		bool ZZ6_CalculateVAT { get; set; }
		string ZZ6_IsGuaranteeConsumed { get; set; }
		string ZZ6_IsGuaranteeReleased { get; set; }
		string ZZ6_IntoInwardProcessing { get; set; }
		string ZZ6_OutOfInwardProcessing { get; set; }
		string ZZ6_IntoOutwardProcessing { get; set; }
		string ZZ6_OutofOutwardProcessing { get; set; }
		string ZZ6_IntoTemporaryImport { get; set; }
		string ZZ6_OutOfTemporaryImport { get; set; }
		string ZZ6_IntoTemporaryExport { get; set; }
		string ZZ6_OutOfTemporaryExport { get; set; }
		string ZZ6_IsTransit { get; set; }
		string ZZ6_IntoVATWarehouse { get; set; }
		string ZZ6_OutOfVATWarehouse { get; set; }

		IEnumerable<IRefCusProcedureAttribute> RefCusProcedureAttribute { get; }
		IEnumerable<IRefCusProcedureLanguage> RefCusProcedureLanguage { get; }
	}
}
