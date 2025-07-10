using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CusCalculationRuleForm))]
	sealed class CusCalculationRuleFormTest : ZFormBasherTest
	{
		public void TestFomCaption()
		{
			using (var form = GetFormToBashCore() as CusCalculationRuleForm)
			{
				form.Show();
				AssertEquals("Customs Calculation Rule", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new CusCalculationRuleForm();
			form.ControllerID = ControllerIDs.Customs.CusCalculationRules;
			return form;
		}
	}
}
