namespace CargoWise.Billing.Collectors.Marketing
{
	#region SuppressResourceStringsCheckRegion
	public class SalesCommunicationManager : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string FeatureCode => "CMC";
		public override string RoleName => "CRM / Sales Management / HRM";
		public override string ModuleName => "Sales and Marketing System";
		public override string FunctionName => "Sales and Marketing Power Functions";
		public override string FeatureName => "Communication: Create - Success";
		public override string TransactionDateUtc => "c.OQ_SystemCreateTimeUtc";
		public override string CreatingUserCode => "c.OQ_SystemCreateUser";
		public override string GuidReference => "c.OQ_PK";
		public override string BillingReference1 => "'CommunicationId=' + c.OQ_CommunicationID";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), (SELECT
	CommunicationMethod = c.OQ_TypeOfCall,
	Purpose = c.OQ_Category,
	RelatedActivityCount = (
		select count(*) from dbo.RelatedActivityPivot
		where (RAP_ChildActivityTableCode = 'OQ' AND RAP_ChildActivityID = c.OQ_PK)
		or (RAP_ParentActivityTableCode = 'OQ' AND RAP_ParentActivityID = c.OQ_PK)),
	HasPContact = CASE WHEN c.OQ_OC is not null THEN 'Y' ELSE 'N' END,
	AttndCount = (select count(*) from dbo.OrgSalesCallAdditionalAttendee a where a.O6_OQ = c.OQ_PK),
	HasNotes = CASE WHEN
		OQ_SalesCallNotes <> ''
		OR OQ_FollowupNotes <> ''
		OR EXISTS (SELECT NULL FROM dbo.StmNote WHERE ST_ParentID = c.OQ_PK AND ST_Table = 'OrgSalesCall'AND ST_NoteType = 'DOC')
		THEN 'Y' ELSE 'N' END
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string ActiveOn => "ALL";
		public override string FromClause => "OrgSalesCall c";
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string WhereClause => string.Empty;
	}
	#endregion // SuppressResourceStringsCheckRegion
}
