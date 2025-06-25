namespace CargoWise.Billing.Collectors.Logistics
{
	public class ElectronicHouseBillOfLadingSwitchedToPaper : ElectronicHouseBillOfLading
	{
		public override string FeatureCode => "EHP";

		public override string FeatureName => "Electronic House Bill Of Lading Switched To Paper";

		public override bool UsedInBilling => false;

		public override string EventReferenceType => "Switched To Paper";
	}
}
