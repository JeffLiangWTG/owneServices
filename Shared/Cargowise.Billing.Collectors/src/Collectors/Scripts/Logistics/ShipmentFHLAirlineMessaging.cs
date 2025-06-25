namespace CargoWise.Billing.Collectors.Logistics
{
	public class ShipmentFHLAirlineMessaging : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "FH1";
		public override string RoleName => "Shipment FHL Airline Messaging";
		public override string ModuleName => "International Logistics";
		public override string FunctionName => "FHL Airline Messaging";
		public override string FeatureName => "Shipment FHL Airline Messaging ";
		public override string GuidReference => "s.JS_PK";
		public override string BillingReference1 => "s.JS_UniqueConsignRef";
		public override string BillingReference2 => "s.JS_HouseBill";
		public override string BranchCode => "s.BranchCode";
		public override string TransactionDateUtc => "s.MessageSendTime";
		public override string FromClause => @"(SELECT 
										JobShipment.JS_PK,
										JobShipment.JS_UniqueConsignRef,
										JobShipment.JS_HouseBill,
										GlbBranch.GB_Code AS BranchCode,
										EDIMessage.EM_SystemCreateTimeUtc AS MessageSendTime
									FROM dbo.JobShipment
									LEFT JOIN dbo.EDIMessage ON EM_ApplicationReference = JS_UniqueConsignRef
									JOIN dbo.GlbBranch on EM_GB = GB_PK
									WHERE EM_ApplicationCode = 'CIM' AND EM_MessageType = 'FHL' AND EM_ReceiveTransmit = 'TRX' AND EM_LinkTable = 'JobConsol') as s";
		public override string WhereClause => string.Empty;
		public override string CompanyCode => string.Empty;
		public override bool UsedInBilling => true;
	}
}
