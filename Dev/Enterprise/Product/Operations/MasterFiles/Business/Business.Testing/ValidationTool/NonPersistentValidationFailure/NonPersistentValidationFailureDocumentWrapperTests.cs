using System.Linq;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DocumentWrapperForTesting))]
	sealed class NonPersistentValidationFailureDocumentWrapperTest : DocumentWrapperTest
	{
		public void TestWorkflowValidationFailedResult_ShouldReturnCorrectType()
		{
			var validationFailed = new NonPersistentRuleValidationResultCollection(Factory);
			var wrapper = new NonPersistentValidationFailureDocumentWrapper(validationFailed);

			var result = wrapper.WorkflowValidationFailedResult;

			AssertType<NonPersistentRuleValidationResultCollection>(result);
		}

		public void TestWorkflowValidationFailedResult_ShouldContainOriginalData()
		{
			var validationFailed = new NonPersistentRuleValidationResultCollection(Factory);
			validationFailed.Add(NonPersistentRuleValidationResultBuilder.Fail().WithMessage("Error message 1").Build());
			validationFailed.Add(NonPersistentRuleValidationResultBuilder.Fail().WithMessage("Error message 2").Build());

			var wrapper = new NonPersistentValidationFailureDocumentWrapper(validationFailed);
			var result = wrapper.WorkflowValidationFailedResult;

			AssertEquals(result.Count, 2);
			AssertCollectionContains(result.Where(x => x.Message == "Error message 1").FirstOrDefault(), result);
			AssertCollectionContains(result.Where(x => x.Message == "Error message 2").FirstOrDefault(), result);
		}
	}
}
