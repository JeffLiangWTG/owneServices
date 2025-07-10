using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	abstract class OrgManagementRelatedCollectionTestCase<T> : ActiveBusinessObjectCollectionTestCase<T>
			where T : OrgManagementRelatedCollection
	{
		protected void AssertValidationResult(RelationValidationResult actualValidationResult, bool expectedIsValid, string expectedReason = "")
		{
			CombineAssertions(() =>
			{
				AssertEquals("IsValid", expectedIsValid, actualValidationResult.IsValid);
				AssertMultilineASCIIEquals("Reason", expectedReason, actualValidationResult.Reason);
			});
		}

		public abstract void TestOrganisations();
		public abstract void TestCheckIsValidOrganisation();
		public abstract void TestAddOrganisation();
		public abstract void TestRemoveOrganisation();
	}
}
