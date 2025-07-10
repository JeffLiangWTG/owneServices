using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(OCRCancelForm))]
	public class OCRCancelFormTest : BasePromptFormTest
	{
		#region Overrides
		public override void TestFormCaptions()
		{
			using (OCRCancelForm form = new OCRCancelForm(new AdditionalMessageInformation(TSWTransactionTypes.Cancel, Factory)))
			{
				AssertEquals("OCR Cancel Msg Sending Form", "Cancel OCR", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new OCRCancelForm(new AdditionalMessageInformation(TSWTransactionTypes.Cancel, Factory));
			MissingResourceStringChecker.ExcludeFromTest(form.ChangeCancelReasonTextBox);
			return form;
		}
		#endregion
	}
}
