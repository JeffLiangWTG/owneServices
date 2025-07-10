using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(EWHMessage))]
	sealed class EWHMessageTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var testMessage = Factory.New<EWHMessage>();
			AssertEquals(ZAEDIMessageTypeList.Codes.ExternalWarehouse, testMessage.EM_MessageType);
			AssertEquals(EDIInterchange.ApplicationCodes.SouthAfricanTransactionOrders, testMessage.EM_ApplicationCode);
		}
	}
}
