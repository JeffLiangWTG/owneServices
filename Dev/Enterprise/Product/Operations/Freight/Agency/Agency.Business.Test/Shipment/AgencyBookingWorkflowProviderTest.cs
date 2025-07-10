using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyBooking))]
	internal class AgencyBookingWorkflowProviderTest : AgencyWorkflowProviderTest<AgencyBooking, AgencyBookingProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get
			{
				return WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode;
			}
		}
	}
}
