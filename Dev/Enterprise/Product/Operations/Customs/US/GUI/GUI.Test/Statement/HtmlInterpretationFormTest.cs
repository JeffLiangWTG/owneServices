using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	using Enterprise.Messaging.GUI;
	using Enterprise.ZArchitecture.GUI.Testing;
	using NUnit.Framework;

	[TestedType(typeof(HtmlInterpretationForm))]
	public class HtmlInterpretationFormTest : ZFormBasherTest
	{
		public void TestMessageDetailsForm()
		{
			using (var form = new HtmlInterpretationForm("HTML INTERPRETATION"))
			{
				form.Show();
				var control = (HtmlInterpretationBox)form.Controls.Find("HtmlInterpretationBox", true)[0];

				UserIdleWorker.Flush();

				var originalDocumentText = form.DocumentText;
				while (control.ReadyState != WebBrowserReadyState.Complete)
				{
					System.Windows.Forms.Application.DoEvents();
				}

				AssertEquals("FormHeading", "Accounting Integration", form.FormHeading);
				Assert("Actual DocumentText:" + form.DocumentText, form.DocumentText.Contains("HTML INTERPRETATION"));
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new HtmlInterpretationForm("HTML INTERPRETATION");
		}
	}
}
