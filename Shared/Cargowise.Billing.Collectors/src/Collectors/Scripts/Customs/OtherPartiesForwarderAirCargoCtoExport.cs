namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class OtherPartiesForwarderAirCargoCtoExport : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "CTE";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs Messaging (non Broker Agent)";
		public override string FunctionName => "Forwarder/CFS/CTO Functions";
		public override string FeatureName => "Import/Export Air Cargo CFS/CTO Customs Functions (Export)";
		public override string TransactionDateUtc => "ed.ED_SystemCreateTimeUtc";
		public override string BillingReference1 => "'REF: ' + ed.ED_BGMReference";
		public override string BillingReference2 => "ed.ED_ManifestType";
		public override string GuidReference => "ed.ED_PK";
		public override string CreatingUserCode => "ed.ED_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					dbo.ExportCustomsManifestHeader ed
					INNER JOIN dbo.ExportCustomsManifestLines el on el.EL_ED = ed.ED_PK";
		public override string WhereClause => @"ed.ED_TransportMode = 'AIR'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "22.12.8.541";
	}
	#endregion
}
