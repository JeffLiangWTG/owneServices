using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(MessageEditForm))]
	sealed class MessageEditFormTest : ZFormBasherTest
	{
		public void TestEditMessage()
		{
			using (MessageEditForm form = new MessageEditForm())
			{
				ZString testString = "This is test message";
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZString resultString = form.EditMessage(testString);
				AssertEquals(testString, resultString);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var formToBash = new MessageEditForm();
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBoxMessage", true).Single());
			return formToBash;
		}
	}
}
