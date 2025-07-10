using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AMSBrokerDownloadMQEDIMessage))]
	sealed class AMSBrokerDownloadMQEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIssuerAndBillNumber()
		{
			var amsMessage = Factory.New<AMSBrokerDownloadMQEDIMessage>();
			AssertEquals("MessageType is set", ApplicationIdentifierCodeList.Codes.BrokerManifestDownload, amsMessage.EM_MessageType);
			AssertEquals("", amsMessage.IssuerAndBillNumber);
			var issuerAndBillNumber = Factory.New<IssuerAndBillNumber>();
			issuerAndBillNumber.CY_Data = "APLU00002837489";
			issuerAndBillNumber.CY_ParentID = amsMessage.PK;
			issuerAndBillNumber.CY_ParentTableCode = EDIMessageSchema.Constants.Prefix;
			AssertEquals("APLU00002837489", amsMessage.IssuerAndBillNumber);
		}
	}
}
