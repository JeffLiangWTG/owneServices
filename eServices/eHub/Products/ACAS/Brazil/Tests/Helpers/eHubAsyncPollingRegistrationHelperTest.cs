using System;
using System.Linq;
using System.Xml;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ACAS.BR.Helpers;
using CargoWise.eHub.Products.ACAS.BR.Schemas.AsyncPolling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ACAS.BR.Tests.Helpers
{
	[TestClass]
	public class eHubAsyncPollingRegistrationHelperTest : TestBase
	{
		[TestMethod]
		public void TestInsertEHubAsyncPollingRegistration()
		{
			eHubAsyncPollingRegistrationHelper.InsertEHubAsyncPollingRegistration("WTLEDITST", "ACAS_BRTest", "NPN", "TaxID", "CS0001", "Console", "SubscribedValue", "http://UShipmentNamespace", "2019-01-01");
			var context = eHubAsyncPollingRegistrationHelper.InternalContextFactory();
			var eHubAsyncPollingRegistration = context.eHubAsyncPollingRegistrations.First();
			var prXml = new XmlDocument();
			prXml.LoadXml(eHubAsyncPollingRegistration.PR_XML);
			Assert.AreEqual("WTLEDITST", eHubAsyncPollingRegistration.eHubClient.CC_ID);
			Assert.AreEqual("WTLTST", eHubAsyncPollingRegistration.eHubClientSystem.EH_ID);
			Assert.AreEqual("ACAS_BRProtocol", eHubAsyncPollingRegistration.eHubRegistrationType.RT_ID);
			Assert.AreEqual("NPN", eHubAsyncPollingRegistration.PR_Text);
			Assert.AreEqual("44444444-4444-4444-4444-444444444444", eHubAsyncPollingRegistration.PR_PK.ToString());
			TestHelper.AssertXmlAreEqual(GetEmbeddedResource("Helpers.TestFiles.InsertEHubAsyncPollingRegistration_PRXML_Expected.xml"), prXml);
		}


		[TestMethod]
		public void TestInsertEHubAsyncPollingRegistration_WithSpecialCharacters()
		{
			eHubAsyncPollingRegistrationHelper.InsertEHubAsyncPollingRegistration("WTLEDITST", "ACAS_BRTest", "F&><\'\"5", "TaxID", "CS0001", "Console", "SubscribedValue", "http://UShipmentNamespace", "2019-01-01");
			var context = eHubAsyncPollingRegistrationHelper.InternalContextFactory();
			var eHubAsyncPollingRegistration = context.eHubAsyncPollingRegistrations.First();
			var prXml = new XmlDocument();
			prXml.LoadXml(eHubAsyncPollingRegistration.PR_XML);
			Assert.AreEqual("WTLEDITST", eHubAsyncPollingRegistration.eHubClient.CC_ID);
			Assert.AreEqual("WTLTST", eHubAsyncPollingRegistration.eHubClientSystem.EH_ID);
			Assert.AreEqual("ACAS_BRProtocol", eHubAsyncPollingRegistration.eHubRegistrationType.RT_ID);
			Assert.AreEqual("F&><\'\"5", eHubAsyncPollingRegistration.PR_Text);
			Assert.AreEqual("44444444-4444-4444-4444-444444444444", eHubAsyncPollingRegistration.PR_PK.ToString());
			TestHelper.AssertXmlAreEqual(GetEmbeddedResource("Helpers.TestFiles.InsertEHubAsyncPollingRegistration_PRXML_ExpectedSpecialChars.xml"), prXml);
		}

		[TestMethod]
		public void TestGetAsyncPollingConfig()
		{
			eHubAsyncPollingRegistrationHelper.InsertEHubAsyncPollingRegistration("WTLEDITST", "ACAS_BRTest", "NPN", "TaxID", "CS0001", "Console", "SubscribedValue", "http://UShipmentNamespace", "2019-01-01");
			var context = eHubAsyncPollingRegistrationHelper.InternalContextFactory();
			var eHubAsyncPollingRegistration = context.eHubAsyncPollingRegistrations.First();
			var prXml = new XmlDocument();

			prXml.LoadXml(eHubAsyncPollingRegistration.PR_XML);

			var staffID = eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "StaffID");
			Assert.AreEqual("NPN", staffID);
			Assert.AreEqual("WTLTST", eHubAsyncPollingRegistration.eHubClientSystem.EH_ID);
			Assert.AreEqual("ACAS_BRProtocol", eHubAsyncPollingRegistration.eHubRegistrationType.RT_ID);
			Assert.AreEqual("NPN", eHubAsyncPollingRegistration.PR_Text);
			Assert.AreEqual("44444444-4444-4444-4444-444444444444", eHubAsyncPollingRegistration.PR_PK.ToString());
		}

		[TestMethod]
		public void TestGetAsyncPollingConfig_WithSpecialCharacters()
		{
			eHubAsyncPollingRegistrationHelper.InsertEHubAsyncPollingRegistration("WTLEDITST", "ACAS_BRTest", "F&><\'\"5", "TaxID", "CS0001", "Console", "SubscribedValue", "http://UShipmentNamespace", "2019-01-01");
			var context = eHubAsyncPollingRegistrationHelper.InternalContextFactory();
			var eHubAsyncPollingRegistration = context.eHubAsyncPollingRegistrations.First();
			var prXml = new XmlDocument();
			prXml.LoadXml(eHubAsyncPollingRegistration.PR_XML);

			Assert.AreEqual("44444444-4444-4444-4444-444444444444", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "PK"));
			Assert.AreEqual("F&><\'\"5", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "StaffID"));
			Assert.AreEqual("TaxID", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "CNPJ"));
			Assert.AreEqual("CS0001", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "ContextKey"));
			Assert.AreEqual("Console", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "ContextType"));
			Assert.AreEqual("SubscribedValue", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "ReferenceValue"));
			Assert.AreEqual("WTLEDITST", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "SenderID"));
			Assert.AreEqual("ACAS_BRTest", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "RecipientID"));
			Assert.AreEqual("http://UShipmentNamespace", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "UShipmentNamespace"));
		}

		[TestMethod]
		public void TestSetAsyncPollingConfig()
		{
			eHubAsyncPollingRegistrationHelper.InsertEHubAsyncPollingRegistration("WTLEDITST", "ACAS_BRTest", "F&><\'\"5", "TaxID", "CS0001", "Console", "SubscribedValue", "http://UShipmentNamespace", "2019-01-01");
			var context = eHubAsyncPollingRegistrationHelper.InternalContextFactory();
			var eHubAsyncPollingRegistration = context.eHubAsyncPollingRegistrations.First();
			var prXml = new XmlDocument();
			prXml.LoadXml(eHubAsyncPollingRegistration.PR_XML);

			Assert.AreEqual("44444444-4444-4444-4444-444444444444", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "PK"));
			Assert.AreEqual("F&><\'\"5", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "StaffID"));
			Assert.AreEqual("TaxID", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "CNPJ"));
			Assert.AreEqual("CS0001", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "ContextKey"));
			Assert.AreEqual("Console", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "ContextType"));
			Assert.AreEqual("SubscribedValue", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "ReferenceValue"));
			Assert.AreEqual("WTLEDITST", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "SenderID"));
			Assert.AreEqual("ACAS_BRTest", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "RecipientID"));
			Assert.AreEqual("http://UShipmentNamespace", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "UShipmentNamespace"));

			eHubAsyncPollingRegistrationHelper.SetAsyncPollingConfig(prXml, "ProtocolNumber", "12345");
			eHubAsyncPollingRegistrationHelper.SetAsyncPollingConfig(prXml, "State", "1");
			eHubAsyncPollingRegistrationHelper.SetAsyncPollingConfig(prXml, "ExpiredDateUTC", "2023-08-11");

			Assert.AreEqual("1", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "State"));
			Assert.AreEqual("12345", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "ProtocolNumber"));
			Assert.AreEqual("2023-08-11", eHubAsyncPollingRegistrationHelper.GetAsyncPollingConfig(prXml, "ExpiredDateUTC"));
		}



		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestPollingRegistrationStatusSchema_WithStateDescription()
        {
            ValidateSchema<AsyncPolling>("Helpers.TestFiles.InsertEHubAsyncPollingRegistration_PRXML_Expected.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestPollingRegistrationStatusSchema_WithoutStateDescription()
        {
            ValidateSchema<AsyncPolling>("Helpers.TestFiles.InsertEHubAsyncPollingRegistration_PRXML_Expected.xml");
        }

		[TestInitialize]
		public void TestInitialize()
		{
			pkFirstCharacter = 0;

			var eHubClients = new TestDbSet<eHubClient>() { new eHubClient { CC_PK = getNewGuid(pkFirstCharacter++), CC_ID = "WTLEDITST" }, new eHubClient { CC_PK = getNewGuid(pkFirstCharacter++), CC_ID = "ACAS_BRTest" } };
			var eHubRegistrationTypes = new TestDbSet<eHubRegistrationType>() { new eHubRegistrationType { RT_PK = getNewGuid(pkFirstCharacter++), RT_ID = "ACAS_BRProtocol", RT_RegistrantType = "AsyncPolling" } };
			var eHubClientSystems = new TestDbSet<eHubClientSystem>() { new eHubClientSystem() { EH_PK = getNewGuid(pkFirstCharacter++), EH_ID = "WTLTST" } };
			var eHubAsyncPollingRegistrations = new TestDbSet<eHubAsyncPollingRegistration>();

			var stubContext = MockRepository.GenerateStub<eHubTransactionsContext>();
			stubContext.eHubClients = eHubClients;
			stubContext.eHubRegistrationTypes = eHubRegistrationTypes;
			stubContext.eHubClientSystems = eHubClientSystems;
			stubContext.eHubAsyncPollingRegistrations = eHubAsyncPollingRegistrations;
			eHubAsyncPollingRegistrationHelper.InternalContextFactory = () => stubContext;
			eHubAsyncPollingRegistrationHelper.InternalGuidFactory = () => getNewGuid(pkFirstCharacter++);
		}

		private int pkFirstCharacter;
		readonly Func<int, Guid> getNewGuid = (int i) => new Guid(Enumerable.Repeat((byte)((i % 16) * 0x11), 16).ToArray());
	}
}
