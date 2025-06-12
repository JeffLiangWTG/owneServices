namespace CargoWise.eServices.Billing.WcfService.HealthCheck
{
	public class ProcessingBillingDatabaseUpdateBillingCubePartialHealthCheckItemProvider : ProcessingBillingDbHealthCheckItemProviderBase
	{
		protected override string JobMethodName => "RecurringProcessUpdateBillingCubePartial";
		public override string JobId => "ProcessBillingDatabaseUpdateBillingCubePartial";
	}
}
