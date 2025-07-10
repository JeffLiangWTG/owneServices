using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Test
{
	internal class DeclarationImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorTest : ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorTest
	{
		protected override IWorkflowProvider WorkflowProviderWithMilestoneToComplete
		{
			get
			{
				return (IWorkflowProvider)Declaration;
			}
		}

		BusinessObject Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					declaration.FillWithValidTestData();
					Order.JD_JE = declaration.PK;
				}

				return declaration;
			}
		}

		BusinessObject declaration;
	}
}
