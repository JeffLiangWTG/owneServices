using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyagePortCall))]
	sealed class CarrierVoyagePortCallWorkflowProviderTest : WorkflowProviderTest<CarrierVoyagePortCall, CarrierVoyagePortCallProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CarrierVoyagePortCallWorkflowDescriptorCode;

		protected override CarrierVoyagePortCall GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var portCall = Factory.NewWithValidTestData<CarrierVoyagePortCall>();
			return portCall;
		}
	}
}
