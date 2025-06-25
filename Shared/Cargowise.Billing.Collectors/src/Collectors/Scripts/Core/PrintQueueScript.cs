namespace CargoWise.Billing.Collectors.Core
{
	#region SuppressResourceStringsCheckRegion

	public class PrintQueueScript : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "PRT";
		public override string RoleName => "Hosting";
		public override string ModuleName => "WiseCloud";
		public override string FunctionName => "Data Storage";
		public override string FeatureName => "Printer";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
(
	SELECT CASE WHEN SQ_DisplayName <> '' THEN SQ_DisplayName ELSE SQ_QueueName END Name,
	ROW_NUMBER() OVER(PARTITION BY CASE WHEN SQ_DisplayName <> '' THEN SQ_DisplayName ELSE SQ_QueueName END ORDER BY SQ_PK) RowNumber,
	SQ_PK
	FROM dbo.StmPrintQueue
	WHERE SQ_AllowPrinting = 1
) Printer";
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string BillingReference1 => "Printer.Name";
		public override string WhereClause => "Printer.Name <> '' AND Printer.RowNumber = 1";
		public override string GuidReference => "Printer.SQ_PK";
		public override string DataGranularity => RefStlItemGrain.MonthlyCurrentDataOnly;
		public override string PreparationScript => Constants.MonthAsDateVariableScript;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "21.6.22.0";
	}

	#endregion // SuppressResourceStringsCheckRegion
}
