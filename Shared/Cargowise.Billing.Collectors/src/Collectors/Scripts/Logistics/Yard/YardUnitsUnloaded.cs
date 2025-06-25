namespace CargoWise.Billing.Collectors.Scripts.Logistics.Yard
{

	#region SuppressResourceStringsCheckRegion

	public class YardUnitsUnloaded : RefStlScriptWithDefaults
	{
		public override string MinCW1Version => "24.11.6.411";

		public override bool UsedInBilling => true;
		public override string FeatureCode => "YUU";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Yard";
		public override string FunctionName => "Get unloaded yard units count";
		public override string FeatureName => "Get unloaded yard units count";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "ytu.YTU_GateInTime";
		public override string BillingReference1 => "yus.YUS_UnitID";
		public override string BillingReference2 => "yus.YUS_YTU_ReceiveTransportationUnit";
		public override string BillingReference3 => "yus.YUS_WW_CurrentYard";
		public override string GuidReference => "yus.YUS_PK";
		public override string CreatingUserCode => "ytu.YTU_SystemLastEditUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
			dbo.CYDYardUnitState yus 
			JOIN dbo.CYDTransportationUnit ytu on yus.YUS_YTU_ReceiveTransportationUnit = ytu.YTU_PK
			JOIN dbo.WhsWarehouse ww on ww.WW_PK = yus.YUS_WW_CurrentYard
			JOIN dbo.GlbBranch gb ON gb.GB_PK = ww.WW_GB_RelatedCompanyBranch
			JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => "yus.YUS_UnloadTime IS NOT NULL";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTimeOffset;
	}

	#endregion
}
