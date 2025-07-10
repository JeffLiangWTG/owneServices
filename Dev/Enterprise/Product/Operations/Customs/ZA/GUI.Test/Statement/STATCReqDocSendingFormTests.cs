using System.Windows.Forms;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(STATACREQDOCSendingForm))]
	sealed class STATCReqDocSendingFormTests : MessageSendingObjectFormTest
	{
		public void TestNothingSelectedMessage()
		{
			using (var form = new STATACREQDOCSendingForm(new STATACREQDOCSendingObjectParent(Factory)))
			{
				form.Show();
				(form.Controls.Find("SendButton", true)[0] as ZButton).PerformClick();
				AssertContains("No FAN numbers selected for Customs Statement Request (REQDOC)", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new STATACREQDOCSendingForm(new STATACREQDOCSendingObjectParent(Factory));
		}
	}
}
