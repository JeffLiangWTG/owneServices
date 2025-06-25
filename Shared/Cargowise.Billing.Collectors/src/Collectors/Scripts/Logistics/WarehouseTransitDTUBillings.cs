using System;

namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class WarehouseTransitDTUBillings : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "WDB";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Transit Warehouse";
		public override string FeatureName => "Warehouse DTU Billing";
		public override string CompanyCode => "DTUBillingData.GC_Code";
		public override string BranchCode => "DTUBillingData.GB_Code";
		public override string TransactionDateUtc => "DTUBillingData.TransactionDate";
		public override string BillingReference1 => "DTUBillingData.WW_WarehouseCode";
		public override string BillingReference2 => "DTUBillingData.WDH_ReferenceNumber";
		public override string BillingReference3 => "DTUBillingData.JH_JobNum";
		public override string BillingReference4 => "DTUBillingData.IsInvoiced";
		public override string GuidReference => "DTUBillingData.JR_PK";
		public override string CreatingUserCode => "DTUBillingData.JH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => $@"(
SELECT 
	GC_Code,
	GB_Code,
	JH_SystemCreateTimeUtc AS TransactionDate,
	WW_WarehouseCode,
	WDH_ReferenceNumber,
	JH_JobNum,
	JH_SystemCreateUser,
	JR_PK,
	JR_Desc,
	CASE
		WHEN JR_AL_APLine IS NOT NULL OR JR_AL_ARLine IS NOT NULL THEN CAST(1 AS BIT)
		ELSE CAST(0 AS BIT)
	END AS IsInvoiced,
	AC_Code,
	AC_ChargeGroup,
	AC_ChargeSubGroup
FROM
	dbo.JobHeader
	JOIN dbo.WhsItemDispatchTransportationUnit ON WDH_PK = JH_ParentID
		AND JH_ParentTableCode  = 'WDH'
		AND JH_SystemCreateTimeUtc >= CAST({Constants.StartDateTimeInclusiveParamName} AS SMALLDATETIME)
		AND JH_SystemCreateTimeUtc < CAST({Constants.EndDateTimeExclusiveParamName} AS SMALLDATETIME)
	JOIN dbo.WhsWarehouse ON WW_PK = WDH_WW_Warehouse
	JOIN dbo.GlbBranch ON GB_PK = WW_GB_RelatedCompanyBranch
	JOIN dbo.GlbCompany ON GC_PK = GB_GC
	JOIN dbo.JobCharge ON JR_JH = JH_PK
	JOIN dbo.AccChargeCode ON AC_PK = JR_AC
) AS DTUBillingData
";
		public override string WhereClause => string.Empty;
		public override string AdditionalRefs => @"
CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(
	SELECT
		ChargeCode = DTUBillingData.AC_Code,
		ChargeDesc = DTUBillingData.JR_Desc,
		ChargeGroup = DTUBillingData.AC_ChargeGroup,
		ChargeSubGroup = DTUBillingData.AC_ChargeSubGroup
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)))
";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string DateType => RefStlDateType.SmallDateTime;
	}

	#endregion
}
