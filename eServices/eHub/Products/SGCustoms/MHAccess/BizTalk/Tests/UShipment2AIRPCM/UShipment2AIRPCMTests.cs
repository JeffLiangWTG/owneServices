using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.UShipment2AIRPCM;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	[TestClass]
	public class UShipment2AIRPCMTests
	{
		const string filePath = "UShipment2AIRPCM.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_UShipment2AIRPCM_EdgeCases()
		{
			AssertMapping("Test9_input.xml", "Test9_output.xml", "Test9_subscription_Input.xml", "Test9_subscription_Output.xml", true);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_UShipment2AIRPCM_Simple_GoodsDescriptionExceedsMaxLength_ShouldBeTrimmed()
		{
			AssertMapping("Test10_input.xml", "Test10_output.xml", "Test10_subscription_Input.xml", "Test10_subscription_Output.xml", true);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRPCMPreferUENGovRegNum()
		{
			AssertMapping("Test11_input.xml", "Test11_output.xml", "Test11_subscription_Input.xml", "Test11_subscription_Output.xml", true);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRPCMPreferUENGovRegNum_InvalidAccount()
		{
			AssertMapping("Test11_input.xml", "Test11_output.xml", "Test11_subscription_Input.xml", "Test11_subscription_Output.xml", false);
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string inputSubscriptionFile, string outputSubscriptionFile, bool isAccountValid)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;
			var subscriptionInput = ReadResource(filePath + inputSubscriptionFile);
			var subscriptionOutput = ReadResource(filePath + outputSubscriptionFile);

			var IDTCounter = 1;
			var CSTCounter = 1;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSGCustomsDataModelAccessor = MockRepository.GenerateStrictMock<SGCustomsDataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SGCustomsTest");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsAccount("SW", "UPEUPE001")).Return("vwgt001");
			mockSGCustomsDataModelAccessor.Expect(x => x.IsAccountValid("SW", "UPEUPE001", "vwgt001")).Return(isAccountValid);
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "vwgt001"));
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsSenderID("vwgt001")).Return("vwgt.vwgt001");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-06T12:00").Repeat.AtLeastOnce();
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-09-06T12:00", "SGSIN")).Return("2017-09-06T04:00");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "MAN0000030I")).Return(subscriptionInput);
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4"))
				.Return(IDTCounter.ToString())
				.WhenCalled(x => x.ReturnValue = IDTCounter++.ToString())
				.Repeat.Any();

			if (isAccountValid)
			{
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(
					Arg<string>.Is.Equal("SGCMSG"),
					Arg<string>.Is.Equal("SGCustomsTest"),
					Arg<string>.Is.Equal("UPEUPE001"),
					Arg<string>.Is.Anything,
					Arg<string>.Is.Equal("MAN0000030"),
					Arg<string>.Is.Equal("ManifestNumber")));
			}

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "MAN0000030I", subscriptionOutput));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRPCM.CST", "@maxlength", "5"))
				.Return(CSTCounter.ToString())
				.WhenCalled(x => x.ReturnValue = CSTCounter++.ToString())
				.Repeat.Any();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2AIRPCM>(input, expectedOutput);

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
