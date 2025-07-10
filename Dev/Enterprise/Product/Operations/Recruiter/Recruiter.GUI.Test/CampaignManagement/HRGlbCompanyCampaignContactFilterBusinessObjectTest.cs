using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MarketingManager.GUI;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(HRGlbCompanyCampaignContactFilterBusinessObject))]
	public class HRGlbCompanyCampaignContactFilterBusinessObjectTest : HRGlbCompanyCampaignContactFilterBusinessObjectTestBase
	{
		#region Text Filters
		#region TestCity
		public void TestCity()
		{
			var cityFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City];
			AssertNotNull(cityFilter);
			CampaignFilter.ActiveModuleFilters.DeleteAll();
			StaffMember.GS_City = "DUMMY_CITY";
			StaffMember.GS_UserAddress1 = "O'Riordan";
			Factory.Save();
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			cityFilter.Property = "DUMMY_CITY";
			cityFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			cityFilter.IsActive = true;
			string expectedSQL = "VCC_City = 'DUMMY_CITY'";
			AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, cityFilter.Query.LiteralTextADOFormatted);
			var collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(cityFilter.Query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			Campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
			Assert("Precondition", Campaign.IsUsingJobApplicantDataSource);
			Factory.Save();
			CampaignFilter = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
			cityFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City];
			cityFilter.Property = StaffMember.GS_City;
			cityFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			cityFilter.IsActive = true;
			collection.Load(CampaignFilter.Filter);
			AssertEquals("There should be no items in the list", 0, collection.Count);
			Applicant.HA_City = StaffMember.GS_City;
			Factory.Save();
			collection.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		#endregion
		#region TestContactNameFilter
		public void TestContactNameFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			StaffMember.GS_FullName = "Elon Musk";
			Factory.Save();
			var contactNameFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ContactName];
			AssertNotNull(contactNameFilter);
			contactNameFilter.Property = StaffMember.GS_FullName;
			contactNameFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			contactNameFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		#endregion
		#region TestEmailAddressFilter
		public void TestEmailAddressFilter()
		{
			var emailAddressFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.EmailAddress];
			AssertNotNull(emailAddressFilter);
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			StaffMember.GS_EmailAddress = "edward.onwodi@wisetechglobal.com";
			Factory.Save();
			emailAddressFilter.Property = StaffMember.GS_EmailAddress;
			emailAddressFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			emailAddressFilter.IsActive = true;
			var query = CampaignFilter.Filter;
			AssertContains("VCC_Email = 'edward.onwodi@wisetechglobal.com'", query.LiteralTextADOFormatted);
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		#endregion
		#region TestResultsAreDataSourceSpecific
		public void TestResultsAreDataSourceSpecific()
		{
			var emailAddressFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.EmailAddress];
			AssertNotNull(emailAddressFilter);
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			StaffMember.GS_EmailAddress = "edward.onwodi@wisetechglobal.com";
			Factory.Save();
			emailAddressFilter.Property = StaffMember.GS_EmailAddress;
			emailAddressFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			emailAddressFilter.IsActive = true;
			var query = CampaignFilter.Filter;
			AssertContains("VCC_Email = 'edward.onwodi@wisetechglobal.com'", query.LiteralTextADOFormatted);
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			SetupApplicantDataSource();
			Assert("Precondition", Campaign.IsUsingJobApplicantDataSource);
			CampaignFilter = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
			emailAddressFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.EmailAddress];
			emailAddressFilter.Property = StaffMember.GS_EmailAddress;
			emailAddressFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			emailAddressFilter.IsActive = true;
			query = CampaignFilter.Filter;
			collection.Load(query);
			AssertEquals("There should be no items in the list", 0, collection.Count);
			Applicant.HA_EmailAddress = "edward.onwodi@wisetechglobal.com";
			Factory.Save();
			collection.Load(query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		#endregion
		#endregion
		#region Staff Filters
		#region TestActiveStatusFilter
		public void TestActiveStatusFilter()
		{
			var campaignFilterForTest = new HRGlbCompanyCampaignContactFilterBusinessObjectForTest(Campaign);
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			Assert("Precondition", StaffMember.GS_IsActive);
			var activeStatusFilter = (ModuleTextFilter)campaignFilterForTest[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffActiveStatus];
			AssertNotNull(activeStatusFilter);

			AllLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					activeStatusFilter.Property = campaignFilterForTest.StatusExposed(true);
					activeStatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
					activeStatusFilter.IsActive = true;
					var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
					collection.Load(campaignFilterForTest.Filter);
					var initialCount = collection.Count;
					StaffMember.GS_IsActive = false;
					Factory.Save();
					collection.Load(campaignFilterForTest.Filter);
					AssertEquals("There should be 1 less active staff member in the list", initialCount - 1, collection.Count);
					activeStatusFilter.Property = campaignFilterForTest.StatusExposed(false);
					collection.Load(campaignFilterForTest.Filter);
					initialCount = collection.Count;
					StaffMember.GS_IsActive = true;
					Factory.Save();
					collection.Load(campaignFilterForTest.Filter);
					AssertEquals("There should be 1 less inactive staff member in the list", initialCount - 1, collection.Count);
				}
			});
		}

		#endregion
		#region TestSalesRepFilter
		public void TestSalesRepFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			var salesRepStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var salesRepStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();
			salesRepStaff1.GS_IsSalesRep = true;
			salesRepStaff2.GS_IsSalesRep = true;
			otherStaff.GS_IsSalesRep = false;
			var salesRepFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffSalesRep];
			salesRepFilter.Property = OrgConstants.FilterControl.SalesRepStatus.Code.SalesRep;
			salesRepFilter.IsActive = true;
			Factory.Save();
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered for Sales Rep staff, Collection should contain SalesRepStaff1", collection.Contains(salesRepStaff1));
			Assert("Filtered for Sales Rep staff, Collection should contain SalesRepStaff2", collection.Contains(salesRepStaff2));
			Assert("Filtered for Sales Rep staff, Collection should NOT contain OtherStaff", !collection.Contains(otherStaff));
			salesRepFilter.Property = OrgConstants.FilterControl.SalesRepStatus.Code.NonSalesRep;
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered for NOT Sales Rep staff, Collection should NOT contain SalesRepStaff1", !collection.Contains(salesRepStaff1));
			Assert("Filtered for NOT Sales Rep staff, Collection should NOT contain SalesRepStaff2", !collection.Contains(salesRepStaff2));
			Assert("Filtered for NOT Sales Rep staff, Collection should contain OtherStaff", collection.Contains(otherStaff));
			salesRepFilter.Property = OrgConstants.FilterControl.SalesRepStatus.Code.AllStaff;
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered for ALL staff, Collection should contain SalesRepStaff1", collection.Contains(salesRepStaff1));
			Assert("Filtered for ALL staff, Collection should contain SalesRepStaff2", collection.Contains(salesRepStaff2));
			Assert("Filtered for ALL staff, Collection should contain OtherStaff", collection.Contains(otherStaff));
			salesRepFilter.Property = "";
			collection.Load(CampaignFilter.Filter);
			Assert("Empty filter, Collection should contain SalesRepStaff1", collection.Contains(salesRepStaff1));
			Assert("Empty filter, Collection should contain SalesRepStaff2", collection.Contains(salesRepStaff2));
			Assert("Empty filter, Collection should contain OtherStaff", collection.Contains(otherStaff));
		}

		#endregion
		#region TestEmploymentDateFilter
		[TestDate(2018, 04, 05)]
		public void TestEmploymentDateFilter()
		{
			var anton = Factory.NewWithValidTestData<GlbStaff>();
			anton.GS_EmploymentDate = new ZDateTime(2007, 4, 2);
			var alex = Factory.NewWithValidTestData<GlbStaff>();
			alex.GS_EmploymentDate = new ZDateTime(2006, 9, 1);
			Factory.Save();
			CampaignFilter = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var employmentDateFilter = (ModuleDateFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffEmploymentDate];
			employmentDateFilter.PropertySearch = "Date range";
			employmentDateFilter.Property1 = ZDateTime.Today;
			employmentDateFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Anton", !collection.Contains(anton));
			Assert("Collection should not contain Alex", !collection.Contains(alex));
			employmentDateFilter.Property1 = new ZDateTime(2007, 1, 1);
			employmentDateFilter.Property2 = ZDateTime.Today;
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Anton", collection.Contains(anton));
			Assert("Collection should not contain Alex", !collection.Contains(alex));
		}

		#endregion
		#region TestDepartureDateFilter
		[TestDate(2018, 04, 05)]
		public void TestDepartureDateFilter()
		{
			var anton = Factory.NewWithValidTestData<GlbStaff>();
			anton.GS_DepartureDate = new ZDateTime(2007, 4, 2);
			var alex = Factory.NewWithValidTestData<GlbStaff>();
			alex.GS_DepartureDate = new ZDateTime(2006, 9, 1);
			Factory.Save();
			CampaignFilter = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var departureDateFilter = (ModuleDateFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffDepartureDate];
			departureDateFilter.PropertySearch = "Date range";
			departureDateFilter.Property1 = ZDateTime.Today;
			departureDateFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Anton", !collection.Contains(anton));
			Assert("Collection should not contain Alex", !collection.Contains(alex));
			departureDateFilter.Property1 = new ZDateTime(2007, 1, 1);
			departureDateFilter.Property2 = ZDateTime.Today;
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Anton", collection.Contains(anton));
			Assert("Collection should not contain Alex", !collection.Contains(alex));
		}

		#endregion
		#region TestNextReviewDateFilter
		[TestDate(2018, 04, 05)]
		public void TestNextReviewDateFilter()
		{
			var anton = Factory.NewWithValidTestData<GlbStaff>();
			anton.GS_NextReviewDate = new ZDateTime(2007, 4, 2);
			var alex = Factory.NewWithValidTestData<GlbStaff>();
			alex.GS_NextReviewDate = new ZDateTime(2006, 9, 1);
			Factory.Save();
			CampaignFilter = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var nextReviewDateFilter = (ModuleDateFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffNextReviewDate];
			nextReviewDateFilter.PropertySearch = "Date range";
			nextReviewDateFilter.Property1 = ZDateTime.Today;
			nextReviewDateFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Anton", !collection.Contains(anton));
			Assert("Collection should not contain Alex", !collection.Contains(alex));
			nextReviewDateFilter.Property1 = new ZDateTime(2007, 1, 1);
			nextReviewDateFilter.Property2 = ZDateTime.Today;
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Anton", collection.Contains(anton));
			Assert("Collection should not contain Alex", !collection.Contains(alex));
		}

		#endregion
		#region TestNotInTheWorkplaceFilter
		public void TestNotInTheWorkplaceFilter()
		{
			var staffUser = Factory.NewWithValidTestData<GlbStaff>();
			var staffUserHoliday = staffUser.Holidays.AddNew();
			AssertEquals(staffUserHoliday.GA_GS, staffUser.PK);
			staffUserHoliday.GA_StartTime = ZDateTime.BrettsBirthday.AddDays(-5);
			staffUserHoliday.GA_EndTime = ZDateTime.BrettsBirthday.AddDays(5);
			CampaignFilter = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var notInTheWorkplaceFilter = (ModuleDateFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffNotInTheWorkplace];
			notInTheWorkplaceFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			notInTheWorkplaceFilter.Property1 = ZDateTime.BrettsBirthday;
			notInTheWorkplaceFilter.Property2 = ZDateTime.BrettsBirthday.AddDays(2);
			notInTheWorkplaceFilter.IsActive = true;
			Factory.Save();
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("User is on holiday during the specified date range therefore staff should be in the collection", collection.Contains(staffUser));
			staffUserHoliday.GA_EndTime = ZDateTime.BrettsBirthday.AddDays(-3);
			Factory.Save();
			collection.Load(CampaignFilter.Filter);
			Assert("Holiday finishes before the specified date range therefore staff should not be in the collection", !collection.Contains(staffUser));
			staffUserHoliday.GA_EndTime = ZDateTime.BrettsBirthday;
			Factory.Save();
			collection.Load(CampaignFilter.Filter);
			Assert("Holiday finishes during the specified date range therefore staff should be in the collection", collection.Contains(staffUser));
			staffUserHoliday.GA_StartTime = ZDateTime.BrettsBirthday.AddDays(2);
			staffUserHoliday.GA_EndTime = ZDateTime.BrettsBirthday.AddDays(4);
			Factory.Save();
			collection.Load(CampaignFilter.Filter);
			Assert("Holiday starts during the specified date range therefore staff should be in the collection", collection.Contains(staffUser));
			staffUserHoliday.GA_StartTime = ZDateTime.BrettsBirthday.AddDays(3);
			Factory.Save();
			collection.Load(CampaignFilter.Filter);
			Assert("Holiday starts after the specified date range therefore staff should not be in the collection", !collection.Contains(staffUser));
		}

		#endregion
		#region TestBirthdayInFilter
		public void TestBirthdayInFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Birthdate = new ZDate(1980, 1, 6);
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Birthdate = new ZDate(1940, 1, 3);
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Birthdate = new ZDate(1940, 7, 3);
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_Birthdate = ZDate.Empty;
			Factory.Save();
			CampaignFilter = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var birthDayFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffBirthdayIn];
			birthDayFilter.Property = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[0];
			birthDayFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain staff1", collection.Contains(staff1));
			Assert("Collection should contain staff2", collection.Contains(staff2));
			Assert("Collection should NOT contain staff3", !collection.Contains(staff3));
			Assert("Collection should NOT contain staff4", !collection.Contains(staff4));
			birthDayFilter.Property = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[6];
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should NOT contain Staff1", !collection.Contains(staff1));
			Assert("Collection should NOT contain Staff2", !collection.Contains(staff2));
			Assert("Collection should contain Staff3", collection.Contains(staff3));
			Assert("Collection should NOT contain Staff4", !collection.Contains(staff4));
			birthDayFilter.Property = GlbStaffFilterProvider.AnyMonth;
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Staff1", collection.Contains(staff1));
			Assert("Collection should contain Staff2", collection.Contains(staff2));
			Assert("Collection should contain Staff3", collection.Contains(staff3));
			Assert("Collection should contain Staff4", collection.Contains(staff4));
			birthDayFilter.Property = "";
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Staff1", collection.Contains(staff1));
			Assert("Collection should contain Staff2", collection.Contains(staff2));
			Assert("Collection should contain Staff3", collection.Contains(staff3));
			Assert("Collection should contain Staff4", collection.Contains(staff4));
			birthDayFilter.Property = "13";
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Staff1", collection.Contains(staff1));
			Assert("Collection should contain Staff2", collection.Contains(staff2));
			Assert("Collection should contain Staff3", collection.Contains(staff3));
			Assert("Collection should contain Staff4", collection.Contains(staff4));
		}

		#endregion
		#region TestSecurityModifiedFilter
		public void TestSecurityModifiedFilter()
		{
			CampaignFilter = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var securityModifiedFilter = (ModuleDateFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffSecurityModified];
			AssertEquals("Security Modified filter should have HideFutureDates set to true.", true, securityModifiedFilter.HideFutureDates);
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var security1 = staff1.StaffSecurityPermissionsCollection.AddNew();
			security1.BranchCode = "BNE";
			security1.DepartmentCode = "BRN";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			securityModifiedFilter.PropertySearch = "Today";
			securityModifiedFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Staff1.", collection.Contains(staff1));
			Assert("Collection should not contain Staff.2", !collection.Contains(staff2));
		}

		#endregion
		#region TestStaffCertificateIssueDateFilter
		[TestDate(2018, 04, 05)]
		public void TestStaffCertificateIssueDateFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			var cert = StaffMember.Certificates.AddNew();
			cert.XZ_ParentTableCode = StaffMember.TablePrefix;
			cert.XZ_ParentID = StaffMember.PK;
			cert.XZ_IssueDate = ZDateTime.Today;
			Factory.Save();
			var dateFilter = (ModuleDateFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffCertificateIssueDate];
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain StaffMember", collection.Contains(StaffMember));
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Tomorrow;
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain StaffMember", !collection.Contains(StaffMember));
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = new ZDateTime(2016, 01, 10);
			dateFilter.Property2 = new ZDateTime(2016, 12, 30);
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain StaffMember", !collection.Contains(StaffMember));
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.NextCalendarMonth;
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain StaffMember", !collection.Contains(StaffMember));
			dateFilter.IsActive = false;
		}

		#endregion
		#region TestStaffCertificateExpiryDateFilter
		[TestDate(2018, 04, 05)]
		public void TestStaffCertificateExpiryDateFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			var cert = StaffMember.Certificates.AddNew();
			cert.XZ_ParentTableCode = StaffMember.TablePrefix;
			cert.XZ_ParentID = StaffMember.PK;
			cert.XZ_ExpiryOrDueDate = new ZDateTime(2017, 01, 01);
			Factory.Save();
			var dateFilter = (ModuleDateFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffCertificateExpiryDate];
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain StaffMember", !collection.Contains(StaffMember));
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Tomorrow;
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain StaffMember", !collection.Contains(StaffMember));
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.Past;
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain StaffMember", collection.Contains(StaffMember));
			dateFilter.IsActive = false;
		}

		#endregion
		#region TestCodeFilter
		public void TestCodeFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			StaffMember.GS_Code = "ELO";
			Factory.Save();
			var codeFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffCode];
			AssertNotNull(codeFilter);
			codeFilter.Property = StaffMember.GS_Code;
			codeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			codeFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		#endregion
		#region TestCodeOrFullNameFilter
		public void TestCodeOrFullNameFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			StaffMember.GS_Code = "ELO";
			StaffMember.GS_FullName = "Electronic Light Musk";
			Factory.Save();
			var codeOrFullNameFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffCodeOrFullName];
			AssertNotNull(codeOrFullNameFilter);
			codeOrFullNameFilter.Property = StaffMember.GS_Code;
			codeOrFullNameFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			codeOrFullNameFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			codeOrFullNameFilter.Property = StaffMember.GS_FullName;
			collection.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		#endregion
		#region TestDriverBranchFilter
		public void TestDriverBranchFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			var curCompany = GlbCompany.CurrentCompany;
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var curBranch = GlbBranch.CurrentBranch;
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var noGroupBranch = Factory.NewWithValidTestData<GlbBranch>();
			var otherGroupBranch = Factory.NewWithValidTestData<GlbBranch>();
			var curBranchGroup = Factory.NewWithValidTestData<GlbGroup>();
			var newBranchGroup = Factory.NewWithValidTestData<GlbGroup>();
			var nonDriverGroup = Factory.NewWithValidTestData<GlbGroup>();
			var otherCompanyGroup = Factory.NewWithValidTestData<GlbGroup>();
			var curBranchDriverStaff = Factory.NewWithValidTestData<GlbStaff>();
			var newBranchDriverStaff = Factory.NewWithValidTestData<GlbStaff>();
			var nonDriverStaff = Factory.NewWithValidTestData<GlbStaff>();
			var otherCompanyStaff = Factory.NewWithValidTestData<GlbStaff>();
			//add branches to companies
			curBranch.GB_GC = curCompany.PK;
			newBranch.GB_GC = curCompany.PK;
			otherGroupBranch.GB_GC = curCompany.PK;
			otherGroupBranch.GB_GC = otherCompany.PK;
			// add drivers to groups
			curBranchDriverStaff.Groups.Add(curBranchGroup);
			newBranchDriverStaff.Groups.Add(newBranchGroup);
			otherCompanyStaff.Groups.Add(otherCompanyGroup);
			// set up registry
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, curBranch.PK.ToGuid(), Guid.Empty, curBranchGroup.PK.ToGuid());
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, newBranchGroup.PK.ToGuid());
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, noGroupBranch.PK.ToGuid(), Guid.Empty, Guid.Empty);
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, otherGroupBranch.PK.ToGuid(), Guid.Empty, otherCompanyGroup.PK.ToGuid());
			Factory.Save();
			var driverBranchFilter = (ModuleTextFilter)CampaignFilter[StaffDriverCollection.DriverBranch];
			driverBranchFilter.IsActive = true;
			AssertEquals("Default filter should be CurrentBranch.", Env.CurrentBranch.Code, driverBranchFilter.Property);
			// All Drivers
			driverBranchFilter.Property = "All Drivers";
			var collection1 = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection1.Load(CampaignFilter.Filter);
			Assert("Collection should contain curBranchDriverStaff", collection1.Contains(curBranchDriverStaff));
			Assert("Collection should contain newBranchDriverStaff", collection1.Contains(newBranchDriverStaff));
			AssertEquals(2, collection1.Count);
			// all staff
			driverBranchFilter.Property = "All Staff";
			var collection2 = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection2.Load(CampaignFilter.Filter);
			Assert("Collection should contain curBranchDriverStaff", collection2.Contains(curBranchDriverStaff));
			Assert("Collection should contain newBranchDriverStaff", collection2.Contains(newBranchDriverStaff));
			Assert("Collection should contain nonDriverStaff", collection2.Contains(nonDriverStaff));
			Assert("Collection should contain otherCompanyStaff", collection2.Contains(otherCompanyStaff));
			// current branch code
			driverBranchFilter.IsActive = true;
			driverBranchFilter.Property = curBranch.GB_Code;
			var collection3 = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection3.Load(CampaignFilter.Filter);
			Assert("Collection should contain curBranchDriverStaff", collection3.Contains(curBranchDriverStaff));
			AssertEquals(1, collection3.Count);
			// new branch code
			driverBranchFilter.IsActive = true;
			driverBranchFilter.Property = newBranch.GB_Code;
			var collection4 = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection4.Load(CampaignFilter.Filter);
			Assert("Collection should contain newBranchDriverStaff", collection4.Contains(newBranchDriverStaff));
			AssertEquals(1, collection4.Count);
			// no group branch
			driverBranchFilter.IsActive = true;
			driverBranchFilter.Property = noGroupBranch.GB_Code;
			var collection5 = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection5.Load(CampaignFilter.Filter);
			//Not sure why doesn't work... When the same test is done in GlbStaffFilterBusinessObject, 0 objects are loaded here and 4 in the all staff section.
			Assert("Collection should not contain nonDriverStaff", !collection5.Contains(nonDriverStaff));
		}

		#endregion
		#region TestGenderFilter
		public void TestGenderFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			var femStaff = Factory.NewWithValidTestData<GlbStaff>();
			femStaff.GS_Gender = Constants.Genders.Woman;
			var manStaff = Factory.NewWithValidTestData<GlbStaff>();
			manStaff.GS_Gender = Constants.Genders.Man;
			Factory.Save();
			var genderFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffGender];
			genderFilter.Property = Constants.Genders.Woman;
			genderFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered for Female staff, Collection should contain femStaff", collection.Contains(femStaff));
			Assert("Filtered for Female staff, Collection should NOT contain manStaff", !collection.Contains(manStaff));
			genderFilter.Property = Constants.Genders.Man;
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered for Male staff, Collection should NOT contain femStaff", !collection.Contains(femStaff));
			Assert("Filtered for Male staff, Collection should contain manStaff", collection.Contains(manStaff));
		}

		#endregion
		#region TestLanguageFilter
		public void TestLanguageFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			StaffMember.GS_WorkingLanguage = SharedConstants.Languages.French;
			var grmStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			grmStaff1.GS_WorkingLanguage = SharedConstants.Languages.German;
			var grmStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			grmStaff2.GS_WorkingLanguage = SharedConstants.Languages.German;
			Factory.Save();
			var languageFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffLanguage];
			languageFilter.Property = SharedConstants.Languages.French;
			languageFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered for French language staff, Collection should contain StaffMember", collection.Contains(StaffMember));
			Assert("Filtered for French language staff, Collection should NOT contain grmStaff1", !collection.Contains(grmStaff1));
			Assert("Filtered for French language staff, Collection should NOT contain grmStaff2", !collection.Contains(grmStaff2));
			languageFilter.Property = SharedConstants.Languages.German;
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered for French language staff, Collection should NOT contain StaffMember", !collection.Contains(StaffMember));
			Assert("Filtered for French language staff, Collection should contain grmStaff1", collection.Contains(grmStaff1));
			Assert("Filtered for French language staff, Collection should contain grmStaff2", collection.Contains(grmStaff2));
		}

		#endregion
		#region Login Name
		public void TestLoginNameFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			StaffMember.GS_LoginName = "Charlie.Dog";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "CatsAre.Bad";
			Factory.Save();
			var loginNameFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffLoginName];
			loginNameFilter.Property = "Charlie";
			loginNameFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Staff1", collection.Contains(StaffMember));
			Assert("Collection should not contain Staff2", !collection.Contains(staff2));
		}

		#endregion
		#region TestCapabilityFilter
		public void TestCapabilityFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			var capability3 = Factory.NewWithValidTestData<GlbCapability>();
			var capability4 = Factory.NewWithValidTestData<GlbCapability>();
			StaffMember.Capabilities.Add(capability1);
			StaffMember.Capabilities.Add(capability2);
			staff2.Capabilities.Add(capability1);
			staff2.Capabilities.Add(capability3);
			Factory.Save();
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			var capabilityFilter = (ModuleGuidFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffCapability];
			capabilityFilter.IsActive = true;
			capabilityFilter.Property = capability1.PK;
			collection.Load(CampaignFilter.Filter);
			AssertEquals(2, collection.Count);
			Assert("Collection should contain StaffMember", collection.Contains(StaffMember));
			Assert("Collection should contain staff2", collection.Contains(staff2));
			capabilityFilter.Property = capability2.PK;
			collection.Load(CampaignFilter.Filter);
			AssertEquals(1, collection.Count);
			Assert("Collection should contain StaffMember", collection.Contains(StaffMember));
			capabilityFilter.Property = capability3.PK;
			collection.Load(CampaignFilter.Filter);
			AssertEquals(1, collection.Count);
			Assert("Collection should contain staff2", collection.Contains(staff2));
			capabilityFilter.Property = capability4.PK;
			collection.Load(CampaignFilter.Filter);
			AssertEquals("Collection should be empty", 0, collection.Count);
		}

		#endregion
		#region TestEmploymentBasisFilter
		public void TestEmploymentBasisFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			var permanentStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			permanentStaff1.GS_EmploymentBasis = "PER";
			var permanentStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			permanentStaff2.GS_EmploymentBasis = "PER";
			var casualStaff = Factory.NewWithValidTestData<GlbStaff>();
			casualStaff.GS_EmploymentBasis = "CAS";
			Factory.Save();
			var employmentBasisFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffEmploymentBasis];
			employmentBasisFilter.Property = "";
			employmentBasisFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered collection should contain staff", collection.Contains(StaffMember));
			Assert("Filtered collection should contain permanentStaff1", collection.Contains(permanentStaff1));
			Assert("Filtered collection should contain permanentStaff2", collection.Contains(permanentStaff2));
			Assert("Filtered collection should contain casualStaff", collection.Contains(casualStaff));
			((ModuleTextFilter)CampaignFilter["Employment Basis"]).Property = "PER";
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered collection should not contain staff", !collection.Contains(StaffMember));
			Assert("Filtered collection should contain permanentStaff1", collection.Contains(permanentStaff1));
			Assert("Filtered collection should contain permanentStaff2", collection.Contains(permanentStaff2));
			Assert("Filtered collection should not contain casualStaff", !collection.Contains(casualStaff));
			((ModuleTextFilter)CampaignFilter["Employment Basis"]).Property = "CAS";
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered collection should not contain staff", !collection.Contains(StaffMember));
			Assert("Filtered collection should not permanentStaff1", !collection.Contains(permanentStaff1));
			Assert("Filtered collection should not permanentStaff2", !collection.Contains(permanentStaff2));
			Assert("Filtered collection should contain casualStaff", collection.Contains(casualStaff));
		}

		#endregion
		#region TestGroupFilter
		public void TestGroupFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			StaffMember.Groups.Add(group1);
			StaffMember.Groups.Add(group2);
			staff2.Groups.Add(group1);
			Factory.Save();
			var groupMembershipFilter = (ModuleGuidFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffGroupMembership];
			groupMembershipFilter.Property = group1.PK;
			groupMembershipFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered for Group 1 staff, Collection should contain Staff1", collection.Contains(StaffMember));
			Assert("Filtered for Group 1 staff, Collection should contain Staff2", collection.Contains(staff2));
			Assert("Filtered for Group 1 staff, Collection should NOT contain Staff3", !collection.Contains(staff3));
			groupMembershipFilter.Property = group2.PK;
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered for Group 2 staff, Collection should contain Staff1", collection.Contains(StaffMember));
			Assert("Filtered for Group 2 staff, Collection should NOT contain Staff2", !collection.Contains(staff2));
			Assert("Filtered for Group 2 staff, Collection should NOT contain Staff3", !collection.Contains(staff3));
			groupMembershipFilter.Property = group3.PK;
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered for Group 3 staff, Collection should NOT contain Staff1", !collection.Contains(StaffMember));
			Assert("Filtered for Group 3 staff, Collection should NOT contain Staff2", !collection.Contains(staff2));
			Assert("Filtered for Group 3 staff, Collection should NOT contain Staff3", !collection.Contains(staff3));
			groupMembershipFilter.Property = ZGuid.Empty;
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered for Group 3 staff, Collection should contain Staff1", collection.Contains(StaffMember));
			Assert("Filtered for Group 3 staff, Collection should contain Staff2", collection.Contains(staff2));
			Assert("Filtered for Group 3 staff, Collection should contain Staff3", collection.Contains(staff3));
		}

		#endregion
		#region TestHomeBranchFilter
		public void TestHomeBranchFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			StaffMember.GS_GB_HomeBranch = Env.CurrentBranchPK;
			var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();
			otherStaff.GS_GB_HomeBranch = otherBranch.PK;
			Factory.Save();
			var codeFilter = (ModuleGuidFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffHomeBranch];
			AssertNotNull(codeFilter);
			codeFilter.Property = Env.CurrentBranchPK;
			codeFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			codeFilter.Property = otherBranch.PK;
			collection.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		#endregion
		#region TestHomeDepartmentFilter
		public void TestHomeDepartmentFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			StaffMember.GS_GE_HomeDepartment = Env.CurrentDepartmentPK;
			var otherDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();
			otherStaff.GS_GE_HomeDepartment = otherDepartment.PK;
			Factory.Save();
			var codeFilter = (ModuleGuidFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffHomeDepartment];
			AssertNotNull(codeFilter);
			codeFilter.Property = Env.CurrentDepartmentPK;
			codeFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			codeFilter.Property = otherDepartment.PK;
			collection.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		#endregion
		#region TestNationalityFilter
		public void TestNationalityFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			StaffMember.GS_RN_NKNationalityCode = "FR";
			var auNationality = Factory.NewWithValidTestData<GlbStaff>();
			auNationality.GS_RN_NKNationalityCode = "AU";
			Factory.Save();
			var nationalityFilter = (ModuleNkFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.StaffNationality];
			nationalityFilter.Property = "FR";
			nationalityFilter.IsActive = true;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered for FR Nationality, Collection should contain frNationality", collection.Contains(StaffMember));
			Assert("Filtered for FR Nationality, Collection should NOT contain auNationality", !collection.Contains(auNationality));
			nationalityFilter.Property = "AU";
			nationalityFilter.IsActive = true;
			collection.Load(CampaignFilter.Filter);
			Assert("Filtered for AU Nationality, Collection should contain auNationality", collection.Contains(auNationality));
			Assert("Filtered for AU Nationality, Collection should NOT contain frNationality", !collection.Contains(StaffMember));
		}

		#endregion
		#region TestStaffRelatedAccreditationAttemptsFilter
		public void TestStaffRelatedAccreditationAttemptsFilter()
		{
			Assert("Precondition", Campaign.IsUsingStaffDataSource);
			var staffFilter = (PersonAccreditationAttemptsFilter)CampaignFilter["Accreditation Attempts (by Staff)"];
			staffFilter.IsActive = true;
			AssertContains("VCC_TableCode = 'GS' and VCC_PK IN (SELECT GS_PK FROM dbo.GlbStaff WHERE GS_PER IN (SELECT PER_PK FROM dbo.GlbPerson WHERE PER_PK IN (SELECT HAA_PER FROM dbo.GlbAccreditationAttempt)))", CampaignFilter.Filter.LiteralTextADO);
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !Collection.Contains(StaffMember));
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			accred.StartAttempt(StaffMember.Person);
			Factory.Save();
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Applicant", Collection.Contains(StaffMember));
		}

		#endregion
		#endregion
		#region Job Applicant Filters
		#region TestApplicantTypeFilter
		public void TestApplicantTypeFilter()
		{
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Applicant.Logs.AddNew(AutoEvents.EditedARecord, "Related Contact");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			SetupApplicantDataSource();
			Factory.Save();
			var applicantTypeFilter = (ModuleFlagsFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ApplicantType];

			applicantTypeFilter.IsActive = false;
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(2, Collection.Count);

			applicantTypeFilter.IsActive = true;
			applicantTypeFilter["Learning Center User"] = false;
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(1, Collection.Count);
			Assert("Collection should contain applicant2", !Collection.Contains(applicant2));

			applicantTypeFilter["Learning Center User"] = true;
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(1, Collection.Count);
			Assert("Collection should only contain Applicant", Collection.Contains(applicant2));
		}

		#endregion
		#region TestAvailabilitiesFilter
		public void TestAvailabilitiesFilter()
		{
			SetupApplicantDataSource();
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var application = campaign.Applications.AddNew();
			application.HP_HA = Applicant.PK;
			Applicant.HA_Availability = "FUL";
			Factory.Save();
			var availabilityFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ApplicantAvailability];
			availabilityFilter.IsActive = true;
			availabilityFilter.Property = "CAS";
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(0, Collection.Count);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			availabilityFilter.Property = "FUL";
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(1, Collection.Count);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
		}

		#endregion
		#region TestHasAppliedFilter
		public void TestHasAppliedFilter()
		{
			SetupApplicantDataSource();
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var application = campaign.Applications.AddNew();
			application.HP_HA = Applicant.PK;
			Factory.Save();
			var hasAppliedFilter = (ModuleFlagsFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ApplicantHasApplied];
			hasAppliedFilter.IsActive = true;
			hasAppliedFilter.Property0 = true;
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(1, Collection.Count);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
		}

		#endregion
		#region TestRegistrationMethodFilter
		public void TestRegistrationMethodFilter()
		{
			SetupApplicantDataSource();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			Applicant.Logs.AddedLog.SL_GS_NKUser = User.WebUserCode;
			applicant2.Logs.AddedLog.SL_GS_NKUser = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();
			var registrationMethodFilter = (ModuleFlagsFilter)CampaignFilter["Registration Method"];
			registrationMethodFilter.IsActive = true;
			registrationMethodFilter["Registered via Web"] = true;
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(1, Collection.Count);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			registrationMethodFilter["Registered via Web"] = false;
			registrationMethodFilter["Registered manually"] = true;
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(1, Collection.Count);
			Assert("Collection should contain applicant2", Collection.Contains(applicant2));
			registrationMethodFilter["Registered via Web"] = true;
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(2, Collection.Count);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			Assert("Collection should contain applicant2", Collection.Contains(applicant2));
			registrationMethodFilter["Registered via Web"] = false;
			registrationMethodFilter["Registered manually"] = false;
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(2, Collection.Count);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			Assert("Collection should contain applicant2", Collection.Contains(applicant2));
		}

		#endregion
		#region TestWorkPermitStatusFilter
		public void TestWorkPermitStatusFilter()
		{
			SetupApplicantDataSource();
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var application = campaign.Applications.AddNew();
			application.HP_HA = Applicant.PK;
			Factory.Save();
			var workPermitStatusFilter = (ModuleTextFilter)CampaignFilter["Work Permit Status"];
			workPermitStatusFilter.IsActive = true;
			workPermitStatusFilter.Property = "SKL";
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(0, Collection.Count);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			workPermitStatusFilter.Property = "RES";
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(1, Collection.Count);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
		}

		#endregion
		#region TestApplicantCertificateIssueDateFilter
		[TestDate(2018, 04, 05)]
		public void TestApplicantCertificateIssueDateFilter()
		{
			SetupApplicantDataSource();
			var cert = Applicant.Certificates.AddNew();
			cert.XZ_ParentTableCode = Applicant.TablePrefix;
			cert.XZ_ParentID = Applicant.PK;
			cert.XZ_IssueDate = ZDateTime.Today;
			Factory.Save();
			var dateFilter = (ModuleDateFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ApplicantCertificateIssueDate];
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Applicant", collection.Contains(Applicant));
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Tomorrow;
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !collection.Contains(Applicant));
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = new ZDateTime(2016, 01, 10);
			dateFilter.Property2 = new ZDateTime(2016, 12, 30);
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !collection.Contains(Applicant));
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.NextCalendarMonth;
			collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !collection.Contains(Applicant));
			dateFilter.IsActive = false;
		}

		#endregion
		#region TestApplicantCertificateExpiryDateFilter
		[TestDate(2018, 04, 05)]
		public void TestApplicantCertificateExpiryDateFilter()
		{
			SetupApplicantDataSource();
			var cert = Applicant.Certificates.AddNew();
			cert.XZ_ParentTableCode = Applicant.TablePrefix;
			cert.XZ_ParentID = Applicant.PK;
			cert.XZ_ExpiryOrDueDate = new ZDateTime(2017, 01, 01);
			Factory.Save();
			var dateFilter = (ModuleDateFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ApplicantCertificateExpiryDate];
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Tomorrow;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.Past;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			dateFilter.IsActive = false;
		}

		#endregion
		#region TestSubmissionTimeFilter
		[TestDate(2015, 11, 12, 12, 0, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestSubmissionTimeFilter()
		{
			SetupApplicantDataSource();
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var application = campaign.Applications.AddNew();
			application.HP_HA = Applicant.PK;
			application.HP_SubmissionTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			var submissionTimeFilter = (ModuleDateFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ApplicantSubmissionTime];
			submissionTimeFilter.IsActive = true;
			submissionTimeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			submissionTimeFilter.Property1 = ZDateTime.UtcNow;
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(1, Collection.Count);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			submissionTimeFilter.Property1 = ZDateTime.UtcNow.AddDays(2);
			submissionTimeFilter.Property2 = ZDateTime.UtcNow.AddDays(3);
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(0, Collection.Count);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
		}

		#endregion
		#region TestApplicationStatusFilter
		public void TestApplicationStatusFilter()
		{
			SetupApplicantDataSource();
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var application = campaign.Applications.AddNew();
			application.HP_HA = Applicant.PK;
			application.HP_CurrentStatus = "INP";
			Factory.Save();
			var applicationStatusFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ApplicantApplicationStatus];
			applicationStatusFilter.IsActive = true;
			applicationStatusFilter.Property = "ST1";
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(0, Collection.Count);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			applicationStatusFilter.Property = "INP";
			Collection.Load(CampaignFilter.Filter);
			AssertEquals(1, Collection.Count);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
		}

		#endregion
		#region TestApplicantCertificatesFilter
		[TestDate(2018, 2, 2, 12, 0, 0)]
		public void TestApplicantCertificatesFilter()
		{
			SetupApplicantDataSource();
			var cert = Applicant.Certificates.AddNew();
			cert.XZ_ParentTableCode = Applicant.TablePrefix;
			cert.XZ_ParentID = Applicant.PK;
			cert.XZ_Type = "CUS";
			cert.XZ_ExpiryOrDueDate = new ZDateTime(2017, 12, 31);
			cert.XZ_RefNumber = "12345";
			Factory.Save();
			var filterA = ((ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ApplicantCertificateNumber]);
			filterA.IsActive = true;
			filterA.Property = "123";
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			filterA.IsActive = false;
			filterA.IsActive = true;
			filterA.Property = "4567";
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			filterA.IsActive = false;
			var filterB = ((ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ApplicantCertificateType]);
			filterB.Property = "CUS";
			filterB.IsActive = true;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			filterB.IsActive = false;
			var filterC = ((ModuleDateFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ApplicantCertificateExpiryDate]);
			filterC.Property1 = new ZDateTime(2017, 12, 10);
			filterC.Property2 = new ZDateTime(2017, 12, 30);
			filterC.IsActive = true;
			filterC.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			filterC.IsActive = false;
			filterC.IsActive = true;
			filterC.PropertySearch = new ZDateTime(2017, 12, 31).SqlFormat;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			filterC.IsActive = false;
		}

		#endregion
		#region TestCampaignDetailsFilter
		public void TestCampaignDetailsFilter()
		{
			SetupApplicantDataSource();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicationCampaign1 = Applicant.Applications.AddNew();
			applicationCampaign1.HP_HV = campaign1.PK;
			var applicationCampaign2 = applicant2.Applications.AddNew();
			applicationCampaign2.HP_HV = campaign2.PK;
			Factory.Save();
			var campaignDetailsFilter = (ModuleGuidFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ApplicantJobOpenings];
			Assert("This filter not supported on Web - yet", !campaignDetailsFilter.IsPublishedOnWeb);
			campaignDetailsFilter.Property = campaign1.PK;
			campaignDetailsFilter.IsActive = true;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			Assert("Collection should not contain applicant2", !Collection.Contains(applicant2));
			campaignDetailsFilter.Property = campaign2.PK;
			campaignDetailsFilter.IsActive = true;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			Assert("Collection should contain applicant2", Collection.Contains(applicant2));
			campaignDetailsFilter.Property = ZGuid.Empty;
			campaignDetailsFilter.IsActive = true;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			Assert("Collection should contain applicant2", Collection.Contains(applicant2));
			var campaign3 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			Factory.Save();
			campaignDetailsFilter.Property = campaign3.PK;
			campaignDetailsFilter.IsActive = true;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			Assert("Collection should not contain applicant2", !Collection.Contains(applicant2));
		}

		#endregion
		#region TestCountryFilter
		public void TestCountryFilter()
		{
			SetupApplicantDataSource();
			var applicant2 = Factory.New<HRJobApplicant>();
			applicant2.HA_FullName = "full name";
			var countries = Factory.Load<RefCountry>(new ZQuery());
			Applicant.HA_RN_NKCountry = countries[0].Code;
			applicant2.HA_RN_NKCountry = countries[1].Code;
			Factory.Save();
			var countryFilter = (ModuleTextFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ApplicantCountry];
			countryFilter.Property = countries[0].Code;
			countryFilter.IsActive = true;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			Assert("Collection should not contain applicant2", !Collection.Contains(applicant2));
			countryFilter.Property = countries[1].Code;
			countryFilter.IsActive = true;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			Assert("Collection should contain applicant2", Collection.Contains(applicant2));
			countryFilter.Property = ZString.Empty;
			countryFilter.IsActive = true;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			Assert("Collection should contain applicant2", Collection.Contains(applicant2));
			countryFilter.Property = countries[2].Code;
			countryFilter.IsActive = true;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			Assert("Collection should not contain applicant2", !Collection.Contains(applicant2));
		}

		#endregion
		#region TestJobRoleFilter
		public void TestJobRoleFilter()
		{
			SetupApplicantDataSource();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			var jobRole1 = Factory.NewWithValidTestData<HRJobRole>();
			var jobRole2 = Factory.NewWithValidTestData<HRJobRole>();
			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign1.HV_HJ_JobRole = jobRole1.PK;
			campaign2.HV_HJ_JobRole = jobRole2.PK;
			var applicationCampaign1 = Applicant.Applications.AddNew();
			applicationCampaign1.HP_HV = campaign1.PK;
			var applicationCampaign2 = applicant2.Applications.AddNew();
			applicationCampaign2.HP_HV = campaign2.PK;
			Factory.Save();
			var jobRoleFilter = (ModuleGuidFilter)CampaignFilter[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.ApplicantJobRole];
			Assert("This filter not supported on Web - yet", !jobRoleFilter.IsPublishedOnWeb);
			jobRoleFilter.Property = jobRole1.PK;
			jobRoleFilter.IsActive = true;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			Assert("Collection should not contain applicant2", !Collection.Contains(applicant2));
			jobRoleFilter.Property = jobRole2.PK;
			jobRoleFilter.IsActive = true;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			Assert("Collection should contain applicant2", Collection.Contains(applicant2));
			jobRoleFilter.Property = ZGuid.Empty;
			jobRoleFilter.IsActive = true;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
			Assert("Collection should contain applicant2", Collection.Contains(applicant2));
			var jobRole3 = Factory.NewWithValidTestData<HRJobRole>();
			Factory.Save();
			jobRoleFilter.Property = jobRole3.PK;
			jobRoleFilter.IsActive = true;
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			Assert("Collection should not contain applicant2", !Collection.Contains(applicant2));
		}

		#endregion
		#region TestApplicantRelatedAccreditationAttemptsFilter
		public void TestApplicantRelatedAccreditationAttemptsFilter()
		{
			SetupApplicantDataSource();
			var applicantFilter = (PersonAccreditationAttemptsFilter)CampaignFilter["Accreditation Attempts (by Applicant)"];
			applicantFilter.IsActive = true;
			AssertContains("VCC_TableCode = 'HA' and VCC_PK IN (SELECT HA_PK FROM dbo.HRJobApplicant WHERE HA_PER IN (SELECT PER_PK FROM dbo.GlbPerson WHERE PER_PK IN (SELECT HAA_PER FROM dbo.GlbAccreditationAttempt)))", CampaignFilter.Filter.LiteralTextADO);
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should not contain Applicant", !Collection.Contains(Applicant));
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			accred.StartAttempt(Applicant.Person);
			Factory.Save();
			Collection.Load(CampaignFilter.Filter);
			Assert("Collection should contain Applicant", Collection.Contains(Applicant));
		}

		#endregion
		void SetupApplicantDataSource()
		{
			Campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
			Factory.Save();
		}

		#endregion
		public void TestCampaignTransitioning()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@test.com";
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;
			var master = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";
			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1.G0_CampaignName = "touch1";
			touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "a";
			touch1.G0_G0_Master = master.PK;
			touch1.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			master.AllTouches.Add(touch1);
			var touch2A = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			touch2A.G0_CampaignName = "touch2a";
			touch2A.G0_Type = "PREAP";
			touch2A.G0_Category = "PRINT";
			touch2A.G0_EstimatedStartedDate = ZDateTime.Now;
			touch2A.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch2A.G0_GS_NKCampaignManager = staff.GS_Code;
			touch2A.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			touch2A.SourceCampaignPK = touch1.PK;
			touch2A.HtmlDocumentBlob = ZBlob.FromAscii("click me");
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "a";
			touch2A.G0_G0_Master = master.PK;
			var touch2B = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			touch2B.G0_CampaignName = "touch2b";
			touch2B.G0_Type = "PREAP";
			touch2B.G0_Category = "PRINT";
			touch2B.G0_EstimatedStartedDate = ZDateTime.Now;
			touch2B.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch2B.G0_GS_NKCampaignManager = staff.GS_Code;
			touch2B.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			touch2B.SourceCampaignPK = touch1.PK;
			touch2B.HtmlDocumentBlob = ZBlob.FromAscii("I'm here");
			touch2B.G0_HorizontalId = 2;
			touch2B.G0_VerticalId = "b";
			touch2B.G0_G0_Master = master.PK;
			var contact1 = Factory.NewWithValidTestData<GlbStaff>();
			contact1.GS_EmailAddress = "contact1@test.com";
			var contact2 = Factory.NewWithValidTestData<GlbStaff>();
			contact2.GS_EmailAddress = "contact2@test.com";
			var item2A = Factory.New<GlbCompanyCampaignItem>();
			item2A.G8_RecipientTableCode = contact1.TablePrefix;
			item2A.G8_RecipientID = contact1.PK;
			item2A.G8_G0 = touch1.PK;
			var item2B = Factory.New<GlbCompanyCampaignItem>();
			item2B.G8_RecipientTableCode = contact2.TablePrefix;
			item2B.G8_RecipientID = contact2.PK;
			item2B.G8_G0 = touch1.PK;
			master.AllTouches.Add(touch2A);
			master.AllTouches.Add(touch2B);
			var rule_2A = touch2A.TransitionRulesToThisCampaign[0];
			var rule_2B = touch2B.TransitionRulesToThisCampaign[0];
			touch2A.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.FIX;
			touch2A.SendSettings.GSC_ScheduleTime = new ZDateTime(2017, 1, 1);
			touch2B.SendSettings.DaysOffset = 5;
			touch2B.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.DEL;
			touch2B.SendSettings.IsUseCurrentTime = true;
			touch2A.SourceCampaignPK = touch1.PK;
			touch2B.SourceCampaignPK = touch1.PK;
			var filterBizObj_2A = new HRGlbCompanyCampaignContactFilterBusinessObject(touch2A);
			((ISetCampaignFilterLayoutContext)filterBizObj_2A).SetContext();
			var strip_2A = filterBizObj_2A.FilterStrips.AddNew("Email Address");
			var filter_2A = (ModuleTextFilter)strip_2A.CurrentModuleFilter;
			filter_2A.IsActive = true;
			filter_2A.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter_2A.Property = "contact1";
			filterBizObj_2A.FillLayoutValues(rule_2A.FilterRule, ModuleIDs.DripMarketingFilterRuleHR);
			var filterBizObj_2B = new HRGlbCompanyCampaignContactFilterBusinessObject(touch2B);
			((ISetCampaignFilterLayoutContext)filterBizObj_2B).SetContext();
			var strip_2B = filterBizObj_2B.FilterStrips.AddNew("Email Address");
			var filter_2B = (ModuleTextFilter)strip_2B.CurrentModuleFilter;
			filter_2B.IsActive = true;
			filter_2B.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter_2B.Property = "contact2";
			filterBizObj_2B.FillLayoutValues(rule_2B.FilterRule, ModuleIDs.DripMarketingFilterRuleHR);
			Factory.Save();
			touch1.CampaignsItemsSent.Reload(true);
			touch1.TransitionAndSchedule();
			var newFactory = new BusinessObjectFactory();
			touch2A = newFactory.Load<HRGlbCompanyCampaign>(touch2A.PK);
			touch2B = newFactory.Load<HRGlbCompanyCampaign>(touch2B.PK);
			AssertEquals(1, touch2A.CampaignsItemsSent.Count);
			AssertEquals(1, touch2B.CampaignsItemsSent.Count);
			AssertEquals(contact1.PK, touch2A.CampaignsItemsSent[0].Recipient.PK);
			AssertEquals(contact2.PK, touch2B.CampaignsItemsSent[0].Recipient.PK);
			AssertEquals(new ZDateTime(2017, 1, 1) /*.ToUniversalBranchTime()*/, touch2A.CampaignsItemsSent[0].ScheduleTimeUtc);
			AssertZDatesWithin5Minutes("", ZDateTime.UtcNow.AddHours(touch2B.SendSettings.GSC_HoursOffset), touch2B.CampaignsItemsSent[0].ScheduleTimeUtc);
			//retransfer
			contact2.GS_EmailAddress = "contact12@test.com";
			touch1.CampaignsItemsSent[1].RecipientFromView.VCC_Email = "contact12@test.com";
			Factory.Save();
			touch1.TransitionAndSchedule();
			touch1.CampaignsItemsSent.Reload(true);
			newFactory = new BusinessObjectFactory();
			touch2A = newFactory.Load<HRGlbCompanyCampaign>(touch2A.PK);
			touch2B = newFactory.Load<HRGlbCompanyCampaign>(touch2B.PK);
			var recipientPks = touch2A.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().Select(item => item.Recipient.PK).ToArray();
			AssertEquals(2, touch2A.CampaignsItemsSent.Count);
			AssertEquals(0, touch2B.CampaignsItemsSent.Count);
			AssertCollectionContains(contact1.PK, recipientPks);
			AssertCollectionContains(contact2.PK, recipientPks);
			AssertEquals(new ZDateTime(2017, 1, 1) /*.ToUniversalBranchTime()*/, touch2A.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().First(x => x.Recipient.PK == contact1.PK).ScheduleTimeUtc);
			AssertZDatesWithin5Minutes("", ZDateTime.Empty, touch2A.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().First(x => x.Recipient.PK == contact2.PK).ScheduleTimeUtc);
		}

		public void TestSensitiveFiltersWithSecurityRights()
		{
			var filterStripBusinessObject = new HRGlbCompanyCampaignContactFilterBusinessObject(null);
			AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City, filterStripBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City]);
			AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State, filterStripBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State]);

			Campaign.ContactDataSource = HRContactDataSourceList.Codes.CampaignTracking;
			filterStripBusinessObject = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);

			AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City, filterStripBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City]);
			AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State, filterStripBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State]);

			Campaign.ContactDataSource = HRContactDataSourceList.Codes.Staff;
			Assert("Precondition", Campaign.IsUsingStaffDataSource);

			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				filterStripBusinessObject = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
				security.StaffViewOtherStaffDetails.IsAllowed = false;
				AssertNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City, filterStripBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City]);
				AssertNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State, filterStripBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State]);

				filterStripBusinessObject = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
				security.StaffViewOtherStaffDetails.IsAllowed = true;
				AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City, filterStripBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City]);
				AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State, filterStripBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State]);
			}

			Campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
			Assert("Precondition", Campaign.IsUsingJobApplicantDataSource);
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				filterStripBusinessObject = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
				security.HRJobApplicantView.IsAllowed = false;
				AssertNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City, filterStripBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City]);
				AssertNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State, filterStripBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State]);

				filterStripBusinessObject = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
				security.HRJobApplicantView.IsAllowed = true;
				AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City, filterStripBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City]);
				AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State, filterStripBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State]);
			}
		}
	}
}
