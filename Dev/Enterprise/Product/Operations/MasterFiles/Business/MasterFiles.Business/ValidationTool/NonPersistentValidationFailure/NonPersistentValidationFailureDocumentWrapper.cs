using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.MasterFiles.Business
{
	public class NonPersistentValidationFailureDocumentWrapper : DocumentWrapper
	{
		public NonPersistentValidationFailureDocumentWrapper(NonPersistentRuleValidationResultCollection validationFailed)
			: base(validationFailed, validationFailed.Factory)
		{
		}
		public NonPersistentRuleValidationResultCollection WorkflowValidationFailedResult => (NonPersistentRuleValidationResultCollection)WrappedObject;
	}
}
