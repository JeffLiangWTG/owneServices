namespace CargoWise.Billing.Collectors.Customs
{
	public class DeliveryOrderCountScript : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "IDO";

		public override string FeatureName => "Israel Import Delivery Order";

		public override string TransactionDateUtc => "EM_SystemCreateTimeUtc";

		public override string GuidReference => "EM_PK";

		public override string FromClause => @"EDIMessage
INNER JOIN JobShipment ON EM_LinkTable = 'JobShipment' AND EM_LinkUniqueID = JS_PK
INNER JOIN GlbBranch on GB_PK = EM_GB 
INNER JOIN GlbCompany on GC_PK = GB_GC
LEFT JOIN CusEntryNum ON CE_ParentTable = 'JobShipment' AND CE_ParentID = JS_PK AND CE_EntryType = 'DLO'
					";

		public override string RoleName => "Customs & Country Specific Integrations";

		public override string ModuleName => "Other Parties Customs and Port Messaging";

		public override string FunctionName => "Forwarder/CFS/CTO Functions";

		public override string CompanyCode => "GC_Code";

		public override string BranchCode => "GB_Code";

		public override string BillingReference1 => "'SHP:' + JS_UniqueConsignRef";
		public override string BillingReference2 => "'ORD:' + CE_EntryNum";

		public override string WhereClause => @"EM_ApplicationCode = 'ILC' AND
EM_MessageType = 'DLO' AND
EM_ReceiveTransmit = 'TRX' AND
EM_Status = 'PRS'";
	}
}
