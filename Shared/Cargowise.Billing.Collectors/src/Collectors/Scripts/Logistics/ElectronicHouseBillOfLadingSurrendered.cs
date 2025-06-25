namespace CargoWise.Billing.Collectors.Logistics
{
	public class ElectronicHouseBillOfLadingSurrendered : ElectronicHouseBillOfLading
	{
		public override string FeatureCode => "EHS";

		public override string FeatureName => "Electronic House Bill Of Lading Surrendered";

		public override bool UsedInBilling => false;

		public override string EventReferenceType => "Surrendered";
	}
}
