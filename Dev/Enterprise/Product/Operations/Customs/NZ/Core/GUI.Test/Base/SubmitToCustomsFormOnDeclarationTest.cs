using System.Windows.Forms;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MessageBuilders;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Base.Testing
{
	[TestedType(typeof(SubmitToCustomsForm))]
	public class SubmitToCustomsFormOnDeclarationTest : ZFormBasherTest
	{
		public void TestSelectDefaultRemarksButton_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			MessageManagerForClearance manager = new Business.MessageBuilders.ECIWriteOff.MessageManager(declaration, MessageManager.OperationType.CancelMessage);
			using (var form = new SubmitToCustomsForm(manager))
			{
				form.Show();
				form.SelectDefaultRemarksButton.PerformClick();
				AssertEquals(typeof(SelectDefaultRemarksForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			MessageManagerForClearance manager = new Business.MessageBuilders.ECIWriteOff.MessageManager(declaration, MessageManager.OperationType.CancelMessage);
			var testedForm = new SubmitToCustomsForm(manager);
			MissingResourceStringChecker.ExcludeFromTest(testedForm.RemarksTextBox);
			return testedForm;
		}
	}
}
