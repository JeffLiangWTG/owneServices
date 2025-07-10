using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(MessageEditForm))]
	sealed class MessageEditFormTest : ZFormBasherTest
	{
		public void TestEditMessage()
		{
			using (MessageEditForm form = new MessageEditForm())
			{
				var testString = "This is test message";
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var resultString = form.EditMessage(testString);
				AssertEquals(testString, resultString);
			}
		}

		protected override Form GetFormToBashCore() => new MessageEditForm();
	}
}
