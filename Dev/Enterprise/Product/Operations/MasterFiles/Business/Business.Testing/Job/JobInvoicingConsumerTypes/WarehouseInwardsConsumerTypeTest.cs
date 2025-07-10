using System;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WarehouseInwardsConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.WarehouseInwards;
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceWarehouse; }
		}

		public override void TestExcludeFromClientVisibleOption()
		{
			AssertEquals("Receives should not have ExcludeFromClientVisibleOption available", true, ConsumerType.ExcludeFromClientVisibleOption);
		}

		protected override void TestShouldExcludeFromPeriodicBillingByDefaultCore()
		{
			using (RatingDataRegistry.Instance.ExcludeInwardsFromPeriodicAutoRating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, ConsumerType.ShouldExcludeFromPeriodicBillingByDefault);
			}
			using (RatingDataRegistry.Instance.ExcludeInwardsFromPeriodicAutoRating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, ConsumerType.ShouldExcludeFromPeriodicBillingByDefault);
			}
		}
	}
}
