using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DtbBookingRateLineConditionsSupporterTest : TestCaseWithFactory
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
			var transportBooking = supporter.ObjectToWrap as DtbBooking;

			AssertNotNull(transportBooking);
			AssertEquals(false, supporter.HasDangerousGoods);

			transportBooking.KM_IsHazardous = true;

			AssertEquals(true, supporter.HasDangerousGoods);
		}

		DtbBookingRateLineConditionsSupporter GetConditionsSupporter()
		{
			var transportBooking = Factory.New<DtbBooking>();
			return new DtbBookingRateLineConditionsSupporter(transportBooking);
		}
	}
}
