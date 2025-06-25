namespace CargoWise.Billing.Collectors.Logistics
{
	public class ElectronicHouseBillOfLadingPublished : ElectronicHouseBillOfLading
	{
		public override string FeatureCode => "EHR";

		public override string FeatureName => "Electronic House Bill Of Lading Published";

		public override bool UsedInBilling => true;

		public override string EventReferenceType => "Original Bill Published";
	}
}
