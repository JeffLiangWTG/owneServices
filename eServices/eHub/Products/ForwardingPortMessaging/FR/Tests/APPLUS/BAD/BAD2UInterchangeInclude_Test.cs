using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.FR.Tests
{
	[TestClass]
	public class BAD2UInterchangeInclude_Test
	{
		const string filePath = "APPLUS.BAD.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestBAD2UInterchangeInclude()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml", "MGI");
			AssertMapping("Test2_fallbackdate_input.xml", "Test2_fallbackdate_output.xml", "MGI", eHubID: "HYEUATCW1");
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string serviceProvider, string eHubID = "")
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var serviceProviderPrefix = serviceProvider.Substring(0, 3);
			var serviceProviderID = serviceProviderPrefix + "ID";
			var serviceProviderSIC = serviceProviderPrefix + "SIC";
			var serviceProviderMSGID = serviceProviderPrefix + "MSG";

			var applusRecipientId = "HYEUATCW1";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(serviceProvider).Repeat.AtLeastOnce();
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CW1").Repeat.AtLeastOnce();
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", applusRecipientId)).Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider)).Return(serviceProvider).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", serviceProvider)).Return(serviceProviderID);

			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "BAD Event Type", "Event Type", "CREATE")).Return("ATH").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "BAD Event Type", "Event Type", "UPDATE")).Return("ATH").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "BAD Event Type", "Event Type", "DELETE")).Return("ATW").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "BAD Event Type", "Status", "CREATE")).Return("Original").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "BAD Event Type", "Status", "UPDATE")).Return("Amendment").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "BAD Event Type", "Status", "DELETE")).Return("Cancellation").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Zone UNLOCO", "Output Code", "FOS")).Return("FRFOS").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Carrier Code", "Output Code", "CMACGMAG")).Return("CDMU").Repeat.Any();

			mockDataModelAccessor.Expect(x => x.GeteHubIDByCode("EUROFMA,EUROFMA", serviceProvider)).Return(eHubID).Repeat.Any();
			if (string.IsNullOrEmpty(eHubID))
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", serviceProvider, "@ST_ID", serviceProviderID, "@value", "EUROFMA,EUROFMA")).Return(applusRecipientId).Repeat.Once();
			}


			var extensionObjects = new Dictionary<string, object>()
	  {
		{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
		{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
		{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
	  };

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<BAD2UInterchangeInclude>(input, expectedOutput);

			mockContextAccessor.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}
	}
}
