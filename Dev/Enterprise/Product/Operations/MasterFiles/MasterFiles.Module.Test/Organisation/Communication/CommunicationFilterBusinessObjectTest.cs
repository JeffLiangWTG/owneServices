using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CommunicationFilterBusinessObject))]
	sealed class CommunicationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestMandatoryFiltersRemainAfterLoadLayout()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SCW";

			Factory.Save();

			var filterBizO = new CommunicationFilterBusinessObject();

			var staffCoordinatorFilter = (ModuleNkFilter)filterBizO[CommunicationFilterBusinessObject.FilterDescription.StaffCoordinator];
			staffCoordinatorFilter.Property = "ADL";
			var staffCoordinatorStrip = filterBizO.FilterStrips.AddNew();
			staffCoordinatorStrip.FilterDescription = staffCoordinatorFilter.Description;
			staffCoordinatorStrip.OrCategory = FilterOrCategory.Red;

			var savedFilter = new DataGridLayoutManager().SavePreconfiguredLayout(filterBizO, "savedFilter", false, false, SaveColumnLayout.Ignore);

			using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.CommunicationManagerViewWithoutBeingRelatedStaff.IsAllowed = false;

				filterBizO.LoadLayout(savedFilter);

				staffCoordinatorStrip = filterBizO.FilterStrips.Cast<FilterStrip>().First(strip => strip.FilterDescription == CommunicationFilterBusinessObject.FilterDescription.StaffCoordinator);
				AssertEquals(FilterOrCategory.Grey, staffCoordinatorStrip.OrCategory);
				AssertEquals("SCW", ((ModuleNkFilter)staffCoordinatorStrip.CurrentModuleFilter).Property);
			}
		}

		public void TestLayoutLoadedWithNullFilters()
		{
			var filterBizO = new CommunicationFilterBusinessObjectWithNullFilterStripsForTest();
			AssertNoExceptionThrown(() => filterBizO.LoadLayout(null));
		}

		public void TestOverrideFilter()
		{
			var filterBizObj = new OrgCommunicationFilterBusinessObjectForTest();
			AssertEquals(CommunicationFilterBusinessObject.FilterDescription.CommunicationID, filterBizObj.ModuleFilterThatOverridesAllOtherFilters_Exposed.Description);
		}

		public void TestCommunicationIDFilter()
		{
			var call1 = Factory.New<OrgSalesCall>();
			call1.OQ_CommunicationID = "CMM0000001";
			var call2 = Factory.New<OrgSalesCall>();
			call2.OQ_CommunicationID = "CMM0000002";

			var filterBizObj = new CommunicationFilterBusinessObject();
			var communicationIdFilter = (ModuleFountainFilter)filterBizObj[CommunicationFilterBusinessObject.FilterDescription.CommunicationID];
			AssertNotNull(communicationIdFilter);
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });

			communicationIdFilter.IsActive = true;
			communicationIdFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			communicationIdFilter.Property = "CMM0000001";
			AssertFilterCollection(filterBizObj, new[] { call1 });

			communicationIdFilter.Property = "CMM0000002";
			AssertFilterCollection(filterBizObj, new[] { call2 });

			communicationIdFilter.Property = "CMM0000003";
			AssertFilterCollection(filterBizObj, Enumerable.Empty<OrgSalesCall>());

			communicationIdFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.NotEqual;
			communicationIdFilter.Property = "CMM0000001";
			AssertFilterCollection(filterBizObj, new[] { call2 });

			communicationIdFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.IsNotBlank;
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });
		}

		public void TestOrganizationFilter()
		{
			var org1 = Factory.New<OrgHeader>();
			var call1 = Factory.New<OrgSalesCall>();
			call1.OQ_OH = org1.PK;
			var org2 = Factory.New<OrgHeader>();
			var call2 = Factory.New<OrgSalesCall>();
			call2.OQ_OH = org2.PK;
			var org3 = Factory.New<OrgHeader>();

			var filterBizObj = new CommunicationFilterBusinessObject();
			var clientFilter = (ModuleGuidFilter)filterBizObj[CommunicationFilterBusinessObject.FilterDescription.Client];
			AssertNotNull(clientFilter);
			AssertEquals("Client", clientFilter.Description);
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });

			clientFilter.IsActive = true;
			clientFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			clientFilter.Property = org1.PK;
			AssertFilterCollection(filterBizObj, new[] { call1 });

			clientFilter.Property = org2.PK;
			AssertFilterCollection(filterBizObj, new[] { call2 });

			clientFilter.Property = org3.PK;
			AssertFilterCollection(filterBizObj, Enumerable.Empty<OrgSalesCall>());

			clientFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.NotEqual;
			clientFilter.Property = org1.PK;
			AssertFilterCollection(filterBizObj, new[] { call2 });

			clientFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.IsNotBlank;
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });
		}

		public void TestSetMandatoryOrganizationFilter()
		{
			var filterBizObj = new CommunicationFilterBusinessObject();
			var orgFilter = (ModuleGuidFilter)filterBizObj.ModuleFilters[CommunicationFilterBusinessObject.FilterDescription.Client];
			AssertEquals(FilterVisibility.Visible, orgFilter.Visibility);
			AssertEquals(FilterOrCategory.None, orgFilter.OrCategory);
			AssertEquals(false, orgFilter.ReadOnly);

			var org = Factory.New<OrgHeader>();
			filterBizObj.MandatoryOrganization = org;
			AssertEquals(FilterVisibility.AlwaysApplied, orgFilter.Visibility);
			AssertEquals(FilterOrCategory.None, orgFilter.OrCategory);
			AssertEquals(true, orgFilter.ReadOnly);
			AssertEquals(org.PK, orgFilter.Property);

			filterBizObj.MandatoryOrganization = null;
			AssertEquals(FilterVisibility.Visible, orgFilter.Visibility);
			AssertEquals(FilterOrCategory.None, orgFilter.OrCategory);
			AssertEquals(false, orgFilter.ReadOnly);
			AssertEquals(ZGuid.Empty, orgFilter.Property);
		}

		public void TestOverallDispositionFilter()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var call1 = org.SalesCalls.AddNew();
			call1.OQ_Status = Constants.Sales.Status.Scheduled;
			var call2 = org.SalesCalls.AddNew();
			call2.OQ_Status = Constants.Sales.Status.Completed;
			var call3 = org.SalesCalls.AddNew();
			call3.OQ_Status = Constants.Sales.Status.Cancelled;
			var call4 = org.SalesCalls.AddNew();
			call4.OQ_Status = "XXX";

			Factory.Save();

			var filterBizObj = new CommunicationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)filterBizObj[CommunicationFilterBusinessObject.FilterDescription.OverallDisposition];
			AssertNotNull(statusFilter);
			AssertFilterCollection(filterBizObj, new[] { call1, call2, call3, call4 });

			statusFilter.IsActive = true;
			statusFilter.Property = OrgSalesCallOverallDispositionList.Codes.Open;
			AssertFilterCollection(filterBizObj, new[] { call1, call4 });

			statusFilter.Property = OrgSalesCallOverallDispositionList.Codes.Closed;
			AssertFilterCollection(filterBizObj, new[] { call2, call3 });
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestDateFilter()
		{
			var call1 = Factory.New<OrgSalesCall>();
			call1.OQ_CallSummary = "call1";
			call1.OQ_CallDate = new ZDateTime(2013, 1, 1);
			call1.OQ_NextCall = new ZDateTime(2013, 1, 2);
			var call2 = Factory.New<OrgSalesCall>();
			call2.OQ_CallSummary = "call2";
			call2.OQ_CallDate = new ZDateTime(2013, 1, 10);
			var call3 = Factory.New<OrgSalesCall>();
			call3.OQ_CallSummary = "call3";
			call3.OQ_NextCall = new ZDateTime(2013, 1, 1);
			var call4 = Factory.New<OrgSalesCall>();
			call4.OQ_CallSummary = "call4";

			var filterBizObj = new CommunicationFilterBusinessObject();
			var dateFilter = (ModuleDateFilter)filterBizObj[CommunicationFilterBusinessObject.FilterDescription.Date];
			AssertNotNull(dateFilter);
			AssertFilterCollection(filterBizObj, new[] { call1, call2, call3, call4 });

			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertFilterCollection(filterBizObj, new[] { call1, call2, call3 });

			dateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertFilterCollection(filterBizObj, new[] { call4 });

			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Invalid;
			dateFilter.Property2 = new ZDateTime(2013, 1, 1, 10, 0, 0);
			AssertFilterCollection(filterBizObj, new[] { call1, call3 });

			dateFilter.Property1 = new ZDateTime(2013, 1, 2);
			dateFilter.Property2 = ZDateTime.Invalid;
			AssertFilterCollection(filterBizObj, new[] { call2 });

			dateFilter.Property1 = new ZDateTime(2013, 1, 10, 10, 0, 0);
			dateFilter.Property2 = new ZDateTime(2013, 1, 10, 10, 0, 0);
			AssertFilterCollection(filterBizObj, new[] { call2 });

			dateFilter.Property1 = new ZDateTime(2013, 1, 2);
			dateFilter.Property2 = new ZDateTime(2013, 1, 2);
			AssertFilterCollection(filterBizObj, Enumerable.Empty<OrgSalesCall>());
		}

		public void TestSubjectFilter()
		{
			var call1 = Factory.New<OrgSalesCall>();
			call1.OQ_CallSummary = "My Communication 1";
			var call2 = Factory.New<OrgSalesCall>();
			call2.OQ_CallSummary = "My Communication 2";

			var filterBizObj = new CommunicationFilterBusinessObject();
			var subjectFilter = (ModuleTextFilter)filterBizObj[CommunicationFilterBusinessObject.FilterDescription.Subject];
			AssertNotNull(subjectFilter);
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });

			subjectFilter.IsActive = true;
			subjectFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			subjectFilter.Property = "My Communication 1";
			AssertFilterCollection(filterBizObj, new[] { call1 });

			subjectFilter.Property = "My Communication 2";
			AssertFilterCollection(filterBizObj, new[] { call2 });

			subjectFilter.Property = "My Communication 3";
			AssertFilterCollection(filterBizObj, Enumerable.Empty<OrgSalesCall>());

			subjectFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.NotEqual;
			subjectFilter.Property = "My Communication 1";
			AssertFilterCollection(filterBizObj, new[] { call2 });

			subjectFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.IsNotBlank;
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });

			subjectFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.IsBlank;
			AssertFilterCollection(filterBizObj, Enumerable.Empty<OrgSalesCall>());
		}

		public void TestCommunicationMethodFilter()
		{
			var call1 = Factory.New<OrgSalesCall>();
			call1.OQ_TypeOfCall = Constants.Sales.CommunicationType.FirstCall;
			var call2 = Factory.New<OrgSalesCall>();
			call2.OQ_TypeOfCall = Constants.Sales.CommunicationType.FollowUp;

			var filterBizObj = new CommunicationFilterBusinessObject();
			var communicationMethodFilter = (ModuleTextFilter)filterBizObj[CommunicationFilterBusinessObject.FilterDescription.CommunicationMethod];
			AssertNotNull(communicationMethodFilter);
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });

			communicationMethodFilter.IsActive = true;
			communicationMethodFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			communicationMethodFilter.Property = Constants.Sales.CommunicationType.FirstCall;
			AssertFilterCollection(filterBizObj, new[] { call1 });

			communicationMethodFilter.Property = Constants.Sales.CommunicationType.FollowUp;
			AssertFilterCollection(filterBizObj, new[] { call2 });

			communicationMethodFilter.Property = Constants.Sales.CommunicationType.Service;
			AssertFilterCollection(filterBizObj, Enumerable.Empty<OrgSalesCall>());

			communicationMethodFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.NotEqual;
			communicationMethodFilter.Property = Constants.Sales.CommunicationType.FirstCall;
			AssertFilterCollection(filterBizObj, new[] { call2 });

			communicationMethodFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.IsNotBlank;
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });
		}

		public void TestCategoryFilter()
		{
			var category = Factory.New<OrgSalesCall>();
			category.OQ_Category = "UDF";

			var filterBizObj = new CommunicationFilterBusinessObject();
			var categoryFilter = (ModuleTextFilter)filterBizObj[CommunicationFilterBusinessObject.FilterDescription.Category];
			AssertNotNull(categoryFilter);
			AssertFilterCollection(filterBizObj, new[] { category });
		}

		public void TestStaffCoordinatorFilter()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ADL";
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "SCW";

			var call1 = Factory.New<OrgSalesCall>();
			call1.OQ_GS_NKSalesRep = "ADL";
			var call2 = Factory.New<OrgSalesCall>();
			call2.OQ_GS_NKSalesRep = "ADL";
			var call3 = Factory.New<OrgSalesCall>();
			call3.OQ_GS_NKSalesRep = "SCW";
			var call4 = Factory.New<OrgSalesCall>();
			call4.OQ_GS_NKSalesRep = null;

			var filterBizObj = new CommunicationFilterBusinessObject();
			var staffCoordinatorFilter = (ModuleNkFilter)filterBizObj[CommunicationFilterBusinessObject.FilterDescription.StaffCoordinator];
			AssertNotNull(staffCoordinatorFilter);
			AssertEquals("Staff Coordinator", staffCoordinatorFilter.Description);
			AssertFilterCollection(filterBizObj, new[] { call1, call2, call3, call4 });

			staffCoordinatorFilter.IsActive = true;
			staffCoordinatorFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			staffCoordinatorFilter.Property = "ADL";
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });

			staffCoordinatorFilter.Property = "SCW";
			AssertFilterCollection(filterBizObj, new[] { call3 });

			staffCoordinatorFilter.Property = "RIS";
			AssertFilterCollection(filterBizObj, Enumerable.Empty<OrgSalesCall>());

			staffCoordinatorFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.NotEqual;
			staffCoordinatorFilter.Property = "ADL";
			AssertFilterCollection(filterBizObj, new[] { call3, call4 });

			staffCoordinatorFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.IsNotBlank;
			AssertFilterCollection(filterBizObj, new[] { call1, call2, call3 });

			staffCoordinatorFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.IsBlank;
			AssertFilterCollection(filterBizObj, new[] { call4 });
		}

		public void TestStaffAttendeeFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SCW";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "RIS";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var call1 = org.SalesCalls.AddNew();
			call1.AdditionalAttendeesStaff.AddNew().O6_AttendeeID = staff1.PK;
			var call2 = org.SalesCalls.AddNew();
			call2.AdditionalAttendeesStaff.AddNew().O6_AttendeeID = staff1.PK;
			var call3 = org.SalesCalls.AddNew();
			call3.AdditionalAttendeesStaff.AddNew().O6_AttendeeID = staff2.PK;
			var call4 = org.SalesCalls.AddNew();

			Factory.Save();

			var filterBizObj = new CommunicationFilterBusinessObject();
			var staffAttendeeFilter = (ModuleGuidFilter)filterBizObj[CommunicationFilterBusinessObject.FilterDescription.StaffAttendee];
			AssertNotNull(staffAttendeeFilter);
			AssertFilterCollection(filterBizObj, new[] { call1, call2, call3, call4 });

			staffAttendeeFilter.IsActive = true;
			staffAttendeeFilter.Property = staff1.PK;
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });

			staffAttendeeFilter.Property = staff2.PK;
			AssertFilterCollection(filterBizObj, new[] { call3 });

			staffAttendeeFilter.Property = staff3.PK;
			AssertFilterCollection(filterBizObj, Enumerable.Empty<OrgSalesCall>());
		}

		public void TestStatusFilter()
		{
			var call1 = Factory.New<OrgSalesCall>();
			call1.OQ_Status = Constants.Sales.Status.ColdProspect;
			var call2 = Factory.New<OrgSalesCall>();
			call2.OQ_Status = Constants.Sales.Status.WarmProspect;

			var filterBizObj = new CommunicationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)filterBizObj[CommunicationFilterBusinessObject.FilterDescription.Status];
			AssertNotNull(statusFilter);
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });

			statusFilter.IsActive = true;
			statusFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			statusFilter.Property = Constants.Sales.Status.ColdProspect;
			AssertFilterCollection(filterBizObj, new[] { call1 });

			statusFilter.Property = Constants.Sales.Status.WarmProspect;
			AssertFilterCollection(filterBizObj, new[] { call2 });

			statusFilter.Property = Constants.Sales.Status.HotProspect;
			AssertFilterCollection(filterBizObj, Enumerable.Empty<OrgSalesCall>());

			statusFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.NotEqual;
			statusFilter.Property = Constants.Sales.Status.ColdProspect;
			AssertFilterCollection(filterBizObj, new[] { call2 });

			statusFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.IsNotBlank;
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });
		}

		public void TestPrimaryContactFilter()
		{
			var org = Factory.New<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();
			var otherContact = org.Contacts.AddNew();

			var call1 = org.SalesCalls.AddNew();
			call1.OQ_OC = contact1.PK;
			var call2 = org.SalesCalls.AddNew();
			call2.OQ_OC = contact1.PK;
			var call3 = org.SalesCalls.AddNew();
			call3.OQ_OC = contact2.PK;
			var call4 = org.SalesCalls.AddNew();
			call4.OQ_OC = ZGuid.Empty;

			var filterBizObj = new CommunicationFilterBusinessObject();
			var primaryContactFilter = (ModuleGuidFilter)filterBizObj[CommunicationFilterBusinessObject.FilterDescription.PrimaryContact];
			AssertNotNull(primaryContactFilter);
			AssertFilterCollection(filterBizObj, new[] { call1, call2, call3, call4 });

			primaryContactFilter.IsActive = true;
			primaryContactFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			primaryContactFilter.Property = contact1.PK;
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });

			primaryContactFilter.Property = contact2.PK;
			AssertFilterCollection(filterBizObj, new[] { call3 });

			primaryContactFilter.Property = otherContact.PK;
			AssertFilterCollection(filterBizObj, Enumerable.Empty<OrgSalesCall>());

			primaryContactFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.NotEqual;
			primaryContactFilter.Property = contact1.PK;
			AssertFilterCollection(filterBizObj, new[] { call3, call4 });

			primaryContactFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.IsNotBlank;
			AssertFilterCollection(filterBizObj, new[] { call1, call2, call3 });

			primaryContactFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.IsBlank;
			AssertFilterCollection(filterBizObj, new[] { call4 });
		}

		public void TestContactAttendeeFilter()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			var otherContact = Factory.NewWithValidTestData<OrgContact>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var call1 = org.SalesCalls.AddNew();
			call1.AdditionalAttendeesContact.AddNew().O6_AttendeeID = contact1.PK;
			call1.AdditionalAttendeesContact.AddNew().O6_AttendeeID = contact2.PK;
			var call2 = org.SalesCalls.AddNew();
			call2.AdditionalAttendeesContact.AddNew().O6_AttendeeID = contact1.PK;
			var call3 = org.SalesCalls.AddNew();
			call3.AdditionalAttendeesContact.AddNew().O6_AttendeeID = contact2.PK;
			var call4 = org.SalesCalls.AddNew();

			Factory.Save();

			var filterBizObj = new CommunicationFilterBusinessObject();
			var contactAttendeeFilter = (ModuleGuidFilter)filterBizObj[CommunicationFilterBusinessObject.FilterDescription.ContactAttendee];
			AssertNotNull(contactAttendeeFilter);
			AssertEquals("Contact Attendee", contactAttendeeFilter.Description);
			AssertFilterCollection(filterBizObj, new[] { call1, call2, call3, call4 });

			contactAttendeeFilter.IsActive = true;
			contactAttendeeFilter.Property = contact1.PK;
			AssertFilterCollection(filterBizObj, new[] { call1, call2 });

			contactAttendeeFilter.Property = contact2.PK;
			AssertFilterCollection(filterBizObj, new[] { call1, call3 });

			contactAttendeeFilter.Property = otherContact.PK;
			AssertFilterCollection(filterBizObj, Enumerable.Empty<OrgSalesCall>());
		}

		[RequiresSTA]
		public void TestCustomFieldsColumns()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = OrgSalesCallWorkflowDescriptor.WorkflowTypeCode;

			var templateDefinition = template.GenCustomColumnDefinitions.AddNew();
			templateDefinition.XC_Name = "CustomString";
			templateDefinition.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();

			var collection = new OrgSalesCallCollection(Factory);
			var filter = new CommunicationFilterBusinessObject();

			using (var form = new ZForm())
			using (var filterControl = new CommunicationFilterControl(collection, filter, null))
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertNotNull(filterControl.FilteredGrid.Columns[CustomPropertyHelper.GeneratePropertyIdentifier("CustomString", typeof(ZString))]);
			}
		}

		public void TestPurposeFilterDescription()
		{
			var filterBiz = new CommunicationFilterBusinessObject();
			AssertEquals(OrganisationsDataRegistry.Instance.CategoryListLabel.DefaultValue, filterBiz["Purpose"].MultilingualDescription);

			OrganisationsDataRegistry.Instance.CategoryListLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Current Purpose");
			var filterBiz2 = new CommunicationFilterBusinessObject();
			AssertEquals("Current Purpose", filterBiz2["Purpose"].MultilingualDescription);
		}

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<OrgSalesCall>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.CommunicationManagerCRMSecurity);
		}

		#endregion

		#region Implementation

		void AssertFilterCollection(CommunicationFilterBusinessObject filterBizObj, IEnumerable<OrgSalesCall> expectedCalls)
		{
			AssertContainsExactElementsInAnyOrder(expectedCalls, Factory.Load<OrgSalesCall>(filterBizObj.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CommunicationFilterBusinessObject();
		}

		class OrgCommunicationFilterBusinessObjectForTest : CommunicationFilterBusinessObject
		{
			public ZQuery GetDateQuery_Exposed(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
			{
				return GetDateQuery(comparisonOperator, value1, value2);
			}

			public ModuleFilter ModuleFilterThatOverridesAllOtherFilters_Exposed
			{
				get { return ModuleFilterThatOverridesAllOtherFilters; }
			}
		}

		class CommunicationFilterBusinessObjectWithNullFilterStripsForTest : CommunicationFilterBusinessObject
		{
			protected override List<FilterStrip> GetFilterStrips()
			{
				return null;
			}
		}

		#endregion
	}
}
