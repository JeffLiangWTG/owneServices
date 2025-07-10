using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbBranchFilterBusinessObject))]
	internal class GlbBranchFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestBranchManagementCodeFilter()
		{
			var branchManagementCodeFilter = (ModuleTextFilter)branchFilter["Branch Management Code"];
			branchManagementCodeFilter.Property = "BRB";
			branchManagementCodeFilter.IsActive = true;
			var collection = new GlbBranchCollection(Factory, branchFilter.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Collection should Contain Branch2", new[] { branch2 }, collection);
		}
		public void TestEmptyFilter()
		{
			var companyFilter = ((ModuleGuidFilter)branchFilter["Company"]);
			companyFilter.IsActive = true;
			GlbBranchCollection collection1 = new GlbBranchCollection(Factory, branchFilter.Filter);
			collection1.Load();

			AssertEquals("Collection should not Contain Branch1", false, collection1.Contains(branch1.PK));
			AssertEquals("Collection Contains Branch2", true, collection1.Contains(branch2.PK));
		}

		public void TestCompanyFilter()
		{
			var companyFilter = ((ModuleGuidFilter)branchFilter["Company"]);
			companyFilter.Property = company1.PK;
			companyFilter.IsActive = true;
			GlbBranchCollection collection2 = new GlbBranchCollection(Factory, branchFilter.Filter);
			collection2.Load();

			AssertEquals("Collection Contains Branch1", true, collection2.Contains(branch1.PK));
			AssertEquals("Collection should not contain Branch2", false, collection2.Contains(branch2.PK));

			companyFilter.Property = ZGuid.Empty;
			GlbBranchCollection collection3 = new GlbBranchCollection(Factory, branchFilter.Filter);
			collection3.Load();
			AssertEquals("Collection Contains Branch1", true, collection3.Contains(branch1.PK));
			AssertEquals("Collection Contains Branch2", true, collection3.Contains(branch2.PK));

			AssertEquals("Visibility of company filter should be AlwaysVisible", FilterVisibility.AlwaysVisible, companyFilter.Visibility);
		}

		public void TestGetModuleFiltersFromGlowCore_WhenIndexSearch_CompanyFilterVisibilityIsAlwaysVisible()
		{
			using (GetIndexSearchRegistryMock())
			using (GetGlowIndexQueryEngineMock())
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				filterStrip.IndexSearchFields = GetSearchFieldCollection();
				filterStrip.SearchType = SearchType.Index;
				filterStrip.LoadModuleFilters();

				var companyFilter = filterStrip["COMPANY"] as IndexSearchModuleGuidFilter;
				AssertNotNull(companyFilter);
				AssertEquals(GlbCompany.CurrentCompany.PK, companyFilter.Property);
				AssertEquals(GlbCompany.CurrentCompany.PK, companyFilter.DefaultProperty);
				AssertEquals(FilterVisibility.AlwaysVisible, companyFilter.Visibility);
			}
		}

		public void TestResolveSearchField_WhenIndexSearch_HasCorrectCategories()
		{
			using (GetIndexSearchRegistryMock())
			using (GetGlowIndexQueryEngineMock())
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				filterStrip.IndexSearchFields = GetSearchFieldCollection();
				filterStrip.SearchType = SearchType.Index;
				filterStrip.LoadModuleFilters();

				var companyFilter = filterStrip["COMPANY"] as IndexSearchModuleGuidFilter;
				AssertNotNull(companyFilter);
				AssertEquals(FilterCategories.Other, companyFilter.Category);

				var cityFilter = filterStrip["CITY"] as IndexSearchModuleTextFilter;
				AssertNotNull(cityFilter);
				AssertEquals(FilterCategories.Locations, cityFilter.Category);

				var stateFilter = filterStrip["STATE"] as IndexSearchModuleTextFilter;
				AssertNotNull(stateFilter);
				AssertEquals(FilterCategories.Locations, stateFilter.Category);

				var webAddressFilter = filterStrip["WEBADDRESS"] as IndexSearchModuleTextFilter;
				AssertNotNull(webAddressFilter);
				AssertEquals(FilterCategories.Locations, webAddressFilter.Category);

				var locationFilter = filterStrip["HOMEPORT"] as IndexSearchModuleTextFilter;
				AssertNotNull(locationFilter);
				AssertEquals(FilterCategories.Locations, locationFilter.Category);

				var activeStatusFilter = filterStrip["ACTIVITYSTATUS"] as IndexSearchModuleTextFilter;
				AssertNotNull(activeStatusFilter);
				AssertEquals(FilterCategories.TextSearch, activeStatusFilter.Category);

				var branchNameFilter = filterStrip["BRANCHNAME"] as IndexSearchModuleTextFilter;
				AssertNotNull(branchNameFilter);
				AssertEquals(FilterCategories.TextSearch, branchNameFilter.Category);

				var managementCodeFilter = filterStrip["ACCOUNTINGGROUPCODE"] as IndexSearchModuleTextFilter;
				AssertNotNull(managementCodeFilter);
				AssertEquals(FilterCategories.TextSearch, managementCodeFilter.Category);
			}
		}

		IDisposable GetIndexSearchRegistryMock()
		{
			var registryMock = new Mock<IGlowRegistry>();
			_ = registryMock.Setup(e => e.IsGlowIndexSearchAllowedForModule(It.IsAny<string>())).Returns(true);
			_ = registryMock.Setup(e => e.MaximumNumberOfModuleFiltersSearchResults).Returns(50);
			return ObjectFactory.Substitute(registryMock.Object);
		}

		IDisposable GetGlowIndexQueryEngineMock()
		{
			var mock = new Mock<IGlowIndexQueryEngine>();
			_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(new SearchFieldCollection(null, Array.Empty<SearchField>()));
			_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject", "IGlbBranch" });
			return ObjectFactory.Substitute(mock.Object);
		}

		SearchFieldCollection GetSearchFieldCollection()
		{
			var entityLookup = new SearchFieldEntityLookup("IGlbCompany", "IGlbCompany", "GlbCompany", "Code", "IsActive");
			var field1 = new SearchField("COMPANY", "Company", typeof(Guid), false, false, 0, entityLookup);
			var field2 = SearchField.Create("CITY", "City");
			var field3 = SearchField.Create("STATE", "State");
			var field4 = SearchField.Create("WEBADDRESS", "Web Address");
			var field5 = SearchField.Create("HOMEPORT", "Location/Port");
			var field6 = SearchField.Create("ACTIVITYSTATUS", "Active Status");
			var field7 = SearchField.Create("BRANCHNAME", "Branch Name");
			var field8 = SearchField.Create("ACCOUNTINGGROUPCODE", "Branch Management Code");
			var ret = new SearchFieldCollection(null, new SearchField[] { field1, field2, field3, field4, field5, field6, field7, field8 });
			return ret;
		}

		#region Implementation

		internal GlbBranchFilterBusinessObject branchFilter;

		internal RefCurrency currency1;
		internal RefCountry country1;
		internal GlbCompany company1;
		internal GlbBranch branch1;
		internal GlbBranch branch2;

		protected override void SetUp()
		{
			base.SetUp();

			branchFilter = GetNewFilterStripBusinessObject() as GlbBranchFilterBusinessObject;

			currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "AA";

			country1 = Factory.New<RefCountry>();
			country1.RN_Code = "AA";
			country1.RN_RX_NKLocalCurrency = currency1.RX_Code;

			var codeCollection = new BranchManagementCodeDescriptionBoolCollection();
			codeCollection.Add("BRA", null, true);
			codeCollection.Add("BRB", null, true);
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, codeCollection);

			company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = country1.Code;
			company1.GC_RX_NKLocalCurrency = currency1.RX_Code;
			company1.GC_Code = "BOB";

			branch1 = Factory.New<GlbBranch>();
			branch2 = Factory.New<GlbBranch>();
			branch1.GB_Code = "ZZ";
			branch1.GB_BranchName = "ZZName";
			branch1.GB_GC = company1.PK;
			branch1.GB_City = "city1";
			branch1.GB_State = "state1";
			branch1.GB_WebAddress = "web1.com";
			branch1.GB_RL_NKHomePort = "AUSYD";
			branch1.GB_AccountingGroupCode = "BRA";

			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "XX";
			branch2.GB_BranchName = "XXName";
			branch2.GB_City = "city2";
			branch2.GB_State = "state2";
			branch2.GB_WebAddress = "web2.com";
			branch2.GB_RL_NKHomePort = "NZAKL";
			branch2.GB_IsActive = false;
			branch2.GB_AccountingGroupCode = "BRB";

			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbBranchFilterBusinessObject();
		}

		#endregion
	}
}
