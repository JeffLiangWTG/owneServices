using System;
using System.Linq;
using CargoWise.eHub.Core.Orchestrations.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrchestrationHelper = CargoWise.eHub.Products.ACAS.BR.Helpers.OrchestrationHelper;

namespace CargoWise.eHub.Products.ACAS.BR.Tests.Helpers
{
	[TestClass]
	public class OrchestrationHelperTest : TestBase
	{
		[TestMethod]
		public void TestGetWcfInsertInboxAndUpdateSubscriptionType()
		{
			WcfSqlOperationMessages.GetCurrentUtcTime = () => new DateTime(2019, 11, 26);
			var expectedXml = OrchestrationHelper.GetWcfInsertInboxAndUpdateAsyncPollingRegistration(
				"99999999-9999-9999-9999-999999999999", "WTLEDINPN", "ACAS_BR_FHL", "INBOX CONTENT", "<AAA></AAA>");
			TestHelper.AssertXmlAreEqual(GetEmbeddedResource("Helpers.TestFiles.InsertInboxAndUpdateSubscriptionType_Expected.xml"), expectedXml);
		}

		[TestInitialize]
		public void TestInitialize()
		{
			guidNum[0] = 0;
			WcfSqlOperationMessages.InternalNewGuid = () => getNewGuid(guidNum[0]++);
		}

		readonly int[] guidNum = { 0 };
		readonly Func<int, Guid> getNewGuid = (int i) => new Guid(Enumerable.Repeat((byte)((i % 16) * 0x11), 16).ToArray());

	}
}