namespace CargoWise.Billing.Collectors.Core
{
	#region SuppressResourceStringsCheckRegion

	public class PrintServerScript : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "PRS";
		public override string RoleName => "Hosting";
		public override string ModuleName => "WiseCloud";
		public override string FunctionName => "Data Storage";
		public override string FeatureName => "Print Server";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
(
	SELECT DISTINCT ps.SPS_ServerName SPS_ServerName, ps.SPS_PK SPS_PK
	FROM dbo.StmPrintQueue pq
	JOIN dbo.StmPrintServer ps ON ps.SPS_PK = pq.SQ_SPS_Server
	WHERE pq.SQ_AllowPrinting = 1
) PrintServer";
		public override string BillingReference1 => "PrintServer.SPS_ServerName";
		public override string GuidReference => "PrintServer.SPS_PK";
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string DataGranularity => RefStlItemGrain.MonthlyCurrentDataOnly;
		public override string PreparationScript => Constants.MonthAsDateVariableScript;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string WhereClause => string.Empty;
		public override string MinCW1Version => "21.6.22.0";
	}

	#endregion // SuppressResourceStringsCheckRegion
}
