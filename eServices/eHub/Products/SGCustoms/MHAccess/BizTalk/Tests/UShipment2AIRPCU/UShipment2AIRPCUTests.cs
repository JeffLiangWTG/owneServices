using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.UShipment2AIRPCU;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	[TestClass]
	public class UShipment2AIRPCUTests
	{
		const string filePath = "UShipment2AIRPCU.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test1()
		{
			AssertMapping(true);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test1_InvalidAccount()
		{
			AssertMapping(false);
		}

		void AssertMapping(bool isAccountValid)
		{
			var input = filePath + "Test5_input.xml";
			var expectedOutput = filePath + "Test5_output.xml";
			var subscriptionInput = ReadResource(filePath + "Test5_subscription_Input.xml");
			var subscriptionOutput = ReadResource(filePath + "Test5_subscription_Output.xml");

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSGCustomsDataModelAccessor = MockRepository.GenerateStrictMock<SGCustomsDataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SGCustomsTest");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsAccount("SW", "UPEUPE001")).Return("vgwt001");
			mockSGCustomsDataModelAccessor.Expect(x => x.IsAccountValid("SW", "UPEUPE001", "vgwt001")).Return(isAccountValid);
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsSenderID("vgwt001")).Return("vgwt.vgwt001");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "vgwt001"));

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-07T12:00").Repeat.AtLeastOnce();
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-09-07T12:00", "SGSIN")).Return("2017-09-07T04:00");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "MAN0000030I")).Return(subscriptionInput);

			var IDTCounter = 3;
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4"))
				.Return(IDTCounter.ToString())
				.WhenCalled(x => x.ReturnValue = IDTCounter++.ToString())
				.Repeat.Any();

			if (isAccountValid)
			{
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D201709070003I", "MAN0000030", "ManifestNumber"));
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D201709070004I", "MAN0000030", "ManifestNumber"));
			}

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "MAN0000030I", subscriptionOutput));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2AIRPCU>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockSGCustomsDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2()
		{
			var input = filePath + "Test7_input.xml";
			var expectedOutput = filePath + "Test7_output.xml";
			var subscriptionInput = ReadResource(filePath + "Test7_subscription_Input.xml");
			var subscriptionOutput = ReadResource(filePath + "Test7_subscription_Output.xml");

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSGCustomsDataModelAccessor = MockRepository.GenerateStrictMock<SGCustomsDataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SGCustomsTest");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsAccount("PW", "UPEUPE001")).Return("vgwt001");
			mockSGCustomsDataModelAccessor.Expect(x => x.IsAccountValid("PW", "UPEUPE001", "vgwt001")).Return(true);
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsSenderID("vgwt001")).Return("vgwt.vgwt001");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "vgwt001"));

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2019-07-03T05:42:18").Repeat.AtLeastOnce();
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2019-07-03T05:42:18", "SGSIN")).Return("2019-07-03T05:42:18");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "MAN0000739I")).Return(subscriptionInput);
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "MAN0000739I", subscriptionOutput));

			var IDTCounter = 1218;
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4"))
				.Return(IDTCounter.ToString())
				.WhenCalled(x => x.ReturnValue = IDTCounter++.ToString())
				.Repeat.Any();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D201907031218I", "MAN0000739", "ManifestNumber"));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2AIRPCU>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockSGCustomsDataModelAccessor.VerifyAllExpectations();
		}

		string ReadResource(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();

			using (Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
