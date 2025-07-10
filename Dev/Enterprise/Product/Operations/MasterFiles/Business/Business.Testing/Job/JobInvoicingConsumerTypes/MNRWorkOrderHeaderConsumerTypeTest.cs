using CargoWise.Application;
using CargoWise.Glow.Model.Interfaces;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MNRWorkOrderHeaderConsumerTypeTest : JobInvoicingConsumerTypesTest
	{
		public void TestBizoType()
		{
			var consumerType = JobInvoicingConsumerTypes.MNRWorkOrderHeader;
			AssertEquals(ObjectFactory.GetType<IMNRWorkOrderHeader>(), consumerType.BizoType);
		}
	}
}
