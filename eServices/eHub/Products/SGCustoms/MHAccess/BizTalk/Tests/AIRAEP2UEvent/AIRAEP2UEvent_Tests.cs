using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Receive.Maps.AIRAEP2UEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	[TestClass]
	public class AIRAEP2UEvent_Tests
	{
		const string filePath = "AIRAEP2UEvent.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AssertMapping1_Message1()
		{
			var input = filePath + "Test01_Message1_input.xml";
			var expectedOutput = filePath + "Test01_Message1_output.xml";

			var subscriptionData = GetEmbeddedResource(filePath + "Test01_SubscriptionData.xml");
			var subscriptionDataString = XDocument.Load(subscriptionData).ToString(SaveOptions.DisableFormatting); // XDocument is used to strip formatting

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGTVWGT001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGTVWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201707078106E", "@referenceType", "ManifestNumber")).Return("MAN0000092");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGTVWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB0707003HAWB0707001MAN000009200005E", "@referenceType", "CNRF")).Return("000022");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGTVWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB0707003HAWB0707001MAN000009200006E", "@referenceType", "CNRF")).Return("000011|00002413|000031337");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGTVWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB0707003HAWB0707002MAN000009200007E", "@referenceType", "CNRF")).Return("");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGTVWGT001", "PRET1.PRET001", "198801949D201707078106E", subscriptionDataString, "AIRAEP"));
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-08-23T11:30:01");

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRAEP2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AssertMapping1_Message1_AIRERRConsignmentsShouldNotBeIncluded()
		{
			var input = filePath + "Test01_Message1_input.xml";
			var expectedOutput = filePath + "Test01_Message1_output_AIRERRNotIncluded.xml";

			var subscriptionData = GetEmbeddedResource(filePath + "Test01_SubscriptionData_AIRERRNotIncluded.xml");
			var subscriptionDataString = XDocument.Load(subscriptionData).ToString(SaveOptions.DisableFormatting); // XDocument is used to strip formatting

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGTVWGT001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGTVWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201707078106E", "@referenceType", "ManifestNumber")).Return("MAN0000092");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGTVWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB0707003HAWB0707001MAN000009200005E", "@referenceType", "CNRF")).Return("AIRERR");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGTVWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB0707003HAWB0707001MAN000009200006E", "@referenceType", "CNRF")).Return("000011|00002413|000031337");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGTVWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB0707003HAWB0707002MAN000009200007E", "@referenceType", "CNRF")).Return("");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGTVWGT001", "PRET1.PRET001", "198801949D201707078106E", subscriptionDataString, "AIRAEP"));
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-08-23T11:30:01");

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRAEP2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AssertMapping1_Message2()
		{
			var input = filePath + "Test01_Message2_input.xml";
			var expectedOutput = filePath + "Test01_Message2_output.xml";

			var subscriptionData = GetEmbeddedResource(filePath + "Test01_SubscriptionData.xml");
			var subscriptionDataString = XDocument.Load(subscriptionData).ToString(SaveOptions.DisableFormatting); // XDocument is used to strip formatting

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGTVWGT001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "VWGTVWGT001", "@ST_ID", "SGCMSG", "@value", "198801949D201707078106E", "@referenceType", "AIRAEP")).Return(subscriptionDataString);
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-23T11:30:00");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "PRET1.PRET001", "VWGTVWGT001", "198801949D|20170707|8106E", "EUPS17G070003", "PermitNumber"));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRAEP2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AssertMapping2_Message1()
		{
			var input = filePath + "Test02_Message1_input.xml";
			var expectedOutput = filePath + "Test02_Message1_output.xml";

			var subscriptionData = GetEmbeddedResource(filePath + "Test02_SubscriptionData.xml");
			var subscriptionDataString = XDocument.Load(subscriptionData).ToString(SaveOptions.DisableFormatting); // XDocument is used to strip formatting

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("Source");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("Destination");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "0000000004000000050006E", "@referenceType", "ManifestNumber")).Return("MAN0000092");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "MAWB0000014HAWB0000013MAN000009200012E", "@referenceType", "CNRF")).Return("0000120|0000221");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "Source", "Destination", "0000000004000000050006E", subscriptionDataString, "AIRAEP"));
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2020-04-13T01:00:00");

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRAEP2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AssertMapping2_Message2()
		{
			var input = filePath + "Test02_Message2_input.xml";
			var expectedOutput = filePath + "Test02_Message2_output.xml";

			var subscriptionData = GetEmbeddedResource(filePath + "Test02_SubscriptionData.xml");
			var subscriptionDataString = XDocument.Load(subscriptionData).ToString(SaveOptions.DisableFormatting); // XDocument is used to strip formatting

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("Source");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("Destination");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "0000000004000000050006E", "@referenceType", "AIRAEP")).Return(subscriptionDataString);
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2020-04-13T04:13:00");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "Source", "Destination", "0000000004|00000005|0006E", "0000000000015", "PermitNumber"));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRAEP2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AssertMapping_EmptySubscriber()
		{
			var input = filePath + "Test01_Message1_input.xml";
			var expectedOutput = filePath + "Test01_Message1_output.xml";


			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("");

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
			};

			try
			{
				var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
				mapTester.ExecuteCompiled<AIRAEP2UEvent>(input, expectedOutput);
				Assert.Fail("Should throw exception when recipient or sender is Empty");
			}
			catch (Exception e)
			{
				Assert.AreEqual(e.Message, "Unable to resolve the recipient_ Cannot found subscriber at mapping AIRAEP");
			}
		}
		public Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(string.Format("Could not locate embedded resource '{0}'", fullResourceName));
			}
			return resource;
		}
	}
}
