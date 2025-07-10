using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbStaffFilterBusinessObject))]
	sealed class GlbStaffFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = new List<Tuple<string, string>>();

			result.Add(TableFilter("GlbStaff", "Group Membership"));
			result.Add(TableFilter(GlbGroupSchema.Constants.TableName, "Group Membership"));
			result.Add(TableFilter(GlbGroupLinkSchema.Constants.TableName, "Group Membership"));

			return result;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var result = new List<Tuple<string, string>>();

			result.Add(TableFilter("GlbStaff", "Group Membership"));
			result.Add(TableFilter(GlbGroupSchema.Constants.TableName, "Group Membership"));
			result.Add(TableFilter(GlbGroupLinkSchema.Constants.TableName, "Group Membership"));

			return result;
		}

		#region Filters

		#region TestSupportUserFilter

		public void TestSupportUserFilter()
		{
			var cwSupport = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, User.SupportUserName));

			var filter = new GlbStaffFilterBusinessObject();
			var collection = new GlbStaffCollection(Factory, filter.Filter);

			Assert(collection.Contains(cwSupport));
		}

		#endregion

		#region TestLeaveDateRangeWithTypeFilter

		public void TestLeaveDateRangeWithTypeFilter()
		{
			var filterBizO = new GlbStaffFilterBusinessObject();
			AssertNotNull((LeaveDateRangeWithTypeFilter)filterBizO["Leave Type and Date"]);
		}

		#endregion

		#region TestLastLoginDateFilter

		[TestUtcOffset(5, 0, 0)]
		public void TestLastLoginDateFilter()
		{
			var anton = Factory.NewWithValidTestData<GlbStaff>();
			anton.GS_LastActivityDate = new ZDateTime(2007, 4, 2); //5:00 in local time, 0:00 in UTC

			var alex = Factory.NewWithValidTestData<GlbStaff>();
			alex.GS_LastActivityDate = new ZDateTime(2006, 9, 1);

			Factory.Save();

			var filter1 = new GlbStaffFilterBusinessObject();
			((ModuleDateFilter)filter1["Last Login Date"]).PropertySearch = "Time range";
			((ModuleDateFilter)filter1["Last Login Date"]).Property1 = new ZDateTime(2007, 4, 2, 4, 0, 0); //4:00 in local time, 23:00 in UTC
			((ModuleDateFilter)filter1["Last Login Date"]).Property2 = new ZDateTime(2007, 4, 2, 6, 0, 0); //6:00 in local time, 1:00 in UTC
			((ModuleDateFilter)filter1["Last Login Date"]).IsActive = true;

			var col1 = new GlbStaffCollection(Factory, filter1.Filter);

			Assert(col1.Contains(anton));
			Assert(!col1.Contains(alex));

			filter1 = new GlbStaffFilterBusinessObject();
			((ModuleDateFilter)filter1["Last Login Date"]).PropertySearch = "Time range";
			((ModuleDateFilter)filter1["Last Login Date"]).Property1 = new ZDateTime(2007, 4, 1, 23, 0, 0); //23:00 in local time, 18:00 in UTC
			((ModuleDateFilter)filter1["Last Login Date"]).Property2 = new ZDateTime(2007, 4, 2, 1, 0, 0); //1:00 in local time, 20:00 in UTC 
			((ModuleDateFilter)filter1["Last Login Date"]).IsActive = true;

			col1 = new GlbStaffCollection(Factory, filter1.Filter);

			Assert(!col1.Contains(anton));
			Assert(!col1.Contains(alex));
		}

		#endregion

		#region TestTextFilters

		#region TestIdpIdFilter

		public void TestIdpIdFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "asdf@gmail.com";

			Factory.Save();

			staff1.Person.PER_IDPUserId = ZGuid.Empty;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "bcdfg@gmail.com";

			Factory.Save();

			staff2.Person.PER_IDPUserId = ZGuid.NewZGuid();

			Factory.Save();

			var glbStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)glbStaffFilter["Identity Provider Id"]).Property = "";
			((ModuleTextFilter)glbStaffFilter["Identity Provider Id"]).IsActive = true;

			var collection = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should contain Staff1", collection.Contains(staff1));
			Assert("Collection should contain Staff2", collection.Contains(staff2));

			((ModuleTextFilter)glbStaffFilter["Identity Provider Id"]).Property = ZGuid.NewZGuid().ToString();
			((ModuleTextFilter)glbStaffFilter["Identity Provider Id"]).IsActive = true;

			collection = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should not contain Staff1", !collection.Contains(staff1));
			Assert("Collection should not contain Staff2", !collection.Contains(staff2));

			((ModuleTextFilter)glbStaffFilter["Identity Provider Id"]).Property = staff2.Person.PER_IDPUserId.ToString();

			collection = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should not contain Staff1", !collection.Contains(staff1));
			Assert("Collection should contain Staff2", collection.Contains(staff2));

			((ModuleTextFilter)glbStaffFilter["Identity Provider Id"]).SqlComparisonOperator = SQLComparisonOperator.IsBlank;

			collection = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should contain Staff1", collection.Contains(staff1));
			Assert("Collection should not contain Staff2", !collection.Contains(staff2));

			((ModuleTextFilter)glbStaffFilter["Identity Provider Id"]).SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;

			collection = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should not contain Staff1", !collection.Contains(staff1));
			Assert("Collection should contain Staff2", collection.Contains(staff2));
		}

		#endregion

		#region TestJobTitleFilter

		public void TestJobTitle()
		{
			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var staffWithoutEp = otherFactory.NewWithValidTestData<GlbStaff>();
			staffWithoutEp.GS_FullName = nameof(staffWithoutEp);

			var staffWithEp = otherFactory.NewWithValidTestData<GlbStaff>();
			staffWithEp.GS_FullName = nameof(staffWithEp);
			staffWithEp.GS_Title = "Senior Dude";

			var nonMatching = otherFactory.NewWithValidTestData<GlbStaff>();
			nonMatching.GS_FullName = nameof(nonMatching);
			nonMatching.GS_Title = "Something else";

			TestConnection.ExecuteNonQuery($"IF (OBJECT_ID(N'[dbo].[TG_{GlbStaffSchema.Constants.TableName}_UpdateAutoVersion]') IS NOT NULL) DISABLE TRIGGER [dbo].[TG_{GlbStaffSchema.Constants.TableName}_UpdateAutoVersion] ON [dbo].[{GlbStaffSchema.Constants.TableName}]");

			otherFactory.Save();
			SetTitleInDb(staffWithEp, string.Empty);
			SetTitleInDb(staffWithoutEp, "Senior Dude");

			var filter = new GlbStaffFilterBusinessObject();
			var titleFilter = (ModuleTextFilter)filter["Title"];
			titleFilter.IsActive = true;
			titleFilter.Property = "Senior Dude";

			var result = new GlbStaffCollection(Factory, filter.Filter).Select(staff => staff.GS_FullName);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Should grab GlbStaff records with GS_Title='search filter'", nameof(staffWithoutEp), result);
				AssertCollectionNotContains("Should not grab GlbStaff records with an employment position title='search filter'", nameof(staffWithEp), result);
				AssertCollectionNotContains("Should not grab GlbStaff that don't match the filter", nameof(nonMatching), result);
			});
		}

		void SetTitleInDb(GlbStaff staff, string title)
			=> AssertEquals("Command should have changed something", 1, TestConnection.ExecuteNonQuery($"UPDATE {GlbStaffSchema.Constants.SqlSchemaName}.{GlbStaffSchema.Constants.TableName} SET {GlbStaffSchema.Constants.GS_Title}='{title}', GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E' WHERE GS_PK={staff.PK.ToSqlGuid()}"));

		#endregion

		#region Email Address

		public void TestEmailAddressFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "asdf@gmail.com";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "bcdfg@gmail.com";

			Factory.Save();

			var glbStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)glbStaffFilter["Email Address"]).Property = "asdf";
			((ModuleTextFilter)glbStaffFilter["Email Address"]).IsActive = true;

			var collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should contain Staff1", collection1.Contains(staff1));
			Assert("Collection should not contain Staff2", !collection1.Contains(staff2));
		}
		#endregion

		#region Login Name

		public void TestLoginNameFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "Charlie.Dog";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "CatsAre.Bad";

			Factory.Save();

			var glbStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)glbStaffFilter["Login Name"]).Property = "Charlie";
			((ModuleTextFilter)glbStaffFilter["Login Name"]).IsActive = true;

			var collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should contain Staff1", collection1.Contains(staff1));
			Assert("Collection should not contain Staff2", !collection1.Contains(staff2));
		}

		#endregion

		#region Full Name

		public void TestFullNameFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "Person One";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "Individual Two";

			Factory.Save();

			var glbStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)glbStaffFilter["Full Name"]).Property = "Person";
			((ModuleTextFilter)glbStaffFilter["Full Name"]).IsActive = true;

			var collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should contain Staff1", collection1.Contains(staff1));
			Assert("Collection should not contain Staff2", !collection1.Contains(staff2));
		}

		public void TestFullNameAccentsExcluded()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Âbleton";
			Factory.Save();

			var filterBizO = (GlbStaffFilterBusinessObject)CachedBusinessObject;
			filterBizO["Full Name (Accents Excluded)"].IsActive = true;
			((ModuleTextFilter)filterBizO["Full Name (Accents Excluded)"]).Property = "Ableton";
			var collection = new GlbStaffCollection(Factory, filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(staff, collection);

			filterBizO["Full Name (Accents Excluded)"].IsActive = false;
			filterBizO["Full Name"].IsActive = true;
			((ModuleTextFilter)filterBizO["Full Name"]).Property = "Ableton";
			collection = new GlbStaffCollection(Factory, filterBizO.Filter);
			AssertEquals(0, collection.Count);
		}
		#endregion

		#region Mobile Phone

		public void TestMobilePhoneFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_MobilePhone = "9999999999";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_MobilePhone = "8888888888";

			Factory.Save();

			var glbStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)glbStaffFilter["Mobile Phone"]).Property = "9999999999";
			((ModuleTextFilter)glbStaffFilter["Mobile Phone"]).IsActive = true;

			var collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should contain Staff1", collection1.Contains(staff1));
			Assert("Collection should not contain Staff2", !collection1.Contains(staff2));
		}
		#endregion

		#region Work Extension

		public void TestWorkExtensionFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_WorkExtension = "21";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_WorkExtension = "32";

			Factory.Save();

			var glbStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)glbStaffFilter["Work Extension"]).Property = "21";
			((ModuleTextFilter)glbStaffFilter["Work Extension"]).IsActive = true;

			var collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should contain Staff1", collection1.Contains(staff1));
			Assert("Collection should not contain Staff2", !collection1.Contains(staff2));
		}
		#endregion

		#region Fax Number

		public void TestFaxNumberFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FaxNum = "1111111111";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FaxNum = "2222222222";

			Factory.Save();

			var glbStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)glbStaffFilter["Fax Number"]).Property = "1111111111";
			((ModuleTextFilter)glbStaffFilter["Fax Number"]).IsActive = true;

			var collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should contain Staff1", collection1.Contains(staff1));
			Assert("Collection should not contain Staff2", !collection1.Contains(staff2));
		}
		#region Home Phone Number

		public void TestHomePhoneNumberFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_HomePhone = "77777777";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_HomePhone = "66666666";

			Factory.Save();

			var glbStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)glbStaffFilter["Home Phone Number"]).Property = "77777777";
			((ModuleTextFilter)glbStaffFilter["Home Phone Number"]).IsActive = true;

			var collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should contain Staff1", collection1.Contains(staff1));
			Assert("Collection should not contain Staff2", !collection1.Contains(staff2));
		}

		#endregion

		#endregion

		#region TestLanguageFilter

		public void TestLanguageFilter()
		{
			var frnStaff = Factory.NewWithValidTestData<GlbStaff>();
			frnStaff.GS_WorkingLanguage = Constants.Languages.French;

			var grmStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			grmStaff1.GS_WorkingLanguage = Constants.Languages.German;
			var grmStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			grmStaff2.GS_WorkingLanguage = Constants.Languages.German;

			Factory.Save();

			var filter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)filter["Language"]).Property = Constants.Languages.French;
			((ModuleTextFilter)filter["Language"]).IsActive = true;
			var result = new GlbStaffCollection(Factory, filter.Filter);
			Assert("Filtered for French language staff, Collection should contain frnStaff", result.Contains(frnStaff));
			Assert("Filtered for French language staff, Collection should NOT contain grmStaff1", !result.Contains(grmStaff1));
			Assert("Filtered for French language staff, Collection should NOT contain grmStaff2", !result.Contains(grmStaff2));

			((ModuleTextFilter)filter["Language"]).Property = Constants.Languages.German;
			((ModuleTextFilter)filter["Language"]).IsActive = true;
			result = new GlbStaffCollection(Factory, filter.Filter);
			Assert("Filtered for French language staff, Collection should NOT contain frnStaff", !result.Contains(frnStaff));
			Assert("Filtered for French language staff, Collection should contain grmStaff1", result.Contains(grmStaff1));
			Assert("Filtered for French language staff, Collection should contain grmStaff2", result.Contains(grmStaff2));
		}

		#endregion

		#region TestGenderFilter

		public void TestGenderFilter()
		{
			var femStaff = Factory.NewWithValidTestData<GlbStaff>();
			femStaff.GS_Gender = Constants.Genders.Woman;

			var manStaff = Factory.NewWithValidTestData<GlbStaff>();
			manStaff.GS_Gender = Constants.Genders.Man;

			Factory.Save();

			var filter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)filter["Gender"]).Property = Constants.Genders.Woman;
			((ModuleTextFilter)filter["Gender"]).IsActive = true;
			var result = new GlbStaffCollection(Factory, filter.Filter);
			Assert("Filtered for Female staff, Collection should contain femStaff", result.Contains(femStaff));
			Assert("Filtered for Female staff, Collection should NOT contain manStaff", !result.Contains(manStaff));

			((ModuleTextFilter)filter["Gender"]).Property = Constants.Genders.Man;
			((ModuleTextFilter)filter["Gender"]).IsActive = true;
			result = new GlbStaffCollection(Factory, filter.Filter);
			Assert("Filtered for Male staff, Collection should NOT contain femStaff", !result.Contains(femStaff));
			Assert("Filtered for Male staff, Collection should contain manStaff", result.Contains(manStaff));
		}

		#endregion

		#region TestOtherReference
		public void TestOtherReference()
		{
			GlbStaff anton = Factory.NewWithValidTestData<GlbStaff>();
			anton.GS_Pager = "OtherReference1";

			GlbStaff alex = Factory.NewWithValidTestData<GlbStaff>();
			alex.GS_Pager = "OtherReference2";

			Factory.Save();

			GlbStaffFilterBusinessObject filter1 = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)filter1["Other References"]).Property = "OtherReference";
			((ModuleTextFilter)filter1["Other References"]).IsActive = true;

			GlbStaffCollection col1 = new GlbStaffCollection(Factory, filter1.Filter);
			Assert(col1.Contains(anton));
			Assert(col1.Contains(alex));

			GlbStaffFilterBusinessObject filter2 = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)filter2["Other References"]).Property = "OtherReference1";
			((ModuleTextFilter)filter2["Other References"]).IsActive = true;

			GlbStaffCollection col2 = new GlbStaffCollection(Factory, filter2.Filter);
			Assert(col2.Contains(anton));
			Assert(!col2.Contains(alex));

			GlbStaffFilterBusinessObject filter3 = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)filter3["Other References"]).Property = "OtherReference2";
			((ModuleTextFilter)filter3["Other References"]).IsActive = true;

			GlbStaffCollection col3 = new GlbStaffCollection(Factory, filter3.Filter);
			Assert(!col3.Contains(anton));
			Assert(col3.Contains(alex));
		}

		#endregion

		#region ExternalIdFilter

		public void TestExternalIdFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_ExternalId = "1";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_ExternalId = "12";

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_ExternalId = string.Empty;
			Factory.Save();

			var cwSupport = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, User.SupportUserName));

			var glbStaffFilter = new GlbStaffFilterBusinessObject();
			var externalIdFilter = ((ModuleTextFilter)glbStaffFilter["External Id"]);
			externalIdFilter.Property = "1";
			externalIdFilter.IsActive = true;
			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should contain Group1", collection1.Contains(staff1));

			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { staff2, staff3, cwSupport }, collection1);

			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { staff3, cwSupport }, collection1);

			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { staff1, staff2 }, collection1);

			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { staff1, staff2 }, collection1);

			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { staff3, cwSupport }, collection1);

			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { staff1, staff2 }, collection1);
		}

		#endregion

		#endregion

		#region TestEmployedDate

		public void TestEmployedDate()
		{
			var anton = Factory.NewWithValidTestData<GlbStaff>();
			anton.GS_EmploymentDate = new ZDateTime(2007, 4, 2);

			var alex = Factory.NewWithValidTestData<GlbStaff>();
			alex.GS_EmploymentDate = new ZDateTime(2006, 9, 1);

			Factory.Save();

			var filter1 = new GlbStaffFilterBusinessObject();
			((ModuleDateFilter)filter1["Employment Date"]).PropertySearch = "Date range";
			((ModuleDateFilter)filter1["Employment Date"]).Property1 = ZDateTime.Today;
			((ModuleDateFilter)filter1["Employment Date"]).IsActive = true;

			var col1 = new GlbStaffCollection(Factory, filter1.Filter);

			Assert(!col1.Contains(anton));
			Assert(!col1.Contains(alex));

			var filter2 = new GlbStaffFilterBusinessObject();
			((ModuleDateFilter)filter2["Employment Date"]).PropertySearch = "Date range";
			((ModuleDateFilter)filter2["Employment Date"]).Property1 = new ZDateTime(2007, 1, 1);
			((ModuleDateFilter)filter2["Employment Date"]).Property2 = ZDateTime.Today;
			((ModuleDateFilter)filter2["Employment Date"]).IsActive = true;

			var col2 = new GlbStaffCollection(Factory, filter2.Filter);

			Assert(col2.Contains(anton));
			Assert(!col2.Contains(alex));
		}

		#endregion

		#region TestBirthdayDateFilterDateRangeOverride

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestBirthdayDateFilterDateRangeOverride()
		{
			ZDateTime oldDate = DateTime.Today.AddYears(-20);
			GlbStaffFilterBusinessObject filter1 = new GlbStaffFilterBusinessObject();
			ModuleDateFilter birthdayFilter = (ModuleDateFilter)filter1["Birth Date"];
			birthdayFilter.PropertySearch = "Date range";
			birthdayFilter.Property1 = oldDate;
			birthdayFilter.Property2 = oldDate;
			birthdayFilter.IsActive = true;
			birthdayFilter.Property1Validation = null;
			birthdayFilter.Property2Validation = null;

			string dateTimeString = oldDate.ToString("dd-MMM-yyyy");
			string oldErrorText = string.Format("The date '{0}' is more than {1} years old and thus is not valid.", dateTimeString, "10");
			birthdayFilter.Validation.ValidateProperty1();
			birthdayFilter.Validation.ValidateProperty2();
			AssertNoErrors(birthdayFilter.Property1Info);
			AssertNoErrors(birthdayFilter.Property2Info);
		}

		#endregion

		#region TestEmploymentDateFilterDateRangeOverride

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestEmploymentDateFilterDateRangeOverride()
		{
			ZDateTime oldDate = DateTime.Today.AddYears(-20);
			GlbStaffFilterBusinessObject filter1 = new GlbStaffFilterBusinessObject();
			ModuleDateFilter employmentFilter = (ModuleDateFilter)filter1["Employment Date"];
			employmentFilter.PropertySearch = "Date range";
			employmentFilter.Property1 = oldDate;
			employmentFilter.Property2 = oldDate;
			employmentFilter.IsActive = true;
			employmentFilter.Property1Validation = null;
			employmentFilter.Property2Validation = null;

			string dateTimeString = oldDate.ToString("dd-MMM-yyyy");
			string oldErrorText = string.Format("The date '{0}' is more than {1} years old and thus is not valid.", dateTimeString, "10");
			employmentFilter.Validation.ValidateProperty1();
			employmentFilter.Validation.ValidateProperty2();
			AssertNoErrors(employmentFilter.Property1Info);
			AssertNoErrors(employmentFilter.Property2Info);
		}

		#endregion

		#region TestDepartureDateFilterDateRangeOverride

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestDepartureDateFilterDateRangeOverride()
		{
			ZDateTime oldDate = DateTime.Today.AddYears(-20);
			GlbStaffFilterBusinessObject filter1 = new GlbStaffFilterBusinessObject();
			ModuleDateFilter departureFilter = (ModuleDateFilter)filter1["Departure Date"];
			departureFilter.PropertySearch = "Date range";
			departureFilter.Property1 = oldDate;
			departureFilter.Property2 = oldDate;
			departureFilter.IsActive = true;
			departureFilter.Property1Validation = null;
			departureFilter.Property2Validation = null;

			string dateTimeString = oldDate.ToString("dd-MMM-yyyy");
			string oldErrorText = string.Format("The date '{0}' is more than {1} years old and thus is not valid.", dateTimeString, "10");
			departureFilter.Validation.ValidateProperty1();
			departureFilter.Validation.ValidateProperty2();
			AssertNoErrors(departureFilter.Property1Info);
			AssertNoErrors(departureFilter.Property2Info);
		}

		#endregion

		#region TestSearchByBirthMonth

		public void TestSearchByBirthMonth()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Birthdate = new ZDate(1980, 1, 6);

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Birthdate = new ZDate(1940, 1, 3);

			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Birthdate = new ZDate(1940, 7, 3);

			GlbStaff staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_Birthdate = ZDate.Empty;

			Factory.Save();

			GlbStaffFilterBusinessObject januaryBirthDayFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)januaryBirthDayFilter["Birthday In"]).Property = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[0];
			((ModuleTextFilter)januaryBirthDayFilter["Birthday In"]).IsActive = true;

			GlbStaffCollection collection1 = new GlbStaffCollection(Factory, januaryBirthDayFilter.Filter);

			Assert("Collection should contain Staff1", collection1.Contains(staff1));
			Assert("Collection should contain Staff2", collection1.Contains(staff2));
			Assert("Collection should NOT contain Staff3", !collection1.Contains(staff3));
			Assert("Collection should NOT contain Staff4", !collection1.Contains(staff4));

			GlbStaffFilterBusinessObject julyBirthDayFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)julyBirthDayFilter["Birthday In"]).Property = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[6];
			((ModuleTextFilter)julyBirthDayFilter["Birthday In"]).IsActive = true;

			collection1 = new GlbStaffCollection(Factory, julyBirthDayFilter.Filter);

			Assert("Collection should NOT contain Staff1", !collection1.Contains(staff1));
			Assert("Collection should NOT contain Staff2", !collection1.Contains(staff2));
			Assert("Collection should contain Staff3", collection1.Contains(staff3));
			Assert("Collection should NOT contain Staff4", !collection1.Contains(staff4));

			GlbStaffFilterBusinessObject anyFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)anyFilter["Birthday In"]).Property = julyBirthDayFilter.StaffFilterProvider.BirthdayMonthList[0].Code;
			((ModuleTextFilter)anyFilter["Birthday In"]).IsActive = true;

			collection1 = new GlbStaffCollection(Factory, anyFilter.Filter);

			Assert("Collection should contain Staff1", collection1.Contains(staff1));
			Assert("Collection should contain Staff2", collection1.Contains(staff2));
			Assert("Collection should contain Staff3", collection1.Contains(staff3));
			Assert("Collection should contain Staff4", collection1.Contains(staff4));

			GlbStaffFilterBusinessObject emptyFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)emptyFilter["Birthday In"]).Property = "";
			((ModuleTextFilter)emptyFilter["Birthday In"]).IsActive = false;

			collection1 = new GlbStaffCollection(Factory, emptyFilter.Filter);

			Assert("Collection should contain Staff1", collection1.Contains(staff1));
			Assert("Collection should contain Staff2", collection1.Contains(staff2));
			Assert("Collection should contain Staff3", collection1.Contains(staff3));
			Assert("Collection should contain Staff4", collection1.Contains(staff4));

			GlbStaffFilterBusinessObject invalidFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)invalidFilter["Birthday In"]).Property = "13";
			((ModuleTextFilter)invalidFilter["Birthday In"]).IsActive = false;

			collection1 = new GlbStaffCollection(Factory, invalidFilter.Filter);

			Assert("Collection should contain Staff1", collection1.Contains(staff1));
			Assert("Collection should contain Staff2", collection1.Contains(staff2));
			Assert("Collection should contain Staff3", collection1.Contains(staff3));
			Assert("Collection should contain Staff4", collection1.Contains(staff4));
		}

		#endregion

		#region TestSearchBySecurityModified

		public void TestSearchBySecurityModified()
		{
			GlbStaffFilterBusinessObject filterObject = new GlbStaffFilterBusinessObject();
			ModuleDateFilter securityModifiedFilter = (ModuleDateFilter)filterObject["Security Modified"];
			AssertEquals("Security Modified filter should have HideFutureDates set to true.", true, securityModifiedFilter.HideFutureDates);

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbSecurity security1 = staff1.StaffSecurityPermissionsCollection.AddNew();
			security1.BranchCode = "BNE";
			security1.DepartmentCode = "BRN";

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			securityModifiedFilter.PropertySearch = "Today";
			securityModifiedFilter.IsActive = true;

			GlbStaffCollection collection = new GlbStaffCollection(Factory, filterObject.Filter);

			Assert("Collection should contain Staff1.", collection.Contains(staff1));
			Assert("Collection should not contain Staff.2", !collection.Contains(staff2));
		}

		#endregion

		#region TestSearchBySecurityModifiedDoesNotCrashIfParametersAreEmpty

		[ExpectNoExceptions]
		public void TestSearchBySecurityModifiedDoesNotCrashIfParametersAreEmpty()
		{
			GlbStaffFilterBusinessObject filterObject = new GlbStaffFilterBusinessObject();

			ModuleDateFilter securityModifiedFilter = (ModuleDateFilter)filterObject["Security Modified"];
			securityModifiedFilter.IsActive = true;
			securityModifiedFilter.PropertySearch = "Date range";

			securityModifiedFilter.Property1 = ZDateTime.Now;
			GlbStaffCollection staffs = new GlbStaffCollection(Factory);
			staffs.AdditionalFilter = filterObject.Filter;

			securityModifiedFilter.Property1 = ZDateTime.Empty;
			securityModifiedFilter.Property2 = ZDateTime.Now;
			staffs.AdditionalFilter = filterObject.Filter;
		}

		#endregion

		#region TestNonOperationalFilter

		public void TestNonOperationalFilter()
		{
			GlbStaff nonOperationalStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff nonOperationalStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff otherStaff = Factory.NewWithValidTestData<GlbStaff>();

			nonOperationalStaff1.GS_IsOperational = false;
			nonOperationalStaff2.GS_IsOperational = false;
			otherStaff.GS_IsOperational = true;

			GlbStaffFilterBusinessObject nonOperationalStaffFilter = new GlbStaffFilterBusinessObject();

			((ModuleTextFilter)nonOperationalStaffFilter["Operational Status"]).Property = OrgConstants.FilterControl.OperationalStatus.Code.NonOperationalStaff;
			((ModuleTextFilter)nonOperationalStaffFilter["Operational Status"]).IsActive = true;

			Factory.Save();

			GlbStaffCollection staffCollection = new GlbStaffCollection(Factory, nonOperationalStaffFilter.Filter);

			Assert("Filtered for Non-Operational staff, Collection should contain NonOperationalStaff1", staffCollection.Contains(nonOperationalStaff1));
			Assert("Filtered for Non-Operational staff, Collection should contain NonOperationalStaff2", staffCollection.Contains(nonOperationalStaff2));
			Assert("Filtered for Non-Operational staff, Collection should NOT contain OtherStaff", !staffCollection.Contains(otherStaff));

			GlbStaffFilterBusinessObject operationalStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)operationalStaffFilter["Operational Status"]).Property = OrgConstants.FilterControl.OperationalStatus.Code.OperationalStaff;
			((ModuleTextFilter)operationalStaffFilter["Operational Status"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, operationalStaffFilter.Filter);

			Assert("Filtered for Operational staff, Collection should NOT contain NonOperationalStaff1", !staffCollection.Contains(nonOperationalStaff1));
			Assert("Filtered for Operational staff, Collection should NOT contain NonOperationalStaff2", !staffCollection.Contains(nonOperationalStaff2));
			Assert("Filtered for Operational staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

			GlbStaffFilterBusinessObject allStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)allStaffFilter["Operational Status"]).Property = OrgConstants.FilterControl.OperationalStatus.Code.AllStaff;
			((ModuleTextFilter)allStaffFilter["Operational Status"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, allStaffFilter.Filter);

			Assert("Filtered for ALL staff, Collection should contain NonOperationalStaff1", staffCollection.Contains(nonOperationalStaff1));
			Assert("Filtered for ALL staff, Collection should contain NonOperationalStaff2", staffCollection.Contains(nonOperationalStaff2));
			Assert("Filtered for ALL staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

			GlbStaffFilterBusinessObject emptyStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)emptyStaffFilter["Operational Status"]).Property = "";
			((ModuleTextFilter)emptyStaffFilter["Operational Status"]).IsActive = false;

			staffCollection = new GlbStaffCollection(Factory, emptyStaffFilter.Filter);

			Assert("Empty filter, Collection should contain NonOperationalStaff1", staffCollection.Contains(nonOperationalStaff1));
			Assert("Empty filter, Collection should contain NonOperationalStaff2", staffCollection.Contains(nonOperationalStaff2));
			Assert("Empty filter, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));
		}

		#endregion

		#region TestSysAdminFilter

		public void TestSysAdminFilter()
		{
			GlbStaff sysAdminStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff sysAdminStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff otherStaff = Factory.NewWithValidTestData<GlbStaff>();

			sysAdminStaff1.GS_IsController = true;
			sysAdminStaff2.GS_IsController = true;
			otherStaff.GS_IsController = false;

			GlbStaffFilterBusinessObject sysAdminStaffFilter = new GlbStaffFilterBusinessObject();

			((ModuleTextFilter)sysAdminStaffFilter["Sys Admin Status"]).Property = OrgConstants.FilterControl.SysAdminStatus.Code.SysAdminStaff;
			((ModuleTextFilter)sysAdminStaffFilter["Sys Admin Status"]).IsActive = true;

			Factory.Save();

			GlbStaffCollection staffCollection = new GlbStaffCollection(Factory, sysAdminStaffFilter.Filter);

			Assert("Filtered for Sys Admin staff, Collection should contain SysAdminStaff1", staffCollection.Contains(sysAdminStaff1));
			Assert("Filtered for Sys Admin staff, Collection should contain SysAdminStaff2", staffCollection.Contains(sysAdminStaff2));
			Assert("Filtered for Sys Admin staff, Collection should NOT contain OtherStaff", !staffCollection.Contains(otherStaff));

			GlbStaffFilterBusinessObject notSysAdminStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)notSysAdminStaffFilter["Sys Admin Status"]).Property = OrgConstants.FilterControl.SysAdminStatus.Code.NonSysAdminStaff;
			((ModuleTextFilter)notSysAdminStaffFilter["Sys Admin Status"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, notSysAdminStaffFilter.Filter);

			Assert("Filtered for NOT Sys Admin staff, Collection should NOT contain SysAdminStaff1", !staffCollection.Contains(sysAdminStaff1));
			Assert("Filtered for NOT Sys Admin staff, Collection should NOT contain SysAdminStaff2", !staffCollection.Contains(sysAdminStaff2));
			Assert("Filtered for NOT Sys Admin staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

			GlbStaffFilterBusinessObject allStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)allStaffFilter["Sys Admin Status"]).Property = OrgConstants.FilterControl.SysAdminStatus.Code.AllStaff;
			((ModuleTextFilter)allStaffFilter["Sys Admin Status"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, allStaffFilter.Filter);

			Assert("Filtered for ALL staff, Collection should contain SysAdminStaff1", staffCollection.Contains(sysAdminStaff1));
			Assert("Filtered for ALL staff, Collection should contain SysAdminStaff2", staffCollection.Contains(sysAdminStaff2));
			Assert("Filtered for ALL staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

			GlbStaffFilterBusinessObject emptyStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)emptyStaffFilter["Sys Admin Status"]).Property = "";
			((ModuleTextFilter)emptyStaffFilter["Sys Admin Status"]).IsActive = false;

			staffCollection = new GlbStaffCollection(Factory, emptyStaffFilter.Filter);

			Assert("Empty filter, Collection should contain SysAdminStaff1", staffCollection.Contains(sysAdminStaff1));
			Assert("Empty filter, Collection should contain SysAdminStaff2", staffCollection.Contains(sysAdminStaff2));
			Assert("Empty filter, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));
		}

		#endregion

		#region TestSalesRepFilter

		public void TestSalesRepFilter()
		{
			GlbStaff salesRepStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff salesRepStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff otherStaff = Factory.NewWithValidTestData<GlbStaff>();

			salesRepStaff1.GS_IsSalesRep = true;
			salesRepStaff2.GS_IsSalesRep = true;
			otherStaff.GS_IsSalesRep = false;

			GlbStaffFilterBusinessObject salesRepStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)salesRepStaffFilter["Sales Rep"]).Property = OrgConstants.FilterControl.SalesRepStatus.Code.SalesRep;
			((ModuleTextFilter)salesRepStaffFilter["Sales Rep"]).IsActive = true;

			Factory.Save();

			GlbStaffCollection staffCollection = new GlbStaffCollection(Factory, salesRepStaffFilter.Filter);

			Assert("Filtered for Sales Rep staff, Collection should contain SalesRepStaff1", staffCollection.Contains(salesRepStaff1));
			Assert("Filtered for Sales Rep staff, Collection should contain SalesRepStaff2", staffCollection.Contains(salesRepStaff2));
			Assert("Filtered for Sales Rep staff, Collection should NOT contain OtherStaff", !staffCollection.Contains(otherStaff));

			GlbStaffFilterBusinessObject notSalesRepStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)notSalesRepStaffFilter["Sales Rep"]).Property = OrgConstants.FilterControl.SalesRepStatus.Code.NonSalesRep;
			((ModuleTextFilter)notSalesRepStaffFilter["Sales Rep"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, notSalesRepStaffFilter.Filter);

			Assert("Filtered for NOT Sales Rep staff, Collection should NOT contain SalesRepStaff1", !staffCollection.Contains(salesRepStaff1));
			Assert("Filtered for NOT Sales Rep staff, Collection should NOT contain SalesRepStaff2", !staffCollection.Contains(salesRepStaff2));
			Assert("Filtered for NOT Sales Rep staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

			GlbStaffFilterBusinessObject allStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)notSalesRepStaffFilter["Sales Rep"]).Property = OrgConstants.FilterControl.SalesRepStatus.Code.AllStaff;
			((ModuleTextFilter)notSalesRepStaffFilter["Sales Rep"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, allStaffFilter.Filter);

			Assert("Filtered for ALL staff, Collection should contain SalesRepStaff1", staffCollection.Contains(salesRepStaff1));
			Assert("Filtered for ALL staff, Collection should contain SalesRepStaff2", staffCollection.Contains(salesRepStaff2));
			Assert("Filtered for ALL staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

			GlbStaffFilterBusinessObject emptyStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)emptyStaffFilter["Sales Rep"]).Property = "";
			((ModuleTextFilter)emptyStaffFilter["Sales Rep"]).IsActive = false;

			staffCollection = new GlbStaffCollection(Factory, emptyStaffFilter.Filter);

			Assert("Empty filter, Collection should contain SalesRepStaff1", staffCollection.Contains(salesRepStaff1));
			Assert("Empty filter, Collection should contain SalesRepStaff2", staffCollection.Contains(salesRepStaff2));
			Assert("Empty filter, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));
		}

		#endregion

		#region TestDatabaseReaderFilter

		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestDatabaseReaderFilter()
		{
			using (RunNonTransactioned())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var databaseReaderStaff1 = Factory.NewWithValidTestData<GlbStaff>();
				var databaseReaderStaff2 = Factory.NewWithValidTestData<GlbStaff>();
				var databaseReaderStaff3 = CreateTestDatabaseAccessStaff("cwRestrictedReaderRole");
				var otherStaff = Factory.NewWithValidTestData<GlbStaff>();

				try
				{
					databaseReaderStaff1.IsReadOnlyDBUser = true;
					databaseReaderStaff2.IsReadOnlyDBUser = true;
					otherStaff.IsReadOnlyDBUser = false;

					var databaseReaderStaffFilter = new GlbStaffFilterBusinessObject();
					((ModuleTextFilter)databaseReaderStaffFilter["Database Reader"]).Property = OrgConstants.FilterControl.DatabaseReaderStatus.Code.DatabaseReaderStaff;
					((ModuleTextFilter)databaseReaderStaffFilter["Database Reader"]).IsActive = true;

					Factory.Save();

					var staffCollection = new GlbStaffCollection(Factory, databaseReaderStaffFilter.Filter);

					Assert("Filtered for Database Reader staff, Collection should contain DatabaseReaderStaff1", staffCollection.Contains(databaseReaderStaff1));
					Assert("Filtered for Database Reader staff, Collection should contain DatabaseReaderStaff2", staffCollection.Contains(databaseReaderStaff2));
					Assert("Filtered for Database Reader staff, Collection should contain DatabaseReaderStaff3", staffCollection.Contains(databaseReaderStaff3));
					Assert("Filtered for Database Reader staff, Collection should NOT contain OtherStaff", !staffCollection.Contains(otherStaff));

					var notDatabaseReaderStaffFilter = new GlbStaffFilterBusinessObject();
					((ModuleTextFilter)notDatabaseReaderStaffFilter["Database Reader"]).Property = OrgConstants.FilterControl.DatabaseReaderStatus.Code.NonDatabaseReaderStaff;
					((ModuleTextFilter)notDatabaseReaderStaffFilter["Database Reader"]).IsActive = true;

					staffCollection = new GlbStaffCollection(Factory, notDatabaseReaderStaffFilter.Filter);

					Assert("Filtered for NOT Database Reader staff, Collection should NOT contain DatabaseReaderStaff1", !staffCollection.Contains(databaseReaderStaff1));
					Assert("Filtered for NOT Database Reader staff, Collection should NOT contain DatabaseReaderStaff2", !staffCollection.Contains(databaseReaderStaff2));
					Assert("Filtered for NOT Database Reader staff, Collection should NOT contain DatabaseReaderStaff2", !staffCollection.Contains(databaseReaderStaff3));
					Assert("Filtered for NOT Database Reader staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

					var allStaffFilter = new GlbStaffFilterBusinessObject();
					((ModuleTextFilter)allStaffFilter["Database Reader"]).Property = OrgConstants.FilterControl.DatabaseReaderStatus.Code.AllStaff;
					((ModuleTextFilter)allStaffFilter["Database Reader"]).IsActive = true;

					staffCollection = new GlbStaffCollection(Factory, allStaffFilter.Filter);

					Assert("Filtered for ALL staff, Collection should contain DatabaseReaderStaff1", staffCollection.Contains(databaseReaderStaff1));
					Assert("Filtered for ALL staff, Collection should contain DatabaseReaderStaff2", staffCollection.Contains(databaseReaderStaff2));
					Assert("Filtered for ALL staff, Collection should contain DatabaseReaderStaff3", staffCollection.Contains(databaseReaderStaff3));
					Assert("Filtered for ALL staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

					var emptyStaffFilter = new GlbStaffFilterBusinessObject();
					((ModuleTextFilter)emptyStaffFilter["Database Reader"]).Property = "";
					((ModuleTextFilter)emptyStaffFilter["Database Reader"]).IsActive = false;

					staffCollection = new GlbStaffCollection(Factory, emptyStaffFilter.Filter);

					Assert("Empty filter, Collection should contain DatabaseReaderStaff1", staffCollection.Contains(databaseReaderStaff1));
					Assert("Empty filter, Collection should contain DatabaseReaderStaff2", staffCollection.Contains(databaseReaderStaff2));
					Assert("Empty filter, Collection should contain DatabaseReaderStaff3", staffCollection.Contains(databaseReaderStaff3));
					Assert("Empty filter, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));
				}
				finally
				{
					databaseReaderStaff1.Delete();
					databaseReaderStaff2.Delete();
				}
			}
		}

		GlbStaff CreateTestDatabaseAccessStaff(string roleName)
		{
			var guid_staff = Guid.NewGuid();
			var guid_group = Guid.NewGuid();

			var sqlText = $@"
				------ GlbStaff
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_staff}', 'TSS', 'TestStaff001', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				------ HRMStaff Database Access Groups
				INSERT dbo.GlbGroup (GG_PK, GG_Code, GG_Desc, GG_IsActive, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser) VALUES ('{guid_group}', 'TG_001', 'Test Group {roleName}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbGroupRole (GGR_PK, GGR_RoleName, GGR_GG_Group, GGR_SystemCreateTimeUtc, GGR_SystemCreateUser, GGR_SystemLastEditTimeUtc, GGR_SystemLastEditUser) VALUES (NEWID(), '{roleName}', '{guid_group}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{guid_group}', '{guid_staff}')
				";

			TestConnection.Command(sqlText).ExecuteNonQuery();

			return Factory.Load<GlbStaff>(guid_staff);
		}

		#endregion

		#region TestDatabaseDeveloperFilter

		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestDatabaseDeveloperFilter()
		{
			using (RunNonTransactioned())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var developerStaff1 = Factory.NewWithValidTestData<GlbStaff>();
				var developerStaff2 = Factory.NewWithValidTestData<GlbStaff>();
				var developerStaff3 = CreateTestDatabaseAccessStaff("db_datawriter");
				var otherStaff = Factory.NewWithValidTestData<GlbStaff>();

				try
				{
					developerStaff1.IsReadOnlyDBUser = true;
					developerStaff1.IsDatabaseDeveloper = true;
					developerStaff2.IsReadOnlyDBUser = true;
					developerStaff2.IsDatabaseDeveloper = true;
					otherStaff.IsReadOnlyDBUser = false;
					otherStaff.IsDatabaseDeveloper = false;

					var databaseDeveloperStaffFilter = new GlbStaffFilterBusinessObject();
					((ModuleTextFilter)databaseDeveloperStaffFilter["Database Developer"]).Property = OrgConstants.FilterControl.DatabaseDeveloperStatus.Code.DatabaseDeveloperStaff;
					((ModuleTextFilter)databaseDeveloperStaffFilter["Database Developer"]).IsActive = true;
					Factory.Save();
					var staffCollection = new GlbStaffCollection(Factory, databaseDeveloperStaffFilter.Filter);
					Assert("Collection should contain developerStaff1", staffCollection.Contains(developerStaff1));
					Assert("Collection should contain developerStaff2", staffCollection.Contains(developerStaff2));
					Assert("Collection should contain developerStaff3", staffCollection.Contains(developerStaff3));
					Assert("Collection should NOT contain OtherStaff", !staffCollection.Contains(otherStaff));

					var notDatabaseDeveloperStaffFilter = new GlbStaffFilterBusinessObject();
					((ModuleTextFilter)notDatabaseDeveloperStaffFilter["Database Developer"]).Property = OrgConstants.FilterControl.DatabaseDeveloperStatus.Code.NonDatabaseDeveloperStaff;
					((ModuleTextFilter)notDatabaseDeveloperStaffFilter["Database Developer"]).IsActive = true;

					staffCollection = new GlbStaffCollection(Factory, notDatabaseDeveloperStaffFilter.Filter);

					Assert("Collection should NOT contain developerStaff1", !staffCollection.Contains(developerStaff1));
					Assert("Collection should NOT contain developerStaff2", !staffCollection.Contains(developerStaff2));
					Assert("Collection should NOT contain developerStaff3", !staffCollection.Contains(developerStaff3));
					Assert("Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

					var allStaffFilter = new GlbStaffFilterBusinessObject();
					((ModuleTextFilter)allStaffFilter["Database Developer"]).Property = OrgConstants.FilterControl.DatabaseDeveloperStatus.Code.AllStaff;
					((ModuleTextFilter)allStaffFilter["Database Developer"]).IsActive = true;

					staffCollection = new GlbStaffCollection(Factory, allStaffFilter.Filter);

					Assert("Collection should contain developerStaff1", staffCollection.Contains(developerStaff1));
					Assert("Collection should contain developerStaff2", staffCollection.Contains(developerStaff2));
					Assert("Collection should contain developerStaff3", staffCollection.Contains(developerStaff3));
					Assert("Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

					var emptyStaffFilter = new GlbStaffFilterBusinessObject();
					((ModuleTextFilter)emptyStaffFilter["Database Developer"]).Property = "";
					((ModuleTextFilter)emptyStaffFilter["Database Developer"]).IsActive = false;

					staffCollection = new GlbStaffCollection(Factory, emptyStaffFilter.Filter);

					Assert("Collection should contain developerStaff1", staffCollection.Contains(developerStaff1));
					Assert("Collection should contain developerStaff2", staffCollection.Contains(developerStaff2));
					Assert("Collection should contain developerStaff3", staffCollection.Contains(developerStaff3));
					Assert("Collection should contain OtherStaff", staffCollection.Contains(otherStaff));
				}
				finally
				{
					developerStaff1.Delete();
					developerStaff2.Delete();
					otherStaff.Delete();
					Factory.Save();
				}
			}
		}

		#endregion

		#region TestBackupOperatorFilter

		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestBackupOperatorFilter()
		{
			var backupOperatorStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var backupOperatorStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			var backupOperatorStaff3 = CreateTestDatabaseAccessStaff("db_backupoperator");
			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();

			backupOperatorStaff1.IsBackupOperator = true;
			backupOperatorStaff2.IsBackupOperator = true;
			otherStaff.IsBackupOperator = false;

			var backupOperatorStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)backupOperatorStaffFilter["Backup Operator"]).Property = OrgConstants.FilterControl.BackupOperatorStatus.Code.BackupOperatorStaff;
			((ModuleTextFilter)backupOperatorStaffFilter["Backup Operator"]).IsActive = true;

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				Factory.Save();
			}

			var staffCollection = new GlbStaffCollection(Factory, backupOperatorStaffFilter.Filter);

			Assert("Filtered for Backup Operator staff, Collection should contain BackupOperatorStaff1", staffCollection.Contains(backupOperatorStaff1));
			Assert("Filtered for Backup Operator staff, Collection should contain BackupOperatorStaff2", staffCollection.Contains(backupOperatorStaff2));
			Assert("Filtered for Backup Operator staff, Collection should contain BackupOperatorStaff3", staffCollection.Contains(backupOperatorStaff3));
			Assert("Filtered for Backup Operator staff, Collection should NOT contain OtherStaff", !staffCollection.Contains(otherStaff));

			var notBackupOperatorStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)notBackupOperatorStaffFilter["Backup Operator"]).Property = OrgConstants.FilterControl.BackupOperatorStatus.Code.NonBackupOperatorStaff;
			((ModuleTextFilter)notBackupOperatorStaffFilter["Backup Operator"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, notBackupOperatorStaffFilter.Filter);

			Assert("Filtered for NOT Backup Operator staff, Collection should NOT contain BackupOperatorStaff1", !staffCollection.Contains(backupOperatorStaff1));
			Assert("Filtered for NOT Backup Operator staff, Collection should NOT contain BackupOperatorStaff2", !staffCollection.Contains(backupOperatorStaff2));
			Assert("Filtered for NOT Backup Operator staff, Collection should NOT contain BackupOperatorStaff3", !staffCollection.Contains(backupOperatorStaff3));
			Assert("Filtered for NOT Backup Operator staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

			var allStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)allStaffFilter["Backup Operator"]).Property = OrgConstants.FilterControl.BackupOperatorStatus.Code.AllStaff;
			((ModuleTextFilter)allStaffFilter["Backup Operator"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, allStaffFilter.Filter);

			Assert("Filtered for ALL staff, Collection should contain BackupOperatorStaff1", staffCollection.Contains(backupOperatorStaff1));
			Assert("Filtered for ALL staff, Collection should contain BackupOperatorStaff2", staffCollection.Contains(backupOperatorStaff2));
			Assert("Filtered for ALL staff, Collection should contain BackupOperatorStaff3", staffCollection.Contains(backupOperatorStaff3));
			Assert("Filtered for ALL staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

			var emptyStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)emptyStaffFilter["Backup Operator"]).Property = "";
			((ModuleTextFilter)emptyStaffFilter["Backup Operator"]).IsActive = false;

			staffCollection = new GlbStaffCollection(Factory, emptyStaffFilter.Filter);

			Assert("Empty filter, Collection should contain BackupOperatorStaff1", staffCollection.Contains(backupOperatorStaff1));
			Assert("Empty filter, Collection should contain BackupOperatorStaff2", staffCollection.Contains(backupOperatorStaff2));
			Assert("Empty filter, Collection should contain BackupOperatorStaff3", staffCollection.Contains(backupOperatorStaff3));
			Assert("Empty filter, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));
		}

		#endregion

		#region TestIsDeviceOnlyFilter

		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestIsDeviceOnlyFilter()
		{
			var deviceOnlyStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var deviceOnlyStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			var nonDeviceOnlyStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var nonDeviceOnlyStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			deviceOnlyStaff1.GS_IsDevice = true;
			deviceOnlyStaff2.GS_IsDevice = true;

			var deviceOnlyStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)deviceOnlyStaffFilter["Is Device Only"]).Property = OrgConstants.FilterControl.IsDeviceOnlyStatus.Code.DeviceOnlyStaff;
			((ModuleTextFilter)deviceOnlyStaffFilter["Is Device Only"]).IsActive = true;

			Factory.Save();

			var staffCollection = new GlbStaffCollection(Factory, deviceOnlyStaffFilter.Filter);

			Assert("Filtered for Is Device Only, Collection should contain deviceOnlyStaff1", staffCollection.Contains(deviceOnlyStaff1));
			Assert("Filtered for Is Device Only, Collection should contain deviceOnlyStaff2", staffCollection.Contains(deviceOnlyStaff2));
			Assert("Filtered for Is Device Only, Collection should NOT contain nonDeviceOnlyStaff1", !staffCollection.Contains(nonDeviceOnlyStaff1));
			Assert("Filtered for Is Device Only, Collection should NOT contain nonDeviceOnlyStaff2", !staffCollection.Contains(nonDeviceOnlyStaff2));

			var nonDeviceOnlyStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)nonDeviceOnlyStaffFilter["Is Device Only"]).Property = OrgConstants.FilterControl.IsDeviceOnlyStatus.Code.NonDeviceOnlyStaff;
			((ModuleTextFilter)nonDeviceOnlyStaffFilter["Is Device Only"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, nonDeviceOnlyStaffFilter.Filter);

			Assert("Filtered for NOT Device Only, Collection should NOT contain deviceOnlyStaff1", !staffCollection.Contains(deviceOnlyStaff1));
			Assert("Filtered for NOT Device Only, Collection should NOT contain deviceOnlyStaff2", !staffCollection.Contains(deviceOnlyStaff2));
			Assert("Filtered for NOT Device Only, Collection should contain nonDeviceOnlyStaff1", staffCollection.Contains(nonDeviceOnlyStaff1));
			Assert("Filtered for NOT Device Only, Collection should contain nonDeviceOnlyStaff2", staffCollection.Contains(nonDeviceOnlyStaff2));

			var allStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)allStaffFilter["Is Device Only"]).Property = OrgConstants.FilterControl.BackupOperatorStatus.Code.AllStaff;
			((ModuleTextFilter)allStaffFilter["Is Device Only"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, allStaffFilter.Filter);

			Assert("Filtered for ALL staff, Collection should contain deviceOnlyStaff1", staffCollection.Contains(deviceOnlyStaff1));
			Assert("Filtered for ALL staff, Collection should contain deviceOnlyStaff2", staffCollection.Contains(deviceOnlyStaff2));
			Assert("Filtered for ALL staff, Collection should contain nonDeviceOnlyStaff1", staffCollection.Contains(nonDeviceOnlyStaff1));
			Assert("Filtered for ALL staff, Collection should contain nonDeviceOnlyStaff2", staffCollection.Contains(nonDeviceOnlyStaff2));

			var emptyStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)emptyStaffFilter["Is Device Only"]).Property = "";
			((ModuleTextFilter)emptyStaffFilter["Is Device Only"]).IsActive = false;

			staffCollection = new GlbStaffCollection(Factory, emptyStaffFilter.Filter);

			Assert("Empty filter, Collection should contain deviceOnlyStaff1", staffCollection.Contains(deviceOnlyStaff1));
			Assert("Empty filter, Collection should contain deviceOnlyStaff2", staffCollection.Contains(deviceOnlyStaff2));
			Assert("Empty filter, Collection should contain nonDeviceOnlyStaff1", staffCollection.Contains(nonDeviceOnlyStaff1));
			Assert("Empty filter, Collection should contain nonDeviceOnlyStaff2", staffCollection.Contains(nonDeviceOnlyStaff2));
		}

		#endregion

		#region TestIsDriverFilter

		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestDriverFilter()
		{
			var driverStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var driverStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			var nonDriverStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var nonDriverStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			driverStaff1.GS_IsDriver = true;
			driverStaff2.GS_IsDriver = true;

			var driverStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)driverStaffFilter["Driver"]).Property = OrgConstants.FilterControl.DriverStatus.Code.Driver;
			((ModuleTextFilter)driverStaffFilter["Driver"]).IsActive = true;

			Factory.Save();

			var staffCollection = new GlbStaffCollection(Factory, driverStaffFilter.Filter);

			Assert("Filtered for Driver, Collection should contain driverStaff1", staffCollection.Contains(driverStaff1));
			Assert("Filtered for Driver, Collection should contain driverStaff2", staffCollection.Contains(driverStaff2));
			Assert("Filtered for Driver, Collection should NOT contain nonDriverStaff1", !staffCollection.Contains(nonDriverStaff1));
			Assert("Filtered for Driver, Collection should NOT contain nonDriverStaff2", !staffCollection.Contains(nonDriverStaff2));

			var nonDriverStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)nonDriverStaffFilter["Driver"]).Property = OrgConstants.FilterControl.DriverStatus.Code.NonDriver;
			((ModuleTextFilter)nonDriverStaffFilter["Driver"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, nonDriverStaffFilter.Filter);

			Assert("Filtered for NOT Driver, Collection should NOT contain driverStaff1", !staffCollection.Contains(driverStaff1));
			Assert("Filtered for NOT Driver, Collection should NOT contain driverStaff2", !staffCollection.Contains(driverStaff2));
			Assert("Filtered for NOT Driver, Collection should contain nonDriverStaff1", staffCollection.Contains(nonDriverStaff1));
			Assert("Filtered for NOT Driver, Collection should contain nonDriverStaff2", staffCollection.Contains(nonDriverStaff2));

			var allStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)allStaffFilter["Driver"]).Property = OrgConstants.FilterControl.DriverStatus.Code.AllStaff;
			((ModuleTextFilter)allStaffFilter["Driver"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, allStaffFilter.Filter);

			Assert("Filtered for ALL staff, Collection should contain driverStaff1", staffCollection.Contains(driverStaff1));
			Assert("Filtered for ALL staff, Collection should contain driverStaff2", staffCollection.Contains(driverStaff2));
			Assert("Filtered for ALL staff, Collection should contain nonDriverStaff1", staffCollection.Contains(nonDriverStaff1));
			Assert("Filtered for ALL staff, Collection should contain nonDriverStaff2", staffCollection.Contains(nonDriverStaff2));

			var emptyStaffFilter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)emptyStaffFilter["Driver"]).Property = "";
			((ModuleTextFilter)emptyStaffFilter["Driver"]).IsActive = false;

			staffCollection = new GlbStaffCollection(Factory, emptyStaffFilter.Filter);

			Assert("Empty filter, Collection should contain driverStaff1", staffCollection.Contains(driverStaff1));
			Assert("Empty filter, Collection should contain driverStaff2", staffCollection.Contains(driverStaff2));
			Assert("Empty filter, Collection should contain nonDriverStaff1", staffCollection.Contains(nonDriverStaff1));
			Assert("Empty filter, Collection should contain nonDriverStaff2", staffCollection.Contains(nonDriverStaff2));
		}

		#endregion

		#region TestIsCurrentUserFilter

		public void TestIsCurrentUserFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.GS_FullName = "Brad 1";
			staff2.GS_FullName = "Brad 2";

			Factory.Save();

			var filterBizo = new GlbStaffFilterBusinessObject();

			var nameFilter = filterBizo.AddTextFilterStrip("Full Name", "Brad");
			var isCurrentUserFilter = filterBizo.AddTextFilterStrip("Is Current User");
			AssertEquals("CUR should be the default option. SAD", GlbStaffFilterBusinessObject.IsCurrentUserOptionsListCodes.CurrentUser, isCurrentUserFilter.Property);

			GlbStaff[] results;

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var query = filterBizo.Filter;
				results = Factory.Load<GlbStaff>(query);

				AssertContainsExactElementsInAnyOrder("'CUR' is selected, so only the currently logged-in staff should be returned. SAD! " + query.LiteralTextSqlFormatted,
					new[] { "Brad 1" }, results.Select(x => x.GS_FullName));

				isCurrentUserFilter.Property = GlbStaffFilterBusinessObject.IsCurrentUserOptionsListCodes.NotCurrentUser;
				query = filterBizo.Filter;
				results = Factory.Load<GlbStaff>(query);

				AssertContainsExactElementsInAnyOrder("'NOT' is selected, so all staff other than the current user should be returned. SAD! " + query.LiteralTextSqlFormatted,
					new[] { "Brad 2" }, results.Select(x => x.GS_FullName));

				isCurrentUserFilter.Property = GlbStaffFilterBusinessObject.IsCurrentUserOptionsListCodes.AllStaff;
				query = filterBizo.Filter;
				results = Factory.Load<GlbStaff>(query);

				AssertContainsExactElementsInAnyOrder("'ALL' is selected, so all staff should be returned. SAD! " + query.LiteralTextSqlFormatted,
					new[] { "Brad 1", "Brad 2" }, results.Select(x => x.GS_FullName));
			}

			using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var query = filterBizo.Filter;
				results = Factory.Load<GlbStaff>(query);

				AssertContainsExactElementsInAnyOrder("'ALL' is still selected, so all staff should be returned. SAD! " + query.LiteralTextSqlFormatted,
					new[] { "Brad 1", "Brad 2" }, results.Select(x => x.GS_FullName));

				isCurrentUserFilter.Property = GlbStaffFilterBusinessObject.IsCurrentUserOptionsListCodes.CurrentUser;
				query = filterBizo.Filter;
				results = Factory.Load<GlbStaff>(query);

				AssertContainsExactElementsInAnyOrder("'CUR' is selected and a different user logged in, so that newly logged-in staff should be returned. SAD! " + query.LiteralTextSqlFormatted,
					new[] { "Brad 2" }, results.Select(x => x.GS_FullName));

				isCurrentUserFilter.Property = GlbStaffFilterBusinessObject.IsCurrentUserOptionsListCodes.NotCurrentUser;
				query = filterBizo.Filter;
				results = Factory.Load<GlbStaff>(query);

				AssertContainsExactElementsInAnyOrder("'NOT' is selected, so all staff other than the current user should be returned. SAD! " + query.LiteralTextSqlFormatted,
					new[] { "Brad 1" }, results.Select(x => x.GS_FullName));
			}
		}

		#endregion

		#region TestDriverBranchFilter

		public void TestDriverBranchFilter()
		{
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

			var filterBizO = new GlbStaffFilterBusinessObject();
			filterBizO[StaffDriverCollection.DriverBranch].IsActive = true;
			AssertEquals("Default filter should be CurrentBranch.", Env.CurrentBranch.Code, ((ModuleTextFilter)filterBizO[StaffDriverCollection.DriverBranch]).Property);

			// All Drivers
			((ModuleTextFilter)filterBizO[StaffDriverCollection.DriverBranch]).Property = "All Drivers";
			var collection1 = new GlbStaffCollection(Factory, filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { curBranchDriverStaff, newBranchDriverStaff }, collection1);

			// all staff
			((ModuleTextFilter)filterBizO[StaffDriverCollection.DriverBranch]).Property = "All Staff";
			var collection2 = new GlbStaffCollection(Factory, filterBizO.Filter);
			Assert(collection2.Contains(curBranchDriverStaff));
			Assert(collection2.Contains(newBranchDriverStaff));
			Assert(collection2.Contains(nonDriverStaff));
			Assert(collection2.Contains(otherCompanyStaff));

			// current branch code
			filterBizO[StaffDriverCollection.DriverBranch].IsActive = true;
			((ModuleTextFilter)filterBizO[StaffDriverCollection.DriverBranch]).Property = curBranch.GB_Code;
			var collection3 = new GlbStaffCollection(Factory, filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { curBranchDriverStaff }, collection3);

			// new branch code
			filterBizO[StaffDriverCollection.DriverBranch].IsActive = true;
			((ModuleTextFilter)filterBizO[StaffDriverCollection.DriverBranch]).Property = newBranch.GB_Code;
			var collection4 = new GlbStaffCollection(Factory, filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { newBranchDriverStaff }, collection4);

			// no group branch
			filterBizO[StaffDriverCollection.DriverBranch].IsActive = true;
			((ModuleTextFilter)filterBizO[StaffDriverCollection.DriverBranch]).Property = noGroupBranch.GB_Code;
			var collection5 = new GlbStaffCollection(Factory, filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(Array.Empty<GlbStaff>(), collection5);
		}

		#endregion

		#region TestGroupFilter

		public void TestGroupFilter()
		{
			GlbGroup group1 = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroup group2 = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroup group3 = Factory.NewWithValidTestData<GlbGroup>();

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.Groups.Add(group1);
			staff1.Groups.Add(group2);
			staff2.Groups.Add(group1);

			Factory.Save();

			GlbStaffFilterBusinessObject group1Filter = new GlbStaffFilterBusinessObject();
			((ModuleGuidFilter)group1Filter["Group Membership"]).Property = group1.PK;
			((ModuleGuidFilter)group1Filter["Group Membership"]).IsActive = true;

			GlbStaffCollection staffCollection = new GlbStaffCollection(Factory, group1Filter.Filter);

			Assert("Filtered for Group 1 staff, Collection should contain Staff1", staffCollection.Contains(staff1));
			Assert("Filtered for Group 1 staff, Collection should contain Staff2", staffCollection.Contains(staff2));
			Assert("Filtered for Group 1 staff, Collection should NOT contain Staff3", !staffCollection.Contains(staff3));

			GlbStaffFilterBusinessObject group2Filter = new GlbStaffFilterBusinessObject();
			((ModuleGuidFilter)group2Filter["Group Membership"]).Property = group2.PK;
			((ModuleGuidFilter)group2Filter["Group Membership"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, group2Filter.Filter);

			Assert("Filtered for Group 2 staff, Collection should contain Staff1", staffCollection.Contains(staff1));
			Assert("Filtered for Group 2 staff, Collection should NOT contain Staff2", !staffCollection.Contains(staff2));
			Assert("Filtered for Group 2 staff, Collection should NOT contain Staff3", !staffCollection.Contains(staff3));

			GlbStaffFilterBusinessObject group3Filter = new GlbStaffFilterBusinessObject();
			((ModuleGuidFilter)group3Filter["Group Membership"]).Property = group3.PK;
			((ModuleGuidFilter)group3Filter["Group Membership"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, group3Filter.Filter);

			Assert("Filtered for Group 3 staff, Collection should NOT contain Staff1", !staffCollection.Contains(staff1));
			Assert("Filtered for Group 3 staff, Collection should NOT contain Staff2", !staffCollection.Contains(staff2));
			Assert("Filtered for Group 3 staff, Collection should NOT contain Staff3", !staffCollection.Contains(staff3));

			GlbStaffFilterBusinessObject emptyFilter = new GlbStaffFilterBusinessObject();
			((ModuleGuidFilter)emptyFilter["Group Membership"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)emptyFilter["Group Membership"]).IsActive = false;

			staffCollection = new GlbStaffCollection(Factory, emptyFilter.Filter);

			Assert("Filtered for Group 3 staff, Collection should contain Staff1", staffCollection.Contains(staff1));
			Assert("Filtered for Group 3 staff, Collection should contain Staff2", staffCollection.Contains(staff2));
			Assert("Filtered for Group 3 staff, Collection should contain Staff3", staffCollection.Contains(staff3));

			GlbStaffFilterBusinessObject group4Filter = new GlbStaffFilterBusinessObject();
			((ModuleGuidFilter)group4Filter["Group Membership"]).Property = group1.PK;
			((ModuleGuidFilter)group4Filter["Group Membership"]).IsActive = true;
			var groupFilter2 = (ModuleGuidFilter)group4Filter.ModuleFilters.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive("Group Membership");
			groupFilter2.Property = group2.PK;
			groupFilter2.IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, group4Filter.Filter);

			Assert("Filtered for Group 1&2 staff, Collection should contain Staff1", staffCollection.Contains(staff1));
			Assert("Filtered for Group 1&2 staff, Collection should NOT contain Staff2", !staffCollection.Contains(staff2));
			Assert("Filtered for Group 1&2 staff, Collection should NOT contain Staff3", !staffCollection.Contains(staff3));
		}

		#endregion

		#region TestCapabilityFilter

		public void TestCapabilityFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			var capability3 = Factory.NewWithValidTestData<GlbCapability>();
			var capability4 = Factory.NewWithValidTestData<GlbCapability>();

			staff1.Capabilities.Add(capability1);
			staff1.Capabilities.Add(capability2);

			staff2.Capabilities.Add(capability1);
			staff2.Capabilities.Add(capability3);

			Factory.Save();

			var bizo = new GlbStaffFilterBusinessObject();
			var filter = (ModuleGuidFilter)bizo["Capability"];
			filter.IsActive = true;
			filter.Property = capability1.PK;

			var results = Factory.Load<GlbStaff>(bizo.Filter);
			AssertEquals(2, results.Length);
			AssertCollectionContains(staff1, results);
			AssertCollectionContains(staff2, results);

			filter.Property = capability2.PK;
			results = Factory.Load<GlbStaff>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertCollectionContains(staff1, results);

			filter.Property = capability3.PK;
			results = Factory.Load<GlbStaff>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertCollectionContains(staff2, results);

			filter.Property = capability4.PK;
			results = Factory.Load<GlbStaff>(bizo.Filter);
			AssertEquals(0, results.Length);
		}

		#endregion

		#region TestDepartmentFilter

		public void TestDepartmentFilter()
		{
			GlbDepartment aAADepartment = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment bBBDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			GlbStaff aAAStaff = Factory.NewWithValidTestData<GlbStaff>();
			aAAStaff.GS_GE_HomeDepartment = aAADepartment.PK;

			GlbStaff bBBStaff = Factory.NewWithValidTestData<GlbStaff>();
			bBBStaff.GS_GE_HomeDepartment = bBBDepartment.PK;

			GlbStaff bBBStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			bBBStaff2.GS_GE_HomeDepartment = bBBDepartment.PK;

			GlbStaffFilterBusinessObject departmentAAAFilter = new GlbStaffFilterBusinessObject();
			((ModuleGuidFilter)departmentAAAFilter["Home Department"]).Property = aAADepartment.PK;
			((ModuleGuidFilter)departmentAAAFilter["Home Department"]).IsActive = true;

			Factory.Save();

			GlbStaffCollection staffCollection = new GlbStaffCollection(Factory, departmentAAAFilter.Filter);

			Assert("Filtered for AAA Department staff, Collection should contain AAAStaff", staffCollection.Contains(aAAStaff));
			Assert("Filtered for AAA Department staff, Collection should NOT contain BBBStaff", !staffCollection.Contains(bBBStaff));
			Assert("Filtered for AAA Department staff, Collection should NOT contain BBBStaff2", !staffCollection.Contains(bBBStaff));

			GlbStaffFilterBusinessObject departmentBBBFilter = new GlbStaffFilterBusinessObject();
			((ModuleGuidFilter)departmentBBBFilter["Home Department"]).Property = bBBDepartment.PK;
			((ModuleGuidFilter)departmentBBBFilter["Home Department"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, departmentBBBFilter.Filter);

			Assert("Filtered for BBB Department staff, Collection should contain BBBStaff", staffCollection.Contains(bBBStaff));
			Assert("Filtered for BBB Department staff, Collection should contain BBBStaff2", staffCollection.Contains(bBBStaff2));
			Assert("Filtered for BBB Department staff, Collection should NOT contain AAAStaff", !staffCollection.Contains(aAAStaff));

			GlbStaffFilterBusinessObject emptyDepartmentFilter = new GlbStaffFilterBusinessObject();
			((ModuleGuidFilter)emptyDepartmentFilter["Home Department"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)emptyDepartmentFilter["Home Department"]).IsActive = false;

			staffCollection = new GlbStaffCollection(Factory, emptyDepartmentFilter.Filter);

			Assert("Filter is empty, Collection should contain AAAStaff", staffCollection.Contains(aAAStaff));
			Assert("Filter is empty, Collection should contain BBBStaff", staffCollection.Contains(bBBStaff));
			Assert("Filter is empty, Collection should contain BBBStaff2", staffCollection.Contains(bBBStaff2));
		}

		#endregion

		#region TestBranchFilter

		public void TestBranchFilter()
		{
			GlbBranch aAABranch = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch bBBBranch = Factory.NewWithValidTestData<GlbBranch>();

			GlbStaff aAAStaff = Factory.NewWithValidTestData<GlbStaff>();
			aAAStaff.GS_GB_HomeBranch = aAABranch.PK;

			GlbStaff bBBStaff = Factory.NewWithValidTestData<GlbStaff>();
			bBBStaff.GS_GB_HomeBranch = bBBBranch.PK;

			GlbStaff bBBStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			bBBStaff2.GS_GB_HomeBranch = bBBBranch.PK;

			Factory.Save();

			GlbStaffFilterBusinessObject branchAAAFilter = new GlbStaffFilterBusinessObject();
			((ModuleGuidFilter)branchAAAFilter["Home Branch"]).Property = aAABranch.PK;
			((ModuleGuidFilter)branchAAAFilter["Home Branch"]).IsActive = true;

			GlbStaffCollection staffCollection = new GlbStaffCollection(Factory, branchAAAFilter.Filter);

			Assert("Filtered for AAA branch staff, Collection should contain AAAStaff", staffCollection.Contains(aAAStaff));
			Assert("Filtered for AAA branch staff, Collection should NOT contain BBBStaff", !staffCollection.Contains(bBBStaff));
			Assert("Filtered for AAA branch staff, Collection should NOT contain BBBStaff2", !staffCollection.Contains(bBBStaff));

			GlbStaffFilterBusinessObject branchBBBFilter = new GlbStaffFilterBusinessObject();
			((ModuleGuidFilter)branchBBBFilter["Home Branch"]).Property = bBBBranch.PK;
			((ModuleGuidFilter)branchBBBFilter["Home Branch"]).IsActive = true;

			staffCollection = new GlbStaffCollection(Factory, branchBBBFilter.Filter);

			Assert("Filtered for BBB branch staff, Collection should contain BBBStaff", staffCollection.Contains(bBBStaff));
			Assert("Filtered for BBB branch staff, Collection should contain BBBStaff2", staffCollection.Contains(bBBStaff2));
			Assert("Filtered for BBB branch staff, Collection should NOT contain AAAStaff", !staffCollection.Contains(aAAStaff));

			GlbStaffFilterBusinessObject emptyBranchFilter = new GlbStaffFilterBusinessObject();
			((ModuleGuidFilter)emptyBranchFilter["Home Branch"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)emptyBranchFilter["Home Branch"]).IsActive = false;

			staffCollection = new GlbStaffCollection(Factory, emptyBranchFilter.Filter);

			Assert("Filter is empty, Collection should contain AAAStaff", staffCollection.Contains(aAAStaff));
			Assert("Filter is empty, Collection should contain BBBStaff", staffCollection.Contains(bBBStaff));
			Assert("Filter is empty, Collection should contain BBBStaff2", staffCollection.Contains(bBBStaff2));
		}

		#endregion

		#region TestSystemAccountsExcluded

		public void TestSystemAccountsExcluded()
		{
			GlbStaff userStaff = Factory.NewWithValidTestData<GlbStaff>();
			userStaff.GS_IsSystemAccount = false;
			GlbStaff adminStaff = Factory.NewWithValidTestData<GlbStaff>();
			adminStaff.GS_IsSystemAccount = true;

			Factory.Save();

			GlbStaffFilterBusinessObject staffFilter = new GlbStaffFilterBusinessObject();

			GlbStaffCollection collection1 = new GlbStaffCollection(Factory, staffFilter.Filter);
			Assert("Collection Contains UserStaff", collection1.Contains(userStaff));
			Assert("Collection does not contain AdminStaff", !collection1.Contains(adminStaff));
		}

		public void TestSystemAccountsExcludedForIndexSearch()
		{
			var staffFilter = new GlbStaffFilterBusinessObject();
			var queries = staffFilter.GetIndexSearchQueries();

			Assert("Except for CWSupport, all other system accounts should be excluded", queries.Any(q => q.ToUrlComponent() == "((ISSYSTEMACCOUNT eq false) or (CODE eq 'E'))"));
		}

		#endregion

		#region TestSecurityRightsFilter

		public void TestSecurityRightsFilter()
		{
			GlbStaffFilterBusinessObject staffFilter = new GlbStaffFilterBusinessObject();
			ModuleFilterCollection originalFilters = staffFilter.TestGetModuleFiltersCoreInternal();
			StaffSecurityModuleFilter securityFilter = staffFilter.SecurityRightsFilter;

			AssertNotNull(securityFilter);

			ModuleFilterCollection newFilters = staffFilter.TestGetModuleFiltersCoreInternal();

			Assert("SecurityRightsFilter should return original filter", ReferenceEquals(securityFilter, staffFilter.SecurityRightsFilter));
			Assert("Should have different Security Rights filter in different collections", !ReferenceEquals(originalFilters[securityFilter.Description], newFilters[securityFilter.Description]));
		}

		#endregion

		#region TestEmploymentBasisFilter

		public void TestEmploymentBasisFilter()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var permanentStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			permanentStaff1.GS_EmploymentBasis = "PER";
			var permanentStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			permanentStaff2.GS_EmploymentBasis = "PER";
			var casualStaff = Factory.NewWithValidTestData<GlbStaff>();
			casualStaff.GS_EmploymentBasis = "CAS";

			Factory.Save();

			var filter = new GlbStaffFilterBusinessObject();
			((ModuleTextFilter)filter["Employment Basis"]).Property = "";
			((ModuleTextFilter)filter["Employment Basis"]).IsActive = true;
			var result = new GlbStaffCollection(Factory, filter.Filter);
			Assert("Filtered collection should contain staff", result.Contains(staff));
			Assert("Filtered collection should contain permanentStaff1", result.Contains(permanentStaff1));
			Assert("Filtered collection should contain permanentStaff2", result.Contains(permanentStaff2));
			Assert("Filtered collection should contain casualStaff", result.Contains(casualStaff));

			((ModuleTextFilter)filter["Employment Basis"]).Property = "PER";
			result = new GlbStaffCollection(Factory, filter.Filter);
			Assert("Filtered collection should not contain staff", !result.Contains(staff));
			Assert("Filtered collection should contain permanentStaff1", result.Contains(permanentStaff1));
			Assert("Filtered collection should contain permanentStaff2", result.Contains(permanentStaff2));
			Assert("Filtered collection should not contain casualStaff", !result.Contains(casualStaff));

			((ModuleTextFilter)filter["Employment Basis"]).Property = "CAS";
			result = new GlbStaffCollection(Factory, filter.Filter);
			Assert("Filtered collection should not contain staff", !result.Contains(staff));
			Assert("Filtered collection should not permanentStaff1", !result.Contains(permanentStaff1));
			Assert("Filtered collection should not permanentStaff2", !result.Contains(permanentStaff2));
			Assert("Filtered collection should contain casualStaff", result.Contains(casualStaff));
		}

		#endregion

		#region TestNationalityFilter

		public void TestNationalityFilter()
		{
			var frNationality = Factory.NewWithValidTestData<GlbStaff>();
			frNationality.GS_RN_NKNationalityCode = "FR";

			var auNationality = Factory.NewWithValidTestData<GlbStaff>();
			auNationality.GS_RN_NKNationalityCode = "AU";

			Factory.Save();

			var filter = new GlbStaffFilterBusinessObject();
			((ModuleNkFilter)filter["Nationality"]).Property = "FR";
			((ModuleNkFilter)filter["Nationality"]).IsActive = true;
			var result = new GlbStaffCollection(Factory, filter.Filter);
			Assert("Filtered for FR Nationality, Collection should contain frNationality", result.Contains(frNationality));
			Assert("Filtered for FR Nationality, Collection should NOT contain auNationality", !result.Contains(auNationality));

			((ModuleNkFilter)filter["Nationality"]).Property = "AU";
			((ModuleNkFilter)filter["Nationality"]).IsActive = true;
			result = new GlbStaffCollection(Factory, filter.Filter);
			Assert("Filtered for AU Nationality, Collection should contain auNationality", result.Contains(auNationality));
			Assert("Filtered for AU Nationality, Collection should NOT contain frNationality", !result.Contains(frNationality));
		}

		#endregion

		#region TestCanLoginFilter

		public void TestUserStaffFilter()
		{
			var staffUser = Factory.NewWithValidTestData<GlbStaff>();
			staffUser.GS_CanLogin = true;

			var staffFilter = new GlbStaffFilterBusinessObject();
			((ModuleFlagsFilter)staffFilter["Can Login"]).Property0 = true;
			((ModuleFlagsFilter)staffFilter["Can Login"]).IsActive = true;

			Factory.Save();

			var staffCollection = new GlbStaffCollection(Factory, staffFilter.Filter);
			Assert("Filter can login true therefore staff should be in the collection", staffCollection.Contains(staffUser));

			staffUser.GS_CanLogin = false;
			Factory.Save();
			staffCollection = new GlbStaffCollection(Factory, staffFilter.Filter);
			Assert("Filter can login true and staff false therefore staff should not be in the collection", !staffCollection.Contains(staffUser));
		}

		#endregion

		#region TestLogEventFilter

		[TestSemaphoreProvider]
		public void TestUserLogEventFilter()
		{
			var staff = CreatStaffMember("Sango", "testpassword", "ts1", true);
			var staff1 = CreatStaffMember("Sangoku", "testpassword", "ts2", true);
			var staff2 = CreatStaffMember("Sangohan", "testpassword", "ts3", true);
			var controller = new UserLoginController();

			var logInfo = controller.LoginUser("Sango", "testpassword", false);
			var logInfo2 = controller.LoginUser("Sangoku", "testpassword", false);
			var logInfo3 = controller.LoginUser("Sangohan", "testpassword", false);

			AssertEquals("User logged in.", true, controller.LoginLocation(logInfo, Env.CurrentBranchPK, Env.CurrentDepartmentPK));
			AssertEquals("User logged in.", true, controller.LoginLocation(logInfo2, Env.CurrentBranchPK, Env.CurrentDepartmentPK));
			AssertEquals("User logged in.", true, controller.LoginLocation(logInfo3, Env.CurrentBranchPK, Env.CurrentDepartmentPK));

			var staffFilter = new GlbStaffFilterBusinessObject();
			((ModuleDateFilter)staffFilter["Login Time"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)staffFilter["Login Time"]).Property1 = ZDateTime.Today.AddDays(-1);
			((ModuleDateFilter)staffFilter["Login Time"]).Property2 = ZDateTime.Today;
			((ModuleDateFilter)staffFilter["Login Time"]).IsActive = true;

			Factory.Save();
			var staffCollection = new GlbStaffCollection(Factory, staffFilter.Filter);
			Assert("Filter can login true therefore staff should be in the collection", staffCollection.Contains(staff));
			Assert("Filter can login true therefore staff should be in the collection", staffCollection.Contains(staff1));
			Assert("Filter can login true therefore staff should be in the collection", staffCollection.Contains(staff2));
		}

		public void TestStaffLoginTimeAndLogoutTimeFilterUsingSL_PostedTimeUtcColumn()
		{
			var staffFilter = new GlbStaffFilterBusinessObject();
			var loginTimeFilter = (ModuleDateFilter)staffFilter["Login Time"];
			var logoutTimeFilter = (ModuleDateFilter)staffFilter["Logout Time"];
			AssertNotNull("loginTimeFilter", loginTimeFilter);
			AssertNotNull("logoutTimeFilter", logoutTimeFilter);

			loginTimeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			loginTimeFilter.Property1 = ZDateTime.Today.AddDays(-1);
			loginTimeFilter.Property2 = ZDateTime.Today;
			loginTimeFilter.IsActive = true;

			logoutTimeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			logoutTimeFilter.Property1 = ZDateTime.Today.AddDays(-1);
			logoutTimeFilter.Property2 = ZDateTime.Today;
			logoutTimeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("loginTimeFilter.ConvertFromLocalToUTC", true, loginTimeFilter.ConvertFromLocalToUTC);
				AssertEquals("logoutTimeFilter.ConvertFromLocalToUTC", true, logoutTimeFilter.ConvertFromLocalToUTC);

				AssertEquals("loginTimeFilter.UserEntersUtcValue", false, loginTimeFilter.UserEntersUtcValue);
				AssertEquals("logoutTimeFilter.UserEntersUtcValue", false, logoutTimeFilter.UserEntersUtcValue);

				AssertContains("SL_SE_NKEvent = 'LGO' and (SL_PostedTimeUtc", staffFilter.Filter.LiteralTextADO);
				AssertContains("SL_SE_NKEvent = 'LGI' and (SL_PostedTimeUtc", staffFilter.Filter.LiteralTextADO);
			});
		}

		GlbStaff CreatStaffMember(string loginName, string password, string code, bool active)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsResource = false;
			staff.GS_CanLogin = true;
			staff.GS_LoginName = loginName;
			staff.GS_Code = code;
			staff.StaffPlainTextPassword = password;
			staff.GS_IsActive = active;
			staff.GS_IsOperational = true;
			staff.GS_IsController = false;
			staff.GS_IsTwoFactorAuthenticationEnabled = false;
			staff.GS_EmailAddress = "e@mail.com";

			var security1 = Factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = staff.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";
			Factory.Save();
			return staff;
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

			var staffFilterBizo = new GlbStaffFilterBusinessObject();
			var notInTheWorkplaceFilter = (ModuleDateFilter)staffFilterBizo["Not In The Workplace"];

			notInTheWorkplaceFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			notInTheWorkplaceFilter.Property1 = ZDateTime.BrettsBirthday;
			notInTheWorkplaceFilter.Property2 = ZDateTime.BrettsBirthday.AddDays(2);
			notInTheWorkplaceFilter.IsActive = true;

			Factory.Save();

			var staffCollection = new GlbStaffCollection(Factory, staffFilterBizo.Filter);
			Assert("User is on holiday during the specified date range therefore staff should be in the collection", staffCollection.Contains(staffUser));

			staffUserHoliday.GA_EndTime = ZDateTime.BrettsBirthday.AddDays(-3);
			Factory.Save();
			staffCollection = new GlbStaffCollection(Factory, staffFilterBizo.Filter);
			Assert("Holiday finishes before the specified date range therefore staff should not be in the collection", !staffCollection.Contains(staffUser));

			staffUserHoliday.GA_EndTime = ZDateTime.BrettsBirthday;
			Factory.Save();
			staffCollection = new GlbStaffCollection(Factory, staffFilterBizo.Filter);
			Assert("Holiday finishes during the specified date range therefore staff should be in the collection", staffCollection.Contains(staffUser));

			staffUserHoliday.GA_StartTime = ZDateTime.BrettsBirthday.AddDays(2);
			staffUserHoliday.GA_EndTime = ZDateTime.BrettsBirthday.AddDays(4);
			Factory.Save();
			staffCollection = new GlbStaffCollection(Factory, staffFilterBizo.Filter);
			Assert("Holiday starts during the specified date range therefore staff should be in the collection", staffCollection.Contains(staffUser));

			staffUserHoliday.GA_StartTime = ZDateTime.BrettsBirthday.AddDays(3);
			Factory.Save();
			staffCollection = new GlbStaffCollection(Factory, staffFilterBizo.Filter);
			Assert("Holiday starts after the specified date range therefore staff should not be in the collection", !staffCollection.Contains(staffUser));
		}

		#endregion

		#region Workflow Custom Filters

		public void TestGetModuleFiltersWhenCustomFilterNamesClashWithReservedNames()
		{
			var customFieldName = "Gender";
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SAR";

			var columnDef = template.GenCustomColumnDefinitions.AddNew();
			columnDef.XC_Name = customFieldName;
			columnDef.XC_Type = Enterprise.MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.String;

			Factory.Save();

			var collection = new GlbStaffFilterBusinessObject().ModuleFilters;

			AssertNotNull(collection[customFieldName]);
			AssertNotNull(collection[customFieldName + " " + WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix]);
		}

		public void TestAddingWorkflowCustomFieldsFilters()
		{
			ModuleFilterCollection filterCollection = new GlbStaffFilterBusinessObject().ModuleFilters;

			AssertNull(filterCollection["customColumn11"]);
			AssertNull(filterCollection["customColumn12"]);
			AssertNull(filterCollection["customColumn21"]);
			AssertNull(filterCollection["customColumn22"]);
			AssertNull(filterCollection["Workflow Flags"]);
			AssertNull(filterCollection["customColumn31"]);

			SetupTemplates();

			filterCollection = new GlbStaffFilterBusinessObject().ModuleFilters;

			AssertEquals(typeof(ModuleTextFilter), filterCollection["customColumn11"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["customColumn12"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["customColumn21"].GetType());
			AssertEquals(typeof(ModuleTextFilter), filterCollection["customColumn22"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
			AssertNull(filterCollection["customColumn31"]);
		}

		void SetupTemplates()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "SAR";

			GenCustomColumnDefinition customColumn11 = template1.GenCustomColumnDefinitions.AddNew();
			customColumn11.XC_Name = "customColumn11";
			customColumn11.XC_Type = AddOnColumnDataType.Codes.String;

			GenCustomColumnDefinition customColumn12 = template1.GenCustomColumnDefinitions.AddNew();
			customColumn12.XC_Name = "customColumn12";
			customColumn12.XC_Type = AddOnColumnDataType.Codes.Integer;

			ProcessTaskTemplate template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "SAR";

			GenCustomColumnDefinition customColumn21 = template2.GenCustomColumnDefinitions.AddNew();
			customColumn21.XC_Name = "customColumn21";
			customColumn21.XC_Type = AddOnColumnDataType.Codes.Datetime;

			GenCustomColumnDefinition customColumn22 = template2.GenCustomColumnDefinitions.AddNew();
			customColumn22.XC_Name = "customColumn22";
			customColumn22.XC_Type = AddOnColumnDataType.Codes.Boolean;

			GenCustomColumnDefinition duplicaColumn = template2.GenCustomColumnDefinitions.AddNew();
			duplicaColumn.XC_Name = "customColumn11";
			duplicaColumn.XC_Type = AddOnColumnDataType.Codes.String;

			ProcessTaskTemplate template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "YYY";

			GenCustomColumnDefinition def31 = template3.GenCustomColumnDefinitions.AddNew();
			def31.XC_Name = "customColumn31";
			def31.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			WorkflowCustomFieldsFilter.ClearCache();
		}

		#endregion

		#region Active Directory filters

		public void TestADLinkedFilter()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();

			var glbStaffFilterBizo = new GlbStaffFilterBusinessObject();
			var adLinkedFilter = (ModuleFlagsFilter)glbStaffFilterBizo["ADLinked"];
			adLinkedFilter.Property0 = false;
			adLinkedFilter.IsActive = true;

			var unlinkedStaffs = new GlbStaffCollection(Factory, glbStaffFilterBizo.Filter);
			Assert("Collection should contain staff1", unlinkedStaffs.Contains(staff1));
			Assert("Collection should contain staff2", unlinkedStaffs.Contains(staff2));
			Assert("Collection should not contain staff3", !unlinkedStaffs.Contains(staff3));

			adLinkedFilter.Property0 = true;
			var linkedStaffs = new GlbStaffCollection(Factory, glbStaffFilterBizo.Filter);
			Assert("Collection should not contain staff1", !linkedStaffs.Contains(staff1));
			Assert("Collection should not contain staff2", !linkedStaffs.Contains(staff2));
			Assert("Collection should contain staff3", linkedStaffs.Contains(staff3));
		}

		public void TestDomainNameFilter()
		{
			var filterObject = new GlbStaffFilterBusinessObject();
			var domainFilter = (ModuleTextFilter)filterObject[GlbStaffSchema.Constants.GS_DomainName];
			AssertNotNull("Domain Filter should always be shown.", domainFilter);

			var domain1 = ObjectFactory.Get<IDomainCredentials>();
			domain1.DomainName = "domain1";
			var aDRegistry = ObjectFactory.Get<IADRegistry>();
			aDRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1 };
			filterObject = new GlbStaffFilterBusinessObject();
			domainFilter = (ModuleTextFilter)filterObject[GlbStaffSchema.Constants.GS_DomainName];
			AssertNotNull("Domain Filter should always be shown.", domainFilter);

			var domain2 = ObjectFactory.Get<IDomainCredentials>();
			domain2.DomainName = "domain2";
			aDRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1, domain2 };
			filterObject = new GlbStaffFilterBusinessObject();
			domainFilter = (ModuleTextFilter)filterObject[GlbStaffSchema.Constants.GS_DomainName];
			AssertNotNull("Domain Filter should always be shown.", domainFilter);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_DomainName = "domain1";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_DomainName = "domain2";
			Factory.Save();

			var collection = new GlbStaffCollection(Factory, filterObject.Filter);
			AssertEquals(true, collection.Contains(staff1));
			AssertEquals(true, collection.Contains(staff2));

			domainFilter.Property = "domain1";
			domainFilter.IsActive = true;
			collection = new GlbStaffCollection(Factory, filterObject.Filter);
			AssertEquals(true, collection.Contains(staff1));
			AssertEquals(false, collection.Contains(staff2));

			domainFilter.Property = "domain2";
			domainFilter.IsActive = true;
			collection = new GlbStaffCollection(Factory, filterObject.Filter);
			AssertEquals(false, collection.Contains(staff1));
			AssertEquals(true, collection.Contains(staff2));
		}

		#endregion

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbStaffFilterBusinessObject();
		}

		#endregion

		#region TestCertificateIssueDate

		public void TestCertificateIssueDateFilter()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();

			GenRegCertAccredMaintList cert = staff.Certificates.AddNew();
			cert.XZ_ParentTableCode = staff.TablePrefix;
			cert.XZ_ParentID = staff.PK;
			cert.XZ_IssueDate = ZDateTime.Today;
			Factory.Save();

			GlbStaffFilterBusinessObject filterBizO = (GlbStaffFilterBusinessObject)CachedBusinessObject;
			ModuleDateFilter dateFilter = ((ModuleDateFilter)filterBizO["Certificate Issue Date"]);

			var results = Factory.Load<GlbStaff>(filterBizO.Filter);

			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			results = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertCollectionContains(staff, results);
			dateFilter.IsActive = false;

			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Tomorrow;
			results = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertCollectionNotContains(staff, results);
			dateFilter.IsActive = false;

			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = new ZDateTime(2016, 01, 10);
			dateFilter.Property2 = new ZDateTime(2016, 12, 30);
			results = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertCollectionNotContains(staff, results);
			dateFilter.IsActive = false;

			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.NextCalendarMonth;
			results = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertCollectionNotContains(staff, results);
			dateFilter.IsActive = false;
		}

		#endregion

		#region TestCertificateExpiryDate

		public void TestCertificateExpiryDateFilter()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();

			GenRegCertAccredMaintList cert = staff.Certificates.AddNew();
			cert.XZ_ParentTableCode = staff.TablePrefix;
			cert.XZ_ParentID = staff.PK;
			cert.XZ_ExpiryOrDueDate = new ZDateTime(2017, 01, 01);
			Factory.Save();

			GlbStaffFilterBusinessObject filterBizO = (GlbStaffFilterBusinessObject)CachedBusinessObject;
			ModuleDateFilter dateFilter = ((ModuleDateFilter)filterBizO["Certificate Expiry Date"]);
			var results = Factory.Load<GlbStaff>(filterBizO.Filter);

			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			results = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertCollectionNotContains(staff, results);
			dateFilter.IsActive = false;

			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Tomorrow;
			results = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertCollectionNotContains(staff, results);
			dateFilter.IsActive = false;

			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.Past;
			results = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertCollectionContains(staff, results);
			dateFilter.IsActive = false;
		}

		#endregion

		#region TestDisposedObjectsOnZGridWithZDescriptionCodeFindBoxColumnStyleInfo

		[ExpectNoExceptions]
		public void TestDisposedObjectsOnZGridWithZDescriptionCodeFindBoxColumnStyleInfo()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			dummyBizO.Collection.Add(Factory.New<DummyChildBiZo>());

			var collection = new ActiveBusinessObjectCollection<DummyChildBiZo>(Factory);
			collection.AddNew();

			using (var form = new ZForm())
			{
				using (var grid = new ZGrid())
				{
					form.Controls.Add(grid);

					grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo { ColumnName = "ColumnTest", Caption = string.Empty });
					grid.SetDataBinding(collection, string.Empty);

					form.Show();
					grid.ResetColumns();
				}
			}
		}

		#endregion

		#region TestSensitiveFiltersWithSecurityRights

		public void TestSensitiveFiltersWithSecurityRights()
		{
			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				AssertSensitiveFiltersWithSecurityRights(security.StaffViewBirthDate, "Birth Date");
				AssertSensitiveFiltersWithSecurityRights(security.StaffViewBirthDate, "Birthday In");
				AssertSensitiveFiltersWithSecurityRights(security.StaffViewHomeAddressDetails, "Home Phone Number");
				AssertSensitiveFiltersWithSecurityRights(security.StaffViewOtherReferences, "Other References");
				AssertSensitiveFiltersWithSecurityRights(security.StaffViewOtherLeave, "Leave Type and Date");
				AssertSensitiveFiltersWithSecurityRights(security.StaffViewOtherCertificates, "Certificate Issue Date");
				AssertSensitiveFiltersWithSecurityRights(security.StaffViewOtherCertificates, "Certificate Expiry Date");
				AssertSensitiveFiltersWithSecurityRights(security.StaffViewOtherEmploymentHistory, "Employment Date");
				AssertSensitiveFiltersWithSecurityRights(security.StaffViewOtherEmploymentHistory, "Departure Date");
				AssertSensitiveFiltersWithSecurityRights(security.StaffViewOtherGender, "Gender");
				AssertSensitiveFiltersWithSecurityRights(security.StaffViewOtherMobilePhone, "Mobile Phone");
				AssertSensitiveFiltersWithSecurityRights(security.StaffViewOtherWorkExtension, "Work Extension");
			}
		}

		#endregion

		[TestExcludeBusinessObjectsAllHaveTestCases]
		internal class DummyChildBiZo : DummyChildBusinessObject
		{
			public DummyChildBiZo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[List("CollectionTest")]
			public ZString ColumnTest { get; set; }

			public ZPropertyInfo ColumnTestInfo => GetZPropertyInfo(nameof(ColumnTest), "ColumnTest");

			public GlbStaffCollection CollectionTest => new GlbStaffCollection(Factory);
		}
	}
}
