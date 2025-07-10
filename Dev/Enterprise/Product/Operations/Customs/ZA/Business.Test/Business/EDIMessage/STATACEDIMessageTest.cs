using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(STATACEDIMessage))]
	sealed class STATACEDIMessageTest : SARSEDIMessageAbstractTest
	{
		public void TestDefaultValues()
		{
			var testMessage = Factory.New<STATACEDIMessage>();
			AssertEquals("STA", testMessage.EM_MessageType);
		}
	}
}
