namespace CargoWise.eServices.Billing.WcfService.HealthCheck
{
	public class ProcessingBillingDatabaseMonthlyAggregationHealthCheckItemProvider : ProcessingBillingDbHealthCheckItemProviderBase
	{
		protected override string JobMethodName => "RecurringProcessMonthlyAggregation";
		public override string JobId => "ProcessBillingDatabaseMonthlyAggregation";
	}
}
