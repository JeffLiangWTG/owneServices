using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.UShipment2AIRAEU;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	[TestClass]
	public class UShipment2AIRAEU_Tests
	{
		const string filePath = "UShipment2AIRAEU.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEU_IDTSubscriptionDoesNotExist()
		{
			try
			{
				AssertMappingExceptionThrown("Test1_input.xml", "Test1_output.xml", false);
				Assert.Fail("Should throw");
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("Cannot find matching subscription for '0813243534EXPHB1MAN0000017E'.", ex.Message);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEU_AllCSTsAreErrored()
		{
			try
			{
				AssertMappingExceptionThrown("Test1_input.xml", "Test1_output.xml", true);
				Assert.Fail("Should throw");
			}
			catch (InvalidOperationException ex)
			{
				Assert.AreEqual("Looks like all the CSTs in the job are in an errored state, you should not submit an AIRAEU in this case.", ex.Message);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEU_SameHouseBillSubmittedInMultipleIDTs()
		{
			AssertMapping_SameHouseBillSubmittedInMultipleIDTs(true);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEU_SameHouseBillSubmittedInMultipleIDTs_InvalidAccount()
		{
			AssertMapping_SameHouseBillSubmittedInMultipleIDTs(false);
		}

		void AssertMapping_SameHouseBillSubmittedInMultipleIDTs(bool isAccountValid)
		{
			var input = filePath + "Test15_input.xml";
			var expectedOutput = filePath + "Test15_output.xml";

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

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "78436666663JKDSFAHLKMAN0001031E", "@referenceType", "IDT")).Return("198801949D|20211103|1975^198801949D|20211103|1978");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "78436666663JKDSFAHLKMAN0001031198801949D|20211103|1975E", "@referenceType", "CST")).Return("58890|58892");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "78436666663JKDSFAHLKMAN0001031198801949D|20211103|1978E", "@referenceType", "CST")).Return("58893");

			// Original IDT 1975
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Return("1983");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D|20211103|1975E", "@referenceType", "UpdateNumber")).Return("2");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D|20211103|1975E", "@referenceType", "PermitNumber")).Return("EUPS21K030005");

			if (isAccountValid)
			{
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D|20211103|1975E", "3", "UpdateNumber"));
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D202111031983E", "MAN0001031", "ManifestNumber"));
			}

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D|20211103|1983E", "198801949D|20211103|1975", "OriginalIDT"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D202111031983E", "78436666663", "MAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D202111031983E", "JKDSFAHLK", "HAWB"));

			// Original IDT 1978
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Return("1984");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D|20211103|1978E", "@referenceType", "UpdateNumber")).Return("2");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D|20211103|1978E", "@referenceType", "PermitNumber")).Return("EUPS21K030007");

			if (isAccountValid)
			{
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D|20211103|1978E", "3", "UpdateNumber"));
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D202111031984E", "MAN0001031", "ManifestNumber"));
			}

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D|20211103|1984E", "198801949D|20211103|1978", "OriginalIDT"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D202111031984E", "78436666663", "MAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "198801949D202111031984E", "JKDSFAHLK", "HAWB"));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2AIRAEU>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockSGCustomsDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEUAmendWith2OriginalIDT()
		{
			AssertMappingMultipleIDT("Test1_input.xml", "Test1_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEU66PackLineAmendSingleIDT()
		{
			AssertMappingMultipleOriginalCST("Test2_input.xml", "Test2_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEU66PackLineAmendSingleIDT_InvalidAccount()
		{
			AssertMappingMultipleOriginalCST("Test2_input.xml", "Test2_output.xml", false);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEU66PackLine110HAWBSAmendSingleIDT()
		{
			AssertMappingMultipleHAWBOverflow("Test3_input.xml", "Test3_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEUCancel()
		{
			AssertMapping("Test4_input.xml", "Test4_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEUPartialDelete()
		{
			AssertMapping("Test5_input.xml", "Test5_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEU_WI00229644()
		{
			var input = filePath + "Test14_input.xml";
			var expectedOutput = filePath + "Test14_output.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSGCustomsDataModelAccessor = MockRepository.GenerateStrictMock<SGCustomsDataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsAccount("PWS", "PRET1.PRET001")).Return("VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.IsAccountValid("PWS", "PRET1.PRET001", "VWGT001")).Return(true);
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsSenderID("VWGT001")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-08-23T11:30:01");
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Return("2017-08-23T18:30:01");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Return("0001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "40682271696771A92SRRV9MAN0035633E", "@referenceType", "IDT")).Return("198801949D|20190207|2704");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "40682271696771A92SRRV9MAN0035633198801949D|20190207|2704E", "@referenceType", "CST")).Return("43376|43377|43378");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D|20190207|2704E", "@referenceType", "UpdateNumber")).Return("2");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D|20190207|2704E", "@referenceType", "PermitNumber")).Return("EUPS19B080035");

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2AIRAEU>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockSGCustomsDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEUCancel_MultipleSubshipments()
		{
			var input = filePath + "Test12_input.xml";
			var expectedOutput = filePath + "Test12_output.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSGCustomsDataModelAccessor = MockRepository.GenerateStrictMock<SGCustomsDataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsAccount("PWS", "PRET1.PRET001")).Return("VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.IsAccountValid("PWS", "PRET1.PRET001", "VWGT001")).Return(true);
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsSenderID("VWGT001")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-08-23T11:30:01");
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Return("2017-08-23T18:30:01");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Return("0001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0001E", "@referenceType", "UpdateNumber")).Return("1");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0001E", "@referenceType", "PermitNumber")).Return("EUPS18H210001");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "198801949D|20170823|0001E", "201612334A|20170823|0001", "OriginalIDT"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "47466666666UIOYUIOYOMAN0000507E", "@referenceType", "IDT")).Return("201612334A|20170823|0001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "47466666666UIOYUIOYOMAN0000507201612334A|20170823|0001E", "@referenceType", "CST")).Return("00038");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "47466666666FJHJLUIHJMAN0000507E", "@referenceType", "IDT")).Return("201612334A|20170823|0001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "47466666666FJHJLUIHJMAN0000507201612334A|20170823|0001E", "@referenceType", "CST")).Return("00038");

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A|20170823|0001E", "2", "UpdateNumber"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "198801949D201708230001E", "UIOYUIOYO|FJHJLUIHJ", "HAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "198801949D201708230001E", "47466666666", "MAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "198801949D201708230001E", "MAN0000507", "ManifestNumber"));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2AIRAEU>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockSGCustomsDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEUDefaultValues_GoodsDescriptionExceedsMaxLength_ShouldBeTrimmed()
		{
			AssertMapping("Test6_input.xml", "Test6_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEUCustomsQty()
		{
			AssertMappingMultipleIDT("Test7_input.xml", "Test7_output.xml");
			AssertMappingMultipleIDT("Test8_input.xml", "Test8_output.xml");
			AssertMappingMultipleIDT("Test9_input.xml", "Test9_output.xml");
			AssertMappingMultipleIDT("Test10_input.xml", "Test10_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2AIRAEUPreferUENGovRegNum()
		{
			AssertMappingMultipleIDT("Test13_input.xml", "Test13_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSubshipments_AIRAEU_AreGroupedUnderSameIDT()
		{
			var inputFile = "Test11_input.xml";
			var expectedOutputFile = "Test11_output.xml";

			var sender = "UPESINPRD";
			var recipient = "SGCustoms";
			var broker = "TKW";
			var SGCustomsAccount = "UPS10P5";

			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSGCustomsDataModelAccessor = MockRepository.GenerateStrictMock<SGCustomsDataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(sender);
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipient);
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsAccount(broker, sender)).Return(SGCustomsAccount);
			mockSGCustomsDataModelAccessor.Expect(x => x.IsAccountValid(broker, sender, SGCustomsAccount)).Return(true);
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsSenderID(SGCustomsAccount)).Return(string.Format("{0}.{1}", SGCustomsAccount.Substring(0, 4), SGCustomsAccount));
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", SGCustomsAccount));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustoms", "@recipientId", "UPESINPRD", "@ST_ID", "SGCMSG", "@value", "40692874003R1F008H4KNKMAN0003944E", "@referenceType", "IDT")).Return("198801949D|20180117|1975");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustoms", "@recipientId", "UPESINPRD", "@ST_ID", "SGCMSG", "@value", "40692874003R1F008H4KNKMAN0003944198801949D|20180117|1975E", "@referenceType", "CST")).Return("26366");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustoms", "@recipientId", "UPESINPRD", "@ST_ID", "SGCMSG", "@value", "40692874003R1F008GL3LWMAN0003944E", "@referenceType", "IDT")).Return("198801949D|20180117|1975");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustoms", "@recipientId", "UPESINPRD", "@ST_ID", "SGCMSG", "@value", "40692874003R1F008GL3LWMAN0003944198801949D|20180117|1975E", "@referenceType", "CST")).Return("26365|26367");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustoms", "@recipientId", "UPESINPRD", "@ST_ID", "SGCMSG", "@value", "198801949D|20180117|1975E", "@referenceType", "UpdateNumber")).Return("1");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustoms", "@recipientId", "UPESINPRD", "@ST_ID", "SGCMSG", "@value", "198801949D|20180117|1975E", "@referenceType", "PermitNumber")).Return("EUPS18A170082");

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-08-23T11:30:01");
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Return("2017-08-23T18:30:01");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Return("0001");

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustoms", "UPESINPRD", "198801949D|20170823|0001E", "198801949D|20180117|1975", "OriginalIDT"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustoms", "UPESINPRD", "198801949D|20180117|1975E", "2", "UpdateNumber"));

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustoms", "UPESINPRD", "198801949D201708230001E", "R1F008GL3LW|R1F008H4KNK", "HAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustoms", "UPESINPRD", "198801949D201708230001E", "40692874003", "MAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "SGCustoms", "UPESINPRD", "198801949D201708230001E", "MAN0003944", "ManifestNumber"));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UShipment2AIRAEU>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockSGCustomsDataModelAccessor.VerifyAllExpectations();
		}

		void AssertMappingMultipleIDT(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSGCustomsDataModelAccessor = MockRepository.GenerateStrictMock<SGCustomsDataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsAccount("PWS", "PRET1.PRET001")).Return("VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.IsAccountValid("PWS", "PRET1.PRET001", "VWGT001")).Return(true);
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsSenderID("VWGT001")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-08-23T11:30:01");
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Return("2017-08-23T18:30:01");

			// Construct_IDT_SubShipments. IDT #1 has 3 house bills, IDT #2 has 2
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN0000017E", "@referenceType", "IDT")).Return("201612334A|20170823|0001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN0000017201612334A|20170823|0001E", "@referenceType", "CST")).Return("00038");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB2MAN0000017E", "@referenceType", "IDT")).Return("201612334A|20170823|0001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB2MAN0000017201612334A|20170823|0001E", "@referenceType", "CST")).Return("00056");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB3MAN0000017E", "@referenceType", "IDT")).Return("201612334A|20170823|0001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB3MAN0000017201612334A|20170823|0001E", "@referenceType", "CST")).Return("00074");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB4MAN0000017E", "@referenceType", "IDT")).Return("201612334A|20170823|0002");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB4MAN0000017201612334A|20170823|0002E", "@referenceType", "CST")).Return("00087");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB5MAN0000017E", "@referenceType", "IDT")).Return("201612334A|20170823|0002");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB5MAN0000017201612334A|20170823|0002E", "@referenceType", "CST")).Return("00099");

			// IDT #1
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Return("0001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0001E", "@referenceType", "UpdateNumber")).Return("1");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0001E", "@referenceType", "PermitNumber")).Return("PERMIT3342");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W|20170823|0001E", "201612334A|20170823|0001", "OriginalIDT"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "0813243534", "MAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "EXPHB1|EXPHB2|EXPHB3", "HAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "MAN0000017", "ManifestNumber"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A|20170823|0001E", "2", "UpdateNumber"));

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB1MAN000001700038E", "000011", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB2MAN000001700056E", "000011", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB3MAN000001700074E", "000011", "CNRF"));

			// IDT #2
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Return("0002");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0002E", "@referenceType", "UpdateNumber")).Return("2");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0002E", "@referenceType", "PermitNumber")).Return("PERMIT3342");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W|20170823|0002E", "201612334A|20170823|0002", "OriginalIDT"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230002E", "0813243534", "MAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230002E", "EXPHB4|EXPHB5", "HAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230002E", "MAN0000017", "ManifestNumber"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A|20170823|0002E", "3", "UpdateNumber"));

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB4MAN000001700087E", "000011", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB5MAN000001700099E", "000011", "CNRF"));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2AIRAEU>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockSGCustomsDataModelAccessor.VerifyAllExpectations();
		}

		void AssertMappingExceptionThrown(string inputFile, string expectedOutputFile, bool hasIDTSubscription)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSGCustomsDataModelAccessor = MockRepository.GenerateStrictMock<SGCustomsDataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsAccount("PWS", "PRET1.PRET001")).Return("VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.IsAccountValid("PWS", "PRET1.PRET001", "VWGT001")).Return(true);
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsSenderID("VWGT001")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-08-23T11:30:01");
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Return("2017-08-23T18:30:01");

			if (hasIDTSubscription)
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper(
						Arg<string>.Is.Equal("SelectSubscribedReference"),
						Arg<string>.Is.Equal("@reference"),
						Arg<string>.Is.Equal("@senderId"),
						Arg<string>.Is.Equal("VWGT.VWGT001"),
						Arg<string>.Is.Equal("@recipientId"),
						Arg<string>.Is.Equal("PRET1.PRET001"),
						Arg<string>.Is.Equal("@ST_ID"),
						Arg<string>.Is.Equal("SGCMSG"),
						Arg<string>.Is.Equal("@value"),
						Arg<string>.Is.Anything,
						Arg<string>.Is.Equal("@referenceType"),
						Arg<string>.Is.Equal("IDT")))
					.Return("199702247W|20170823|0001");

				mockCodeMapper.Expect(x => x.CallActionProcedureHelper(
						Arg<string>.Is.Equal("SelectSubscribedReference"),
						Arg<string>.Is.Equal("@reference"),
						Arg<string>.Is.Equal("@senderId"),
						Arg<string>.Is.Equal("VWGT.VWGT001"),
						Arg<string>.Is.Equal("@recipientId"),
						Arg<string>.Is.Equal("PRET1.PRET001"),
						Arg<string>.Is.Equal("@ST_ID"),
						Arg<string>.Is.Equal("SGCMSG"),
						Arg<string>.Is.Equal("@value"),
						Arg<string>.Is.Anything,
						Arg<string>.Is.Equal("@referenceType"),
						Arg<string>.Is.Equal("CST")))
					.Return("");
			}
			else
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN0000017E", "@referenceType", "IDT")).Return("");
			}

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2AIRAEU>(input, expectedOutput);
		}

		void AssertMappingMultipleOriginalCST(string inputFile, string expectedOutputFile, bool isAccountValid = true)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSGCustomsDataModelAccessor = MockRepository.GenerateStrictMock<SGCustomsDataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsAccount("BKG", "PRET1.PRET001")).Return("VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.IsAccountValid("BKG", "PRET1.PRET001", "VWGT001")).Return(isAccountValid);
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsSenderID("VWGT001")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-08-23T11:30:01");
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Return("2017-08-23T18:30:01");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A|20170823|0001E", "201612334A|20170823|0001", "OriginalIDT"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Return("0001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0001E", "@referenceType", "UpdateNumber")).Return("1");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0001E", "@referenceType", "PermitNumber")).Return("EUPS18A170082");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB1MAN0000092E", "@referenceType", "IDT")).Return("201612334A|20170823|0001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB1MAN0000092201612334A|20170823|0001E", "@referenceType", "CST")).Return("00038|00039");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN000009200038E",
				"000011|000022|000033|000044|000055|000066|000077|000088|000099|0001010|0001111|0001212|0001313|0001414|0001515|0001616|0001717|0001818|0001919|0002020|0002121|0002222|0002323|0002424|0002525|0002626|0002727|0002828|0002929|0003030|0003131|0003232|0003333|0003434|0003535|0003636|0003737|0003838|0003939|0004040|0004141|0004242|0004343|0004444|0004545|0004646|0004747|0004848|0004949|0005050", "CNRF"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB2MAN0000092E", "@referenceType", "IDT")).Return("201612334A|20170823|0001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB2MAN0000092201612334A|20170823|0001E", "@referenceType", "CST")).Return("00056");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB2MAN000009200056E", "0000167|0000268|0000369|0000470|0000571", "CNRF"));

			if (isAccountValid)
			{
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A|20170823|0001E", "2", "UpdateNumber"));
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "MAN0000092", "ManifestNumber"));
			}

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "HAWB1|HAWB2", "HAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN000009200039E", "0005151|0005252|0005353|0005454|0005555|0005656|0005757|0005858|0005959|0006060|0006161|0006262|0006363|0006464|0006565|0006666", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "045-99736483", "MAWB"));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2AIRAEU>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockSGCustomsDataModelAccessor.VerifyAllExpectations();
		}

		void AssertMappingMultipleHAWBOverflow(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSGCustomsDataModelAccessor = MockRepository.GenerateStrictMock<SGCustomsDataModelAccessor>();

			var ArticleItemNumber = 1;
			var SubscribedCSTCounter = 1;

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsAccount("BKG", "PRET1.PRET001")).Return("VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.IsAccountValid("BKG", "PRET1.PRET001", "VWGT001")).Return(true);
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsSenderID("VWGT001")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-08-23T11:30:01");
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Return("2017-08-23T18:30:01");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4"))
				.Return(ArticleItemNumber.ToString()).Repeat.Twice()
				.WhenCalled(x => x.ReturnValue = ArticleItemNumber++.ToString());

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0001E", "@referenceType", "UpdateNumber")).Return("1");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0001E", "@referenceType", "PermitNumber")).Return("EUPS18A170082");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0001E", "@referenceType", "UpdateNumber")).Return("2");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0001E", "@referenceType", "PermitNumber")).Return("EUPS18A170083");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A|20170823|0001E", "201612334A|20170823|0001", "OriginalIDT"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A|20170823|0002E", "201612334A|20170823|0001", "OriginalIDT"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "012-12334123", "MAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230002E", "012-12334123", "MAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "MAN0000091", "ManifestNumber"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230002E", "MAN0000091", "ManifestNumber"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper(Arg<string>.Is.Equal("SelectSubscribedReference"),
				Arg<string>.Is.Equal("@reference"),
				Arg<string>.Is.Equal("@senderId"),
				Arg<string>.Is.Equal("VWGT.VWGT001"),
				Arg<string>.Is.Equal("@recipientId"),
				Arg<string>.Is.Equal("PRET1.PRET001"),
				Arg<string>.Is.Equal("@ST_ID"),
				Arg<string>.Is.Equal("SGCMSG"),
				Arg<string>.Is.Equal("@value"),
				Arg<string>.Is.Anything,
				Arg<string>.Is.Equal("@referenceType"),
				Arg<string>.Is.Equal("IDT"))).Return("201612334A|20170823|0001");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper(Arg<string>.Is.Equal("SelectSubscribedReference"),
				Arg<string>.Is.Equal("@reference"),
				Arg<string>.Is.Equal("@senderId"),
				Arg<string>.Is.Equal("VWGT.VWGT001"),
				Arg<string>.Is.Equal("@recipientId"),
				Arg<string>.Is.Equal("PRET1.PRET001"),
				Arg<string>.Is.Equal("@ST_ID"),
				Arg<string>.Is.Equal("SGCMSG"),
				Arg<string>.Is.Equal("@value"),
				Arg<string>.Is.Anything,
				Arg<string>.Is.Equal("@referenceType"),
				Arg<string>.Is.Equal("CST"))).Return("00001").WhenCalled(x =>
				{
					if (SubscribedCSTCounter % 10 == 0)
					{
						x.ReturnValue = "00001|00002";
					}
					SubscribedCSTCounter++;
				});

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A|20170823|0001E", "2", "UpdateNumber"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A|20170823|0001E", "3", "UpdateNumber"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Equal("SGCMSG"), Arg<string>.Is.Equal("VWGT.VWGT001"), Arg<string>.Is.Equal("PRET1.PRET001"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("CNRF")));

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Equal("SGCMSG"), Arg<string>.Is.Equal("VWGT.VWGT001"), Arg<string>.Is.Equal("PRET1.PRET001"), Arg<string>.Is.Equal("201612334A201708230001E"), Arg<string>.Is.Anything, Arg<string>.Is.Equal("HAWB")));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Equal("SGCMSG"), Arg<string>.Is.Equal("VWGT.VWGT001"), Arg<string>.Is.Equal("PRET1.PRET001"), Arg<string>.Is.Equal("201612334A201708230002E"), Arg<string>.Is.Anything, Arg<string>.Is.Equal("HAWB")));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2AIRAEU>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockSGCustomsDataModelAccessor.VerifyAllExpectations();
		}

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSGCustomsDataModelAccessor = MockRepository.GenerateStrictMock<SGCustomsDataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VWGT.VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsAccount("PWS", "PRET1.PRET001")).Return("VWGT001");
			mockSGCustomsDataModelAccessor.Expect(x => x.IsAccountValid("PWS", "PRET1.PRET001", "VWGT001")).Return(true);
			mockSGCustomsDataModelAccessor.Expect(x => x.GetSGCustomsSenderID("VWGT001")).Return("VWGT.VWGT001");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-08-23T11:30:01");
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Return("2017-08-23T18:30:01");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Return("0001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0001E", "@referenceType", "UpdateNumber")).Return("1");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A|20170823|0001E", "@referenceType", "PermitNumber")).Return("PERMIT3342");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W|20170823|0001E", "201612334A|20170823|0001", "OriginalIDT"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN0000017E", "@referenceType", "IDT")).Return("201612334A|20170823|0001");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN0000017201612334A|20170823|0001E", "@referenceType", "CST")).Return("00038");

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB1MAN000001700038E", "000011", "CNRF")).Repeat.Any();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A|20170823|0001E", "2", "UpdateNumber"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "EXPHB1", "HAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "0813243534", "MAWB"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "MAN0000017", "ManifestNumber"));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2AIRAEU>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockSGCustomsDataModelAccessor.VerifyAllExpectations();
		}
	}
}
