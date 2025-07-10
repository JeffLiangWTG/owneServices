using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal sealed class VoidingSequenceNumberBusinessObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckVoidingToNumber()
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_MaximumNumberDigits = 5;
			sequence.XD_StartNumber = 10;
			sequence.XD_EndNumber = 100;
			sequence.XD_NextNumber = 5;
			sequence.XD_IsActive = true;

			VoidingSequenceNumberBusinessObject bo = new VoidingSequenceNumberBusinessObject(sequence);
			bo.VoidingToNumber = "20";
			AssertEquals(false, bo.VoidingToNumberInfo.HasErrors());

			bo.VoidingToNumber = "abc12";
			Assert(bo.VoidingToNumberInfo.HasError("The To number should only consist of numbers."));

			bo.VoidingToNumber = "000100";
			Assert(bo.VoidingToNumberInfo.HasError("The length of To number exceeds the maximum number of digits of the sequence"));

			bo.VoidingToNumber = "00004";
			Assert(bo.VoidingToNumberInfo.HasError("The To number must be greater than the From number"));

			bo.VoidingToNumber = "00101";
			Assert(bo.VoidingToNumberInfo.HasError("The To number can't be greater than the last number in the sequence"));
		}
	}
}
