using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(CREReplaceForm))]
	public class CREReplaceFormTest : BasePromptFormTest
	{
		#region Overrides
		public override void TestFormCaptions()
		{
			using (CREReplaceForm form = new CREReplaceForm(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory)))
			{
				AssertEquals("CRE Replace Msg Sending Form", "Replace ICR/CRE", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new CREReplaceForm(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory));
			MissingResourceStringChecker.ExcludeFromTest(form.ChangeCancelReasonTextBox);
			return form;
		}
		#endregion
	}
}
