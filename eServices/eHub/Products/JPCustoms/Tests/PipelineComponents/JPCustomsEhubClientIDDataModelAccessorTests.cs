using System.Linq;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class JPCustomsEhubClientIDDataModelAccessorTests
	{
		const string SystemID = "HYECM2";

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetEHubClientIDFromCredentials_MatchCompanyLevel_Success()
        {
            var mockContextFactory = MockContext();
            var context = mockContextFactory.CreateContext();
            var rt = context.eHubRegistrationTypes.First(x => x.RT_ID == "JPCustomsAccount_ClientLevel");
            var eh = context.eHubClientSystems.First(x => x.EH_ID == SystemID);
            var client = new eHubClient { CC_ID = "TSTUsername" };
            var clientRegistration = new eHubClientRegistration
            {
                eHubRegistrationType = rt,
                CX_RT = rt.RT_PK,
                CX_Code = "TSTUsername",
                CX_Password1 = "TSTPassword",
                eHubClient = client
            };
            context.eHubClientRegistrations.Add(clientRegistration);

            var dataModelAccessor = new JPCustomsEhubClientIDDataModelAccessor(mockContextFactory);
            Assert.AreEqual("TSTUsername", dataModelAccessor.GetEHubClientIDFromCredentials("TSTUsername"));
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetEHubClientIDInClientSystemRegistration_SameUsername_GetCompanyFirst()
		{
			var mockContextFactory = MockContext();
			var context = mockContextFactory.CreateContext();
            var rt = context.eHubRegistrationTypes.First(x => x.RT_ID == "JPCustomsAccount_ClientLevel");
			var eh = context.eHubClientSystems.First(x => x.EH_ID == SystemID);
			var systemRegistration = new eHubClientSystemRegistration
			{
				eHubRegistrationType = rt,
				CD_RT = rt.RT_PK,
				CD_Code = "TSTUsername",
				CD_Attr1 = "TSTPassword",
				eHubClientSystem = eh,
				CD_EH = eh.EH_PK
			};
			context.eHubClientSystemRegistrations.Add(systemRegistration);
            var client = new eHubClient { CC_ID = "TSTUsername" };
            var clientRegistration = new eHubClientRegistration
            {
                eHubRegistrationType = rt,
                CX_RT = rt.RT_PK,
                CX_Code = "TSTUsername",
                CX_Password1 = "TSTPassword1",
                eHubClient = client
            };
            context.eHubClientRegistrations.Add(clientRegistration);


			var dataModelAccessor = new JPCustomsEhubClientIDDataModelAccessor(mockContextFactory);
            Assert.AreEqual("TSTUsername", dataModelAccessor.GetEHubClientIDFromCredentials("TSTUsername"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetEHubClientIDFromCredentials_ReturnNull()
		{
			var mockContextFactory = MockContext();
			var context = mockContextFactory.CreateContext();

			var dataModelAccessor = new JPCustomsEhubClientIDDataModelAccessor(mockContextFactory);
			Assert.AreEqual(null, dataModelAccessor.GetEHubClientIDFromCredentials("TSTUsername"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetEHubClientIDFromCredentials_FromEHubClient_Success()
		{
			var mockContextFactory = MockContext();
			var context = mockContextFactory.CreateContext();
			var client = new eHubClient { CC_ID = "HYETSTCM2" };
			context.eHubClients.Add(client);
            var rt = context.eHubRegistrationTypes.First(x => x.RT_ID == "JPCustomsAccount_SystemLevel");
			var eh = context.eHubClientSystems.First(x => x.EH_ID == SystemID);
			var systemRegistration = new eHubClientSystemRegistration
			{
				eHubRegistrationType = rt,
				CD_RT = rt.RT_PK,
				CD_Code = "TSTUsername",
				CD_Attr1 = "TSTPassword",
				eHubClientSystem = eh,
				CD_EH = eh.EH_PK
			};
			context.eHubClientSystemRegistrations.Add(systemRegistration);

			var dataModelAccessor = new JPCustomsEhubClientIDDataModelAccessor(mockContextFactory);
			Assert.AreEqual(client.CC_ID, dataModelAccessor.GetEHubClientIDFromCredentials("TSTUsername"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetEHubClientIDFromCredentials_NoMatchingEHubClient_ReturnNull()
		{
			var mockContextFactory = MockContext();
			var context = mockContextFactory.CreateContext();
			var rt = context.eHubRegistrationTypes.First(x => x.RT_ID == "JPCustomsAccount_SystemLevel");
			var eh = context.eHubClientSystems.First(x => x.EH_ID == SystemID);
			var systemRegistration = new eHubClientSystemRegistration
			{
				eHubRegistrationType = rt,
				CD_RT = rt.RT_PK,
				CD_Code = "TSTUsername",
				CD_Attr1 = "TSTPassword",
				eHubClientSystem = eh,
				CD_EH = eh.EH_PK
			};
            context.eHubClientSystemRegistrations.Add(systemRegistration);

			var dataModelAccessor = new JPCustomsEhubClientIDDataModelAccessor(mockContextFactory);
			Assert.AreEqual(null, dataModelAccessor.GetEHubClientIDFromCredentials("TSTUsername"));
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetEHubClientIDFromCredentials_FallbackToClientSystemRegistration()
        {
            var mockContextFactory = MockContext();
            var context = mockContextFactory.CreateContext();
            var rt = context.eHubRegistrationTypes.First(x => x.RT_ID == "JPCustomsAccount_SystemLevel");
            var eh = context.eHubClientSystems.First(x => x.EH_ID == SystemID);
            var systemRegistration = new eHubClientSystemRegistration
            {
                eHubRegistrationType = rt,
                CD_RT = rt.RT_PK,
                CD_Code = "TSTUsername",
                CD_Attr1 = "TSTPassword",
                eHubClientSystem = eh,
                CD_EH = eh.EH_PK
            };
            var clientRegistration = new eHubClientRegistration
            {
                eHubRegistrationType = rt,
                CX_RT = rt.RT_PK,
                CX_Code = "UserNameNotMatched",
                CX_Password1 = "TSTPassword",
                CX_Qualifier = "Q1"
            };
            context.eHubClientSystemRegistrations.Add(systemRegistration);
            context.eHubClientRegistrations.Add(clientRegistration);

            var dataModelAccessor = new JPCustomsEhubClientIDDataModelAccessor(mockContextFactory);
            Assert.AreEqual(null, dataModelAccessor.GetEHubClientIDFromCredentials("TSTUsername"));
        }

		private ContextFactory<eHubTransactionsContext> MockContext()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);

			var testRegistrationTypes = new TestDbSet<eHubRegistrationType>();
			mockContext.Stub(x => x.eHubRegistrationTypes).Return(testRegistrationTypes);
            var registrationTypeClient = new eHubRegistrationType { RT_ID = "JPCustomsAccount_ClientLevel" };
            var registrationTypeSystem = new eHubRegistrationType { RT_ID = "JPCustomsAccount_SystemLevel" };
            mockContext.eHubRegistrationTypes.Add(registrationTypeClient);
            mockContext.eHubRegistrationTypes.Add(registrationTypeSystem);

			var testClients = new TestDbSet<eHubClient>();
			mockContext.Stub(x => x.eHubClients).Return(testClients);

			var testClientSystems = new TestDbSet<eHubClientSystem>();
			mockContext.Stub(x => x.eHubClientSystems).Return(testClientSystems);

			var testClientSystem = new eHubClientSystem { EH_ID = SystemID };
			testClientSystems.Add(testClientSystem);

			var testClientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>();
			mockContext.Stub(x => x.eHubClientSystemRegistrations).Return(testClientSystemRegistrations);

            var testClientRegistration = new TestDbSet<eHubClientRegistration>();
            mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegistration);

			return mockContextFactory;
		}
	}
}
