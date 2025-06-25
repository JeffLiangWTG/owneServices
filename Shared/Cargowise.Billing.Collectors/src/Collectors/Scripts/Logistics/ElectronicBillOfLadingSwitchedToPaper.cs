namespace CargoWise.Billing.Collectors.Logistics
{
	public class ElectronicBillOfLadingSwitchedToPaper : ElectronicBillLading
	{
		public override string FeatureCode => "EBP";

		public override string FeatureName => "Electronic Bill Of Lading Switched To Paper";

		public override bool UsedInBilling => false;

		public override string EventReferenceType => "Switched To Paper";
	}
}
