using System;

namespace CargoWise.Billing.Collectors.Logistics
{
	public class DangerousGoodsJobCount : RefStlScriptWithDefaults
	{
		public override string MinCW1Version => "20.10.3.0"; //Minimum CW1 version that contains UNDGSubstancePivot table

		public override string FeatureCode => "DGJ";

		public override DateTime CollectionStartDateUtc => new DateTime(2025, 1, 1);

		public override string FeatureName => "Dangerous Goods Job Number Collector";

		public override string ModuleName => "Dangerous Goods";

		public override string RoleName => "Dangerous Goods Job Numbers";

		public override string FunctionName => "Dangerous Goods Job Numbers Collector";

		public override string TransactionDateUtc => "undgSubstances.SL_PostedTimeUtc";

		public override string GuidReference => "undgSubstances.SL_PK";

		public override string BranchCode => "globalBranch.GB_Code";

		public override string CompanyCode => "globalCompany.GC_Code";

		public override bool UsedInBilling => false;

		public override string BillingReference1 => "undgSubstances.SL_Table";

		public override string CreatingUserCode => "undgSubstances.SL_GS_NKUser";

		public override string AdditionalRefs => @"
CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(
SELECT
	undgSubstances.DG_Class AS [DG Class],
	undgSubstances.DP_Standard AS [DP Standard],
	undgSubstances.DP_UNNO AS [DP UNNO],
	undgSubstances.DP_Variant AS [DP Variant]
FOR JSON PATH,
	WITHOUT_ARRAY_WRAPPER
)))";

		public override string PreparationScript => $@"
WITH FilteredLog AS (
  SELECT 
    * 
  FROM 
    dbo.StmALog 
  WHERE 
    SL_SE_NKEvent = 'DGC'
), 
FilteredDGLogs AS (
  SELECT 
    undgSubstancePivot.DP_PK, 
    undgSubstancePivot.DP_UNNO, 
    undgSubstancePivot.DP_Variant, 
    undgSubstancePivot.DP_Standard, 
    log.* 
  FROM 
    FilteredLog log 
    JOIN dbo.UNDGDataItem undgDataItem ON undgDataItem.DI_ParentID = log.SL_Parent 
    JOIN dbo.UNDGSubstancePivot undgSubstancePivot ON undgSubstancePivot.DP_ParentId = undgDataItem.DI_PK AND undgSubstancePivot.DP_ParentTableCode = 'DI'
  UNION ALL 
  SELECT 
    undgSubstancePivot.DP_PK, 
    undgSubstancePivot.DP_UNNO, 
    undgSubstancePivot.DP_Variant, 
    undgSubstancePivot.DP_Standard, 
    log.* 
  FROM 
    FilteredLog log 
    JOIN dbo.UNDGSubstancePivot undgSubstancePivot ON undgSubstancePivot.DP_ParentId = log.SL_Parent
), 
FilteredDGSubstances AS (
  SELECT
	undgSubstance.DG_Class,
	undgLog.DP_PK,
	undgLog.DP_UNNO, 
	undgLog.DP_Variant, 
	undgLog.DP_Standard,
	undgLog.SL_PK,
	undgLog.SL_Table,
	undgLog.SL_GS_NKUser,
	undgLog.SL_GB_NKBranch,
	undgLog.SL_PostedTimeUtc
  FROM 
    FilteredDGLogs undgLog 
    JOIN dbo.UNDGSubstance undgSubstance ON (
      undgSubstance.DG_UNNO = undgLog.DP_UNNO 
      AND undgSubstance.DG_Variant = undgLog.DP_Variant 
      AND undgSubstance.DG_Standard = undgLog.DP_Standard
    )
)
";

		public override string FromClause => @"
					FilteredDGSubstances undgSubstances
					LEFT JOIN dbo.GlbBranch globalBranch ON globalBranch.GB_Code = undgSubstances.SL_GB_NKBranch
					LEFT JOIN dbo.GlbCompany globalCompany ON globalCompany.GC_PK = globalBranch.GB_GC";

		public override string WhereClause => string.Empty;
	}
}
