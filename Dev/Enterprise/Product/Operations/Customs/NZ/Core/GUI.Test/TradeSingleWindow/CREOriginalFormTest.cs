using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(CREOriginalForm))]
	public class CREOriginalFormTest : BasePromptFormTest
	{
		#region Overrides
		public override void TestFormCaptions()
		{
			using (CREOriginalForm form = new CREOriginalForm(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory)))
			{
				AssertEquals("CRE Original Msg Sending Form", "Send ICR/CRE", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new CREOriginalForm(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory));
			MissingResourceStringChecker.ExcludeFromTest(form.ManualProcessingTextBox);
			MissingResourceStringChecker.ExcludeFromTest(form.FreeTextTextBox);
			return form;
		}
		#endregion
	}
}
