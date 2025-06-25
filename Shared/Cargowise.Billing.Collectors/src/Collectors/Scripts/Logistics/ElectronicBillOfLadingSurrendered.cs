namespace CargoWise.Billing.Collectors.Logistics
{
	public class ElectronicBillOfLadingSurrendered : ElectronicBillLading
	{
		public override string FeatureCode => "EBS";

		public override string FeatureName => "Electronic Bill Of Lading Surrendered";

		public override bool UsedInBilling => false;

		public override string EventReferenceType => "Surrendered";
	}
}
