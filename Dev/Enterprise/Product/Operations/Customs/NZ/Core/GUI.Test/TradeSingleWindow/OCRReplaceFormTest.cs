using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(OCRReplaceForm))]
	public class OCRReplaceFormTest : BasePromptFormTest
	{
		#region Overrides
		public override void TestFormCaptions()
		{
			using (OCRReplaceForm form = new OCRReplaceForm(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory)))
			{
				AssertEquals("OCR Replace Msg Sending Form", "Replace OCR", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new OCRReplaceForm(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory));
			MissingResourceStringChecker.ExcludeFromTest(form.ChangeCancelReasonTextBox);
			return form;
		}
		#endregion
	}
}
