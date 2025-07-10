using CargoWise.Application;
using CargoWise.Glow.Model.Interfaces;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CYDReceiveAdviceConsumerTypeTest : JobInvoicingConsumerTypesTest
	{
		public void TestBizoType()
		{
			var receiveAdviceConsumerType = JobInvoicingConsumerTypes.CYDReceiveAdvice;
			AssertEquals(ObjectFactory.GetType<ICYDReceiveAdvice>(), receiveAdviceConsumerType.BizoType);
		}
	}
}
