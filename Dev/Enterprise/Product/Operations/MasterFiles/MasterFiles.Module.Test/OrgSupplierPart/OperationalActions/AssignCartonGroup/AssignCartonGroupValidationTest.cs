using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AssignCartonGroupValidationTest : BusinessObjectValidationTestCase
	{
		#region TestValidateCartonGroupPK

		public void TestValidateCartonGroupPK()
		{
			var group = Factory.New<IWhsCartonGroup>();
			group.WCG_Code = "G1";
			group.WCG_Description = "Group";

			var applicator = GetNewApplicator();
			applicator.CartonGroupPK = group.PK;
			AssertNoError(applicator.CartonGroupPKInfo, "Enter a valid Carton Group.");

			applicator.CartonGroupPK = ZGuid.NewZGuid();
			AssertHasError(applicator.CartonGroupPKInfo, "Enter a valid Carton Group.");
		}

		#endregion

		#region TestValidateOrganisationPK

		public void TestValidateOrganisationPK()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var applicator = GetNewApplicator();
			applicator.OrganisationPK = ZGuid.NewZGuid();
			AssertHasError(applicator.OrganisationPKInfo, "Enter a valid Organization.");

			applicator.OrganisationPK = org.PK;
			AssertNoError(applicator.OrganisationPKInfo, "Enter a valid Organization.");
		}

		#endregion

		#region TestAutoValidationType

		public void TestAutoValidationType()
		{
			AssertEquals(typeof(AssignCartonGroupValidation), GetNewApplicator().Validation.AutoValidationType);
		}

		#endregion

		#region Implementation

		AssignCartonGroupMethodApplicator GetNewApplicator()
		{
			return new AssignCartonGroupMethodApplicator("test", Factory);
		}

		#endregion
	}
}
