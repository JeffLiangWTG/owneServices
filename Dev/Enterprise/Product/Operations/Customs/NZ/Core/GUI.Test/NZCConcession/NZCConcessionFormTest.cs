using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	[TestedType(typeof(NZCConcessionForm))]
	public class NZCConcessionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			NZCConcession concession = Factory.New<NZCConcession>();
			var form = new NZCConcessionForm(concession);
			MissingResourceStringChecker.ExcludeFromTest(form.u0_DateActiveToDateEdit);
			MissingResourceStringChecker.ExcludeFromTest(form.u2_Calc_TariffsApplicableTextBox);
			MissingResourceStringChecker.ExcludeFromTest(form.u2_DescriptionTextBox);
			MissingResourceStringChecker.ExcludeFromTest(form.u2_CodeTextBox);
			return form;
		}
	}
}
