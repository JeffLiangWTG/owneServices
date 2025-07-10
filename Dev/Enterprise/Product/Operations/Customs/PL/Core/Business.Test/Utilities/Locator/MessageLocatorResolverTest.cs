using CargoWise.Customs.PL.MessageContracts.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class MessageLocatorResolverTest : TestCase
{
	const string LocatorCommonTypeName = "Enterprise.Customs.PL.Business.LocatorCommon";
	const string LocatorPLCTypeName = "Enterprise.Customs.PL.Business.LocatorPLC";
	const string LocatorNCTSTypeName = "Enterprise.Customs.PL.NCTS.Business.LocatorNCTS";
	const string LocatorPLXTypeName = "Enterprise.Customs.PL.ExitControl.Business.LocatorPLX";

	public void TestGetMessageLocator_None() => AssertMessageLocator(MessageTypes.None, null);

	public void TestGetMessageLocator_Common() => AssertMessageLocator(MessageTypes.Common, LocatorCommonTypeName);

	public void TestGetMessageLocator_DocumentHandlingPort() => AssertMessageLocator(MessageTypes.DocumentHandlingPort, null);

	public void TestGetMessageLocator_AIS() => AssertMessageLocator(MessageTypes.AIS, LocatorPLCTypeName);

	public void TestGetMessageLocator_AES() => AssertMessageLocator(MessageTypes.AES, LocatorPLCTypeName);

	public void TestGetMessageLocator_AESAIS() => AssertMessageLocator(MessageTypes.AESAIS, LocatorPLCTypeName);

	public void TestGetMessageLocator_Arrival() => AssertMessageLocator(MessageTypes.Arrival, LocatorNCTSTypeName);

	public void TestGetMessageLocator_Departure() => AssertMessageLocator(MessageTypes.Departure, LocatorNCTSTypeName);

	public void TestGetMessageLocator_ArrivalAndDeparture() => AssertMessageLocator(MessageTypes.ArrivalAndDeparture, LocatorNCTSTypeName);

	public void TestGetMessageLocator_ExportControl() => AssertMessageLocator(MessageTypes.ExportControl, LocatorPLXTypeName);

	public void TestGetMessageLocator_Unknown() => AssertMessageLocator((MessageTypes)128, null);

	void AssertMessageLocator(MessageTypes messageType, string expectedTypeName)
	{
		var messageLocatorResolver = new MessageLocatorResolver();
		var messageLocator1 = messageLocatorResolver.GetMessageLocator(messageType);
		var messageLocator2 = messageLocatorResolver.GetMessageLocator(messageType);
		var actualTypeName = messageLocator1?.GetType().FullName;
		CombineAssertions(() =>
		{
			AssertEquals("Type name should be expected.", expectedTypeName, actualTypeName);
			AssertSame("Message locator should be a singleton.", messageLocator1, messageLocator2);
		});
	}
}
