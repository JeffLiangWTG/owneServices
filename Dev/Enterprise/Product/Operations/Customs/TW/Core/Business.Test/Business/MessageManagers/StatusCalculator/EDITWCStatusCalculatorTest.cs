using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.MessageManagers;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class EDITWCStatusCalculatorTest : TestCaseWithFactory
	{
		public void TestGetMessageAcknowledgedStatus()
		{
			var entry = Factory.New<CusEntryHeader>();
			var message = Factory.New<TWMessage>();
			message.EM_LinkedObject = entry;
			var calculator = new EDITWCStatusCalculator(message);
			entry.CH_Status = "AWO";
			AssertEquals("ACO", calculator.GetMessageAcknowledgedStatus());
			entry.CH_Status = "AWC";
			AssertEquals("ACC", calculator.GetMessageAcknowledgedStatus());
			entry.CH_Status = "AWG";
			AssertEquals("ACG", calculator.GetMessageAcknowledgedStatus());
			entry.CH_Status = "AWE";
			AssertEquals("ACE", calculator.GetMessageAcknowledgedStatus());
		}

		public void TestGetMessageRejectedStatus()
		{
			var entry = Factory.New<CusEntryHeader>();
			var message = Factory.New<TWMessage>();
			message.EM_LinkedObject = entry;
			var calculator = new EDITWCStatusCalculator(message);
			entry.CH_Status = "AWO";
			AssertEquals("ERO", calculator.GetMessageRejectedStatus());
			entry.CH_Status = "AWC";
			AssertEquals("ERC", calculator.GetMessageRejectedStatus());
			entry.CH_Status = "AWG";
			AssertEquals("ERG", calculator.GetMessageRejectedStatus());
			entry.CH_Status = "AWE";
			AssertEquals("ERE", calculator.GetMessageRejectedStatus());
		}

		public void TestGetMessageAwaitingStatus()
		{
			var entry = Factory.New<CusEntryHeader>();
			var message = Factory.New<TWMessage>();
			message.EM_LinkedObject = entry;
			var calculator = new EDITWCStatusCalculator(message);
			message.EM_MessageType = "ICD";
			AssertEquals("AWO", calculator.GetMessageAwaitingStatus("9"));
			AssertEquals("AWC", calculator.GetMessageAwaitingStatus("5"));
			message.EM_MessageType = "ECD";
			AssertEquals("AWO", calculator.GetMessageAwaitingStatus("9"));
			AssertEquals("AWC", calculator.GetMessageAwaitingStatus("5"));
			message.EM_MessageType = "ADM";
			AssertEquals("AWE", calculator.GetMessageAwaitingStatus("9"));
			AssertEquals("AWE", calculator.GetMessageAwaitingStatus("5"));
			message.EM_MessageType = "IEA";
			AssertEquals("AWG", calculator.GetMessageAwaitingStatus("9"));
			AssertEquals("AWG", calculator.GetMessageAwaitingStatus("5"));
		}
	}
}
