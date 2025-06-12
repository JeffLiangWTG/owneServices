using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ForwardingPortMessaging.BE.Outbound.Helpers;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Tests.Outbound
{

    [TestClass]
    public class ConnectionDetailHelperTest
    {
        [ClassInitialize]
        public static void SetupDatabaseMocks(TestContext context)
        {

            var transformationSet1 = new eHubTransformationSet
            {
                TS_PK = Guid.NewGuid(),
                TS_Name = "NXPORT System Configuration"
            };
            var transformationSets = new TestDbSet<eHubTransformationSet>{ transformationSet1 };

            var eHubCodeSet1 = new eHubCodeSet
            {
                CS_PK = Guid.NewGuid(),
                CS_Name = "Connection Details",
                eHubTransformationSet = transformationSet1
            };
            var eHubCodeSets = new TestDbSet<eHubCodeSet>{ eHubCodeSet1 };

            var eHubCodeSetResult1 = new eHubCodeSetResult {
               CR_PK = Guid.NewGuid(),
               CR_Name = "ClientId",
               eHubCodeSet = eHubCodeSet1
            };
            var eHubCodeSetResult2 = new eHubCodeSetResult
            {
                CR_PK = Guid.NewGuid(),
                CR_Name = "Secret",
                eHubCodeSet = eHubCodeSet1
            };
            var eHubCodeSetResults = new TestDbSet<eHubCodeSetResult> { eHubCodeSetResult1, eHubCodeSetResult2 };


            var eHubCodeMapKey1 = new eHubCodeMapKey
            {
                CK_PK = Guid.NewGuid(),
                eHubCodeSet = eHubCodeSet1,
            };
            var eHubCodeMapKeys = new TestDbSet<eHubCodeMapKey> { eHubCodeMapKey1 };


            var eHubCodeMapValue1 = new eHubCodeMapValue
            {
                CV_CK = Guid.NewGuid(),
                CV_OutputCode = "ClientId_OutputCode",
                eHubCodeMapKey = eHubCodeMapKey1,
                eHubCodeSetResult = eHubCodeSetResult1
            };
            var eHubCodeMapValue2 = new eHubCodeMapValue
            {
                CV_CK = Guid.NewGuid(),
                CV_OutputCode = "Secret_OutputCode",
                eHubCodeSetResult = eHubCodeSetResult2
            };
            var eHubCodeMapValues = new TestDbSet<eHubCodeMapValue> { eHubCodeMapValue1, eHubCodeMapValue2 };

            var stubContext = MockRepository.GenerateStub<eHubTransactionsContext>();
            stubContext.eHubTransformationSets = transformationSets;
            stubContext.eHubCodeSets = eHubCodeSets;
            stubContext.eHubCodeSetResults = eHubCodeSetResults;
            stubContext.eHubCodeMapKeys = eHubCodeMapKeys;
            stubContext.eHubCodeMapValues = eHubCodeMapValues;
            ConnectionDetailHelper.GetDbContext = () => stubContext;
        }

        [TestMethod]
        public void TestGetConnectionDetails()
        {
            var connectionDetailItem = ConnectionDetailHelper.GetConnectionDetails();
            Assert.AreEqual("ClientId_OutputCode", connectionDetailItem.ClientId);
            Assert.AreEqual("Secret_OutputCode", connectionDetailItem.Secret);
        }
    }
}
