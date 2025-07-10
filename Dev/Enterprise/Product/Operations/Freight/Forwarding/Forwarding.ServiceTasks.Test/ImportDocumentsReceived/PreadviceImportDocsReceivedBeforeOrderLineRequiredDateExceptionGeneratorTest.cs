using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Test
{
	internal class PreadviceImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorTest : ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorTest
	{
		protected override IWorkflowProvider WorkflowProviderWithMilestoneToComplete
		{
			get
			{
				return Preadvise;
			}
		}

		JobShipmentPreplanning Preadvise
		{
			get
			{
				if (preadvise == null)
				{
					preadvise = Factory.NewWithValidTestData<JobShipmentPreplanning>();
					preadvise.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
					Order.JD_EF_ShipmentPrePlanning = preadvise.PK;
				}

				return preadvise;
			}
		}

		JobShipmentPreplanning preadvise;
	}
}
