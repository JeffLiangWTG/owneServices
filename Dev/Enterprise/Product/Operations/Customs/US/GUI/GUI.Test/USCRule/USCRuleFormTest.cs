using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(USCRuleForm))]
	sealed class USCRuleFormTest : ZFormBasherTest
	{
		public void TestUSCRuleUserControlIsAdded()
		{
			USCRule rule = Factory.New<USCRule>();
			using (USCRuleForm form = new USCRuleForm(rule))
			{
				form.Show();
				USCRuleUserControl controlAdded = null;

				foreach (Control control in form.MainTabControlExposedForTesting.TabPages[0].Controls)
				{
					if (control is USCRuleUserControl)
					{
						controlAdded = (USCRuleUserControl)control;
						break;
					}
				}

				AssertNotNull(controlAdded);
				AssertEquals(DockStyle.Fill, controlAdded.Dock);
			}
		}

		protected override Form GetFormToBashCore()
		{
			USCRule rule = Factory.New<USCRule>();
			USCTariffRule tariffRule = rule.Tariffs.AddNew();
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			Factory.Save();
			return new USCRuleForm(rule);
		}
	}
}
