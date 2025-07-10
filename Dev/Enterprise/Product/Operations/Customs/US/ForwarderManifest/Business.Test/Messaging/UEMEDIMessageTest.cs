using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(UEMEDIMessage))]
	public class UEMEDIMessageTest : EDIMessageTest
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(ApplicationCodeList.Codes.USExportManifest, EDIMessage.EM_ApplicationCode);
		}

		public void TestResetToQueuedStatus()
		{
			EDIMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			EDIMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			EDIMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.Brokerage;
			AssertEquals(EDIMessageTypeList.Codes.XDC, EDIMessage.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.Brokerage, EDIMessage.EM_MessageSubType);

			EDIMessage.ResetToQueuedStatus();
			AssertEquals(EDIMessageTypeList.Codes.XDC, EDIMessage.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.Brokerage, EDIMessage.EM_MessageSubType);
		}

		public void TestGetMessageReferenceNumber()
		{
			Factory.Save();
			AssertEquals(string.Empty, EDIMessage.EM_MessageNum);

			Factory.Save();
			var company = GlbCompany.CurrentCompany;
			AssertEquals(company.LicenceKeyIdentifier + "_1", EDIMessage.EM_MessageNum);
		}
	}
}
