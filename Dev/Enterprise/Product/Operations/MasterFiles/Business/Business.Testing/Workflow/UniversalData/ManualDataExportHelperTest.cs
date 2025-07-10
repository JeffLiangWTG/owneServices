using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Business.UniversalData.Testing
{
	public sealed class ManualDataExportHelperTest : TestCaseWithFactory
	{
		public void TestGetMessage_Success()
		{
			var status = EDIMessageStatusList.Codes.ProcessedOK;
			var expectType = MessageTypes.Success;
			var expectResult = "Instruction successfully updated related job.";

			TestGetMessageCore(status, expectType, expectResult);
		}

		public void TestGetMessage_Error()
		{
			var status = EDIMessageStatusList.Codes.Discarded;
			var expectType = MessageTypes.Error;
			var expectResult = "Instruction had an issue while processing (DCD - Discarded). Check DEX logs for details.";

			TestGetMessageCore(status, expectType, expectResult);
		}

		public void TestGetMessage_Warning()
		{
			var status = EDIMessageStatusList.Codes.Pending;
			var expectType = MessageTypes.Warning;
			var expectResult = "Instruction has been queued to send. Check DEX logs for details.";

			TestGetMessageCore(status, expectType, expectResult);
		}

		void TestGetMessageCore(string status, string expectType, string expectMessage)
		{
			var result = ManualDataExportHelper.GetMessage(status, "Instruction", "job");
			AssertEquals(expectType, result.ErrorType);
			AssertEquals(expectMessage, result.Message);
		}
	}
}
