using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class TransportRateLineConditionsSupporterTest : TestCaseWithFactory
	{
		public void TestExportBroker()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.ExportBroker);
		}

		public void TestImportBroker()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.ImportBroker);
		}

		public void TestSendingAgent()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.SendingAgent);
		}

		public void TestReceivingAgent()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.ReceivingAgent);
		}

		public void TestControllingAgent()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.ControllingAgent);
		}

		public void TestDepartureCFS()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.DepartureCFS);
		}

		public void TestArrivalCFS()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.ArrivalCFS);
		}

		public void TestHasDangerousGoods()
		{
			var supporter = GetConditionsSupporter();
			var transportBooking = supporter.ObjectToWrap as DtbTransport;

			AssertNotNull(transportBooking);
			AssertEquals(false, supporter.HasDangerousGoods);

			transportBooking.KM_IsHazardous = true;

			AssertEquals(true, supporter.HasDangerousGoods);
		}

		protected abstract TransportRateLineConditionsSupporter GetConditionsSupporter();
	}
}
