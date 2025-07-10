using CargoWise.Application;
using CargoWise.Glow.Model.Interfaces;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CYDReleaseAdviceConsumerTypeTest : JobInvoicingConsumerTypesTest
	{
		public void TestBizoType()
		{
			var releaseAdviceConsumerType = JobInvoicingConsumerTypes.CYDReleaseAdvice;
			AssertEquals(ObjectFactory.GetType<ICYDReleaseAdvice>(), releaseAdviceConsumerType.BizoType);
		}
	}
}
