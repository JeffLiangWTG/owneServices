using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	abstract class RelatedActivityPivotCollectionTestCase<T> : ActiveBusinessObjectCollectionTestCase<T>
			where T : RelatedActivityPivotCollection
	{
		protected void AssertValidationResult(RelationValidationResult actualValidationResult, bool expectedIsValid, string expectedReason = "")
		{
			CombineAssertions(() =>
				{
					AssertEquals("IsValid", expectedIsValid, actualValidationResult.IsValid);
					AssertMultilineASCIIEquals("Reason", expectedReason, actualValidationResult.Reason);
				});
		}

		protected void AssertUpdateResult(RelationUpdateResult actualUpdateResult, bool expectedSuccess, string expectedReason = "")
		{
			AssertUpdateResult("", actualUpdateResult, expectedSuccess, expectedReason);
		}

		protected void AssertUpdateResult(string message, RelationUpdateResult actualUpdateResult, bool expectedSuccess, string expectedReason = "")
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Success", expectedSuccess, actualUpdateResult.Success);
				AssertMultilineASCIIEquals("Reason", expectedReason, actualUpdateResult.Reason);
			});
		}
	}
}
