using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDICodeMappingRelationshipLocalCodeModuleFilter))]
	sealed class EDICodeMappingRelationshipLocalCodeModuleFilterTest : ModuleTextFilterTest
	{
		public void TestFilter_EmptyQuery()
		{
			SetUpOrgPatternMatchOverridesForTest(out var ohPks, out var opmoPks);

			var filter = new EDICodeMappingFilterBusinessObject();

			var shipCodeFilter = (EDICodeMappingRelationshipLocalCodeModuleFilter)filter[FilterDescription.RelationshipLocalCode];
			shipCodeFilter.IsActive = true;

			var result = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[0], opmoPks[1], opmoPks[2], opmoPks[3], opmoPks[4], opmoPks[5] }, result);
		}

		public void TestFilter_EmptyQuery_RelationshipOnlyAndLocalCodeEmpty()
		{
			SetUpOrgPatternMatchOverridesForTest(out var ohPks, out var opmoPks);

			var filter = new EDICodeMappingFilterBusinessObject();

			var shipCodeFilter = (EDICodeMappingRelationshipLocalCodeModuleFilter)filter[FilterDescription.RelationshipLocalCode];
			shipCodeFilter.IsActive = true;

			shipCodeFilter.Relationship = "ORG";
			var result = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[2], opmoPks[3], opmoPks[4] }, result);
		}

		public void TestFilter_IsBlankQuery()
		{
			SetUpOrgPatternMatchOverridesForTest(out var ohPks, out var opmoPks);

			var filter = new EDICodeMappingFilterBusinessObject();

			var shipCodeFilter = (EDICodeMappingRelationshipLocalCodeModuleFilter)filter[FilterDescription.RelationshipLocalCode];
			shipCodeFilter.IsActive = true;

			shipCodeFilter.Relationship = "PKG";
			shipCodeFilter.ComparisonOperatorForOrgCo = EDICodeMappingRelationshipLocalCodeModuleFilter.ComparisonConstants.IsBlank;
			var result = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);

			AssertEquals(0, result.Count());
		}

		public void TestFilter_LocalGuid()
		{
			SetUpOrgPatternMatchOverridesForTest(out var ohPks, out var opmoPks);

			var filter = new EDICodeMappingFilterBusinessObject();

			var shipCodeFilter = (EDICodeMappingRelationshipLocalCodeModuleFilter)filter[FilterDescription.RelationshipLocalCode];
			shipCodeFilter.IsActive = true;

			shipCodeFilter.ComparisonOperator = EDICodeMappingRelationshipLocalCodeModuleFilter.ComparisonConstants.Exact;

			shipCodeFilter.Relationship = "ORG";
			shipCodeFilter.ComparisonOperatorForOrgCo = EDICodeMappingRelationshipLocalCodeModuleFilter.ComparisonConstants.Exact;
			shipCodeFilter.OrgCoGuid = ohPks[1];
			var resultForORG = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[4] }, resultForORG);
		}

		public void TestFilter_LocalCode()
		{
			SetUpOrgPatternMatchOverridesForTest(out var ohPks, out var opmoPks);

			var filter = new EDICodeMappingFilterBusinessObject();

			var shipCodeFilter = (EDICodeMappingRelationshipLocalCodeModuleFilter)filter[FilterDescription.RelationshipLocalCode];
			shipCodeFilter.IsActive = true;

			shipCodeFilter.Relationship = "PKG";
			shipCodeFilter.ComparisonOperatorForOrgCo = EDICodeMappingRelationshipLocalCodeModuleFilter.ComparisonConstants.Contains;
			shipCodeFilter.OrgCoName = "CO";
			var resultForORG = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[5] }, resultForORG);
		}

		public void TestFilter_LocalCodeWithFindBox()
		{
			SetUpOrgPatternMatchOverridesForTest(out var ohPks, out var opmoPks);

			var filter = new EDICodeMappingFilterBusinessObject();

			var shipCodeFilter = (EDICodeMappingRelationshipLocalCodeModuleFilter)filter[FilterDescription.RelationshipLocalCode];
			shipCodeFilter.IsActive = true;

			shipCodeFilter.Relationship = "CHC";
			shipCodeFilter.ComparisonOperatorForOrgCo = EDICodeMappingRelationshipLocalCodeModuleFilter.ComparisonConstants.Contains;
			shipCodeFilter.OrgCoName = "CO";
			var resultForORG = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[0], opmoPks[1] }, resultForORG);
		}

		public void TestSerialize_Deserialize_PropertiesFromToXml()
		{
			SetUpOrgPatternMatchOverridesForTest(out var ohPks, out var opmoPks);

			var filter = new EDICodeMappingFilterBusinessObject();

			filter.FilterStrips.AddNew(FilterDescription.RelationshipLocalCode);

			var shipCodeFilter = (EDICodeMappingRelationshipLocalCodeModuleFilter)filter[FilterDescription.RelationshipLocalCode];
			shipCodeFilter.IsActive = true;
			shipCodeFilter.ComparisonOperator = EDICodeMappingRelationshipLocalCodeModuleFilter.ComparisonConstants.Exact;
			shipCodeFilter.Relationship = "ORG";
			shipCodeFilter.ComparisonOperatorForOrgCo = EDICodeMappingRelationshipLocalCodeModuleFilter.ComparisonConstants.Exact;
			shipCodeFilter.OrgCoGuid = ohPks[1];

			var savedFilter = filter.SaveLayout("savedFilter");

			filter.FilterStrips.DeleteAll();

			filter.LoadLayout(savedFilter);
			var loadedFilter = (EDICodeMappingRelationshipLocalCodeModuleFilter)filter[FilterDescription.RelationshipLocalCode];

			AssertEquals(true, loadedFilter.IsActive);
			AssertEquals(EDICodeMappingRelationshipLocalCodeModuleFilter.ComparisonConstants.Exact, loadedFilter.ComparisonOperator);
			AssertEquals("ORG", loadedFilter.Relationship);
			AssertEquals(EDICodeMappingRelationshipLocalCodeModuleFilter.ComparisonConstants.Exact, loadedFilter.ComparisonOperatorForOrgCo);
			AssertEquals(ohPks[1], loadedFilter.OrgCoGuid);
			AssertEquals(string.Empty, loadedFilter.OrgCoName);
		}

		public void TestValidationType()
		{
			AssertType<EDICodeMappingRelationshipLocalCodeModuleFilterValidation>(GetEDICodeMappingRelationshipLocalCodeModuleFilterForTest().GetNewValidationForTest());
		}

		void SetUpOrgPatternMatchOverridesForTest(out Guid[] ohPks, out Guid[] opmoPks)
		{
			var orgHeader0 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader0.OH_Code = "ORG0";
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "ORG1";
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "ORG2";

			var orgPatternMatchOverride0 = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride0.OO_OH = orgHeader0.PK;
			orgPatternMatchOverride0.OO_Relationship = "CHC";
			orgPatternMatchOverride0.OrgCoNameorGuid = "CO1";

			var orgPatternMatchOverride1 = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride1.OO_OH = orgHeader0.PK;
			orgPatternMatchOverride1.OO_Relationship = "CHC";
			orgPatternMatchOverride1.OrgCoNameorGuid = "CO2";

			var orgPatternMatchOverride2 = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride2.OO_OH = orgHeader1.PK;
			orgPatternMatchOverride2.OO_Relationship = "ORG";
			orgPatternMatchOverride2.OrgCoNameorGuid = orgHeader0.PK.ToString();

			var orgPatternMatchOverride3 = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride3.OO_OH = orgHeader2.PK;
			orgPatternMatchOverride3.OO_Relationship = "ORG";
			orgPatternMatchOverride3.OrgCoNameorGuid = orgHeader0.PK.ToString();

			var orgPatternMatchOverride4 = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride4.OO_OH = orgHeader2.PK;
			orgPatternMatchOverride4.OO_Relationship = "ORG";
			orgPatternMatchOverride4.OrgCoNameorGuid = orgHeader1.PK.ToString();

			var orgPatternMatchOverride5 = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride5.OO_OH = orgHeader2.PK;
			orgPatternMatchOverride5.OO_Relationship = "PKG";
			orgPatternMatchOverride5.OrgCoNameorGuid = "ICO";

			Factory.Save();

			ohPks = new[] { orgHeader0.PK.ToGuid(), orgHeader1.PK.ToGuid(), orgHeader2.PK.ToGuid() };
			opmoPks = new[]
			{
				orgPatternMatchOverride0.PK.ToGuid(),
				orgPatternMatchOverride1.PK.ToGuid(),
				orgPatternMatchOverride2.PK.ToGuid(),
				orgPatternMatchOverride3.PK.ToGuid(),
				orgPatternMatchOverride4.PK.ToGuid(),
				orgPatternMatchOverride5.PK.ToGuid()
			};
		}

		IEnumerable<Guid> GetFilteredOrgPatternMatchOverridePkCollection(ZQuery filter)
		{
			return new OrgPatternMatchOverrideCollectionForOrgs(Factory, filter).Select(x => x.PK.ToGuid());
		}

		protected override BusinessObject GetNewBusinessObject() => new EDICodeMappingRelationshipLocalCodeModuleFilter("Test", Factory.New<OrgPatternMatchOverride>());

		EDICodeMappingRelationshipLocalCodeModuleFilterForTest GetEDICodeMappingRelationshipLocalCodeModuleFilterForTest()
		{
			return new EDICodeMappingRelationshipLocalCodeModuleFilterForTest("Test", new ReadOnlyBusinessObjectFactory().New<OrgPatternMatchOverride>());
		}
	}
}
