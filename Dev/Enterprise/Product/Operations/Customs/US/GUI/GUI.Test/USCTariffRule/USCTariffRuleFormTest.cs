using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(USCTariffRuleForm))]
	sealed class USCTariffRuleFormTest : ZFormBasherTest
	{
		public void TestShowPreSaveDialogs()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();
			rule.U1_RuleCode = TariffRuleList.Codes.AssembledAbroadOfUSProducts;
			rule.U1_Tariff = "99990084";
			rule.U1_DateFrom = ZDateTime.Today;
			USCRuleSecondaryTariff secondaryTariff = rule.SecondaryTariffs.AddNew();
			secondaryTariff.U3_TariffFrom = "010101";
			secondaryTariff.U3_DateFrom = ZDateTime.Today;
			using (USCTariffRuleForm form = new USCTariffRuleForm(rule))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();
				AssertEquals(ODisplayMode.Edit, form.DisplayMode);
				Assert(rule.SecondaryTariffs.Count > 0);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FireSaveButton();
				AssertEquals(ODisplayMode.Browse, form.DisplayMode);
				Assert(rule.SecondaryTariffs.Count == 0);
			}
		}

		public void TestTariffRuleUserControlAdded()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();
			using (USCTariffRuleForm form = new USCTariffRuleForm(rule))
			{
				form.Show();
				USCTariffRuleUserControl controlAdded = null;
				foreach (Control control in form.MainTabControlExposedForTesting.TabPages[0].Controls)
				{
					if (control is USCTariffRuleUserControl)
					{
						controlAdded = (USCTariffRuleUserControl)control;
						break;
					}
				}

				AssertNotNull(controlAdded);
				AssertEquals(DockStyle.Fill, controlAdded.Dock);
			}
		}

		public void TestTariffRuleFormCaption()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();
			using (USCTariffRuleForm form = new USCTariffRuleForm(rule))
			{
				form.Show();
				AssertContains("Tariff Rule", form.Text);
				rule.U1_Tariff = "00001122";
				AssertContains("Tariff Rule 0000.11.22", form.Text);
				AssertNotContains("Tariff Rule 0000.11.22 - 3333.44", form.Text);
				rule.U1_TariffTo = "333344";
				AssertContains("Tariff Rule 0000.11.22 - 3333.44", form.Text);
			}
		}

		public void TestHideShowTabPages()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();
			using (USCTariffRuleForm form = new USCTariffRuleForm(rule))
			{
				form.Show();
				Assert("Precondition: NOT rule.HasAssociatedSecondaryTariffs", !rule.EligibleForAssociatedSecondaryTariffs);
				AssertNull("AssociatedTarrifsTabPage should NOT be visible", form.MainTabControlExposedForTesting.TabPages["AssociatedTarrifsTabPage"]);
				rule.U1_RuleCode = TariffRuleList.Codes.EligibleForSecondaryTariffNumbers;
				Assert("Precondition: rule.HasAssociatedSecondaryTariffs", rule.EligibleForAssociatedSecondaryTariffs);
				Assert("AssociatedTarrifsTabPage should be visible", ((ZTabPage)form.MainTabControlExposedForTesting.TabPages["AssociatedTarrifsTabPage"]).TabVisible);
			}
		}

		protected override Form GetFormToBashCore() => new USCTariffRuleForm(Factory.New<USCTariffRule>());
	}
}
