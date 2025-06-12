using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GlobalInvoice.Italy.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;
using Rhino.Mocks;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace CargoWise.eHub.Products.GlobalInvoice.Italy.Tests.Helpers
{
    [TestClass]
    public class MessageHelperTests
    {
        [TestMethod]
        public void TestGetSoapFromResponse()
        {
            var mockMessage = MockRepository.GenerateMock<XLANGMessage>();
            var mockPart = MockRepository.GenerateMock<XLANGPart>();
            string messageText = @"--MIMEBoundary_3b0ea5a891370c30a8a0ed65b11d093c476c62357085a9aa
Content-Type: application/xop+xml; charset=utf-8; type=""text/xml""
Content-Transfer-Encoding: binary
Content-ID: <0.2b0ea5a891370c30a8a0ed65b11d093c476c62357085a9aa@apache.org>


--MIMEBoundary_3b0ea5a891370c30a8a0ed65b11d093c476c62357085a9aa--
<ns2:rispostaSdIRiceviFile xmlns:ns2=""http://www.fatturapa.gov.it/sdi/ws/trasmissione/v1.0/types""><IdentificativoSdI>4247116</IdentificativoSdI><DataOraRicezione>2018-11-21T04:30:13.107+01:00</DataOraRicezione></ns2:rispostaSdIRiceviFile>";
            var bodyStream = new MemoryStream(Encoding.Default.GetBytes(messageText));


            mockMessage.Stub(x => x[0]).Return(mockPart);
            mockPart.Stub(x => x.RetrieveAs(typeof(Stream))).Return(bodyStream);

            var result = MessageHelper.GetSoapFromResponse(mockMessage);

            var expected = @"<ns2:rispostaSdIRiceviFile xmlns:ns2=""http://www.fatturapa.gov.it/sdi/ws/trasmissione/v1.0/types""><IdentificativoSdI>4247116</IdentificativoSdI><DataOraRicezione>2018-11-21T04:30:13.107+01:00</DataOraRicezione></ns2:rispostaSdIRiceviFile>";
            Assert.AreEqual(expected, result.OuterXml);
        }

        [TestMethod]
        public void TestIsDuplicate_Duplicate()
        {
            var ST_PK = new Guid("D4A90731-9866-433A-B78C-E8B94FBBA85F");
            var CC_PK_Sender = new Guid("ebc2f463-128f-498f-9ec3-56536687c6cf");
            var CC_PK_Recipient = new Guid("2306d85b-f01f-4d2a-b920-f829f89f4e52");
            var messageTrackingID = "e5a282c5-d773-4e6c-b80a-9587c205d8c0";

            var eHubSubscriptionTypes = new TestDbSet<eHubSubscriptionType> { new eHubSubscriptionType { ST_PK = ST_PK, ST_ID = "GEI_IT" } };
            var eHubClient = new TestDbSet<eHubClient> {
                new eHubClient { CC_PK = CC_PK_Sender, CC_ID = "FOOBARBAZ" },
                new eHubClient { CC_PK = CC_PK_Recipient, CC_ID = "GEI_ITALY" }
            };
            var eHubSubscriptionValues = new TestDbSet<eHubSubscriptionValue> {
                new eHubSubscriptionValue { SV_PK = new Guid("585b6ac4-1269-4c0d-8cd8-7bfcc40aaa1a"), SV_ST = ST_PK, SV_CC_Sender = CC_PK_Sender, SV_CC_Recipient = CC_PK_Recipient, SV_Value = messageTrackingID, SV_Reference = messageTrackingID }
            };

            var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
            mockeHubTransactionsContext.Stub(_ => _.eHubSubscriptionTypes).Return(eHubSubscriptionTypes).Repeat.Any();
            mockeHubTransactionsContext.Stub(_ => _.eHubClients).Return(eHubClient).Repeat.Any();
            mockeHubTransactionsContext.Stub(_ => _.eHubSubscriptionValues).Return(eHubSubscriptionValues).Repeat.Any();
            MessageHelper.NewEHubTransactionContext = () => mockeHubTransactionsContext;

            var logger = new OrchestrationLogger();

            Assert.AreEqual(true, MessageHelper.IsDuplicate(messageTrackingID, "ea5f3361-4214-445d-b9c8-332cbc7edb71", "FOOBARBAZ", "GEI_ITALY", logger));
            mockeHubTransactionsContext.VerifyAllExpectations();
        }

        [TestMethod]
        public void TestIsDuplicate_Unique()
        {
            var ST_PK = new Guid("D4A90731-9866-433A-B78C-E8B94FBBA85F");
            var CC_PK_Sender = new Guid("ebc2f463-128f-498f-9ec3-56536687c6cf");
            var CC_PK_Recipient = new Guid("2306d85b-f01f-4d2a-b920-f829f89f4e52");
            var messageTrackingID = "2317a1e6-cc17-49db-afec-0f80a23652c8";

            var eHubSubscriptionTypes = new TestDbSet<eHubSubscriptionType> { new eHubSubscriptionType { ST_PK = ST_PK, ST_ID = "GEI_IT" } };
            var eHubClient = new TestDbSet<eHubClient> {
                new eHubClient { CC_PK = CC_PK_Sender, CC_ID = "FOOBARBAZ" },
                new eHubClient { CC_PK = CC_PK_Recipient, CC_ID = "GEI_ITALY" }
            };
            var eHubSubscriptionValues = new TestDbSet<eHubSubscriptionValue> {
                new eHubSubscriptionValue { SV_PK = new Guid("585b6ac4-1269-4c0d-8cd8-7bfcc40aaa1a"), SV_ST = ST_PK, SV_CC_Sender = CC_PK_Sender, SV_CC_Recipient = CC_PK_Recipient, SV_Value = messageTrackingID, SV_Reference = messageTrackingID }
            };

            var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
            mockeHubTransactionsContext.Stub(_ => _.eHubSubscriptionTypes).Return(eHubSubscriptionTypes).Repeat.Any();
            mockeHubTransactionsContext.Stub(_ => _.eHubClients).Return(eHubClient).Repeat.Any();
            mockeHubTransactionsContext.Stub(_ => _.eHubSubscriptionValues).Return(eHubSubscriptionValues).Repeat.Any();
            mockeHubTransactionsContext.Expect(_ => _.SaveChanges()).Return(0);
            MessageHelper.NewEHubTransactionContext = () => mockeHubTransactionsContext;

            var logger = new OrchestrationLogger();

            Assert.AreEqual(false, MessageHelper.IsDuplicate("5e82d878-374d-407e-a325-1bf8a11b1b5a", "ea5f3361-4214-445d-b9c8-332cbc7edb71", "FOOBARBAZ", "GEI_ITALY", logger));
            mockeHubTransactionsContext.VerifyAllExpectations();
        }

        [TestMethod]
        public void TestIsDuplicate_IgnoreException()
        {
            MessageHelper.NewEHubTransactionContext = () => { throw new Exception("Test Exception"); };
            var logger = new OrchestrationLogger();
            Assert.AreEqual(false, MessageHelper.IsDuplicate("5e82d878-374d-407e-a325-1bf8a11b1b5a", "ea5f3361-4214-445d-b9c8-332cbc7edb71", "FOOBARBAZ", "GEI_ITALY", logger));
        }
    }
}
