using System;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class WarehouseOrdersConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType() => JobInvoicingConsumerTypes.WarehouseOutwards;
		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceWarehouse;
		public override void TestExcludeFromClientVisibleOption() => AssertEquals("Orders should not have ExcludeFromClientVisibleOption available", true, ConsumerType.ExcludeFromClientVisibleOption);
		protected override void TestShouldExcludeFromPeriodicBillingByDefaultCore()
		{
			using (RatingDataRegistry.Instance.ExcludeOrdersFromPeriodicAutoRating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, ConsumerType.ShouldExcludeFromPeriodicBillingByDefault);
			}
			using (RatingDataRegistry.Instance.ExcludeOrdersFromPeriodicAutoRating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, ConsumerType.ShouldExcludeFromPeriodicBillingByDefault);
			}
		}
	}
}
