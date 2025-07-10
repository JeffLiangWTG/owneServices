using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(CPCForm))]
	sealed class CPCFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var sgcpc = declaration.CPCs.AddNew();
			declaration.HasChanges = false;
			sgcpc.HasChanges = false;
			var formToBash = new CPCForm(sgcpc);
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("pC1", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("pC2", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("pC3", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("aPCDescription", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox1", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox2", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox3", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox4", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox5", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox6", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox7", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox8", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox9", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox10", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox11", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox12", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox13", true).Single());
			return formToBash;
		}
	}
}
