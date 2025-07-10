using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(OCROriginalForm))]
	public class OCROriginalFormTest : BasePromptFormTest
	{
		#region Overrides
		public override void TestFormCaptions()
		{
			using (OCROriginalForm form = new OCROriginalForm(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory)))
			{
				AssertEquals("OCR Original Msg Sending Form", "Send OCR", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new OCROriginalForm(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory));
			MissingResourceStringChecker.ExcludeFromTest(form.ManualProcessingTextBox);
			MissingResourceStringChecker.ExcludeFromTest(form.FreeTextTextBox);
			return form;
		}
		#endregion
	}
}
