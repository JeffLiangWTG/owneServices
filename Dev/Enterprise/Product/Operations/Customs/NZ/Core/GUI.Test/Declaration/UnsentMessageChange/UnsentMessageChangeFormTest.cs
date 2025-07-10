using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	[TestedType(typeof(UnsentMessageChangeForm))]
	public class UnsentMessageChangeFormTest : ZFormBasherTest
	{
		public void TestProceedButton()
		{
			var heldMessageInfo = new HeldMessageSyncInfo(null);
			using (var testForm = new UnsentMessageChangeForm(heldMessageInfo))
			{
				ZFormModaliser.ShowDialogWithoutDispose(testForm);
				testForm.ProceedButton.PerformClick();
				AssertEquals(typeof(UnsentMessageChangeForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
				AssertEquals("Should have no errors", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("ShouldRecreateMessage value?", false, heldMessageInfo.ShouldRecreateMessage);
				AssertEquals("RemarksForLeavingMessageUnchanged IsEmpty?", true, heldMessageInfo.RemarksForLeavingMessageUnchanged.IsEmpty);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new UnsentMessageChangeForm(new HeldMessageSyncInfo(null));
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return (control.Name == "CancelSaveRadioButton" || base.ShouldIgnoreMissingBindingMember(control));
		}
	}
}
