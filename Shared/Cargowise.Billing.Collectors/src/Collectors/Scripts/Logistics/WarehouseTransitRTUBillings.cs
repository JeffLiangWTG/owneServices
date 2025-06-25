using System;

namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class WarehouseTransitRTUBillings : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "WRB";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Transit Warehouse";
		public override string FeatureName => "Warehouse RTU Billing";
		public override string CompanyCode => "RTUBillingData.GC_Code";
		public override string BranchCode => "RTUBillingData.GB_Code";
		public override string TransactionDateUtc => "RTUBillingData.TransactionDate";
		public override string BillingReference1 => "RTUBillingData.WW_WarehouseCode";
		public override string BillingReference2 => "RTUBillingData.WRH_ReferenceNumber";
		public override string BillingReference3 => "RTUBillingData.JH_JobNum";
		public override string BillingReference4 => "RTUBillingData.IsInvoiced";
		public override string GuidReference => "RTUBillingData.JR_PK";
		public override string CreatingUserCode => "RTUBillingData.JH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => $@"(
SELECT
	GC_Code,
	GB_Code,
	JH_SystemCreateTimeUtc AS TransactionDate,
	WW_WarehouseCode,
	WRH_ReferenceNumber,
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
	JOIN dbo.WhsItemReceiveTransportationUnit ON WRH_PK = JH_ParentID
		AND JH_ParentTableCode  = 'WRH'
		AND JH_SystemCreateTimeUtc >= CAST({Constants.StartDateTimeInclusiveParamName} AS SMALLDATETIME)
		AND JH_SystemCreateTimeUtc < CAST({Constants.EndDateTimeExclusiveParamName} AS SMALLDATETIME)
	JOIN dbo.WhsWarehouse ON WW_PK = WRH_WW_Warehouse
	JOIN dbo.GlbBranch ON GB_PK = WW_GB_RelatedCompanyBranch
	JOIN dbo.GlbCompany ON GC_PK = GB_GC
	JOIN dbo.JobCharge ON JR_JH = JH_PK
	JOIN dbo.AccChargeCode ON AC_PK = JR_AC
) AS RTUBillingData
";
		public override string WhereClause => string.Empty;
		public override string AdditionalRefs => @"
CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(
    SELECT
		ChargeCode = RTUBillingData.AC_Code,
		ChargeDesc = RTUBillingData.JR_Desc,
		ChargeGroup = RTUBillingData.AC_ChargeGroup,
		ChargeSubGroup = RTUBillingData.AC_ChargeSubGroup
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)))
";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string DateType => RefStlDateType.SmallDateTime;
	}

	#endregion
}
