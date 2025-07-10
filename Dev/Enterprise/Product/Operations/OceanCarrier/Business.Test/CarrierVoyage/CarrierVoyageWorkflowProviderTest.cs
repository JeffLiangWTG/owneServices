using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyage))]
	sealed class CarrierVoyageWorkflowProviderTest : WorkflowProviderTest<CarrierVoyage, CarrierVoyageProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CarrierVoyageWorkflowDescriptorCode;

		protected override CarrierVoyage GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var voyage = Factory.NewWithValidTestData<CarrierVoyage>();
			return voyage;
		}
	}
}
