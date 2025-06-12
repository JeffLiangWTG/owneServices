using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.UShipment2AIRAED;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	[TestClass]
	public class UShipment2AIRAED_Tests
	{
		const string filePath = "UShipment2AIRAED.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_UShipment2AIRAED_SameHouseBillSubmittedInMultipleIDTs()
		{
			AssertMapping_SameHouseBillSubmittedInMultipleIDTs(true);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_UShipment2AIRAED_SameHouseBillSubmittedInMultipleIDTs_InvalidAccount()
		{
			AssertMapping_SameHouseBillSubmittedInMultipleIDTs(false);
		}

		void AssertMapping_SameHouseBillSubmittedInMultipleIDTs(bool isAccountValid)
		{
			var input = filePath + "Test1_input.xml";
			var expectedOutput = filePath + "Test1_output.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSGCustomsDataModelAccessor = MockRepository.GenerateStrictMock<SGCustomsDataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SGCustomsTest");
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsAccount("PW", "UPEUPE001")).Return("VWGT002");
			mockSGCustomsDataModelAccessor.Expect(x => x.IsAccountValid("PW", "UPEUPE001", "VWGT002")).Return(isAccountValid);
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsSenderID("VWGT002")).Return("VWGT.VWGT002");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT002"));

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2021-11-03T11:30:01");
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2021-11-03T11:30:01", "SGSIN")).Return("2021-11-03T18:30:01");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Return("1978");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRAED.CST", "@maxlength", "5")).Return("58893");

			if (isAccountValid)
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "78436666663JKDSFAHLKMAN0001031E", "@referenceType", "IDT"))
					.Return("198801949D|20211103|1975");

				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D|20211103|1978E", "1", "UpdateNumber"));
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D202111031978E", "MAN0001031", "ManifestNumber"));
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D202111031978E", "78436666663", "MAWB"));
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D202111031978E", "JKDSFAHLK", "HAWB"));

				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "78436666663JKDSFAHLKMAN0001031E", "198801949D|20211103|1975^198801949D|20211103|1978", "IDT"));
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "78436666663JKDSFAHLKMAN0001031198801949D|20211103|1978E", "58893", "CST"));
			}

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Equal("SGCMSG"), Arg<string>.Is.Equal("SGCustomsTest"), Arg<string>.Is.Equal("UPEUPE001"),
				Arg<string>.Is.Equal("78436666663JKDSFAHLKMAN000103158893E"), Arg<string>.Is.Anything, Arg<string>.Is.Equal("CNRF")));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2AIRAED>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockSGCustomsDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_UShipment2AIRAED_EdgeCases_GoodsDescriptionExceedsMaximumLength_ShouldBeTrimmed()
		{
			AssertMapping("Test12_input.xml", "Test12_output.xml");
		}

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var IDTCount = 1;
			var CSTCount = 1;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSGCustomsDataModelAccessor = MockRepository.GenerateStrictMock<SGCustomsDataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SGCustomsTest");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsAccount("SW", "UPEUPE001")).Return("VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.IsAccountValid("SW", "UPEUPE001", "VWGT001")).Return(true);
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsSenderID("VWGT001")).Return("VWGT.VWGT001");

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-08-23T11:30:01");
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Return("2017-08-23T18:30:01");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4"))
				.Return("0001")
				.WhenCalled(x =>
				{
					x.ReturnValue = IDTCount++.ToString("D4");
				}).Repeat.AtLeastOnce();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRAED.CST", "@maxlength", "5"))
				.Return("00001")
				.WhenCalled(x =>
				{
					x.ReturnValue = CSTCount++.ToString("D5");
				}).Repeat.AtLeastOnce();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper(
					Arg<string>.Is.Equal("SelectSubscribedReference"),
					Arg<string>.Is.Equal("@reference"),
					Arg<string>.Is.Equal("@senderId"),
					Arg<string>.Is.Equal("SGCustomsTest"),
					Arg<string>.Is.Equal("@recipientId"),
					Arg<string>.Is.Equal("UPEUPE001"),
					Arg<string>.Is.Equal("@ST_ID"),
					Arg<string>.Is.Equal("SGCMSG"),
					Arg<string>.Is.Equal("@value"),
					Arg<string>.Is.Anything,
					Arg<string>.Is.Equal("@referenceType"),
					Arg<string>.Is.Equal("IDT")))
				.Return("").Repeat.AtLeastOnce();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D|20170823|0001E", "1", "UpdateNumber"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D|20170823|0002E", "1", "UpdateNumber"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D201708230001E", "MAN0000030", "ManifestNumber"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D201708230002E", "MAN0000030", "ManifestNumber"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D201708230001E", "03155555555", "MAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D201708230002E", "03155555555", "MAWB"));

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Equal("SGCMSG"), Arg<string>.Is.Equal("SGCustomsTest"), Arg<string>.Is.Equal("UPEUPE001"),
				Arg<string>.Matches(s => s.StartsWith("198801949D20170823")), Arg<string>.Is.Anything, Arg<string>.Is.Equal("HAWB"))).Repeat.AtLeastOnce();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Equal("SGCMSG"), Arg<string>.Is.Equal("SGCustomsTest"), Arg<string>.Is.Equal("UPEUPE001"),
				Arg<string>.Is.Anything, Arg<string>.Is.Equal("198801949D|20170823|0001"), Arg<string>.Is.Equal("IDT"))).Repeat.AtLeastOnce();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Equal("SGCMSG"), Arg<string>.Is.Equal("SGCustomsTest"), Arg<string>.Is.Equal("UPEUPE001"),
				Arg<string>.Is.Anything, Arg<string>.Is.Equal("198801949D|20170823|0002"), Arg<string>.Is.Equal("IDT"))).Repeat.AtLeastOnce();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Equal("SGCMSG"), Arg<string>.Is.Equal("SGCustomsTest"), Arg<string>.Is.Equal("UPEUPE001"),
				Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("CST"))).Repeat.AtLeastOnce();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Equal("SGCMSG"), Arg<string>.Is.Equal("SGCustomsTest"), Arg<string>.Is.Equal("UPEUPE001"),
				Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("CNRF"))).Repeat.AtLeastOnce();

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2AIRAED>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockSGCustomsDataModelAccessor.VerifyAllExpectations();
		}
	}
}
