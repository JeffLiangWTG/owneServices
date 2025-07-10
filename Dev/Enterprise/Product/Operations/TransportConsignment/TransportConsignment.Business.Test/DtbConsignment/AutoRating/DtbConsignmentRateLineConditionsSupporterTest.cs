using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRateLineConditionsSupporterTest : TestCaseWithFactory
	{
		#region TestExportBroker

		public void TestExportBroker()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.ExportBroker);
		}

		#endregion

		#region TestImportBroker

		public void TestImportBroker()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.ImportBroker);
		}

		#endregion

		#region TestSendingAgent

		public void TestSendingAgent()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.SendingAgent);
		}

		#endregion

		#region TestReceivingAgent

		public void TestReceivingAgent()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.ReceivingAgent);
		}

		#endregion

		#region TestControllingAgent

		public void TestControllingAgent()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.ControllingAgent);
		}

		#endregion

		#region TestDepartureCFS

		public void TestDepartureCFS()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.DepartureCFS);
		}

		#endregion

		#region TestArrivalCFS

		public void TestArrivalCFS()
		{
			var conditionSupporter = GetConditionsSupporter();
			AssertNull(conditionSupporter.ArrivalCFS);
		}

		#endregion

		#region TestHasDangerousGoods

		public void TestHasDangerousGoods()
		{
			var supporter = GetConditionsSupporter();
			var consignment = supporter.ObjectToWrap as DtbConsignment;

			AssertNotNull(consignment);
			AssertEquals(false, supporter.HasDangerousGoods);

			consignment.LTC_IsHazardous = true;
			AssertEquals(true, supporter.HasDangerousGoods);
		}

		#endregion

		#region Implementation

		DtbConsignmentRateLineConditionsSupporter GetConditionsSupporter()
		{
			return new DtbConsignmentRateLineConditionsSupporter(Factory.New<DtbConsignment>());
		}

		#endregion
	}
}
