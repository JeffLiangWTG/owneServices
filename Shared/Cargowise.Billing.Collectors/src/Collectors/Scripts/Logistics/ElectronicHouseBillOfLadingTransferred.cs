namespace CargoWise.Billing.Collectors.Logistics
{
	public class ElectronicHouseBillOfLadingTransferred : ElectronicHouseBillOfLading
	{
		public override string FeatureCode => "EHT";

		public override string FeatureName => "Electronic House Bill Of Lading Transferred";

		public override bool UsedInBilling => false;

		public override string EventReferenceType => "Original Bill Transferred";
	}
}
