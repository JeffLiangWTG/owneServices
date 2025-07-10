using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SalesUserControl))]
	sealed class SalesUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new SalesUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifySales", "IsModifySalesClientRelationship", "IsModifySalesOpportunityManagement", "IsModifySalesClientSummary", "IsModifySalesTradeProfile" }; }
		}

		public override string GetCheckpointNameFromSecurityControlItem(string itemName)
		{
			switch (itemName)
			{
				case "IsModifySales":
					return "ClientIntelligenceModify";
				case "IsModifySalesClientRelationship":
					return "ClientIntelligenceModifyClientRelationship";
				case "IsModifySalesClientSummary":
					return "ClientIntelligenceModifyClientSummary";
				case "IsModifySalesTradeProfile":
					return "ClientIntelligenceModifyTradeProfile";
				default:
					return base.GetCheckpointNameFromSecurityControlItem(itemName);
			}
		}

		[RequiresSTA]
		public void TestTabPageSecurity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsSalesLead = false;
			Env.Security.OrgSalesView.IsAllowed = true;
			Env.Security.ClientIntelligenceViewClientSummary.IsAllowed = true;
			Env.Security.ClientIntelligenceViewClientRelationship.IsAllowed = true;
			Env.Security.ClientIntelligenceViewTradeProfile.IsAllowed = true;
			Env.Security.ClientIntelligenceViewSalesActivity.IsAllowed = true;
			Env.Security.ClientIntelligenceViewOpportunityManagement.IsAllowed = true;
			Env.Security.ClientIntelligenceViewCommunicationManager.IsAllowed = true;

			using (var form = new ZForm(org))
			using (var control = new SalesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(6, control.SalesTabControl.TabPages.Count);
				foreach (ZTabPage tabPage in control.SalesTabControl.TabPages)
				{
					if (tabPage is ZTabPagePlugIn)
					{
						// ZTabPagePlugin security should be handled in its own class
						continue;
					}
					else
					{
						AssertEquals(tabPage.Text, 0, tabPage.Controls.Find("coveringLabel", false).Length);
						AssertNull(tabPage.Text, tabPage.LicenceCheckpoint);
					}
				}
			}

			org.OH_IsSalesLead = true;
			Env.Security.OrgSalesView.IsAllowed = false;
			Env.Security.ClientIntelligenceViewClientSummary.IsAllowed = false;
			Env.Security.ClientIntelligenceViewClientRelationship.IsAllowed = false;
			Env.Security.ClientIntelligenceViewTradeProfile.IsAllowed = false;
			Env.Security.ClientIntelligenceViewSalesActivity.IsAllowed = false;
			Env.Security.ClientIntelligenceViewOpportunityManagement.IsAllowed = false;
			Env.Security.ClientIntelligenceViewCommunicationManager.IsAllowed = false;

			using (var form = new ZForm(org))
			using (var control = new SalesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(6, control.SalesTabControl.TabPages.Count);
				foreach (ZTabPage tabPage in control.SalesTabControl.TabPages)
				{
					if (tabPage is ZTabPagePlugIn)
					{
						// ZTabPagePlugin security should be handled in its own class
						continue;
					}
					else
					{
						AssertEquals(tabPage.Text, 1, tabPage.Controls.Find("coveringLabel", false).Length);
						AssertNull(tabPage.Text, tabPage.LicenceCheckpoint);
					}
				}
			}

			Env.Security.OrgSalesView.IsAllowed = true;
			Env.Security.ClientIntelligenceViewClientSummary.IsAllowed = true;
			Env.Security.ClientIntelligenceViewClientRelationship.IsAllowed = true;
			Env.Security.ClientIntelligenceViewTradeProfile.IsAllowed = true;
			Env.Security.ClientIntelligenceViewSalesActivity.IsAllowed = true;
			Env.Security.ClientIntelligenceViewOpportunityManagement.IsAllowed = true;
			Env.Security.ClientIntelligenceViewCommunicationManager.IsAllowed = true;

			using (var form = new ZForm(org))
			using (var control = new SalesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(6, control.SalesTabControl.TabPages.Count);
				foreach (ZTabPage tabPage in control.SalesTabControl.TabPages)
				{
					if (tabPage is ZTabPagePlugIn)
					{
						// ZTabPagePlugin security should be handled in its own class
						continue;
					}
					else
					{
						AssertEquals(tabPage.Text, 0, tabPage.Controls.Find("coveringLabel", false).Length);
						AssertNotNull(tabPage.Text, tabPage.LicenceCheckpoint);
					}
				}
			}
		}
	}
}
