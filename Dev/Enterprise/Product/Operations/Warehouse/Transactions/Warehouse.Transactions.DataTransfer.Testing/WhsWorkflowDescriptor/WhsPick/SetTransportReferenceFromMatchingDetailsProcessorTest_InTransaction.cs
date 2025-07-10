using System;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class SetTransportReferenceFromMatchingDetailsProcessorTest_InTransaction : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GenerateTransportReferenceNumberProcessor(null));
		}

		#endregion

		#region TestProcess

		public void TestProcess()
		{
			SetTransportReferenceFromMatchingDetailsProcessorTestCore.TestProcess(Factory, Helper);
		}

		#endregion

		#region TestProcessRanking

		public void TestProcessRanking()
		{
			SetTransportReferenceFromMatchingDetailsProcessorTestCore.TestProcessRanking(Factory, Helper);
		}

		#endregion

		#region TestProcess_Error_InvalidOrder

		public void TestProcess_Error_InvalidOrder()
		{
			SetTransportReferenceFromMatchingDetailsProcessorTestCore.TestProcess_Error_InvalidOrder(Factory, Helper);
		}

		#endregion

		#region TestProcess_Error_NoNumberFountain

		public void TestProcess_Error_NoNumberFountain()
		{
			SetTransportReferenceFromMatchingDetailsProcessorTestCore.TestProcess_Error_NoNumberFountain(Factory, Helper);
		}

		#endregion

	}
}
