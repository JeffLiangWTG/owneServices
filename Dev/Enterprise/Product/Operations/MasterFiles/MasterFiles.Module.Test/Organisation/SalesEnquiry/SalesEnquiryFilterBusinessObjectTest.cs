using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(SalesEnquiryFilterBusinessObject))]
	internal class SalesEnquiryFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestOriginalCallDateFilter()
		{
			SalesEnquiry enquiryHistoricallyOld = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiryHistoricallyOld.O1_LeadCalledDate = new ZDateTime(2010, 1, 1);
			SalesEnquiry enquiryOld = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiryOld.O1_LeadCalledDate = new ZDateTime(2011, 1, 1);
			SalesEnquiry enquiryModeratelyOld = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiryModeratelyOld.O1_LeadCalledDate = new ZDateTime(2012, 1, 1);
			SalesEnquiry enquiryModeratelyNew = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiryModeratelyNew.O1_LeadCalledDate = new ZDateTime(2013, 1, 1);
			SalesEnquiry enquiryNew = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiryNew.O1_LeadCalledDate = new ZDateTime(2013, 8, 8);
			SalesEnquiry enquiryBrandNew = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiryBrandNew.O1_LeadCalledDate = new ZDateTime(2013, 12, 11);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Original Call Date"];

			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2009, 12, 12);
			filter.Property2 = ZDateTime.Empty;

			SalesEnquiryCollection enquires1 = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			AssertEquals(6, enquires1.Count);
			AssertCollectionContains(enquiryHistoricallyOld, enquires1);
			AssertCollectionContains(enquiryOld, enquires1);
			AssertCollectionContains(enquiryModeratelyOld, enquires1);
			AssertCollectionContains(enquiryModeratelyNew, enquires1);
			AssertCollectionContains(enquiryNew, enquires1);
			AssertCollectionContains(enquiryBrandNew, enquires1);

			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2009, 12, 10);
			filter.Property2 = new ZDateTime(2012, 12, 30);

			SalesEnquiryCollection enquires2 = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			AssertEquals(3, enquires2.Count);
			AssertCollectionContains(enquiryHistoricallyOld, enquires2);
			AssertCollectionContains(enquiryOld, enquires2);
			AssertCollectionContains(enquiryModeratelyOld, enquires2);
			AssertCollectionNotContains(enquiryModeratelyNew, enquires2);
			AssertCollectionNotContains(enquiryNew, enquires2);
			AssertCollectionNotContains(enquiryBrandNew, enquires2);

			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2013, 12, 11);

			SalesEnquiryCollection enquires3 = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			AssertEquals(6, enquires3.Count);
			AssertCollectionContains(enquiryHistoricallyOld, enquires3);
			AssertCollectionContains(enquiryOld, enquires3);
			AssertCollectionContains(enquiryModeratelyOld, enquires3);
			AssertCollectionContains(enquiryModeratelyNew, enquires3);
			AssertCollectionContains(enquiryNew, enquires3);
			AssertCollectionContains(enquiryBrandNew, enquires3);
		}

		public void TestAddressOneFilter()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrg("XXXYYY", "Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry2 = GetEnquiryWithOrg("XXXXXX", "Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			SalesEnquiry enquiry3 = GetEnquiryWithOrgAddress("XXX111", "Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry4 = GetEnquiryWithOrgAddress("XXX222", "Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			SalesEnquiry enquiry5 = GetEnquiryWithoutOrg("Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry6 = GetEnquiryWithoutOrg("Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Address 1"];
			AssertTextFilter(filter, (x) => x.O1_Address1, enquiry1, enquiry2, enquiry3, enquiry4, enquiry5, enquiry6);
		}

		public void TestAddressTwoFilter()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrg("XXXYYY", "Paris", "Paris, just under the bridge of Alexander III.", "12345", "Paris", "75");
			SalesEnquiry enquiry2 = GetEnquiryWithOrg("XXXXXX", "Sydney", "Sydney, just under the Harbor bridge.", "54321", "Sydney", "NSW");
			SalesEnquiry enquiry3 = GetEnquiryWithOrgAddress("XXX111", "Paris", "Paris, just under the bridge of Alexander III.", "12345", "Paris", "75");
			SalesEnquiry enquiry4 = GetEnquiryWithOrgAddress("XXX222", "Sydney", "Sydney, just under the Harbor bridge.", "54321", "Sydney", "NSW");
			SalesEnquiry enquiry5 = GetEnquiryWithoutOrg("Paris", "Paris, just under the bridge of Alexander III.", "12345", "Paris", "75");
			SalesEnquiry enquiry6 = GetEnquiryWithoutOrg("Sydney", "Sydney, just under the Harbor bridge.", "54321", "Sydney", "NSW");

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Address 2"];
			AssertTextFilter(filter, (x) => x.O1_Address2, enquiry1, enquiry2, enquiry3, enquiry4, enquiry5, enquiry6);
		}

		public void TestStatusFilter()
		{
			SalesEnquiry enquiryOPN = Factory.NewWithValidTestData<SalesEnquiry>();
			SalesEnquiry enquiryCLS = Factory.NewWithValidTestData<SalesEnquiry>();
			SalesEnquiry enquiryCNV = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiryOPN.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Open;
			enquiryCLS.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;
			enquiryCNV.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Status"];
			AssertTextFilter(filter, (x) => x.O1_LeadStatus, enquiryOPN, enquiryCLS, enquiryCNV);

			AssertEquals("Status filter does not have IsBlank operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			AssertEquals("Status filter does not have IsNotBlank operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
			AssertEquals("Status filter does not have StartsWith operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			AssertEquals("Status filter does not have NotStartsWith operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotStartsWith));
			AssertEquals("Status filter does not have Contains operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.Contains));
			AssertEquals("Status filter does not have NotContain operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotContain));
		}

		public void TestTypeFilter()
		{
			SalesEnquiry enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			SalesEnquiry enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			SalesEnquiry enquiry3 = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry1.O1_EnquiryType = "INQ";
			enquiry2.O1_EnquiryType = "CCR";
			enquiry3.O1_EnquiryType = "MIT";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Type"];
			AssertTextFilter(filter, (x) => x.O1_EnquiryType, enquiry1, enquiry2, enquiry3);

			AssertEquals("Type filter does not have IsBlank operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			AssertEquals("Type filter does not have IsNotBlank operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
			AssertEquals("Type filter does not have StartsWith operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			AssertEquals("Type filter does not have NotStartsWith operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotStartsWith));
			AssertEquals("Type filter does not have Contains operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.Contains));
			AssertEquals("Type filter does not have NotContain operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotContain));
		}

		public void TestCloseReasonFilter()
		{
			SalesEnquiry enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			SalesEnquiry enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			SalesEnquiry enquiry3 = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry1.O1_CloseReason = "";
			enquiry2.O1_CloseReason = "NOT";
			enquiry3.O1_CloseReason = "XYZ";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Close Reason"];
			AssertTextFilter(filter, (x) => x.O1_CloseReason, enquiry1, enquiry2, enquiry3);
		}

		public void TestOrgNameFilter()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrg("XXXYYY", "Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry2 = GetEnquiryWithOrg("XXXXXX", "Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			SalesEnquiry enquiry3 = GetEnquiryWithoutOrg("Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry4 = GetEnquiryWithoutOrg("Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			enquiry1.Header.OH_FullName = "OrgA11";
			enquiry2.Header.OH_FullName = "OrgB11";
			enquiry3.O1_CompanyName = "OrgA22";
			enquiry4.O1_CompanyName = "OrgB22";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Organization Name"];
			AssertTextFilter(filter, (x) => x.O1_CompanyName, enquiry1, enquiry2, enquiry3, enquiry4);
		}

		public void TestOrgCity()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrg("XXXYYY", "Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry2 = GetEnquiryWithOrg("XXXXXX", "Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			SalesEnquiry enquiry3 = GetEnquiryWithoutOrg("Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry4 = GetEnquiryWithoutOrg("Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["City"];

			AssertTextFilter(filter, (x) => x.O1_City, enquiry1, enquiry2, enquiry3, enquiry4);
		}

		public void TestOrgState()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrg("XXXYYY", "Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry2 = GetEnquiryWithOrg("XXXXXX", "Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			SalesEnquiry enquiry3 = GetEnquiryWithoutOrg("Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry4 = GetEnquiryWithoutOrg("Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["State"];

			AssertTextFilter(filter, (x) => x.O1_State, enquiry1, enquiry2, enquiry3, enquiry4);
		}

		public void TestOrgPostCode()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrg("XXXYYY", "Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry2 = GetEnquiryWithOrg("XXXXXX", "Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			SalesEnquiry enquiry3 = GetEnquiryWithoutOrg("Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry4 = GetEnquiryWithoutOrg("Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Post Code"];
			AssertTextFilter(filter, (x) => x.O1_PostCode, enquiry1, enquiry2, enquiry3, enquiry4);
		}

		public void TestReferringOrg()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrg("XXXYYY", "Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry2 = GetEnquiryWithOrg("XXXXXX", "Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			SalesEnquiry enquiry3 = GetEnquiryWithoutOrg("Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry4 = GetEnquiryWithoutOrg("Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");

			OrgHeader sourceOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader sourceOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			enquiry1.O1_OH_SourceOfLead = sourceOrg1.PK;
			enquiry2.O1_OH_SourceOfLead = sourceOrg1.PK;
			enquiry3.O1_OH_SourceOfLead = sourceOrg1.PK;
			enquiry4.O1_OH_SourceOfLead = sourceOrg2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Referring Organization"];
			filter.Property = sourceOrg1.PK;
			filter.IsActive = true;

			SalesEnquiryCollection collection = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			Assert("Expect collection to contain enquiry1", collection.Contains(enquiry1));
			Assert("Expect collection to contain enquiry2", collection.Contains(enquiry2));
			Assert("Expect collection to contain enquiry3", collection.Contains(enquiry3));
			Assert("Expect collection not to contain enquiry4", !collection.Contains(enquiry4));

			filter.Property = sourceOrg2.PK;
			collection = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			Assert("Expect collection not to contain enquiry1", !collection.Contains(enquiry1));
			Assert("Expect collection not to contain enquiry2", !collection.Contains(enquiry2));
			Assert("Expect collection not to contain enquiry3", !collection.Contains(enquiry3));
			Assert("Expect collection to contain enquiry4", collection.Contains(enquiry4));
		}

		public void TestReferringContactName()
		{
			var enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry3 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry4 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry5 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry6 = Factory.NewWithValidTestData<SalesEnquiry>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var contact1 = org1.Contacts.AddNew();
			var contact2 = org1.Contacts.AddNew();
			var contact3 = org2.Contacts.AddNew();
			var contact4 = org2.Contacts.AddNew();
			contact1.OC_ContactName = "Sales";
			contact2.OC_ContactName = "Operations";
			contact3.OC_ContactName = "Sales";
			contact4.OC_ContactName = "Operations";

			enquiry1.O1_OH_SourceOfLead = org1.PK;
			enquiry2.O1_OH_SourceOfLead = org1.PK;
			enquiry3.O1_OH_SourceOfLead = org2.PK;
			enquiry4.O1_OH_SourceOfLead = org2.PK;
			enquiry5.O1_OH_SourceOfLead = org2.PK;

			enquiry1.O1_OC_ReferringContact = contact1.PK;
			enquiry2.O1_OC_ReferringContact = contact2.PK;
			enquiry3.O1_OC_ReferringContact = contact3.PK;
			enquiry4.O1_OC_ReferringContact = contact4.PK;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Referring Contact Name"];
			AssertTextFilter(filter, (x) => x.ReferringContactName, enquiry1, enquiry2, enquiry3, enquiry4, enquiry5, enquiry6);
		}

		public void TestReferToOrg()
		{
			var enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry3 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry4 = Factory.NewWithValidTestData<SalesEnquiry>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			enquiry1.O1_OH_ReferTo = org1.PK;
			enquiry2.O1_OH_ReferTo = org1.PK;
			enquiry3.O1_OH_ReferTo = org2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Refer To Organization"];
			filter.Property = org1.PK;
			filter.IsActive = true;

			SalesEnquiryCollection collection = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			CombineAssertions(() =>
			{
				Assert("Collection should contain enquiry1", collection.Contains(enquiry1));
				Assert("Collection should contain enquiry2", collection.Contains(enquiry2));
				Assert("Collection should not contain enquiry3", !collection.Contains(enquiry3));
				Assert("Collection should not contain enquiry4", !collection.Contains(enquiry4));
			});

			filter.Property = org2.PK;
			collection = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			CombineAssertions(() =>
			{
				Assert("Collection should not contain enquiry1", !collection.Contains(enquiry1));
				Assert("Collection should not contain enquiry2", !collection.Contains(enquiry2));
				Assert("Collection should contain enquiry3", collection.Contains(enquiry3));
				Assert("Collection should not contain enquiry4", !collection.Contains(enquiry4));
			});
		}

		public void TestReferToContactName()
		{
			var enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry3 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry4 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry5 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry6 = Factory.NewWithValidTestData<SalesEnquiry>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var contact1 = org1.Contacts.AddNew();
			var contact2 = org1.Contacts.AddNew();
			var contact3 = org2.Contacts.AddNew();
			var contact4 = org2.Contacts.AddNew();
			contact1.OC_ContactName = "Sales";
			contact2.OC_ContactName = "Operations";
			contact3.OC_ContactName = "Sales";
			contact4.OC_ContactName = "Operations";

			enquiry1.O1_OH_ReferTo = org1.PK;
			enquiry2.O1_OH_ReferTo = org1.PK;
			enquiry3.O1_OH_ReferTo = org2.PK;
			enquiry4.O1_OH_ReferTo = org2.PK;
			enquiry5.O1_OH_ReferTo = org2.PK;

			enquiry1.O1_OC_ReferToContact = contact1.PK;
			enquiry2.O1_OC_ReferToContact = contact2.PK;
			enquiry3.O1_OC_ReferToContact = contact3.PK;
			enquiry4.O1_OC_ReferToContact = contact4.PK;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Refer To Contact Name"];
			AssertTextFilter(filter, (x) => x.ReferToContactName, enquiry1, enquiry2, enquiry3, enquiry4, enquiry5, enquiry6);
		}

		public void TestAssignedStaffBranch()
		{
			var enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry3 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry4 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry5 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry6 = Factory.NewWithValidTestData<SalesEnquiry>();

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "BR1";
			branch2.GB_Code = "BR2";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TS1";
			staff2.GS_Code = "TS2";
			staff3.GS_Code = "TS3";
			staff1.GS_GB_HomeBranch = branch1.PK;
			staff2.GS_GB_HomeBranch = branch2.PK;

			enquiry1.O1_GS_NKRepAssigned = staff1.GS_Code;
			enquiry2.O1_GS_NKRepAssigned = staff2.GS_Code;
			enquiry3.O1_GS_NKRepAssigned = staff3.GS_Code;
			enquiry4.O1_GS_NKRepAssigned = staff1.GS_Code;
			enquiry5.O1_GS_NKRepAssigned = staff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Assigned Staff Branch"];
			filter.Property = branch1.PK;
			filter.IsActive = true;

			var collection = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			CombineAssertions(() =>
			{
				Assert("Collection should contain enquiry1", collection.Contains(enquiry1));
				Assert("Collection should not contain enquiry2", !collection.Contains(enquiry2));
				Assert("Collection should not contain enquiry3", !collection.Contains(enquiry3));
				Assert("Collection should contain enquiry4", collection.Contains(enquiry4));
				Assert("Collection should not contain enquiry5", !collection.Contains(enquiry5));
				Assert("Collection should not contain enquiry6", !collection.Contains(enquiry6));
			});

			filter.Property = branch2.PK;
			collection = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			CombineAssertions(() =>
			{
				Assert("Collection should not contain enquiry1", !collection.Contains(enquiry1));
				Assert("Collection should contain enquiry2", collection.Contains(enquiry2));
				Assert("Collection should not contain enquiry3", !collection.Contains(enquiry3));
				Assert("Collection should not contain enquiry4", !collection.Contains(enquiry4));
				Assert("Collection should contain enquiry5", collection.Contains(enquiry5));
				Assert("Collection should not contain enquiry6", !collection.Contains(enquiry6));
			});

			filter.Property = ZGuid.Empty;
			filter.IsActive = false;
			collection = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			CombineAssertions(() =>
			{
				Assert("Collection should contain enquiry1", collection.Contains(enquiry1));
				Assert("Collection should contain enquiry2", collection.Contains(enquiry2));
				Assert("Collection should contain enquiry3", collection.Contains(enquiry3));
				Assert("Collection should contain enquiry4", collection.Contains(enquiry4));
				Assert("Collection should contain enquiry5", collection.Contains(enquiry5));
				Assert("Collection should contain enquiry6", collection.Contains(enquiry6));
			});
		}

		public void TestLeadInterest()
		{
			CodeDescriptionBoolCollection leadInterests = new CodeDescriptionBoolCollection();
			leadInterests.Add("YEA", null, true);
			leadInterests.Add("NAY", null, true);
			leadInterests.Add("HUH", null, false);
			OrganisationsDataRegistry.Instance.SalesEnquiryLeadInterests.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, leadInterests);

			SalesEnquiry enquiry1 = GetEnquiryWithOrg("XXXYYY", "Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry2 = GetEnquiryWithOrg("XXXXXX", "Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			SalesEnquiry enquiry3 = GetEnquiryWithoutOrg("Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry4 = GetEnquiryWithoutOrg("Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			enquiry1.O1_InterestLevel = "YEA";
			enquiry2.O1_InterestLevel = "NAY";
			enquiry3.O1_InterestLevel = "HUH";
			enquiry4.O1_InterestLevel = "";

			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Lead Interest"];
			AssertTextFilter(filter, (x) => x.O1_InterestLevel, enquiry1, enquiry2, enquiry3, enquiry4);
		}

		public void TestBusinessRegNo()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrg("XXXYYY", "Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry2 = GetEnquiryWithOrg("XXXXXX", "Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			SalesEnquiry enquiry3 = GetEnquiryWithoutOrg("Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry4 = GetEnquiryWithoutOrg("Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			enquiry1.Header.PrimaryRegistrationNumber.Number = "21 003 980 130";
			enquiry2.Header.PrimaryRegistrationNumber.Number = "";
			enquiry3.O1_BusinessRegNo = "21 003 980 1301";
			enquiry4.O1_BusinessRegNo = "";

			Factory.Save();
			AssertEquals("", enquiry2.Header.PrimaryRegistrationNumber.Number);

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Business Registration Number"];
			AssertTextFilter(filter, (x) => x.O1_BusinessRegNo, enquiry1, enquiry2, enquiry3, enquiry4);
		}

		public void TestFax()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrgContact("XXXYYY", "Contact A", "12345678", "A@test.com");
			SalesEnquiry enquiry2 = GetEnquiryWithOrgContact("XXXXXX", "Contact B", "87654321", "B@test.com");
			SalesEnquiry enquiry3 = GetEnquiryWithOrgContact("XXXZZZ", "Contact C", "11111111", "C@test.com");
			SalesEnquiry enquiry4 = GetEnquiryWithoutOrgContact("Contact B", "87654321", "B@test.com");
			SalesEnquiry enquiry5 = GetEnquiryWithoutOrgContact("Contact B", "87654321", "B@test.com");
			SalesEnquiry enquiry6 = GetEnquiryWithoutOrgContact("Contact C", "11111111", "C@test.com");
			enquiry1.Header.Contacts[0].OC_Fax = "123456789";
			enquiry2.Header.Contacts[0].OC_Fax = "23456789";
			enquiry3.Header.Contacts[0].OC_Fax = "";
			enquiry4.O1_Fax = "12345679";
			enquiry5.O1_Fax = "12345671";
			enquiry6.O1_Fax = "";

			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Fax"];
			AssertTextFilter(filter, (x) => x.O1_Fax, enquiry1, enquiry2, enquiry3, enquiry4);
		}

		public void TestLocation()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrg("XXXYYY", "Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry2 = GetEnquiryWithOrg("XXXXXX", "Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			SalesEnquiry enquiry3 = GetEnquiryWithoutOrg("Paris, just under the bridge of Alexander III.", "", "12345", "Paris", "75");
			SalesEnquiry enquiry4 = GetEnquiryWithoutOrg("Sydney, just under the Harbor bridge.", "", "54321", "Sydney", "NSW");
			enquiry1.Header.OH_RL_NKClosestPort = "AUSYD";
			enquiry2.Header.OH_RL_NKClosestPort = "NZAKL";
			enquiry3.O1_PortOrCountry = "AU";
			enquiry4.O1_PortOrCountry = "AUBNE";

			Factory.Save();
			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO["Country/ Port"];
			filter.Property = "AU";
			filter.IsActive = true;
			SalesEnquiryCollection collection = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			Assert("Expect collection to contain enquiry1", collection.Contains(enquiry1));
			Assert("Expect collection not to contain enquiry2", !collection.Contains(enquiry2));
			Assert("Expect collection to contain enquiry3", collection.Contains(enquiry3));
			Assert("Expect collection to contain enquiry4", collection.Contains(enquiry4));

			filter.Property = "AUSYD";
			collection = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			Assert("Expect collection to contain enquiry1", collection.Contains(enquiry1));
			Assert("Expect collection not to contain enquiry2", !collection.Contains(enquiry2));
			Assert("Expect collection to contain enquiry3", collection.Contains(enquiry3));
			Assert("Expect collection not to contain enquiry4", !collection.Contains(enquiry4));

			filter.Property = "AUBNE";
			collection = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			Assert("Expect collection not to contain enquiry1", !collection.Contains(enquiry1));
			Assert("Expect collection not to contain enquiry2", !collection.Contains(enquiry2));
			Assert("Expect collection to contain enquiry3", collection.Contains(enquiry3));
			Assert("Expect collection to contain enquiry4", collection.Contains(enquiry4));
		}

		public void TestOrCategory()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrg("XXXYYY", "Addr 1", "", "AAA", "Paris", "111");
			SalesEnquiry enquiry2 = GetEnquiryWithOrg("XXXXXX", "Addr 2", "", "BBB", "Sydney", "111");
			SalesEnquiry enquiry3 = GetEnquiryWithOrg("XXXDDD", "Addr 3", "", "CCC", "Sydney", "222");
			SalesEnquiry enquiry4 = GetEnquiryWithoutOrg("Addr 4", "", "AAA", "Paris", "222");
			SalesEnquiry enquiry5 = GetEnquiryWithoutOrg("Addr 5", "", "BBB", "Sydney", "333");
			SalesEnquiry enquiry6 = GetEnquiryWithoutOrg("Addr 6", "", "CCC", "Sydney", "333");

			Factory.Save();

			ModuleTextFilter postcodeFilter1 = (ModuleTextFilter)FilterStripBizO["Post Code"];
			postcodeFilter1.Property = "AAA";
			postcodeFilter1.SqlComparisonOperator = SQLComparisonOperator.Equal;
			postcodeFilter1.IsActive = true;
			postcodeFilter1.OrCategory = FilterOrCategory.Red;

			ModuleTextFilter postcodeFilter2 = (ModuleTextFilter)FilterStripBizO.CreateDuplicateFor("Post Code");
			postcodeFilter2.Property = "CCC";
			postcodeFilter2.SqlComparisonOperator = SQLComparisonOperator.Equal;
			postcodeFilter2.IsActive = true;
			postcodeFilter2.OrCategory = FilterOrCategory.Red;

			ModuleTextFilter stateFilter1 = (ModuleTextFilter)FilterStripBizO["State"];
			stateFilter1.Property = "111";
			stateFilter1.SqlComparisonOperator = SQLComparisonOperator.Equal;
			stateFilter1.IsActive = true;
			stateFilter1.OrCategory = FilterOrCategory.Blue;

			ModuleTextFilter stateFilter2 = (ModuleTextFilter)FilterStripBizO.CreateDuplicateFor("State");
			stateFilter2.Property = "222";
			stateFilter2.SqlComparisonOperator = SQLComparisonOperator.Equal;
			stateFilter2.IsActive = true;
			stateFilter2.OrCategory = FilterOrCategory.Blue;

			SalesEnquiryCollection collection = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);

			Assert("Expect collection to contain enquiry1", collection.Contains(enquiry1));
			Assert("Expect collection not to contain enquiry2", !collection.Contains(enquiry2));
			Assert("Expect collection to contain enquiry3", collection.Contains(enquiry3));
			Assert("Expect collection to contain enquiry4", collection.Contains(enquiry4));
			Assert("Expect collection not to contain enquiry5", !collection.Contains(enquiry5));
			Assert("Expect collection not to contain enquiry6", !collection.Contains(enquiry6));
		}

		public void TestSalesTeam()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var staffA = Factory.NewWithValidTestData<GlbStaff>();
			var staffB = Factory.NewWithValidTestData<GlbStaff>();
			var staffC = Factory.NewWithValidTestData<GlbStaff>();
			var staffD = Factory.NewWithValidTestData<GlbStaff>();

			var teamX = Factory.NewWithValidTestData<GlbGroup>();
			var teamY = Factory.NewWithValidTestData<GlbGroup>();

			var linkStaffA = Factory.NewWithValidTestData<GlbGroupLink>();
			var linkStaffB = Factory.NewWithValidTestData<GlbGroupLink>();
			var linkStaffC = Factory.NewWithValidTestData<GlbGroupLink>();

			staffA.GS_Code = "AAA";
			staffB.GS_Code = "BBB";
			staffC.GS_Code = "CCC";
			staffD.GS_Code = "DDD";

			teamX.GG_Code = "XXX";
			teamX.GG_Desc = "Sales team X";
			teamX.GG_IsSales = true;
			teamY.GG_Code = "YYY";
			teamY.GG_Desc = "Sales team Y";
			teamY.GG_IsSales = true;

			linkStaffA.GK_GS = staffA.PK;
			linkStaffA.GK_GG = teamX.PK;

			linkStaffB.GK_GS = staffB.PK;
			linkStaffB.GK_GG = teamX.PK;

			linkStaffC.GK_GS = staffC.PK;
			linkStaffC.GK_GG = teamY.PK;

			var enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry1.OrgPk = org1.PK;
			enquiry1.O1_GS_NKRepAssigned = staffA.GS_Code;

			var enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry2.OrgPk = org1.PK;
			enquiry2.O1_GS_NKRepAssigned = staffB.GS_Code;

			var enquiry3 = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry3.OrgPk = org1.PK;
			enquiry3.O1_GS_NKRepAssigned = staffC.GS_Code;

			var enquiry4 = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry4.OrgPk = org1.PK;

			var enquiry5 = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry5.OrgPk = org1.PK;
			enquiry5.O1_GS_NKRepAssigned = staffD.GS_Code;

			Factory.Save();

			SalesEnquiryCollection enquiries;
			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO["Sales Team"];

			filter.IsActive = true;
			filter.Property = "GGG";
			enquiries = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			AssertEquals(0, enquiries.Count);

			filter.Property = "XXX";
			enquiries = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			AssertEquals(2, enquiries.Count);
			AssertCollectionContains(enquiry1, enquiries);
			AssertCollectionContains(enquiry2, enquiries);

			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			enquiries = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			AssertEquals(3, enquiries.Count);
			AssertCollectionContains(enquiry3, enquiries);
			AssertCollectionContains(enquiry4, enquiries);
			AssertCollectionContains(enquiry5, enquiries);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			enquiries = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			AssertEquals(2, enquiries.Count);
			AssertCollectionContains(enquiry4, enquiries);
			AssertCollectionContains(enquiry5, enquiries);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			enquiries = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);
			AssertEquals(3, enquiries.Count);
			AssertCollectionNotContains(enquiry4, enquiries);
			AssertCollectionNotContains(enquiry5, enquiries);
		}

		SalesEnquiry GetEnquiryWithOrg(string orgCode, string address1, string address2, string postcode, string city, string state)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = orgCode;

			OrgAddress orgAddress = org.Addresses[0];
			orgAddress.OA_Address1 = address1;
			orgAddress.OA_Address2 = address2;
			orgAddress.OA_PostCode = postcode;
			orgAddress.OA_City = city;
			orgAddress.OA_State = state;

			SalesEnquiry enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.OrgPk = org.PK;

			return enquiry;
		}

		SalesEnquiry GetEnquiryWithOrgAddress(string orgCode, string address1, string address2, string postcode, string city, string state)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = orgCode;

			org.Addresses[0].OA_Address1 = address1;

			OrgAddress orgAddress = org.Addresses.AddNew();
			orgAddress.OA_Address1 = address1;
			orgAddress.OA_Address2 = address2;
			orgAddress.OA_PostCode = postcode;
			orgAddress.OA_City = city;
			orgAddress.OA_State = state;

			SalesEnquiry enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.OrgPk = org.PK;
			enquiry.O1_OA_LinkedAddress = orgAddress.PK;

			return enquiry;
		}

		SalesEnquiry GetEnquiryWithoutOrg(string address1, string address2, string postcode, string city, string state)
		{
			SalesEnquiry enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_Address1 = address1;
			enquiry.O1_Address2 = address2;
			enquiry.O1_PostCode = postcode;
			enquiry.O1_City = city;
			enquiry.O1_State = state;

			return enquiry;
		}

		public void TestContactName()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrgContact("XXXYYY", "Contact A", "12345678", "A@test.com");
			SalesEnquiry enquiry2 = GetEnquiryWithOrgContact("XXXXXX", "Contact B", "87654321", "B@test.com");
			SalesEnquiry enquiry3 = GetEnquiryWithoutOrgContact("Contact A", "12345678", "A@test.com");
			SalesEnquiry enquiry4 = GetEnquiryWithoutOrgContact("Contact B", "87654321", "B@test.com");

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Contact Name"];
			AssertTextFilter(filter, (x) => x.O1_ContactName, enquiry1, enquiry2, enquiry3, enquiry4);

			SalesEnquiry enquiry5 = GetEnquiryWithOrgContact("XXXDDD", "Contact CCC", "33333333", "C@test.com", false);
			Factory.Save();
			AssertTextFilter(filter, (x) => x.O1_ContactName, enquiry5);
		}

		public void TestContactPhone()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrgContact("XXXYYY", "Contact A", "12345678", "A@test.com");
			SalesEnquiry enquiry2 = GetEnquiryWithOrgContact("XXXXXX", "Contact B", "87654321", "B@test.com");
			SalesEnquiry enquiry3 = GetEnquiryWithoutOrgContact("Contact A", "12345678", "A@test.com");
			SalesEnquiry enquiry4 = GetEnquiryWithoutOrgContact("Contact B", "87654321", "B@test.com");

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Work Phone"];
			AssertTextFilter(filter, (x) => x.O1_Phone, enquiry1, enquiry2, enquiry3, enquiry4);

			SalesEnquiry enquiry5 = GetEnquiryWithOrgContact("XXXDDD", "Contact CCC", "33333333", "C@test.com", false);
			Factory.Save();
			AssertTextFilter(filter, (x) => x.O1_Phone, enquiry5);
		}

		public void TestContactEmail()
		{
			SalesEnquiry enquiry1 = GetEnquiryWithOrgContact("XXXYYY", "Contact A", "12345678", "A@test.com");
			SalesEnquiry enquiry2 = GetEnquiryWithOrgContact("XXXXXX", "Contact B", "87654321", "B@test.com");
			SalesEnquiry enquiry3 = GetEnquiryWithoutOrgContact("Contact A", "12345678", "A@test.com");
			SalesEnquiry enquiry4 = GetEnquiryWithoutOrgContact("Contact B", "87654321", "B@test.com");

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Email"];
			AssertTextFilter(filter, (x) => x.O1_Email, enquiry1, enquiry2, enquiry3, enquiry4);

			SalesEnquiry enquiry5 = GetEnquiryWithOrgContact("XXXDDD", "Contact CCC", "33333333", "C@test.com", false);
			Factory.Save();
			AssertTextFilter(filter, (x) => x.O1_Email, enquiry5);
		}

		void AssertTextFilter(ModuleTextFilter filter, EnquiryStringValue getValue, params SalesEnquiry[] enquiries)
		{
			ZString longestValue = enquiries.Select(x => getValue(x)).OrderByDescending(x => x.Length).First();
			if (longestValue.Length <= 1)
			{
				throw new ArgumentException("All values are blank or length one. Must have at least one value with length > 1.");
			}

			filter.Property = longestValue;
			filter.IsActive = true;
			AssertTextFilter(filter, SQLComparisonOperator.Equal, getValue, enquiries);
			AssertTextFilter(filter, SQLComparisonOperator.NotEqual, getValue, enquiries);

			filter.Property = longestValue.Substring(1);
			AssertTextFilter(filter, SQLComparisonOperator.Contains, getValue, enquiries);
			AssertTextFilter(filter, SQLComparisonOperator.NotContains, getValue, enquiries);

			filter.Property = longestValue.Substring(0, 1);
			AssertTextFilter(filter, SQLComparisonOperator.StartsWith, getValue, enquiries);
			AssertTextFilter(filter, SQLComparisonOperator.DoesNotStartWith, getValue, enquiries);

			AssertTextFilter(filter, SpecialComparisonOperator.IsBlank, getValue, enquiries);
			AssertTextFilter(filter, SpecialComparisonOperator.IsNotBlank, getValue, enquiries);
		}

		void AssertTextFilter(ModuleTextFilter filter, SQLComparisonOperator op, EnquiryStringValue getValue, params SalesEnquiry[] enquiries)
		{
			filter.SqlComparisonOperator = op;
			SalesEnquiryCollection collection = new SalesEnquiryCollection(Factory, FilterStripBizO.Filter);

			for (int i = 1; i <= enquiries.Length; ++i)
			{
				var enquiry = enquiries[i - 1];
				bool contains = collection.Contains(enquiry);
				ZString text = getValue(enquiry);
				if (op == SQLComparisonOperator.Equal)
				{
					AssertEquals(i + ") Value <" + text + "> Equal filter <" + filter.Property + "> ", text == filter.Property, contains);
				}
				else if (op == SQLComparisonOperator.NotEqual)
				{
					AssertEquals(i + ") Value <" + text + "> NotEqual filter <" + filter.Property + "> ", text != filter.Property, contains);
				}
				else if (op == SQLComparisonOperator.StartsWith)
				{
					AssertEquals(i + ") Value <" + text + "> StartWith filter <" + filter.Property + "> ", text.StartsWith(filter.Property), contains);
				}
				else if (op == SQLComparisonOperator.DoesNotStartWith)
				{
					AssertEquals(i + ") Value <" + text + "> NotStartWith <" + filter.Property + "> ", !text.StartsWith(filter.Property), contains);
				}
				else if (op == SQLComparisonOperator.Contains)
				{
					AssertEquals(i + ") Value <" + text + "> Contains filter <" + filter.Property + "> ", text.Contains(filter.Property), contains);
				}
				else if (op == SQLComparisonOperator.NotContains)
				{
					AssertEquals(i + ") Value <" + text + "> NotContains filter <" + filter.Property + "> ", !text.Contains(filter.Property), contains);
				}
				else if (op == SpecialComparisonOperator.IsBlank)
				{
					AssertEquals(i + ") Value <" + text + "> IsBlank", text.IsEmpty, contains);
				}
				else if (op == SpecialComparisonOperator.IsNotBlank)
				{
					AssertEquals(i + ") Value <" + text + "> IsNotBlank", !text.IsEmpty, contains);
				}
			}
		}

		delegate ZString EnquiryStringValue(SalesEnquiry enquiry);

		SalesEnquiry GetEnquiryWithOrgContact(string orgCode, string contactName, string phone, string email, bool setLinkedContact = true)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = orgCode;

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = contactName;
			contact.OC_Phone = phone;
			contact.OC_Email = email;

			SalesEnquiry enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			if (setLinkedContact)
			{
				enquiry.OrgPk = org.PK;
				enquiry.O1_OC_LinkedContact = contact.PK;
			}
			else
			{
				enquiry.O1_ContactName = contactName;
				enquiry.O1_Phone = phone;
				enquiry.O1_Email = email;
				enquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			}

			return enquiry;
		}

		SalesEnquiry GetEnquiryWithoutOrgContact(string contactName, string phone, string email)
		{
			SalesEnquiry enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_ContactName = contactName;
			enquiry.O1_Phone = phone;
			enquiry.O1_Email = email;

			return enquiry;
		}

		public void TestAddWorkflowCustomFieldsFilters()
		{
			var collection = new SalesEnquiryFilterBusinessObject().ModuleFilters;

			AssertNull(collection["Custom1 String"]);
			AssertNull(collection["Custom1 Integer"]);
			AssertNull(collection["Workflow Flags"]);
			AssertNull(collection["Custom2 Boolean"]);
			AssertNull(collection["Custom2 Datetime"]);
			AssertNull(collection["Unrelated Custom String"]);

			PrepareTemplatesWithCustomFields();

			collection = new SalesEnquiryFilterBusinessObject().ModuleFilters;

			AssertEquals(typeof(ModuleTextFilter), collection["Custom1 String"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), collection["Custom1 Integer"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), collection["Workflow Flags"].GetType());
			AssertEquals(typeof(ModuleTextFilter), collection["Custom2 Boolean"].GetType());
			AssertEquals(typeof(ModuleDateFilter), collection["Custom2 Datetime"].GetType());
			AssertNull(collection["Unrelated Custom String"]);
		}

		void PrepareTemplatesWithCustomFields()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = SalesEnquiryWorkflowDescriptor.WorkflowTypeCode;

			var template1Definition1 = template1.GenCustomColumnDefinitions.AddNew();
			template1Definition1.XC_Name = "Custom1 String";
			template1Definition1.XC_Type = AddOnColumnDataType.Codes.String;

			var template1Definition2 = template1.GenCustomColumnDefinitions.AddNew();
			template1Definition2.XC_Name = "Custom1 Integer";
			template1Definition2.XC_Type = AddOnColumnDataType.Codes.Integer;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = SalesEnquiryWorkflowDescriptor.WorkflowTypeCode;

			var template2Definition1 = template2.GenCustomColumnDefinitions.AddNew();
			template2Definition1.XC_Name = "Custom2 Boolean";
			template2Definition1.XC_Type = AddOnColumnDataType.Codes.Boolean;

			var template2Definition2 = template2.GenCustomColumnDefinitions.AddNew();
			template2Definition2.XC_Name = "Custom2 Datetime";
			template2Definition2.XC_Type = AddOnColumnDataType.Codes.Datetime;

			var unrelatedTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			unrelatedTemplate.P0_ProcessType = "ZZZ";

			var unrelatedTemplateDefinition = unrelatedTemplate.GenCustomColumnDefinitions.AddNew();
			unrelatedTemplateDefinition.XC_Name = "Unrelated Custom String";
			unrelatedTemplateDefinition.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			WorkflowCustomFieldsFilter.ClearCache();
		}

		public void TestCreatedOnWebInternalFilter()
		{
			var filterStripBO = GetNewFilterStripBusinessObject();
			filterStripBO.QueryObjectType = typeof(SalesEnquiry);
			ModuleTextFilter filter = (ModuleTextFilter)filterStripBO["Created On Web/Internal"];
			var filterOptions = filter.List as CodeDescriptionPairList;

			AssertEquals(3, filterOptions.Count);

			AssertEquals("ALL", filterOptions[0].Code);
			AssertEquals("All Records", filterOptions[0].Description);

			AssertEquals("WEB", filterOptions[1].Code);
			AssertEquals("Created using Web Inquiry", filterOptions[1].Description);

			AssertEquals("ENT", filterOptions[2].Code);
			AssertEquals($"Created using {Constants.ProductName}", filterOptions[2].Description);

			filter.Property = "WEB";
			AssertEquals("O1_SystemCreateUser = 'ZZ'", filter.Query.LiteralTextADO);
		}

		#region CRM Security

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<SalesEnquiry>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.InquiryManagerCRMSecurity);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new SalesEnquiryFilterBusinessObject();
		}

		protected virtual FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				if (filterStripBizO == null)
				{
					filterStripBizO = GetNewFilterStripBusinessObject();
				}
				return filterStripBizO;
			}
		}

		FilterStripBusinessObject filterStripBizO;

		#endregion

		#region Index Search

		public void TestIndexSearchFiltersOfOrgColdCallRegister()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.SalesEnquiry))
			using (var mocker = new GlowIndexQueryEngineMock(
				mock =>
				{
					_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetSearchFieldCollection());
					_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IOrgColdCallRegister" });
				}))
			{
				var statusFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["LeadStatus"];
				AssertEquals(FilterCategories.StatusAndFlags, statusFilter.Category);
				AssertEquals(OrgColdCallRegisterSchema.O1_LeadStatus.MaxLength, statusFilter.MaxLength);
				Assert(statusFilter.ComparisonOperator_List.Count == 2 && statusFilter.ComparisonOperator_List.CodesAsString == "all exact, none exact");

				var enquiryTypeFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["EnquiryType"];
				AssertEquals(FilterCategories.StatusAndFlags, statusFilter.Category);
				AssertEquals(OrgColdCallRegisterSchema.O1_EnquiryType.MaxLength, enquiryTypeFilter.MaxLength);
				Assert(enquiryTypeFilter.ComparisonOperator_List.Count == 2 && enquiryTypeFilter.ComparisonOperator_List.CodesAsString == "all exact, none exact");

				var closeReasonFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["CloseReason"];
				AssertEquals(FilterCategories.StatusAndFlags, closeReasonFilter.Category);

				var organizationFilter = (IndexSearchModuleGuidFilter)module.FilterBusinessObject["OrganizationGuid"];
				AssertEquals(FilterCategories.Organisations, organizationFilter.Category);

				var organizationNameFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["OrganizationName"];
				AssertEquals(FilterCategories.Organisations, organizationNameFilter.Category);

				var assignedStaffFilter = (IndexSearchModuleNKFilter)module.FilterBusinessObject["AssignedStaff"];
				AssertEquals(FilterCategories.Organisations, assignedStaffFilter.Category);

				var salesTeamFilter = (IndexSearchModuleNKFilter)module.FilterBusinessObject["SalesTeam"];
				AssertEquals(FilterCategories.Organisations, salesTeamFilter.Category);

				var assignedStaffBranchFilter = (IndexSearchModuleGuidFilter)module.FilterBusinessObject["AssignedStaffBranch"];
				AssertEquals(FilterCategories.Organisations, assignedStaffBranchFilter.Category);

				var referToOrganizationFilter = (IndexSearchModuleGuidFilter)module.FilterBusinessObject["ReferToOrganization"];
				AssertEquals(FilterCategories.Organisations, referToOrganizationFilter.Category);

				var referringOrganizationFilter = (IndexSearchModuleGuidFilter)module.FilterBusinessObject["ReferringOrganization"];
				AssertEquals(FilterCategories.Organisations, referringOrganizationFilter.Category);

				var referToContactNameFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["ReferToContactName"];
				AssertEquals(FilterCategories.Organisations, referToContactNameFilter.Category);

				var referringContactNameFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["ReferringContactName"];
				AssertEquals(FilterCategories.Organisations, referringContactNameFilter.Category);

				var inquiryIDFilter = (IndexSearchModuleFountainFilter)module.FilterBusinessObject["InquiryID"];
				AssertEquals(FilterCategories.NumbersAndReferences, inquiryIDFilter.Category);

				var legacyInquiryIDFilter = (IndexSearchModuleFountainFilter)module.FilterBusinessObject["Legacy Inquiry ID"];
				AssertEquals(FilterCategories.NumbersAndReferences, legacyInquiryIDFilter.Category);
				AssertEquals("Legacy Inquiry ID", legacyInquiryIDFilter.MultilingualDescription.ToString());
			}
		}

		SearchFieldCollection GetSearchFieldCollection()
		{
			var ruleLookupOfField1 = new SearchFieldRuleLookup(Guid.NewGuid());
			var field1 = new SearchField("LeadStatus", "Status", typeof(string), false, false, 0, ruleLookup: ruleLookupOfField1);
			var ruleLookupOfField2 = new SearchFieldRuleLookup(Guid.NewGuid());
			var field2 = new SearchField("EnquiryType", "Type", typeof(string), false, false, 0, ruleLookup: ruleLookupOfField2);
			var field3 = SearchField.Create("CloseReason", "Close Reason");
			var field4 = SearchField.Create("OrganizationGuid", "Organization", typeof(Guid));
			var entityLookupOfField5 = new SearchFieldEntityLookup("IOrgColdCallRegister", "IOrgColdCallRegister", "GlbStaff", "", "");
			var field5 = new SearchField("AssignedStaff", "Assigned Staff", typeof(string), false, false, 0, entityLookup: entityLookupOfField5);
			var field6 = SearchField.Create("SalesTeam", "Sales Team");
			var entityLookupOfField7 = new SearchFieldEntityLookup("IOrgColdCallRegister", "IOrgColdCallRegister", "GlbBranch", "", "");
			var field7 = new SearchField("AssignedStaffBranch", "Assigned Staff Branch", typeof(Guid), false, false, 0, entityLookup: entityLookupOfField7);
			var field8 = SearchField.Create("ReferToOrganization", "Refer To Organization", typeof(Guid));
			var field9 = SearchField.Create("ReferringOrganization", "Referring Organization", typeof(Guid));
			var field10 = SearchField.Create("ReferToContactName", "Refer To ContactName");
			var field11 = SearchField.Create("ReferringContactName", "Referring Contact Name");
			var field12 = SearchField.Create("InquiryID", "Inquiry ID");
			var field13 = SearchField.Create("OrganizationName", "Organization Name");
			var field14 = new SearchField("OrganizationName", "Organization Name", typeof(string), true);

			var ret = new SearchFieldCollection("IOrgColdCallRegister", [field1, field2, field3, field4, field5, field6, field7, field8, field9, field10, field11, field12, field13, field14]);

			return ret;
		}

		#endregion
	}
}
