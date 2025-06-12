using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Products.ForwardingPortMessaging.BE.Outbound.Helpers;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Tests.Outbound
{
    [TestClass]
    public class ClientRegistrationHelperTest
    {



        [ClassInitialize]
        public static void SetupDatabaseMocks(TestContext context)
        {

            var nxportAPIPK = Guid.NewGuid();
            var nxportPK = Guid.NewGuid();

            var eHubRegistrationType1 = new eHubRegistrationType
            {
                RT_PK = nxportAPIPK,
                RT_ID = "NXPORTAPI",
                RT_Description = "NXPORT API ID",
                RT_RegistrantType = "Client"
            };

            var eHubRegistrationType2 = new eHubRegistrationType
            {
                RT_PK = nxportPK,
                RT_ID = "NXPORT",
                RT_Description = "NXPORT Client ID",
                RT_RegistrantType = "Client"
            };
            var eHubRegistrationTypes = new TestDbSet<eHubRegistrationType>
            {
                eHubRegistrationType1, eHubRegistrationType2            
            };

            var clientPK1 = Guid.NewGuid();
            var clientPK2 = Guid.NewGuid();
            var client1 = new eHubClient
            {
                CC_PK = clientPK1,
                CC_ID = "HYEBNEUAT",
            };
            var client2 = new eHubClient
            {
                CC_PK = clientPK2,
                CC_ID = "HYETSTTST",
            };
            var eHubClients = new TestDbSet<eHubClient>
            {
                client1,
                client2
            };

            var eHubClientRegistrations = new TestDbSet<eHubClientRegistration>
            {
                new eHubClientRegistration
                {
                    CX_PK = Guid.NewGuid(),
                    CX_CC = clientPK1,
                    CX_Attr1 = "ClientRegistration1",
                    CX_Code = "TestCase1",
                    CX_RT = nxportAPIPK,
                    CX_Qualifier = "HAM",
                    eHubClient = client1,
                    eHubRegistrationType = eHubRegistrationType1
                }
            };

            var stubContext = MockRepository.GenerateStub<eHubTransactionsContext>();
            stubContext.eHubClients = eHubClients;
            stubContext.eHubRegistrationTypes = eHubRegistrationTypes;
            stubContext.eHubClientRegistrations = eHubClientRegistrations;
            ClientRegistrationHelper.GetDbContext = () => stubContext;
        }

        [TestMethod]
        public void TestGetEHubClientRegistrationItem()
        {
			var logger = new OrchestrationLogger();
            var clientRegistration = ClientRegistrationHelper.GetEHubClientRegistrationItem("HYEBNEUAT", "HAM", "NXPORTAPI");
            Assert.AreEqual("TestCase1", ClientRegistrationHelper.GetEHubClientRegistrationItem("HYEBNEUAT", "HAM", "NXPORTAPI").Code);
            Assert.AreEqual("TestCase1", ClientRegistrationHelper.GetEHubClientRegistrationItemWithRetries("HYEBNEUAT", "HAM", "NXPORTAPI", logger).Code);
        }


    }
}
