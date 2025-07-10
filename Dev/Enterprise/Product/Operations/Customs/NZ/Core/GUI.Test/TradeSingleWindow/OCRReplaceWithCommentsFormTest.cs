using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(OCRReplaceWithCommentsForm))]
	public class OCRReplaceWithCommentsFormTest : BasePromptFormTest
	{
		#region Overrides
		public override void TestFormCaptions()
		{
			using (OCRReplaceWithCommentsForm form = new OCRReplaceWithCommentsForm(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory)))
			{
				AssertEquals("OCR Replace Msg Sending Form", "Replace OCR", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new OCRReplaceWithCommentsForm(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory));
			MissingResourceStringChecker.ExcludeFromTest(form.ChangeCancelReasonTextBox);
			MissingResourceStringChecker.ExcludeFromTest(form.ManualProcessingTextBox);
			MissingResourceStringChecker.ExcludeFromTest(form.FreeTextTextBox);
			return form;
		}
		#endregion
	}
}
