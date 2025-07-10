using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class CampaignFilterStripTest : TestCaseWithFactory
	{
		public void TestHandlesStaffAssignmentPersonAndRoleModuleFilter()
		{
			AssertProvidesControlsFor(new StaffAssignmentPersonAndRoleModuleFilter("Staff Assignment and Role Module Filter Test", new GlbCompanyCampaignContactFilterBusinessObject()));
		}

		public void TestHandlesCampaignContactLinkActivityModuleFilter()
		{
			AssertProvidesControlsFor(new CampaignContactLinkActivityModuleFilter("Campaign Contact Link Activity Module Filter Test", new GlbCompanyCampaignContactFilterBusinessObject(), Factory.New<GlbCompanyCampaign>()));
		}

		public void TestHandlesCampaignContactContextLinkActivityModuleFilter()
		{
			AssertProvidesControlsFor(new CampaignContactContextLinkActivityModuleFilter("Campaign Contact Context Link Activity Module Filter Test", new GlbCompanyCampaignContactFilterBusinessObject(), Factory.New<GlbCompanyCampaign>()));
		}

		public void TestHandlesCampaignContactDestinationURLLinkActivityModuleFilter()
		{
			AssertProvidesControlsFor(new CampaignContactDestinationURLLinkActivityModuleFilter("Campaign Contact Destination URL Link Activity Module FilterTest", new GlbCompanyCampaignContactFilterBusinessObject(), Factory.New<GlbCompanyCampaign>()));
		}

		public void TestHandlesCampaignContactNumberFilter()
		{
			AssertProvidesControlsFor(new CampaignContactNumberFilter("Campaign Contact Number Filter Test", (SQLComparisonOperator a, ZInt b) => new ZQuery()));
		}

		public void TestHandlesUniqueDaysActivityCountFilter()
		{
			AssertProvidesControlsFor(new UniqueDaysActivityCountFilter("Unique Days Activity Count Filter Test", (SQLComparisonOperator a, ZInt b) => new ZQuery()));
		}

		public void TestHandlesOrganisationHasSalesRelationFilter()
		{
			AssertProvidesControlsFor(new OrganisationHasSalesRelationFilter("Filter Prefix", "Organisation Has Sales Relation Filter Test", OrgHeaderSchema.PK));
		}

		public void TestHandlesUtcOffsetFilter()
		{
			AssertProvidesControlsFor(new UtcOffsetFilter("UTC Offset Filter Test", (ZString a, ZString b) => new ZQuery(), new RefTimeZoneLookups(Factory).OffsetFromUtcList));
		}

		public void TestHandlesModuleOrgSalesMainCompetitorModuleFilter()
		{
			AssertProvidesControlsFor(new OrgSalesMainCompetitorModuleFilter("Sales Main Competitor Filter Test", (ZGuid a, ZString b) => new ZQuery()));
		}

		public void TestHandlesModuleOrgHasMainCompetitorModuleFilter()
		{
			AssertProvidesControlsFor(new OrgHasMainCompetitorModuleFilter("Has Main Competitor Filter Test", (ZBool a, ZString b) => new ZQuery()));
		}

		#region Implementation

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (var strip = new CampaignFilterStrip())
			{
				Control[] controls = GetCurrentFilterControls(strip, filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);

				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}

		Control[] GetCurrentFilterControls(CampaignFilterStrip strip, ModuleFilter filter)
		{
			MethodInfo info = typeof(CampaignFilterStrip).GetMethod(
				"GetCurrentFilterControls", BindingFlags.Instance | BindingFlags.NonPublic);

			return (Control[])info.Invoke(strip, new object[] { filter });
		}

		#endregion
	}
}
