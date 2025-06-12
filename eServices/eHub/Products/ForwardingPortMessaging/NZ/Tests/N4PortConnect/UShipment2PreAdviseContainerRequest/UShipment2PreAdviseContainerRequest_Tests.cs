using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.NZ.N4PortConnect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.NZ.Tests
{
	[TestClass]
	public class UShipment2PreAdviseContainerRequest_Tests
	{
		const string filePath = "N4PortConnect.UShipment2PreAdviseContainerRequest.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2PreAdviseContainerRequest_PortConnect()
		{
			AssertMapping("PortConnect.Test1_input_ORG.xml", "PortConnect.Test1_output_ORG.xml", "ORG", "NZTRG", serviceProvider: "PORTCONNECT");
			AssertMapping("PortConnect.Test2_input_AMD.xml", "PortConnect.Test2_output_AMD.xml", "AMD", "NZTRG", "11223344556677", serviceProvider: "PORTCONNECT");
			AssertMapping("PortConnect.Test3_input_WTH.xml", "PortConnect.Test3_output_WTH.xml", "WTH", "NZTRG", "11223344556677", serviceProvider: "PORTCONNECT");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2PreAdviseContainerRequest_N4()
		{
			AssertMapping("N4.Test1_input_ORG.xml", "N4.Test1_output_ORG.xml", "ORG", "NZLYT", serviceProvider:"N4");
			AssertMapping("N4.Test2_input_AMD.xml", "N4.Test2_output_AMD.xml", "AMD", "NZLYT", "11223344556677", serviceProvider: "N4");
			AssertMapping("N4.Test3_input_WTH.xml", "N4.Test3_output_WTH.xml", "WTH", "NZLYT", "11223344556677", serviceProvider: "N4");
		}

		void AssertMapping(string sourceFile, string expectedFile, string purpose, string operationalPort, string previousMessageIdentifier = "", string serviceProvider = "")
		{
			var input = filePath + sourceFile;
			var expectedOutput = filePath + expectedFile;

			var senderID = "WTLDAUILA";
			var branch = "AKL";
			var dataSourceKey = "C00001386";
			var messageIdentifier = string.IsNullOrEmpty(previousMessageIdentifier) ? "14141414141414" : previousMessageIdentifier;
			var documentName = "Pre-Advice Export Notification (NZ)";
			var forwardingType = "ForwardingConsol_" + dataSourceKey;
			var portConnectID = "tradingPartner123";
			var currentDateTime = "20240418130420";
			var serviceProviderID = "ID";
			var serviceProviderMSGID = serviceProviderID + "MSG";
			var serviceProviderPrefix = "PRF" + serviceProviderID;
			var messageReference = serviceProviderPrefix + messageIdentifier;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("WTLDAUILA");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(serviceProvider);

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider)).Return(serviceProvider).Repeat.Once();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", serviceProvider)).Return(serviceProviderMSGID).Repeat.Once();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", serviceProvider)).Return(serviceProviderID).Repeat.Once();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", serviceProvider)).Return(serviceProviderPrefix).Repeat.Once();

			mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode(senderID, branch, serviceProvider)).Return(portConnectID).Repeat.Once();
			mockDataModelAccessor.Expect(x => x.GetClientRegistrationAttri1AsString(senderID, branch, serviceProvider)).Return("user123").Repeat.Once();

			var preAdvice = "PCIU5600395_" + operationalPort + "_" + portConnectID;
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", preAdvice, "@referenceType", "PreAdvice")).Return(string.IsNullOrEmpty(previousMessageIdentifier) ? string.Empty : serviceProviderPrefix + previousMessageIdentifier).Repeat.Once();

			if (string.IsNullOrEmpty(previousMessageIdentifier))
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", $"CargoWise.eHub.Products.ForwardingPortMessaging.NZ.{serviceProvider}.PreAdviseContainerRequest", "@maxlength", "14")).Return(messageIdentifier).Repeat.Once();
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReference, preAdvice, "PreAdvice")).Repeat.Once();
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, preAdvice, messageReference, "PreAdvice")).Repeat.Once();
			}

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReference, operationalPort, "OperationPort")).Repeat.Once();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReference, purpose, "Purpose")).Repeat.Once();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReference, documentName, "DocumentName")).Repeat.Once();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReference, forwardingType, "ForwardingType")).Repeat.Once();

			mockDateMapper.Stub(x => x.CurrentDateTimeUTC(Arg.Is("yyyyMMddHHmmssfff"))).Return(currentDateTime).Repeat.Once();

			if (serviceProvider == "N4")
			{
				mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", messageReference + ".xml")).Repeat.Once();
			}
			else
			{
				mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", senderID + "_" + dataSourceKey + "_" + currentDateTime + ".xml")).Repeat.Once();
			}

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UShipment2PreAdviseContainerRequest>(input, expectedOutput);

			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}
	}
}
