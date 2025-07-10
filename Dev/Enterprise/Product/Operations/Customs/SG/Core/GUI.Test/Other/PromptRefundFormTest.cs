using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(PromptRefundForm))]
	sealed class PromptRefundFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var formToBash = new PromptRefundForm(new RefundAdditionalMessageInformation(Factory.New<JobDeclaration>(), Factory));
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zCalcEdit1", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zCalcEdit2", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zCalcEdit3", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zCalcEdit4", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zCalcEdit5", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zCalcEdit6", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zDropEdit1", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zDropEdit3", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox4", true).Single());
			return formToBash;
		}
	}
}
