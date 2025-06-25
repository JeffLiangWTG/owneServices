namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	sealed public class ContainerLoadListLineUsage : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "CLL";
		public override string ModuleName => "Order Manager";
		public override string RoleName => "Order Manager Usage";
		public override string FunctionName => "Container Load List Line Usage Collectors";
		public override string FeatureName => "Order Manager Container Load List Line Usage Collector";
		public override string TransactionDateUtc => "log.SL_PostedTimeUtc";
		public override string BillingReference1 => "containerLoadListHeader.CLH_LoadListId";
		public override string BillingReference2 => "supplierBookingLine.JSL_BookingLineId";
		public override string BillingReference3 => "container.JC_ContainerNum";
		public override string GuidReference => "containerLoadListLine.CLL_PK";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					StmALog log
					JOIN dbo.ContainerLoadListLine containerLoadListLine ON containerLoadListLine.CLL_PK = log.SL_Parent
					LEFT JOIN dbo.ContainerLoadListHeader containerLoadListHeader ON containerLoadListHeader.CLH_PK = containerLoadListLine.CLL_CLH_LoadListHeader
					LEFT JOIN dbo.JobContainer container ON container.JC_PK = containerLoadListLine.CLL_JC_Container
					LEFT JOIN dbo.JobSupplierBookingLine supplierBookingLine ON supplierBookingLine.JSL_PK = containerLoadListLine.CLL_JSL_BookingLine
					LEFT JOIN dbo.GlbBranch globalBranch ON globalBranch.GB_Code = log.SL_GB_NKBranch
					LEFT JOIN dbo.GlbCompany globalCompany ON globalCompany.GC_PK = globalBranch.GB_GC";
		public override string WhereClause => "log.SL_SE_NKEvent = 'ADD' AND containerLoadListLine.CLL_LoadMode = 'CY'";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string DateType => RefStlDateType.DateTime;
		public override string CreatingUserCode => "log.SL_GS_NKUser";
		public override string BranchCode => "globalBranch.GB_Code";
		public override string CompanyCode => "globalCompany.GC_Code";
	}

	#endregion
}
