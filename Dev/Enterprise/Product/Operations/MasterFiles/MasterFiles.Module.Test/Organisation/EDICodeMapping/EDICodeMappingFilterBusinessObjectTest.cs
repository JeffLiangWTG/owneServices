using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDICodeMappingFilterBusinessObject))]
	sealed class EDICodeMappingFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDICodeMappingFilterBusinessObject();
		}

		public void TestOrganizationFilter()
		{
			SetUpOrgPatternMatchOverridesForTest(out var ohPks, out var opmoPks);

			var filter = new EDICodeMappingFilterBusinessObject();

			var orgFilter = (ModuleGuidFilter)filter[FilterDescription.Organization];
			orgFilter.IsActive = true;

			orgFilter.Property = ohPks[0];
			var resultForOrg0 = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);
			orgFilter.Property = ohPks[1];
			var resultForOrg1 = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);
			orgFilter.Property = ohPks[2];
			var resultForOrg2 = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[0], opmoPks[1] }, resultForOrg0);
			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[2] }, resultForOrg1);
			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[3], opmoPks[4] }, resultForOrg2);
		}

		public void TestForeignCodeFilter()
		{
			SetUpOrgPatternMatchOverridesForTest(out var ohPks, out var opmoPks);

			var filter = new EDICodeMappingFilterBusinessObject();

			var fCodeFilter = (ModuleTextFilter)filter[FilterDescription.ForeignCode];
			fCodeFilter.IsActive = true;

			fCodeFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			var resultForIsBlank = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);
			fCodeFilter.Property = "FA1a";
			fCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var resultForEqual = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);
			fCodeFilter.Property = "FA1";
			fCodeFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			var resultForStartsWith = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);
			fCodeFilter.Property = "B1";
			fCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			var resultForContains = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[1] }, resultForIsBlank);
			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[0] }, resultForEqual);
			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[0], opmoPks[3], opmoPks[4] }, resultForStartsWith);
			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[2] }, resultForContains);
		}

		public void TestRelationshipLocalCodeFilter()
		{
			SetUpOrgPatternMatchOverridesForTest(out var ohPks, out var opmoPks);

			var filter = new EDICodeMappingFilterBusinessObject();

			var shipCodeFilter = (EDICodeMappingRelationshipLocalCodeModuleFilter)filter[FilterDescription.RelationshipLocalCode];
			shipCodeFilter.IsActive = true;

			shipCodeFilter.ComparisonOperator = EDICodeMappingRelationshipLocalCodeModuleFilter.ComparisonConstants.Exact;

			shipCodeFilter.Relationship = "ORG";
			shipCodeFilter.ComparisonOperatorForOrgCo = EDICodeMappingRelationshipLocalCodeModuleFilter.ComparisonConstants.Exact;
			shipCodeFilter.OrgCoGuid = ohPks[0];
			var resultForORG = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);

			shipCodeFilter.Relationship = "CHC";
			shipCodeFilter.ComparisonOperatorForOrgCo = EDICodeMappingRelationshipLocalCodeModuleFilter.ComparisonConstants.StartsWith;
			shipCodeFilter.OrgCoName = "CO";
			var resultForCHC = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[2], opmoPks[3] }, resultForORG);
			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[0], opmoPks[1] }, resultForCHC);
		}

		public void TestContextFilter()
		{
			SetUpOrgPatternMatchOverridesForTest(out var ohPks, out var opmoPks);

			var filter = new EDICodeMappingFilterBusinessObject();

			var contextFilter = (ModuleTextFilter)filter[FilterDescription.Context];
			contextFilter.IsActive = true;

			contextFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			var resultForIsBlank = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);
			contextFilter.Property = "CC1";
			contextFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var resultForEqual = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);
			contextFilter.Property = "CA";
			contextFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			var resultForStartsWith = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);
			contextFilter.Property = "CC";
			contextFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			var resultForContains = GetFilteredOrgPatternMatchOverridePkCollection(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[3], opmoPks[4] }, resultForIsBlank);
			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[0] }, resultForEqual);
			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[1] }, resultForStartsWith);
			AssertContainsExactElementsInAnyOrder(new[] { opmoPks[0], opmoPks[2] }, resultForContains);
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
			orgPatternMatchOverride0.OO_ForeignCode = "FA1a";
			orgPatternMatchOverride0.OO_Context = "CC1";
			orgPatternMatchOverride0.OO_Relationship = "CHC";
			orgPatternMatchOverride0.OrgCoNameorGuid = "CO1";

			var orgPatternMatchOverride1 = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride1.OO_OH = orgHeader0.PK;
			orgPatternMatchOverride1.OO_ForeignCode = string.Empty;
			orgPatternMatchOverride1.OO_Context = "CA2";
			orgPatternMatchOverride1.OO_Relationship = "CHC";
			orgPatternMatchOverride1.OrgCoNameorGuid = "CO2";

			var orgPatternMatchOverride2 = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride2.OO_OH = orgHeader1.PK;
			orgPatternMatchOverride2.OO_ForeignCode = "FB1c";
			orgPatternMatchOverride2.OO_Context = "CC2";
			orgPatternMatchOverride2.OO_Relationship = "ORG";
			orgPatternMatchOverride2.OrgCoNameorGuid = orgHeader0.PK.ToString();

			var orgPatternMatchOverride3 = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride3.OO_OH = orgHeader2.PK;
			orgPatternMatchOverride3.OO_ForeignCode = "FA1c";
			orgPatternMatchOverride3.OO_Context = string.Empty;
			orgPatternMatchOverride3.OO_Relationship = "ORG";
			orgPatternMatchOverride3.OrgCoNameorGuid = orgHeader0.PK.ToString();

			var orgPatternMatchOverride4 = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride4.OO_OH = orgHeader2.PK;
			orgPatternMatchOverride4.OO_ForeignCode = "FA1b";
			orgPatternMatchOverride4.OO_Context = string.Empty;
			orgPatternMatchOverride4.OO_Relationship = "ORG";
			orgPatternMatchOverride4.OrgCoNameorGuid = orgHeader1.PK.ToString();

			Factory.Save();

			ohPks = new[] { orgHeader0.PK.ToGuid(), orgHeader1.PK.ToGuid(), orgHeader2.PK.ToGuid() };
			opmoPks = new[]
			{
				orgPatternMatchOverride0.PK.ToGuid(),
				orgPatternMatchOverride1.PK.ToGuid(),
				orgPatternMatchOverride2.PK.ToGuid(),
				orgPatternMatchOverride3.PK.ToGuid(),
				orgPatternMatchOverride4.PK.ToGuid()
			};
		}

		IEnumerable<Guid> GetFilteredOrgPatternMatchOverridePkCollection(ZQuery filter)
		{
			return new OrgPatternMatchOverrideCollectionForOrgs(Factory, filter).Select(x => x.PK.ToGuid());
		}
	}
}
