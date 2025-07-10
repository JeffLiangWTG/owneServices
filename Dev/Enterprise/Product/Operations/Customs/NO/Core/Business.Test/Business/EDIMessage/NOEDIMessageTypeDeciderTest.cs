using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NOEDIMessageTypeDecider))]
sealed class NOEDIMessageTypeDeciderTest : TestCaseWithFactory
{
	public void TestGetTypeForLoad()
	{
		var typeDecider = new NOEDIMessageTypeDecider();
		var message = Factory.New<NOEDIMessage>();
		var row = ((INeedRow)message).Row;

		CombineAssertions(() =>
		{
			AssertTypeForLoadIs<CUSDECEDIMessage>(EDIMessageConstants.MessageTypes.CUSDEC);
			AssertTypeForLoadIs<CUSRESEDIMessage>(EDIMessageConstants.MessageTypes.CUSRES);
			AssertTypeForLoadIs<EmmaEDIMessage>(EDIMessageConstants.MessageTypes.EMMA);
			AssertTypeForLoadIs<CUSRESEDIMessage>("rEs");
			AssertTypeForLoadIs<NOEDIMessage>(string.Empty);
		});

		void AssertTypeForLoadIs<TExpected>(string messageType)
		{
			message.EM_MessageType = messageType;
			AssertEquals($"When MessageType is '{messageType}'", typeof(TExpected), typeDecider.GetTypeForLoad(row, Factory));
		}
	}
}
