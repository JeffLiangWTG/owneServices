using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Controllers;

public partial class RefCusProcedureController : DataSetControllerBase<RefCusProcedure>
{
	protected override object TransformData(RefCusProcedure dataSet, string version)
	{
		var latestVersion = (RefCusProcedure)base.TransformData(dataSet, version);
		if (latestVersion != null && !Adaptor.IsSRDbVersion(version) && Adaptor.ParseVersion(version)?.Item2 < 22)
		{
			var priorVersion = new RefCusProcedurePrior_0_22_9
			{
				ZZ6_Category = latestVersion.ZZ6_Category,
				ZZ6_ProcedureCode = latestVersion.ZZ6_ProcedureCode,
				ZZ6_PreviousProcedureCode = latestVersion.ZZ6_PreviousProcedureCode,
				ZZ6_Concession = latestVersion.ZZ6_Concession,
				ZZ6_Description = latestVersion.ZZ6_Description,
				ZZ6_RN_CountryOrGrouping = latestVersion.ZZ6_RN_CountryOrGrouping,
				ZZ6_ZZZ_NKDataGrouping = latestVersion.ZZ6_ZZZ_NKDataGrouping,
				ZZ6_ShipmentType = latestVersion.ZZ6_ShipmentType,
				ZZ6_CalculateDuty = latestVersion.ZZ6_CalculateDuty,
				ZZ6_Group = latestVersion.ZZ6_Group,
				ZZ6_LandedCost = latestVersion.ZZ6_LandedCost,
				ZZ6_IntoWarehouse = latestVersion.ZZ6_IntoWarehouse == "Y" ? true : false,
				ZZ6_OutOfWarehouse = latestVersion.ZZ6_OutOfWarehouse == "Y" ? true : false,
				ZZ6_StartDate = latestVersion.ZZ6_StartDate,
				ZZ6_EndDate = latestVersion.ZZ6_EndDate,
				ZZ6_TemporaryProcedure = latestVersion.ZZ6_TemporaryProcedure,
				ZZ6_CalculateVAT = latestVersion.ZZ6_CalculateVAT
			};
			priorVersion.RefCusProcedureAttributes = latestVersion.RefCusProcedureAttributes;
			return priorVersion;
		}
		return latestVersion;
	}
}
