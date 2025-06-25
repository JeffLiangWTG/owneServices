namespace CargoWise.Billing.Collectors.Warehouse
{
	#region SuppressResourceStringsCheckRegion

	public class WinCEDeviceUsageCollector : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "RF2";
		public override string RoleName => "WinCE RF";
		public override string ModuleName => "WinCE RF Applications";
		public override string FunctionName => "WinCE RF Device Usage";
		public override string FeatureName => "WinCE RF Device Usage";
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string GuidReference => "ActivityLog.S7_PK";
		public override string BillingReference1 => "FirstFiftyDeviceID";
		public override string BillingReference2 => "FinalFiftyDeviceID";
		public override string BillingReference3 => "Branches";
		public override string BillingReference4 => "SHADeviceID";

		// Taking first 50 characters from SHA2_256 as SHA1 and similar will be deprecated in SQL Server 2016 onwards
		// Truncated hash should still be strong and sufficient for this use case: https://crypto.stackexchange.com/questions/161/should-i-use-the-first-or-last-bits-from-a-sha-256-hash/163#163
		public override string FromClause => @"
(
	SELECT
		LEFT(stm.S7_DeviceID, 50) as FirstFiftyDeviceID,
		Right(stm.S7_DeviceID, 50) as FinalFiftyDeviceID,
		CONVERT(NVARCHAR(50), HASHBYTES('SHA2_256', stm.S7_DeviceID), 2) as SHADeviceID,
		ROW_NUMBER() OVER(PARTITION BY stm.S7_DeviceID ORDER BY stm.S7_OpenDateTimeUTC) RowNumber,
		branches,
		S7_PK
	FROM
		dbo.StmActivityLog stm
		JOIN
		(
			SELECT S7_DeviceID,
			LEFT(dbo.CLRConcatenateAgg(DISTINCT GB_Code, ',', 0), 1000) as branches
			FROM
				dbo.StmActivityLog
				JOIN dbo.GlbBranch on GB_PK = S7_ParentID and S7_DeviceID <> ''
			GROUP BY S7_DeviceID
		) as branches ON branches.S7_DeviceID = stm.S7_DeviceID
	WHERE
		stm.S7_DeviceID <> ''
		AND stm.S7_DeviceID NOT LIKE 'AndroidID - %'
		AND stm.S7_FormCaption IN ('RFM', 'EMT' )
) ActivityLog";
		public override string WhereClause => "ActivityLog.RowNumber = 1";
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string MinCW1Version => "22.3.17.112"; // Minimum CW1 version that contains s7_DeviceID column
		public override string PreparationScript => Constants.MonthAsDateVariableScript;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
	}

	#endregion
}
