using CargoWise.Application;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CYDAdHocServiceOrderConsumerTypeTest : JobInvoicingConsumerTypesTest
	{
		public void TestBizoType()
		{
			var consumerType = JobInvoicingConsumerTypes.CYDAdHocServiceOrder;
			AssertEquals(ObjectFactory.GetType<ICYDAdHocServiceOrder>(), consumerType.BizoType);
		}
	}
}
