using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Business.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Base.Testing
{
	[TestedType(typeof(SubmitToCustomsForm))]
	public class SubmitToCustomsFormOnCusMAWBTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var mawb = Factory.New<CusMAWB>();
			MessageManagerForClearance manager = new Business.MessageBuilders.ECIWriteOff.Express.MessageManagerForCusMAWB(mawb, MessageManager.OperationType.SubmitMessage);
			var testedForm = new SubmitToCustomsForm(manager);
			MissingResourceStringChecker.ExcludeFromTest(testedForm.RemarksTextBox);
			return testedForm;
		}
	}
}
