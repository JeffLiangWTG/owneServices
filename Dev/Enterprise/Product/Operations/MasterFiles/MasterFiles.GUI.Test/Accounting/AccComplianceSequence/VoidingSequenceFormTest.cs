using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(VoidingSequenceForm))]
	sealed class VoidingSequenceFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			VoidingSequenceNumberBusinessObject voidingObj = new VoidingSequenceNumberBusinessObject(sequence);
			return new VoidingSequenceForm(voidingObj, null);
		}

		public void TestVoidNumbers()
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			sequence.XD_Code = "LM1";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 100;
			sequence.XD_NextNumber = 1;
			sequence.XD_MaximumNumberDigits = 9;
			AssertEquals("precondition:", 1, (int)sequence.XD_NextNumber);

			VoidingSequenceNumberBusinessObject voidingbo = new VoidingSequenceNumberBusinessObject(sequence);
			voidingbo.VoidingToNumber = "50";
			using (VoidingSequenceForm form = new VoidingSequenceForm(voidingbo, null))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OKButton_Click(null, null);
				AssertEquals("postcondition:", 51, (int)sequence.XD_NextNumber);
			}
		}
	}
}
