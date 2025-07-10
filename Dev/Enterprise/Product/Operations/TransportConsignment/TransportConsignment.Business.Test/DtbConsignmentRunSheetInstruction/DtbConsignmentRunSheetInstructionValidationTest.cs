using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRunSheetInstructionValidationTest : BusinessObjectValidationTestCase
	{
		#region TestValidateK1_TimeIn

		public void TestValidateK1_TimeIn()
		{
			var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertNoErrors("Precondition", instruction.K1_TimeInInfo);

			instruction.K1_TimeIn = ZDateTimeOffset.Now;
			AssertHasError(instruction.K1_TimeInInfo, "Instruction must be accepted by driver before set Time In.");

			instruction.K1_IsAcceptedByDriver = true;
			instruction.Validation.ValidateK1_TimeIn();
			AssertNoErrors(instruction.K1_TimeInInfo);

			instruction.K1_TimeOut = instruction.K1_TimeIn.AddDays(-2);
			instruction.Validation.ValidateK1_TimeIn();
			AssertHasError(instruction.K1_TimeInInfo, "The Time In cannot be after the Time Out.");

			instruction.K1_TimeOut = instruction.K1_TimeIn.AddDays(1);
			instruction.Validation.ValidateK1_TimeIn();
			AssertNoErrors("Date range is now valid, should not have an error.", instruction.K1_TimeInInfo);
		}

		#endregion

		#region TestValidateK1_TimeOut

		public void TestValidateK1_TimeOut()
		{
			var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertNoErrors("Precondition", instruction.K1_TimeOutInfo);

			instruction.K1_TimeOut = ZDateTimeOffset.Now;
			AssertNoErrors(instruction.K1_TimeOutInfo);

			instruction.K1_TimeIn = instruction.K1_TimeOut.AddDays(2);
			instruction.Validation.ValidateK1_TimeOut();
			AssertHasError(instruction.K1_TimeOutInfo, "The Time Out cannot be prior to the Time In.");

			instruction.K1_TimeOut = instruction.K1_TimeIn.AddDays(1);
			instruction.Validation.ValidateK1_TimeOut();
			AssertNoErrors("Date range is now valid, should not have an error.", instruction.K1_TimeOutInfo);
		}

		#endregion

		#region TestCheckK1_FailureNotes

		public void TestCheckK1_FailureReason()
		{
			var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertNoErrors("Precondition", instruction.K1_FailureReasonInfo);

			instruction.K1_FailureNotes = "Blah-blah";
			instruction.Validation.ValidateK1_FailureReason();
			AssertHasError(instruction.K1_FailureReasonInfo, "Reason is required if there are notes.");

			instruction.K1_FailureReason = FailureReasonList.Codes.Other;
			AssertNoErrors("Failure reason was supplied, notes should not have an error.", instruction.K1_FailureReasonInfo);

			instruction.K1_FailureNotes = "";
			instruction.K1_FailureReason = "";
			AssertNoErrors("No notes - no errors", instruction.K1_FailureReasonInfo);
		}

		#endregion
	}
}
