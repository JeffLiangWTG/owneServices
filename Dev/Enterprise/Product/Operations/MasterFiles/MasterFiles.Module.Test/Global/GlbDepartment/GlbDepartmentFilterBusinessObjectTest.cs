using System.Collections.Generic;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbDepartmentFilterBusinessObject))]
	sealed class GlbDepartmentFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestModeReadonly()
		{
			GlbDepartmentFilterBusinessObject filter = new GlbDepartmentFilterBusinessObject();
			((ModuleTextFilter)filter["Activity"]).Property = "";
			((ModuleTextFilter)filter["Activity"]).IsActive = true;
			AssertEquals("Mode list should be empty", 0, ((ModuleTextFilter)filter["Mode"]).List.Count);
			AssertHasWarnings("Warning should exist if no Activity filter has been chosen", ((ModuleTextFilter)filter["Mode"]).PropertyInfo);

			((ModuleTextFilter)filter["Activity"]).Property = "S";
			((ModuleTextFilter)filter["Activity"]).IsActive = true;
			AssertNotEquals("Mode list should not be empty", 0, ((ModuleTextFilter)filter["Mode"]).List.Count);
			AssertNoWarnings("There should be no warning as Activity filter is not empty", ((ModuleTextFilter)filter["Mode"]).PropertyInfo);
		}

		public void TestDepartmentCodeFilter()
		{
			GlbDepartment department1 = Factory.New<GlbDepartment>();
			department1.GE_Code = "CSQ";

			GlbDepartment department2 = Factory.New<GlbDepartment>();
			department2.GE_Code = "HAR";

			GlbDepartmentFilterBusinessObject filter = new GlbDepartmentFilterBusinessObject();

			((ModuleTextFilter)filter["Code"]).Property = "CSQ";
			((ModuleTextFilter)filter["Code"]).IsActive = true;

			GlbDepartmentCollection coll = new GlbDepartmentCollection(Factory);

			coll.AdditionalFilter = filter.Filter;
			AssertCollectionContains(department1, coll);
			AssertCollectionNotContains(department2, coll);
		}

		public void TestDepartmentDescriptionFilter()
		{
			GlbDepartment department1 = Factory.New<GlbDepartment>();
			department1.GE_Desc = "Nibuz";

			GlbDepartment department2 = Factory.New<GlbDepartment>();
			department2.GE_Desc = "Hednahshkar";

			GlbDepartmentFilterBusinessObject filter = new GlbDepartmentFilterBusinessObject();

			((ModuleTextFilter)filter["Description"]).Property = "Nibuz";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			GlbDepartmentCollection coll = new GlbDepartmentCollection(Factory);

			coll.AdditionalFilter = filter.Filter;
			AssertCollectionContains(department1, coll);
			AssertCollectionNotContains(department2, coll);
		}

		public void TestDepartmentMultilingualDescriptionFilter()
		{
			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "TSD";
			department.GE_Desc = "Freight Services";
			Factory.Save();

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockData = Res.UseMockData())
			{
				var key = department.GE_DescInfo.CustomizableDataResourceStrings.Source.GetKey(null, "Freight Services");
				mockData.Put(key, new ResourceStringData(key, "货运服务"));

				var filterObj = new GlbDepartmentFilterBusinessObject();

				var translatableFilter = (ModuleTranslatableTextFilter)filterObj["Description_Local"];
				AssertNotNull(translatableFilter);
				translatableFilter.Property = "货运服务T";
				translatableFilter.IsActive = true;

				GlbDepartmentCollection coll = new GlbDepartmentCollection(Factory);

				coll.AdditionalFilter = filterObj.Filter;
				AssertCollectionNotContains(department, coll);

				translatableFilter.Property = "货运服务";
				coll.AdditionalFilter = filterObj.Filter;
				AssertCollectionContains(department, coll);
			}
		}

		void setUpDepartments()
		{
			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "FEX";
			department1.GE_SystemCode = true;

			GlbDepartment department2 = Factory.NewWithValidTestData<GlbDepartment>();
			department2.GE_Code = "FIY";
			department2.GE_SystemCode = true;

			GlbDepartment department3 = Factory.NewWithValidTestData<GlbDepartment>();
			department3.GE_Code = "GIZ";
			department3.GE_SystemCode = true;

			GlbDepartment department4 = Factory.NewWithValidTestData<GlbDepartment>();
			department4.GE_Code = "FXS";
			department4.GE_SystemCode = true;

			GlbDepartment department5 = Factory.NewWithValidTestData<GlbDepartment>();
			department5.GE_Code = "FYA";
			department5.GE_SystemCode = true;

			GlbDepartment department6 = Factory.NewWithValidTestData<GlbDepartment>();
			department6.GE_Code = "PPP";
			department6.GE_GE = department1.PK;
			department6.GE_SystemCode = false;

			GlbDepartment department7 = Factory.NewWithValidTestData<GlbDepartment>();
			department7.GE_Code = "QQQ";
			department7.GE_GE = department3.PK;
			department7.GE_SystemCode = false;

			GlbDepartment department8 = Factory.NewWithValidTestData<GlbDepartment>();
			department8.GE_Code = "ZZZ";
			department8.GE_GE = department5.PK;
			department8.GE_SystemCode = false;

			GlbDepartment department9 = Factory.NewWithValidTestData<GlbDepartment>();
			department9.GE_Code = "XXX";
			department9.GE_GE = department5.PK;
			department9.GE_SystemCode = true;

			Factory.Save();
		}

		public void TestActivityFilter()
		{
			setUpDepartments();

			GlbDepartmentFilterBusinessObject filter = new GlbDepartmentFilterBusinessObject();
			GlbDepartmentCollection coll = new GlbDepartmentCollection(Factory);

			((ModuleTextFilter)filter["Activity"]).Property = "F";
			((ModuleTextFilter)filter["Activity"]).IsActive = true;
			coll.AdditionalFilter = filter.Filter;
			AssertEquals("Should contain dept1 for activity = Forwarding", 1, coll.Find(c => c.GE_Code == "FEX").Count());
			AssertEquals("Should contain dept2 for activity = Forwarding", 1, coll.Find(c => c.GE_Code == "FIY").Count());
			AssertEquals("Should NOT contain dept3 for activity = Forwarding", 0, coll.Find(c => c.GE_Code == "GIZ").Count());
			AssertEquals("Should contain dept4 for activity = Forwarding", 1, coll.Find(c => c.GE_Code == "FXS").Count());
			AssertEquals("Should contain dept5 for activity = Forwarding", 1, coll.Find(c => c.GE_Code == "FYA").Count());
			AssertEquals("Should contain dept6 for activity = Forwarding", 1, coll.Find(c => c.GE_Code == "PPP").Count());
			AssertEquals("Should NOT contain dept7 for activity = Forwarding", 0, coll.Find(c => c.GE_Code == "QQQ").Count());
			AssertEquals("Should contain dept8 for activity = Forwarding", 1, coll.Find(c => c.GE_Code == "ZZZ").Count());
		}

		public void TestActivityMiscellaneousFilter()
		{
			setUpDepartments();

			GlbDepartmentFilterBusinessObject filter = new GlbDepartmentFilterBusinessObject();
			GlbDepartmentCollection coll = new GlbDepartmentCollection(Factory);

			((ModuleTextFilter)filter["Activity"]).Property = "M";
			((ModuleTextFilter)filter["Activity"]).IsActive = true;
			coll.AdditionalFilter = filter.Filter;
			AssertEquals("Should NOT contain dept1 for activity = Forwarding", 0, coll.Find(c => c.GE_Code == "FEX").Count());
			AssertEquals("Should NOT contain dept2 for activity = Forwarding", 0, coll.Find(c => c.GE_Code == "FIY").Count());
			AssertEquals("Should NOT contain dept3 for activity = Forwarding", 0, coll.Find(c => c.GE_Code == "GIZ").Count());
			AssertEquals("Should NOT contain dept4 for activity = Forwarding", 0, coll.Find(c => c.GE_Code == "FXS").Count());
			AssertEquals("Should NOT contain dept5 for activity = Forwarding", 0, coll.Find(c => c.GE_Code == "FYA").Count());
			AssertEquals("Should NOT contain dept6 for activity = Forwarding", 0, coll.Find(c => c.GE_Code == "PPP").Count());
			AssertEquals("Should NOT contain dept7 for activity = Forwarding", 0, coll.Find(c => c.GE_Code == "QQQ").Count());
			AssertEquals("Should NOT contain dept8 for activity = Forwarding", 0, coll.Find(c => c.GE_Code == "ZZZ").Count());
			AssertEquals("Should contain dept9 for activity = Forwarding", 1, coll.Find(c => c.GE_Code == "XXX").Count());
		}

		public void TestDirectionFilter()
		{
			setUpDepartments();

			GlbDepartmentFilterBusinessObject filter = new GlbDepartmentFilterBusinessObject();
			GlbDepartmentCollection coll = new GlbDepartmentCollection(Factory);

			((ModuleTextFilter)filter["Direction"]).Property = "E";
			((ModuleTextFilter)filter["Direction"]).IsActive = true;
			coll.AdditionalFilter = filter.Filter;
			AssertEquals("Should contain dept1 for direction = Export", 1, coll.Find(c => c.GE_Code == "FEX").Count());
			AssertEquals("Should NOT contain dept2 for direction = Export", 0, coll.Find(c => c.GE_Code == "FIY").Count());
			AssertEquals("Should NOT contain dept3 for direction = Export", 0, coll.Find(c => c.GE_Code == "GIZ").Count());
			AssertEquals("Should NOT contain dept4 for direction = Export", 0, coll.Find(c => c.GE_Code == "FXS").Count());
			AssertEquals("Should NOT contain dept5 for direction = Export", 0, coll.Find(c => c.GE_Code == "FYA").Count());
			AssertEquals("Should contain dept6 for direction = Export", 1, coll.Find(c => c.GE_Code == "PPP").Count());
			AssertEquals("Should NOT contain dept7 for direction = Export", 0, coll.Find(c => c.GE_Code == "QQQ").Count());
			AssertEquals("Should NOT contain dept8 for direction = Export", 0, coll.Find(c => c.GE_Code == "ZZZ").Count());
		}

		public void TestDirectionOtherFilter()
		{
			setUpDepartments();

			GlbDepartmentFilterBusinessObject filter = new GlbDepartmentFilterBusinessObject();
			GlbDepartmentCollection coll = new GlbDepartmentCollection(Factory);

			((ModuleTextFilter)filter["Direction"]).Property = "O";
			((ModuleTextFilter)filter["Direction"]).IsActive = true;
			coll.AdditionalFilter = filter.Filter;
			AssertEquals("Should NOT contain dept1 for direction = Export", 0, coll.Find(c => c.GE_Code == "FEX").Count());
			AssertEquals("Should NOT contain dept2 for direction = Export", 0, coll.Find(c => c.GE_Code == "FIY").Count());
			AssertEquals("Should NOT contain dept3 for direction = Export", 0, coll.Find(c => c.GE_Code == "GIZ").Count());
			AssertEquals("Should contain dept4 for direction = Export", 1, coll.Find(c => c.GE_Code == "FXS").Count());
			AssertEquals("Should contain dept5 for direction = Export", 1, coll.Find(c => c.GE_Code == "FYA").Count());
			AssertEquals("Should NOT contain dept6 for direction = Export", 0, coll.Find(c => c.GE_Code == "PPP").Count());
			AssertEquals("Should NOT contain dept7 for direction = Export", 0, coll.Find(c => c.GE_Code == "QQQ").Count());
			AssertEquals("Should contain dept8 for direction = Export", 1, coll.Find(c => c.GE_Code == "ZZZ").Count());
			AssertEquals("Should contain dept9 for direction = Export", 1, coll.Find(c => c.GE_Code == "XXX").Count());
		}

		public void TestModeFilter()
		{
			setUpDepartments();

			GlbDepartmentFilterBusinessObject filter = new GlbDepartmentFilterBusinessObject();
			GlbDepartmentCollection coll = new GlbDepartmentCollection(Factory);

			((ModuleTextFilter)filter["Mode"]).Property = "A";
			((ModuleTextFilter)filter["Mode"]).IsActive = true;
			coll.AdditionalFilter = filter.Filter;
			AssertEquals("Should NOT contain dept1 for mode = Air", 0, coll.Find(c => c.GE_Code == "FEX").Count());
			AssertEquals("Should NOT contain dept2 for mode = Air", 0, coll.Find(c => c.GE_Code == "FIY").Count());
			AssertEquals("Should NOT contain dept3 for mode = Air", 0, coll.Find(c => c.GE_Code == "GIZ").Count());
			AssertEquals("Should NOT contain dept4 for mode = Air", 0, coll.Find(c => c.GE_Code == "FXS").Count());
			AssertEquals("Should contain dept5 for mode = Air", 1, coll.Find(c => c.GE_Code == "FYA").Count());
			AssertEquals("Should NOT contain dept6 for mode = Air", 0, coll.Find(c => c.GE_Code == "PPP").Count());
			AssertEquals("Should NOT contain dept7 for mode = Air", 0, coll.Find(c => c.GE_Code == "QQQ").Count());
			AssertEquals("Should contain dept8 for mode = Air", 1, coll.Find(c => c.GE_Code == "ZZZ").Count());
		}

		public void TestModeOtherFilter()
		{
			setUpDepartments();

			GlbDepartmentFilterBusinessObject filter = new GlbDepartmentFilterBusinessObject();
			GlbDepartmentCollection coll = new GlbDepartmentCollection(Factory);

			((ModuleTextFilter)filter["Activity"]).Property = "F";
			((ModuleTextFilter)filter["Activity"]).IsActive = true;
			((ModuleTextFilter)filter["Mode"]).Property = "O";
			((ModuleTextFilter)filter["Mode"]).IsActive = true;
			coll.AdditionalFilter = filter.Filter;
			AssertEquals("Should contain dept1 for mode = Air", 1, coll.Find(c => c.GE_Code == "FEX").Count());
			AssertEquals("Should contain dept2 for mode = Air", 1, coll.Find(c => c.GE_Code == "FIY").Count());
			AssertEquals("Should NOT contain dept3 for mode = Air", 0, coll.Find(c => c.GE_Code == "GIZ").Count());
			AssertEquals("Should NOT contain dept4 for mode = Air", 0, coll.Find(c => c.GE_Code == "FXS").Count());
			AssertEquals("Should NOT contain dept5 for mode = Air", 0, coll.Find(c => c.GE_Code == "FYA").Count());
			AssertEquals("Should contain dept6 for mode = Air", 1, coll.Find(c => c.GE_Code == "PPP").Count());
			AssertEquals("Should NOT contain dept7 for mode = Air", 0, coll.Find(c => c.GE_Code == "QQQ").Count());
			AssertEquals("Should NOT contain dept8 for mode = Air", 0, coll.Find(c => c.GE_Code == "ZZZ").Count());
			AssertEquals("Should contain dept9 for mode = Air", 1, coll.Find(c => c.GE_Code == "XXX").Count());
		}

		public void TestFreightAndFreightAdjacentFiltersAreIgnoredInProductivityWiseMode()
		{
			AssertEquals("PRE: we started our testing framework in CW1 mode", false, DataRegistry.Instance.ProductivityWiseModeEnabled);
			var deptFiltBizoModFilts = new GlbDepartmentFilterBusinessObject().ModuleFilters;

			CombineAssertions("We want these filters in CW1 Mode", () =>
			{
				AssertEquals(true, deptFiltBizoModFilts.Any(modFilt => modFilt.Description.Equals("Activity")));
				AssertEquals(true, deptFiltBizoModFilts.Any(modFilt => modFilt.Description.Equals("Direction")));
				AssertEquals(true, deptFiltBizoModFilts.Any(modFilt => modFilt.Description.Equals("Mode")));
			});

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			AssertEquals("PRE: we succesfully moved to ProductivityWise mode", true, DataRegistry.Instance.ProductivityWiseModeEnabled);

			deptFiltBizoModFilts = new GlbDepartmentFilterBusinessObject().ModuleFilters;

			CombineAssertions("We do NOT want these filters in ProductivityWise Mode", () =>
			{
				AssertEquals(false, deptFiltBizoModFilts.Any(modFilt => modFilt.Description.Equals("Activity")));
				AssertEquals(false, deptFiltBizoModFilts.Any(modFilt => modFilt.Description.Equals("Direction")));
				AssertEquals(false, deptFiltBizoModFilts.Any(modFilt => modFilt.Description.Equals("Mode")));
			});

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			AssertEquals("PRE: we succesfully moved out of ProductivityWise mode", false, DataRegistry.Instance.ProductivityWiseModeEnabled);

			deptFiltBizoModFilts = new GlbDepartmentFilterBusinessObject().ModuleFilters;

			CombineAssertions("We want these filters in CW1 Mode", () =>
			{
				AssertEquals(true, deptFiltBizoModFilts.Any(modFilt => modFilt.Description.Equals("Activity")));
				AssertEquals(true, deptFiltBizoModFilts.Any(modFilt => modFilt.Description.Equals("Direction")));
				AssertEquals(true, deptFiltBizoModFilts.Any(modFilt => modFilt.Description.Equals("Mode")));
			});
		}

		public void TestIndexSearchActivityFilter()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbDepartment))
			using (var mocker = new GlowIndexQueryEngineMock(
				mock =>
				{
					_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetSearchFieldCollection());
					_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IGlbDepartment" });
				}))
			{
				AssertEquals(false, DataRegistry.Instance.ProductivityWiseModeEnabled);
				var filterBusinessObject = (GlbDepartmentFilterBusinessObject)module.FilterBusinessObject;
				var activityFilter = (IndexSearchModuleTextFilter)filterBusinessObject["Activity"];
				AssertNotNull(activityFilter);

				activityFilter.Property = "C";
				AssertEquals($"((startswith({filterBusinessObject.ActivityFieldName},'C')) or (startswith({filterBusinessObject.ParentCodeFieldName},'C')))", activityFilter.GetGlowIndexQuery().ToUrlComponent());

				activityFilter.Property = "D";
				AssertEquals($"((startswith({filterBusinessObject.ActivityFieldName},'D')) or (startswith({filterBusinessObject.ParentCodeFieldName},'D')))", activityFilter.GetGlowIndexQuery().ToUrlComponent());

				activityFilter.Property = "F";
				AssertEquals($"((startswith({filterBusinessObject.ActivityFieldName},'F')) or (startswith({filterBusinessObject.ParentCodeFieldName},'F')))", activityFilter.GetGlowIndexQuery().ToUrlComponent());

				activityFilter.Property = "L";
				AssertEquals($"((startswith({filterBusinessObject.ActivityFieldName},'L')) or (startswith({filterBusinessObject.ParentCodeFieldName},'L')))", activityFilter.GetGlowIndexQuery().ToUrlComponent());

				activityFilter.Property = "S";
				AssertEquals($"((startswith({filterBusinessObject.ActivityFieldName},'S')) or (startswith({filterBusinessObject.ParentCodeFieldName},'S')))", activityFilter.GetGlowIndexQuery().ToUrlComponent());

				activityFilter.Property = "T";
				AssertEquals($"((startswith({filterBusinessObject.ActivityFieldName},'T')) or (startswith({filterBusinessObject.ParentCodeFieldName},'T')))", activityFilter.GetGlowIndexQuery().ToUrlComponent());

				activityFilter.Property = "W";
				AssertEquals($"((startswith({filterBusinessObject.ActivityFieldName},'W')) or (startswith({filterBusinessObject.ParentCodeFieldName},'W')))", activityFilter.GetGlowIndexQuery().ToUrlComponent());

				activityFilter.Property = "G";
				AssertEquals($"((startswith({filterBusinessObject.ActivityFieldName},'G')) or (startswith({filterBusinessObject.ParentCodeFieldName},'G')))", activityFilter.GetGlowIndexQuery().ToUrlComponent());

				activityFilter.Property = "Y";
				AssertEquals($"((startswith({filterBusinessObject.ActivityFieldName},'Y')) or (startswith({filterBusinessObject.ParentCodeFieldName},'Y')))", activityFilter.GetGlowIndexQuery().ToUrlComponent());

				activityFilter.Property = "M";
				AssertEquals($"((((not(startswith({filterBusinessObject.ActivityFieldName},'C'))) and (not(startswith({filterBusinessObject.ActivityFieldName},'D'))) and (not(startswith({filterBusinessObject.ActivityFieldName},'F'))) and (not(startswith({filterBusinessObject.ActivityFieldName},'L'))) and (not(startswith({filterBusinessObject.ActivityFieldName},'S'))) and (not(startswith({filterBusinessObject.ActivityFieldName},'T'))) and (not(startswith({filterBusinessObject.ActivityFieldName},'W'))) and (not(startswith({filterBusinessObject.ActivityFieldName},'G'))) and (not(startswith({filterBusinessObject.ActivityFieldName},'Y')))) and (({filterBusinessObject.ActivityFieldName} ne null) and ({filterBusinessObject.ActivityFieldName} ne ''))) or (((not(startswith({filterBusinessObject.ParentCodeFieldName},'C'))) and (not(startswith({filterBusinessObject.ParentCodeFieldName},'D'))) and (not(startswith({filterBusinessObject.ParentCodeFieldName},'F'))) and (not(startswith({filterBusinessObject.ParentCodeFieldName},'L'))) and (not(startswith({filterBusinessObject.ParentCodeFieldName},'S'))) and (not(startswith({filterBusinessObject.ParentCodeFieldName},'T'))) and (not(startswith({filterBusinessObject.ParentCodeFieldName},'W'))) and (not(startswith({filterBusinessObject.ParentCodeFieldName},'G'))) and (not(startswith({filterBusinessObject.ParentCodeFieldName},'Y')))) and (({filterBusinessObject.ParentCodeFieldName} ne null) and ({filterBusinessObject.ParentCodeFieldName} ne ''))))", activityFilter.GetGlowIndexQuery().ToUrlComponent());
			}
		}

		public void TestIndexSearchActivityFilterWhenProductivityWiseModeEnabled()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbDepartment))
			using (var mocker = new GlowIndexQueryEngineMock(
				mock =>
				{
					_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetSearchFieldCollection());
					_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IGlbDepartment" });
				}))
			{
				DataRegistry.Instance.ProductivityWiseModeEnabled = true;
				AssertEquals(null, module.FilterBusinessObject["Activity"]);
			}
		}

		SearchFieldCollection GetSearchFieldCollection()
		{
			var field1 = SearchField.Create("CWDEFAULTHIDDENACTIVITY", "CWDEFAULTHIDDENACTIVITY");
			var ret = new SearchFieldCollection("IGlbDepartment", new SearchField[] { field1 });
			return ret;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbDepartmentFilterBusinessObject();
		}
	}
}
