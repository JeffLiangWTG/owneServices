using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(CREReplaceWithCommentsForm))]
	public class CREReplaceWithCommentsFormTest : BasePromptFormTest
	{
		#region Overrides
		public override void TestFormCaptions()
		{
			using (CREReplaceWithCommentsForm form = new CREReplaceWithCommentsForm(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory)))
			{
				AssertEquals("CRE Replace Msg Sending Form", "Replace ICR/CRE", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new CREReplaceWithCommentsForm(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory));
			MissingResourceStringChecker.ExcludeFromTest(form.ChangeCancelReasonTextBox);
			MissingResourceStringChecker.ExcludeFromTest(form.ManualProcessingTextBox);
			MissingResourceStringChecker.ExcludeFromTest(form.FreeTextTextBox);
			return form;
		}
		#endregion
	}
}
