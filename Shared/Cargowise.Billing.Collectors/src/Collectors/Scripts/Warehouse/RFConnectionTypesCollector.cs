namespace CargoWise.Billing.Collectors.Warehouse
{
	#region SuppressResourceStringsCheckRegion
	public class RFConnectionTypesCollector : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "RDC";
		public override string RoleName => "Warehouse RF";
		public override string ModuleName => "RFDeviceConnections";
		public override string FunctionName => "RF Device Connection Types Usage";
		public override string FeatureName => "RF Device Connection Types Usage";
		public override string DataGranularity => RefStlItemGrain.MonthlyCurrentDataOnly;
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string GuidReference => "ConnectionLog.S7_PK";
		public override string BillingReference1 => "CAST(ISNULL(HttpsConnections * 100.0 / NULLIF(HttpsConnections + HttpConnections, 0), 0) AS DECIMAL(5, 2))";
		public override string BillingReference2 => "IsPatchApplied";
		public override string BillingReference3 => "FirstFiftyDeviceID";
		public override string BillingReference4 => "SHADeviceID";
		// Taking first 50 characters from SHA2_256 as SHA1 and similar will be deprecated in SQL Server 2016 onwards
		// Truncated hash should still be strong and sufficient for this use case: https://crypto.stackexchange.com/questions/161/should-i-use-the-first-or-last-bits-from-a-sha-256-hash/163#163
		public override string FromClause => @$"
(
	SELECT
		LEFT(S7_DeviceID, 50) AS FirstFiftyDeviceID,
		CONVERT(NVARCHAR(50), HASHBYTES('SHA2_256', S7_DeviceID), 2) AS SHADeviceID,
		CASE WHEN SUM(IsUndefinedConnection) > 0 THEN 0 ELSE 1 END IsPatchApplied,
		SUM(HttpConnections) HttpConnections,
		SUM(HttpsConnections) HttpsConnections,
		STUFF(MIN(CONVERT(VARCHAR, S7_OpenDateTimeUtc, 126) + CONVERT(NVARCHAR(36), S7_PK)), 1, 19, '') AS S7_PK
	FROM 
		dbo.StmActivityLog stm
		CROSS APPLY
		(
			SELECT
				CASE WHEN S7_Keystrokes = 1 THEN 1 ELSE 0 END HttpConnections,
				CASE WHEN S7_Keystrokes = 2 THEN 1 ELSE 0 END HttpsConnections,
				CASE WHEN S7_Keystrokes < 1 OR S7_KeyStrokes > 2 THEN 1 ELSE 0 END IsUndefinedConnection
		) Connections
	WHERE
		stm.S7_DeviceID <> ''
		AND stm.S7_FormCaption IN ('RFM', 'EMT' )
		AND stm.S7_OpenDateTimeUTC >= {Constants.StartDateTimeInclusiveParamName}
		AND stm.S7_OpenDateTimeUTC < {Constants.EndDateTimeExclusiveParamName}
	GROUP BY
		S7_DeviceID
) ConnectionLog";
		public override string WhereClause => string.Empty;
		public override bool UsedInBilling => false;
		public override string ActiveOn => "ALL";
		public override string PreparationScript => Constants.MonthAsDateVariableScript;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
	}
	#endregion
}
