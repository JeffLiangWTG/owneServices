using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(NonPersistentValidationToolParent))]
sealed class NonPersistentValidationToolParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestIValidationToolParent()
	{
		var nonPersistentValidationToolParent = new NonPersistentValidationToolParent(WorkflowProvider);
		var validationToolParent = (IValidationToolParent)nonPersistentValidationToolParent;
		AssertSame("ProcessTemplateValidations", nonPersistentValidationToolParent.ProcessTemplateValidations, validationToolParent.ProcessTemplateValidations);
	}

	public void TestProcessTemplateValidations()
	{
		var nonPersistentValidationToolParent = new NonPersistentValidationToolParent(WorkflowProvider);
		AssertType<ProcessTemplateValidationCollection>(nonPersistentValidationToolParent.ProcessTemplateValidations);
	}

	protected override BusinessObject GetNewBusinessObject() => new NonPersistentValidationToolParent(WorkflowProvider);

	IWorkflowProvider WorkflowProvider
	{
		get
		{
			if (workflowProvider is null)
			{
				var mockBusiness = new Mock<IBusiness>();
				mockBusiness.SetupGet(x => x.Factory).Returns(Factory);
				var mockWorkflowProvider = mockBusiness.As<IWorkflowProvider>();
				workflowProvider = mockWorkflowProvider.Object;
			}

			return workflowProvider;
		}
	}
	IWorkflowProvider workflowProvider;
}
