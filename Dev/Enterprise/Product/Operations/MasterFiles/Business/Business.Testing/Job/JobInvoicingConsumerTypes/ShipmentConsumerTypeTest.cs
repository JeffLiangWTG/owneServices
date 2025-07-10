using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ShipmentConsumerTypeTest : BaseShipmentConsumerTypeTest
	{
		public override void TestShouldDisplayClientContractNumber()
		{
			AssertShouldDisplayClientContractNumber(ChargeableFactorSource.TransportBooking, isBookingWithQuote: true, shouldDisplayClientContractNumber: false);
			AssertShouldDisplayClientContractNumber(ChargeableFactorSource.TransportBooking, isBookingWithQuote: false, shouldDisplayClientContractNumber: false);
			AssertShouldDisplayClientContractNumber(ChargeableFactorSource.International, isBookingWithQuote: true, shouldDisplayClientContractNumber: false);
			AssertShouldDisplayClientContractNumber(ChargeableFactorSource.International, isBookingWithQuote: false, shouldDisplayClientContractNumber: true);

			base.TestShouldDisplayClientContractNumber();
		}

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType() => JobInvoicingConsumerTypes.Shipment;

		void AssertShouldDisplayClientContractNumber(ChargeableFactorSource chargeableFactorSource, bool isBookingWithQuote, bool shouldDisplayClientContractNumber)
		{
			var jobInvoicingSupporter = CreateJobInvoicingSupporterWithChargeableFactorSource(chargeableFactorSource, isBookingWithQuote);
			var job = CreateJobInvoicingPlugin(jobInvoicingSupporter);
			AssertEquals(shouldDisplayClientContractNumber, ConsumerType.ShouldDisplayClientContractNumber(job));
		}

		IJobInvoicingSupporter CreateJobInvoicingSupporterWithChargeableFactorSource(ChargeableFactorSource chargeableFactorSource, bool isBookingWithQuote)
		{
			var jobInvoicingSupporterWithChargeableFactorSource = new Mock<IJobInvoicingSupporterWithChargeableFactorSource>();
			jobInvoicingSupporterWithChargeableFactorSource.Setup(x => x.ChargeableFactorSource).Returns(chargeableFactorSource);
			jobInvoicingSupporterWithChargeableFactorSource.Setup(x => x.IsBookingWithQuote).Returns(isBookingWithQuote);
			return jobInvoicingSupporterWithChargeableFactorSource.Object;
		}
	}
}
