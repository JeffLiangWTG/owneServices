using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrganisationContainerControlTest : TestCaseWithFactory
	{
		public void TestNormalForm()
		{
			using (ZForm testForm = new ZForm(TestHeader))
			{
				using (OrganisationContainerControlForTesting testControl = new OrganisationContainerControlForTesting())
				{
					testForm.Controls.Add(testControl);
					testForm.Show();
					Assert("Client Intelligence NOT toggled", !testControl.ClientIntelligenceFormToggled);
					Assert("Competitor Intelligence NOT toggled", !testControl.CompetitorIntelligenceFormToggled);
				}
			}
		}

		public void TestClientIntelligenceForm()
		{
			using (ZClientIntelligenceForm testForm = new ZClientIntelligenceForm(TestHeader))
			{
				using (OrganisationContainerControlForTesting testControl = new OrganisationContainerControlForTesting())
				{
					testForm.Controls.Add(testControl);
					testForm.Show();
					Assert("Client Intelligence toggled", testControl.ClientIntelligenceFormToggled);
					Assert("Competitor Intelligence NOT toggled", !testControl.CompetitorIntelligenceFormToggled);
				}
			}
		}

		[RequiresSTA]
		public void TestCompetitorIntelligenceForm()
		{
			using (ZCompetitorIntelligenceForm testForm = new ZCompetitorIntelligenceForm(TestHeader))
			{
				using (OrganisationContainerControlForTesting testControl = new OrganisationContainerControlForTesting())
				{
					testForm.Controls.Add(testControl);
					testForm.Show();
					Assert("Client Intelligence NOT toggled", !testControl.ClientIntelligenceFormToggled);
					Assert("Competitor Intelligence toggled", testControl.CompetitorIntelligenceFormToggled);
				}
			}
		}

		public OrgHeader TestHeader
		{
			get { return Factory.NewWithValidTestData<OrgHeader>(); }
		}
	}
}
