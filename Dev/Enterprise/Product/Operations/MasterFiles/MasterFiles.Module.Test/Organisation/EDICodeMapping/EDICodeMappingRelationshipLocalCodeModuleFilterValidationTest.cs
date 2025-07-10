using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class EDICodeMappingRelationshipLocalCodeModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRelationship()
		{
			var filter = new EDICodeMappingRelationshipLocalCodeModuleFilter("TestValidateRelationship", Factory.New<OrgPatternMatchOverride>());

			filter.Relationship = "ORG";
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.RelationshipInfo);

			filter.Relationship = "XXX";
			filter.Validation.ValidateAll();
			AssertHasErrors(filter.RelationshipInfo);
		}

		public void TestValidateLocalGuid()
		{
			var orgPk = Factory.NewWithValidTestData<OrgHeader>().PK.ToGuid();
			Factory.Save();

			var filter = new EDICodeMappingRelationshipLocalCodeModuleFilter("TestValidateRelationship", Factory.New<OrgPatternMatchOverride>())
			{
				Relationship = "ORG"
			};

			filter.OrgCoGuid = orgPk;
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.OrgCoGuidInfo);

			filter.OrgCoGuid = ZGuid.NewZGuid();
			filter.Validation.ValidateAll();
			AssertHasErrors(filter.OrgCoGuidInfo);
		}

		public void TestValidateLocalCode()
		{
			var filter = new EDICodeMappingRelationshipLocalCodeModuleFilter("TestValidateRelationship", Factory.New<OrgPatternMatchOverride>())
			{
				Relationship = "PKG"
			};

			filter.OrgCoName = "UNT";
			filter.Validation.ValidateAll();
			AssertNoWarnings(filter.OrgCoNameInfo);

			filter.OrgCoName = "XXX";
			filter.Validation.ValidateAll();
			AssertHasWarnings(filter.OrgCoNameInfo);
		}

		public void TestValidateLocalCodeWithFindBox()
		{
			var filter = new EDICodeMappingRelationshipLocalCodeModuleFilter("TestValidateRelationship", Factory.New<OrgPatternMatchOverride>())
			{
				Relationship = "CHC"
			};

			filter.OrgCoName = "ADMTN";
			filter.Validation.ValidateAll();
			AssertNoWarnings(filter.OrgCoNameInfo);

			filter.OrgCoName = "XXX";
			filter.Validation.ValidateAll();
			AssertHasWarnings(filter.OrgCoNameInfo);
		}
	}
}
