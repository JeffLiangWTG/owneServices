using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(StaffAssignmentPersonAndRoleModuleFilter))]
	public class StaffAssignmentPersonAndRoleModuleFilterTest : ModuleTextFilterTest
	{
		public void TestFilter_All()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "NEO";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var staffAssignment = org.StaffAssignments.AddNew();
			staffAssignment.O8_GS_NKPersonResponsible = staff.GS_Code;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;

			Factory.Save();

			GlbCompanyCampaignContactFilterBusinessObjectForTest filterBizO = new GlbCompanyCampaignContactFilterBusinessObjectForTest(campaign);
			GlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(filterBizO);
			var filter = filterBizO["Staff Assignment Person And Role"] as StaffAssignmentPersonAndRoleModuleFilter;
			filter.IsActive = true;

			filter.StaffAssignmentPerson = "NEO";

			ZQuery query = filterBizO.Filter;
			GlbCampaignContactCollection collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, campaign);
			collection.Load(query);

			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void TestFilter_WithIsBlankOperator()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "NEO";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var staffAssignment = org.StaffAssignments.AddNew();
			staffAssignment.O8_GS_NKPersonResponsible = staff.GS_Code;
			staffAssignment.O8_Role = "PRJ";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_Title = "MY_TITLE";

			Factory.Save();

			GlbCompanyCampaignContactFilterBusinessObjectForTest filterBizO = new GlbCompanyCampaignContactFilterBusinessObjectForTest(campaign);
			var filter = filterBizO["Staff Assignment Person And Role"] as StaffAssignmentPersonAndRoleModuleFilter;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;
			AssertEquals(true, filter.StaffAssignmentPersonInfo.ReadOnly);

			filter.StaffAssignmentRole = "PRJ";

			ZQuery query = filterBizO.Filter;
			GlbCampaignContactCollection collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, campaign);
			query.AddToFilter(ViewCampaignContactSchema.VCC_Title, "MY_TITLE");
			collection.Load(query);

			AssertEquals("There should be no item in the list", 0, collection.Count);
		}

		public void TestSerialize_Deserialize_PropertiesFromToXml()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var filterStripBizO = new DummyFilterStripBusinessObject();

			var filterBusinessObject = new GlbCompanyCampaignContactFilterBusinessObject(campaign);
			var filter = new StaffAssignmentPersonAndRoleModuleFilter("Staff Assignment Person/Role", filterBusinessObject);

			filterStripBizO.AddModuleFilterForTest(filter);
			FilterStrip strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.StaffAssignmentPerson = "ZBX";
			filter.StaffAssignmentRole = "ACC";

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = "";
			strip.Delete();

			var loadedFilter = (StaffAssignmentPersonAndRoleModuleFilter)filterStripBizO[filter.Description];

			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals("Staff Assignment Person", "ZBX", loadedFilter.StaffAssignmentPerson);
			AssertEquals("Staff Assignment Role", "ACC", loadedFilter.StaffAssignmentRole);
		}

		public void TestStaffRolesList()
		{
			var roles = new CodeDescriptionPairList(DataRegistry.Instance.OrgStaffMemberAssignmentRoles);
			roles.AddPair("PAR", "WisePartner");
			DataRegistry.Instance.OrgStaffMemberAssignmentRoles = roles;

			var filter = GetNewBusinessObject() as StaffAssignmentPersonAndRoleModuleFilter;
			AssertContainsExactElementsInAnyOrder(roles, filter.StaffRolesList);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			return new StaffAssignmentPersonAndRoleModuleFilter("Test", new GlbCompanyCampaignContactFilterBusinessObject(campaign));
		}

		class GlbCompanyCampaignContactFilterBusinessObjectForTest : GlbCompanyCampaignContactFilterBusinessObject
		{
			public GlbCompanyCampaignContactFilterBusinessObjectForTest(GlbCompanyCampaign campaign)
				: base(campaign)
			{
			}
		}

		#endregion
	}
}
