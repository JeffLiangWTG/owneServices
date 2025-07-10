using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(VoidingSequenceNumberBusinessObject))]
	internal sealed class VoidingSequenceNumberBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			return new VoidingSequenceNumberBusinessObject(sequence);
		}

		#endregion

		#region Tests

		public void TestPaddingVoidingToNumber()
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_MaximumNumberDigits = 8;
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 100;
			sequence.XD_NextNumber = 5;
			sequence.XD_IsActive = true;

			VoidingSequenceNumberBusinessObject bo = new VoidingSequenceNumberBusinessObject(sequence);
			bo.VoidingToNumber = "20";
			AssertEquals("00000020", bo.VoidingToNumber);
		}

		public void TestVoidNumbersInRange()
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_MaximumNumberDigits = 8;
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 100;
			sequence.XD_NextNumber = 5;
			sequence.XD_IsActive = true;

			VoidingSequenceNumberBusinessObject bo = new VoidingSequenceNumberBusinessObject(sequence);
			bo.VoidingToNumber = "20";
			bo.VoidNumbersInRange();
			AssertEquals("next number in sequence", 21m, sequence.XD_NextNumber);
			AssertEquals("next number in sequence", true, sequence.XD_IsActive);

			bo.VoidingToNumber = "100";
			bo.VoidNumbersInRange();
			AssertEquals("next number in sequence", 101m, sequence.XD_NextNumber);
			AssertEquals("next number in sequence", false, sequence.XD_IsActive);
		}

		#endregion
	}
}
