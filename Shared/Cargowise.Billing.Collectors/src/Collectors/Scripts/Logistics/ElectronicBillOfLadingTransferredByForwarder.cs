namespace CargoWise.Billing.Collectors.Logistics
{
	public class ElectronicBillOfLadingTransferredByForwarder : ElectronicBillLading
	{
		public override string FeatureCode => "EBT";

		public override string FeatureName => "Electronic Bill Of Lading Transferred by Forwarder";

		public override bool UsedInBilling => true;

		public override string EventReferenceType => "Original Bill Transferred";

		public override string EventReferenceOld => "Forwarder";
	}
}
