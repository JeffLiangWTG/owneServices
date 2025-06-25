namespace CargoWise.Billing.Collectors.Logistics
{
	public class ElectronicBillOfLadingReceived : ElectronicBillLading
	{
		public override string FeatureCode => "EBR";

		public override string FeatureName => "Electronic Bill Of Lading Received";

		public override bool UsedInBilling => false;

		public override string EventReferenceType => "Original Bill Published";
	}
}
