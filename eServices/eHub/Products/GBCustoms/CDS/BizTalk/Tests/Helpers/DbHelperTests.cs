using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.CDS.BT.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.CDS.BT.Tests.Helpers
{
    [TestClass]
    public class DbHelperTests
    {
        [TestMethod]
        public void DbHelper_GetClientPk()
        {
            var eHubClients = new TestDbSet<eHubClient>
            {
                new eHubClient {CC_PK = new Guid("00000000-0000-0000-0000-000000000000"), CC_ID = "AAAAAAAAA"},
            };
            var mockeHubTransactions = MockRepository.GenerateMock<eHubTransactionsContext>();
            mockeHubTransactions.Stub(x => x.eHubClients).Return(eHubClients);
            DbHelpers.ContextFactory = () => mockeHubTransactions;

            var result = DbHelpers.GetClientPk("AAAAAAAAA");

            Assert.AreEqual("00000000-0000-0000-0000-000000000000", result);
        }

        [TestMethod]
        public void DbHelper_GetSubscriptionTypePk()
        {
            var eHubSubscriptionTypes = new TestDbSet<eHubSubscriptionType>
            {
                new eHubSubscriptionType {ST_PK = new Guid("00000000-0000-0000-0000-000000000000"), ST_ID = "AAAAAAA"},
            };
            var mockeHubTransactions = MockRepository.GenerateMock<eHubTransactionsContext>();
            mockeHubTransactions.Stub(x => x.eHubSubscriptionTypes).Return(eHubSubscriptionTypes);
            DbHelpers.ContextFactory = () => mockeHubTransactions;

            var result = DbHelpers.GetSubscriptionTypePk("AAAAAAA");

            Assert.AreEqual("00000000-0000-0000-0000-000000000000", result);
        }

		[TestMethod]
		public void DbHelper_GetMessageTypePk()
		{
			var eHubMessageType = new TestDbSet<eHubMessageType>
			{
				new eHubMessageType {DT_PK = new Guid("00000000-0000-0000-0000-000000000000"), DT_Code = "AAAAAAAAA"},
			};
			var mockeHubTransactions = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockeHubTransactions.Stub(x => x.eHubMessageTypes).Return(eHubMessageType);
			DbHelpers.ContextFactory = () => mockeHubTransactions;

			var result = DbHelpers.GetMessageTypePK("AAAAAAAAA");

			Assert.AreEqual("00000000-0000-0000-0000-000000000000", result);
		}
	}
}
