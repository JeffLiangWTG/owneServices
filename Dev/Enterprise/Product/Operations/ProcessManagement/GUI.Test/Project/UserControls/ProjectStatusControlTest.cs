using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	class ProjectStatusControlTest : TestCaseWithFactory
	{
		[TestUtcOffset(10, 0, 0)]
		[TestDate(2014, 3, 19, 23, 59, 59)]
		public void TestProjectClosedOrDeferredBox()
		{
			// Make sure that the colour changes depending on local time, not UTC time, despite UTC time being persistent
			var project = Factory.New<Project>();
			using (var form = new ProjectForm(project))
			{
				form.Show();
				var control = (ProjectStatusControl)((ZDynamicControlCreationUserControl)form.Controls.Find("State", true)[0]).HostedControl;
				AssertEquals("ProjectClosedBox.ForeColor - default", System.Drawing.SystemColors.ControlText, control.GetProjectClosedOrDeferredBox().ForeColor);
				AssertEquals("Project Closed", control.GetProjectClosedOrDeferredBox().GetExtension<LabelCaptionRenderer>().Caption);

				var jobHeader = ProcessJobHeaderProvider.GetForParent(project, Factory);
				jobHeader.DoNotStartBeforeDateLocal = ZDateTime.Now.AddDays(1);
				AssertEquals("ProjectClosedBox.ForeColor - deferred", System.Drawing.Color.Red, control.GetProjectClosedOrDeferredBox().ForeColor);
				AssertEquals("Deferred Until", control.GetProjectClosedOrDeferredBox().GetExtension<LabelCaptionRenderer>().Caption);

				jobHeader.DoNotStartBeforeDateLocal = ZDateTime.Now;
				AssertEquals("ProjectClosedBox.ForeColor - deferred date met", System.Drawing.Color.DarkGreen, control.GetProjectClosedOrDeferredBox().ForeColor);
				AssertEquals("Def. Date Met", control.GetProjectClosedOrDeferredBox().GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		public void TestOpportunityDetails()
		{
			var opportunitySalesPerson = Factory.NewWithValidTestData<GlbStaff>();
			opportunitySalesPerson.GS_Code = "OPS";
			opportunitySalesPerson.GS_FullName = "Opp Sales Person";

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OpportunityDescription = "What a great opportunity!";
			opportunity.P8_OpportunityID = "O00001234";
			opportunity.P8_GS_NKPrimarySalesPerson = opportunitySalesPerson.GS_Code;

			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_P8_Opportunity = opportunity.PK;

			Factory.Save();

			using (var form = new ProjectForm(project))
			{
				form.Show();
				UserIdleWorker.Flush(); // the description binding does not occur withouth this
				var projectStatusControl = (ProjectStatusControl)form.FindSingle<ZDynamicControlCreationUserControl>("State").HostedControl;

				var opportunityControl = projectStatusControl.FindSingle<ZGuidFindBox>("OpportunityGuidFindBox");
				AssertEquals("O00001234", opportunityControl.CodeBox.Text);
				AssertEquals("What a great opportunity!", opportunityControl.DescriptionBox.Text);
				Assert("Opportunity ID control should be read-only", opportunityControl.ReadOnly);

				var opportunitySalesPersonControl = projectStatusControl.FindSingle<ZCodeFindBox>("OpportunitySalesPersonCodeFindBox");
				AssertEquals("OPS", opportunitySalesPersonControl.CodeBox.Text);
				AssertEquals("Opp Sales Person", opportunitySalesPersonControl.DescriptionBox.Text);
				Assert("Opportunity Sales Person control should be read-only", opportunitySalesPersonControl.ReadOnly);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKP");
		}
	}
}
