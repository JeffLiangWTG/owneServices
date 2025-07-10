using CargoWise.Application;
using CargoWise.Glow.Model.Interfaces;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CYDTransportationUnitConsumerTypeTest : JobInvoicingConsumerTypesTest
	{
		public void TestBizoType()
		{
			var cYDTransportationUnitConsumerType = JobInvoicingConsumerTypes.CYDTransportationUnit;
			AssertEquals(ObjectFactory.GetType<ICYDTransportationUnit>(), cYDTransportationUnitConsumerType.BizoType);
		}
	}
}
