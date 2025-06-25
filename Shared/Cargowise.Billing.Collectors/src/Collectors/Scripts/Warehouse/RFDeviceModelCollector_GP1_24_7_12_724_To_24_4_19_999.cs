namespace CargoWise.Billing.Collectors.Warehouse
{
	public class RFDeviceModelCollector_GP1_24_7_12_724_To_24_7_12_999 : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "RDM";
		public override string RoleName => "Warehouse RF";
		public override string ModuleName => "RFDeviceModels";
		public override string FunctionName => "RF Device Model Details";
		public override string FeatureName => "RF Device Model Details";
		public override string DataGranularity => RefStlItemGrain.MonthlyCurrentDataOnly;
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string GuidReference => "S7_PK";
		public override string BillingReference1 => "DeviceDetails";
		public override string BillingReference2 => "FirstFiftyDeviceID";
		public override string BillingReference3 => "FinalFiftyDeviceID";
		public override string BillingReference4 => "SHADeviceID";
		// Taking first 50 characters from SHA2_256 as SHA1 and similar will be deprecated in SQL Server 2016 onwards
		// Truncated hash should still be strong and sufficient for this use case: https://crypto.stackexchange.com/questions/161/should-i-use-the-first-or-last-bits-from-a-sha-256-hash/163#163
		public override string FromClause => @"
(
	SELECT
		stm.s7_DeviceDetails DeviceDetails,
		LEFT(stm.S7_DeviceID, 50) as FirstFiftyDeviceID,
		Right(stm.S7_DeviceID, 50) as FinalFiftyDeviceID,
		CONVERT(NVARCHAR(50), HASHBYTES('SHA2_256', stm.S7_DeviceID), 2) as SHADeviceID,
		STUFF(MIN(CONVERT(VARCHAR, S7_OpenDateTimeUtc, 126) + CONVERT(NVARCHAR(36), S7_PK)), 1, 19, '') AS S7_PK
   	FROM
		dbo.StmActivityLog stm
	WHERE
		stm.S7_DeviceID <> ''
		AND stm.s7_DeviceDetails <> ''
		AND stm.S7_FormCaption IN ('RFM', 'EMT' )
	GROUP BY
        S7_DeviceID,
		s7_DeviceDetails
) DeviceDetails";
		public override string WhereClause => string.Empty;
		public override bool UsedInBilling => false;
		public override string ActiveOn => "ALL";
		public override string MinCW1Version => "24.7.12.724"; // Minimum CW1 version that contains s7_DeviceDetails column
		public override string MaxCW1Version => "24.7.12.999";
		public override string PreparationScript => Constants.MonthAsDateVariableScript;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
	}
}
