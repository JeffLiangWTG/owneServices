using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRunSheetInstructionDBConstraintsTest : DtbConsignmentTestCaseWithFactory
	{
		public void TestConstraint_K1_TimeIn_Require_K1_IsAcceptedByDriver()
		{
			var runSheet = Helper.CreateRunSheet();
			var runSheetInstruction = runSheet.RunSheetInstructions.AddNew();
			Factory.Save();

			AssertEquals("Pre-condition:", false, runSheetInstruction.K1_IsAcceptedByDriver);
			runSheetInstruction.K1_TimeIn = ZDateTimeOffset.Now;
			AssertExceptionThrown("Constraints should stop saving instruction with K1_TimeIn is not null and K1_IsAcceptedByDriver = false.", typeof(ZSaveException),
				() => Factory.Save());

			runSheetInstruction.K1_TimeIn = ZDateTimeOffset.Empty;
			runSheetInstruction.K1_IsAcceptedByDriver = true;
			Factory.Save();

			runSheetInstruction.K1_TimeIn = ZDateTimeOffset.Now;
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestConstraint_K1_TimeOut_Require_K1_TimeIn()
		{
			var runSheet = Helper.CreateRunSheet();
			var runSheetInstruction = runSheet.RunSheetInstructions.AddNew();
			runSheetInstruction.K1_IsAcceptedByDriver = true;
			Factory.Save();

			var now = ZDateTimeOffset.Now;

			AssertEquals("Pre-condition:", true, runSheetInstruction.K1_TimeIn.IsEmpty);
			runSheetInstruction.K1_TimeOut = ZDateTimeOffset.Now;
			AssertExceptionThrown("Constraints should stop saving instruction with K1_TimeOut is not null and K1_TimeIn = null.", typeof(ZSaveException),
				() => Factory.Save());

			runSheetInstruction.K1_TimeIn = now.AddHours(1);
			runSheetInstruction.K1_TimeOut = ZDateTimeOffset.Empty;
			Factory.Save();
			AssertEquals("Pre-condition: K1_TimeIn has value.", false, new BusinessObjectFactory().Load<DtbConsignmentRunSheetInstruction>(runSheetInstruction.PK).K1_TimeIn.IsEmpty);
			AssertEquals("Pre-condition: K1_TimeOut is empty.", true, new BusinessObjectFactory().Load<DtbConsignmentRunSheetInstruction>(runSheetInstruction.PK).K1_TimeOut.IsEmpty);

			runSheetInstruction.K1_TimeOut = ZDateTimeOffset.Now;
			AssertExceptionThrown("Constraints should stop saving instruction with K1_TimeOut is earlier than K1_TimeIn.", typeof(ZSaveException),
				() => Factory.Save());

			runSheetInstruction.K1_TimeIn = now.AddHours(-1);
			runSheetInstruction.K1_TimeOut = ZDateTimeOffset.Empty;
			Factory.Save();

			runSheetInstruction.K1_TimeOut = ZDateTimeOffset.Now;
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestConstraint_K1_FailureNotes_Require_K1_FailureReason()
		{
			var runSheet = Helper.CreateRunSheet();
			var runSheetInstruction = runSheet.RunSheetInstructions.AddNew();
			Factory.Save();

			AssertEquals("Pre-condition:", true, runSheetInstruction.K1_FailureReason.IsEmpty);
			runSheetInstruction.K1_FailureNotes = "I have notes.";
			AssertExceptionThrown("Constraints should stop saving instruction with K1_FailureReason is '' and K1_FailureNotes <> ''.", typeof(ZSaveException),
				() => Factory.Save());

			runSheetInstruction.K1_FailureReason = "OTH";
			runSheetInstruction.K1_FailureNotes = "";
			Factory.Save();

			runSheetInstruction.K1_FailureNotes = "I have notes.";
			AssertNoExceptionThrown(() => Factory.Save());
		}
	}
}
