namespace CargoWise.Billing.Collectors.Warehouse
{
	#region SuppressResourceStringsCheckRegion

	public class WinCEDeviceApplicationUsageCollector : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "RF3";
		public override string RoleName => "WinCE RF";
		public override string ModuleName => "WinCE RF Application Usage";
		public override string FunctionName => "WinCE RF Application Device Usage";
		public override string FeatureName => "WinCE RF Application Device Usage";
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string GuidReference => "ActivityLog.S7_PK";
		public override string BillingReference1 => "FirstFiftyDeviceID";
		public override string BillingReference2 => "SHADeviceID";
		public override string BillingReference3 => "ApplicationCode";
		public override string BillingReference4 => "S7_PK";

		// Taking first 50 characters from SHA2_256 as SHA1 and similar will be deprecated in SQL Server 2016 onwards
		// Truncated hash should still be strong and sufficient for this use case: https://crypto.stackexchange.com/questions/161/should-i-use-the-first-or-last-bits-from-a-sha-256-hash/163#163
		public override string FromClause => @$"
(
		SELECT
			LEFT(S7_DeviceID, 50) AS FirstFiftyDeviceID,
			CONVERT(NVARCHAR(50), HASHBYTES('SHA2_256', S7_DeviceID), 2) AS SHADeviceID,
			dbo.CLRConcatenateAgg(DISTINCT S7_FormCaption, ', ', 0) as ApplicationCode,
			STUFF(MIN(CONVERT(VARCHAR, S7_OpenDateTimeUtc, 126) + CONVERT(NVARCHAR(36), S7_PK)), 1, 19, '') AS S7_PK
		FROM 
			dbo.StmActivityLog stm
		WHERE
			stm.S7_DeviceID <> ''
			AND stm.S7_DeviceID NOT LIKE 'AndroidID - %'
			AND stm.S7_FormCaption IN ('RFM', 'EMT' )
			AND stm.S7_OpenDateTimeUTC >= {Constants.StartDateTimeInclusiveParamName}
			AND stm.S7_OpenDateTimeUTC <  {Constants.EndDateTimeExclusiveParamName}
		GROUP BY
			S7_DeviceID
) ActivityLog";

		public override string WhereClause => string.Empty;
		public override bool UsedInBilling => false;
		public override string ActiveOn => "ALL";
		public override string MinCW1Version => "22.3.17.112"; // Minimum CW1 version that contains s7_DeviceID column
		public override string PreparationScript => Constants.MonthAsDateVariableScript;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
	}

	#endregion
}
