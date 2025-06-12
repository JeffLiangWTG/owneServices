using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.XPath;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Receive.Maps.AIRERR2UEvent;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	[TestClass]
	public class AIRERR2UEvent_Tests
	{
		const string filePath = "AIRERR2UEvent.TestFiles.";
		const string testDate = "2017-09-20T14:21:01";

		string ReadResource(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();

			using (Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Import_NoHawbThenLoadFromExistingSubscription()
		{
			var input = filePath + "Test1_input.xml";
			var expectedOutput = filePath + "Test1_output.xml";
			var subscriptionInput = ReadResource("AIRERR2UEvent.TestFiles.Test1_Subscription_Input.xml");
			var subscriptionOutput = ReadResource("AIRERR2UEvent.TestFiles.Test1_Subscription_Output.xml");

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-06T12:00:00").Repeat.AtLeastOnce();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201907261302I", "@referenceType", "ManifestNumber")).Return("MAN0000781");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAN0000781I")).Return(subscriptionInput);
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAN0000781I", subscriptionOutput));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);

			mockCodeMapper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Export_AIRAEDSingleERRWithLineReference()
		{
			AssertMappingExportAIRAED("Test2_input.xml", "Test2_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Export_AIRAEU_UsingIDT()
		{
			AssertMappingExportAIRAEU("Test5_input.xml", "Test5_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Export_AIRAED_ManyERRsUsingIDT()
		{
			AssertMappingExportAIRAED("Test6_input.xml", "Test6_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Export_SequenceNumber_Invalid()
		{
			AssertMappingExportSequenceNumber("Test7_input.xml", "Test7_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Export_SequenceNumber_Decrement()
		{
			AssertMappingExportSequenceNumber("Test8_input.xml", "Test8_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Export_AIRAEU_WithLineReference()
		{
			AssertMappingExportAIRAEU("Test9_input.xml", "Test9_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Export_AIRAED_SER()
		{
			AssertMappingExportAIRAED_SER("Test10_input.xml", "Test10_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Export_AIRAED_SERWithThreeCSTBlocks()
		{
			AssertMappingExportAIRAED_SERWithThreeCSTBlocks("Test15_input.xml", "Test15_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Export_AIRAED_SERWithThreeCSTBlocksAndTwoHawbs()
		{
			AssertMappingExportAIRAED_SERWithThreeCSTBlocksAndTwoHawbs("Test17_input.xml", "Test17_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Export_AIRAEU_UpdateNumberNotInSequence()
		{
			AssertMappingExportAIRAEU_UpdateNumberNotInSequence("Test11_input.xml", "Test11_output.xml");
		}

		void AssertMappingExportSequenceNumber(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return(testDate);

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "ManifestNumber")).Return("MAN0000057");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D|20170929|0026E", "@referenceType", "OriginalIDT")).Return("198801949D|20170928|0055");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D|20170928|0055E", "@referenceType", "UpdateNumber")).Return("2");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "UPEUPE001", "PRET1.PRET001", "198801949D|20170928|0055E", "1", "UpdateNumber"));
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "406 23HKG 06172V562FSZJBXMAN0000057198801949D|20170928|0055E", "@referenceType", "CST")).Return("00002");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "406 23HKG 06172V562FSZJBXMAN000005700002E", "@referenceType", "CNRF")).Return("0008321");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "406 23HKG 06172V562FSZJBXMAN000005700002E", "AIRERR", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "406 23HKG 06172V562FSZJBXMAN0000057198801949D|20170928|0055E", "", "CST")).Repeat.Any();

			// Test7 specific, because its ERR3.1 = E23
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "UPEUPE001", "PRET1.PRET001", "198801949D|20170928|0055E", "001", "UpdateNumber")).Repeat.Any();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		void AssertMappingExportAIRAED(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return(testDate);

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "ManifestNumber")).Return("MAN0000057");

			// Test2 specific
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "406 23HKG 06172V562FSZJBXMAN0000057198801949D|20170929|0026E", "@referenceType", "CST")).Return("12345").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "406 23HKG 06172V562FSZJBXMAN000005712345E", "@referenceType", "CNRF")).Return("0008321").Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "406 23HKG 06172V562FSZJBXMAN000005712345E", "AIRERR", "CNRF")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "406 23HKG 06172V562FSZJBXMAN0000057198801949D|20170929|0026E", "", "CST")).Repeat.Any();

			// Test6 specific
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "MAWB")).Return("MAWB1").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "HAWB")).Return("HAWB1|HAWB2").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB1MAN0000057198801949D|20170929|0026E", "@referenceType", "CST")).Return("12346").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB2MAN0000057198801949D|20170929|0026E", "@referenceType", "CST")).Return("12347").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB1MAN000005712346E", "@referenceType", "CNRF")).Return("0000167|0000268|0000369|0000470|0000571").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB2MAN000005712347E", "@referenceType", "CNRF")).Return("000012|000023").Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAWB1HAWB1MAN000005712346E", "AIRERR", "CNRF")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAWB1HAWB2MAN000005712347E", "AIRERR", "CNRF")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAWB1HAWB1MAN0000057198801949D|20170929|0026E", "", "CST")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAWB1HAWB2MAN0000057198801949D|20170929|0026E", "", "CST")).Repeat.Any();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);

			mockCodeMapper.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		void AssertMappingExportAIRAED_SER(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("WTLDSGSGC");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SGCustomsTest");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return(testDate);

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "ManifestNumber")).Return("MAN0000192");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "363636773BILL20171127H1MAN0000192198801949D|20170929|0026E", "@referenceType", "CST")).Return("00716");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "363636773BILL20171127H1MAN000019200716E", "@referenceType", "CNRF")).Return("000013|000024|000035|000046");

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "SGCustomsTest", "WTLDSGSGC", "363636773BILL20171127H1MAN000019200716E", "AIRERR", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "SGCustomsTest", "WTLDSGSGC", "363636773BILL20171127H1MAN0000192198801949D|20170929|0026E", "", "CST"));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		void AssertMappingExportAIRAED_SERWithThreeCSTBlocks(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("WTLDSGSGC");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SGCustomsTest");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return(testDate);

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "ManifestNumber")).Return("MAN0000192");

			// Only CNRF that contains ERR4.3 = 00078 will be in output, the corresponding CST is 00717 so this will be removed from the CSTs.
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "63636555551MYBADBILLMAN0000192198801949D|20170929|0026E", "@referenceType", "CST")).Return("00716|00717|00718");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "63636555551MYBADBILLMAN000019200716E", "@referenceType", "CNRF")).Return("000011|000022");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "63636555551MYBADBILLMAN000019200717E", "@referenceType", "CNRF")).Return("0005151|0007879|0008182");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "63636555551MYBADBILLMAN000019200718E", "@referenceType", "CNRF")).Return("00101102|00102103|00103104|00104105");

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "SGCustomsTest", "WTLDSGSGC", "63636555551MYBADBILLMAN000019200717E", "AIRERR", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "SGCustomsTest", "WTLDSGSGC", "63636555551MYBADBILLMAN0000192198801949D|20170929|0026E", "00716|00718", "CST"));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		void AssertMappingExportAIRAED_SERWithThreeCSTBlocksAndTwoHawbs(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("WTLDSGSGC");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SGCustomsTest");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return(testDate);

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "ManifestNumber")).Return("MAN0000192");

			// ERR #1 and #2 has the same HAWB
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "63636555551HJFGJHFMAN0000192198801949D|20170929|0026E", "@referenceType", "CST")).Return("00716|00717|00718|00719");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "63636555551HJFGJHFMAN000019200716E", "@referenceType", "CNRF")).Return("000011|000033").Repeat.Twice();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "63636555551HJFGJHFMAN000019200717E", "@referenceType", "CNRF")).Return("0005151|0005252|0005353|0005454").Repeat.Twice();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "63636555551HJFGJHFMAN000019200718E", "@referenceType", "CNRF")).Return("00101102").Repeat.Twice();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "63636555551HJFGJHFMAN000019200719E", "@referenceType", "CNRF")).Return("5829108").Repeat.Twice();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "SGCustomsTest", "WTLDSGSGC", "63636555551HJFGJHFMAN000019200716E", "AIRERR", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "SGCustomsTest", "WTLDSGSGC", "63636555551HJFGJHFMAN000019200719E", "AIRERR", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "SGCustomsTest", "WTLDSGSGC", "63636555551HJFGJHFMAN0000192198801949D|20170929|0026E", "00717|00718", "CST"));

			// ERR #3 has another HAWB
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "63636555551FJKHDSHGJFHJKMAN0000192198801949D|20170929|0026E", "@referenceType", "CST")).Return("00716|00717|00718");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "63636555551FJKHDSHGJFHJKMAN000019200716E", "@referenceType", "CNRF")).Return("000011|000033");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "63636555551FJKHDSHGJFHJKMAN000019200717E", "@referenceType", "CNRF")).Return("0005151|0005252");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "63636555551FJKHDSHGJFHJKMAN000019200718E", "@referenceType", "CNRF")).Return("00101102|00102103|00103104");

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "SGCustomsTest", "WTLDSGSGC", "63636555551FJKHDSHGJFHJKMAN000019200716E", "AIRERR", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "SGCustomsTest", "WTLDSGSGC", "63636555551FJKHDSHGJFHJKMAN0000192198801949D|20170929|0026E", "00717|00718", "CST"));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		void AssertMappingExportAIRAEU(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return(testDate);

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "ManifestNumber")).Return("MAN0000057");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D|20170929|0026E", "@referenceType", "OriginalIDT")).Return("198801949D|20170928|0055");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D|20170928|0055E", "@referenceType", "UpdateNumber")).Return("002");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "UPEUPE001", "PRET1.PRET001", "198801949D|20170928|0055E", "1", "UpdateNumber"));

			// Test5 specific
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "MAWB")).Return("MAWB1").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "HAWB")).Return("HAWB1|HAWB2").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB1MAN0000057198801949D|20170928|0055E", "@referenceType", "CST")).Return("12345").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB2MAN0000057198801949D|20170928|0055E", "@referenceType", "CST")).Return("12346").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB1MAN000005712345E", "@referenceType", "CNRF")).Return("0000185|0000291|0000372").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB2MAN000005712346E", "@referenceType", "CNRF")).Return("000012").Repeat.Any();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAWB1HAWB1MAN000005712345E", "AIRERR", "CNRF")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAWB1HAWB1MAN0000057198801949D|20170928|0055E", "", "CST")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAWB1HAWB2MAN000005712346E", "AIRERR", "CNRF")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAWB1HAWB2MAN0000057198801949D|20170928|0055E", "", "CST")).Repeat.Any();

			// Test9 specific
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "406 23HKG 06172V562FSZJBXMAN0000057198801949D|20170928|0055E", "@referenceType", "CST")).Return("00002").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "406 23HKG 06172V562FSZJBXMAN000005700002E", "@referenceType", "CNRF")).Return("0008321").Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "406 23HKG 06172V562FSZJBXMAN000005700002E", "AIRERR", "CNRF")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "406 23HKG 06172V562FSZJBXMAN0000057198801949D|20170928|0055E", "", "CST")).Repeat.Any();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		void AssertMappingExportAIRAEU_UpdateNumberNotInSequence(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return(testDate);

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "ManifestNumber")).Return("MAN0000057");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D|20170929|0026E", "@referenceType", "OriginalIDT")).Return("198801949D|20170928|0055");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D|20170928|0055E", "@referenceType", "UpdateNumber")).Return("002");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "UPEUPE001", "PRET1.PRET001", "198801949D|20170928|0055E", "1", "UpdateNumber"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "UPEUPE001", "PRET1.PRET001", "198801949D|20170928|0055E", "001", "UpdateNumber"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "MAWB")).Return("MAWB1");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "HAWB")).Return("HAWB1");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB1MAN0000057198801949D|20170928|0055E", "@referenceType", "CST")).Return("12345");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB1MAN000005712345E", "@referenceType", "CNRF")).Return("0000185|0000291|0000372");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAWB1HAWB1MAN000005712345E", "AIRERR", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAWB1HAWB1MAN0000057198801949D|20170928|0055E", "", "CST"));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Export_ErrorCodeE00ShouldBeIgnored()
		{
			AssertMapping_Export_ErrorCodeShouldBeIgnored("Test4", true);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Export_ErrorCodeE12ShouldBeIgnored()
		{
			AssertMapping_Export_ErrorCodeShouldBeIgnored("Test14", false);
		}

		void AssertMapping_Export_ErrorCodeShouldBeIgnored(string fileName, bool isAirAEU)
		{
			var input = filePath + $"{ fileName }_input.xml";
			var expectedOutput = filePath + $"{ fileName }_output.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return(testDate);

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "ManifestNumber")).Return("MAN0000057");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything)).Repeat.Never();

			if (isAirAEU)
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D|20170929|0026E", "@referenceType", "OriginalIDT")).Return("198801949D|20170928|0055");
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "406 23HKG 06172V562FSZJBXMAN0000057198801949D|20170928|0055E", "@referenceType", "CST")).Return("12345");
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "406 23HKG 06172V562FSZJBXMAN000005712345E", "@referenceType", "CNRF")).Return("0008321");
			}
			else
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "406 23HKG 06172V562FSZJBXMAN0000057198801949D|20170929|0026E", "@referenceType", "CST")).Return("12345");
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "406 23HKG 06172V562FSZJBXMAN000005712345E", "@referenceType", "CNRF")).Return("0008321").Repeat.Twice();
			}


			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D|20170928|0055E", "@referenceType", "UpdateNumber")).Return("").Repeat.Never();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries(
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything)).Repeat.Never();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);

			mockCodeMapper.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Export_MultipleERRsWithDifferentHAWBAndErrorCode_ERR43IsSERFallbackToCST()
		{
			var input = filePath + "Test12_input.xml";
			var expectedOutput = filePath + "Test12_output.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SGCustomsTest");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2022-01-28T05:53:29");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "SGCustomsTest", "@ST_ID", "SGCMSG", "@value", "198801949D202201282131E", "@referenceType", "ManifestNumber")).Return("MAN0001064");

			// ERR #1 #2 #3 has the same HAWB
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "SGCustomsTest", "@ST_ID", "SGCMSG", "@value", "93828777770DSGFFSDMAN0001064198801949D|20220128|2131E", "@referenceType", "CST")).Return("12345");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "SGCustomsTest", "@ST_ID", "SGCMSG", "@value", "93828777770DSGFFSDMAN000106412345E", "@referenceType", "CNRF")).Return("000014|000025|000036").Repeat.Times(3);
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "SGCustomsTest", "93828777770DSGFFSDMAN000106412345E", "AIRERR", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "SGCustomsTest", "93828777770DSGFFSDMAN0001064198801949D|20220128|2131E", "", "CST"));

			// ERR #4 #5 has another HAWB, ERR #5 finds SER subscription, ERR #4 falls back to CST subscription
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "SGCustomsTest", "@ST_ID", "SGCMSG", "@value", "93828777770FDSSGDSGMAN0001064198801949D|20220128|2131E", "@referenceType", "CST")).Return("00001|12346");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "SGCustomsTest", "@ST_ID", "SGCMSG", "@value", "93828777770FDSSGDSGMAN000106400001E", "@referenceType", "CNRF")).Return("000068|000079").Repeat.Times(3);
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "SGCustomsTest", "@ST_ID", "SGCMSG", "@value", "93828777770FDSSGDSGMAN000106412346E", "@referenceType", "CNRF")).Return("00103108|00104109").Repeat.Twice();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "SGCustomsTest", "93828777770FDSSGDSGMAN000106412346E", "AIRERR", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "SGCustomsTest", "93828777770FDSSGDSGMAN000106400001E", "AIRERR", "CNRF"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValueWithRetries("SGCMSG", "UPEUPE001", "SGCustomsTest", "93828777770FDSSGDSGMAN0001064198801949D|20220128|2131E", "", "CST"));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);

			mockCodeMapper.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Import_ErrorDescription()
		{
			var input = filePath + "Test19_input.xml";
			var expectedOutput = filePath + "Test19_output.xml";
			var subscriptionInput = ReadResource("AIRERR2UEvent.TestFiles.Test19_Subscription_Input.xml");
			var subscriptionOutput = ReadResource("AIRERR2UEvent.TestFiles.Test19_Subscription_Output.xml");

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-06T12:00:00").Repeat.AtLeastOnce();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201908218297I", "@referenceType", "ManifestNumber")).Return("MAN0053335");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAN0053335I")).Return(subscriptionInput);
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAN0053335I", subscriptionOutput));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }

			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);

			mockCodeMapper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Import_ErrorCodeE12ShouldBeIgnored()
		{
			var input = filePath + "Test16_input.xml";
			var expectedOutput = filePath + "Test16_output.xml";
			var subscriptionInput = ReadResource("AIRERR2UEvent.TestFiles.Test16_Subscription_Input.xml");
			var subscriptionOutput = ReadResource("AIRERR2UEvent.TestFiles.Test16_Subscription_Output.xml");

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSubscriptionHelper = MockRepository.GeneratePartialMock<SGCustomsSubscriptionHelper>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2017-09-06T12:00:00").Repeat.AtLeastOnce();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201909301430I", "@referenceType", "ManifestNumber")).Return("MAN0000845");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAN0000845I")).Return(subscriptionInput);
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAN0000845I", subscriptionOutput)).Repeat.Never();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);

			mockCodeMapper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();

			mockSubscriptionHelper.AssertWasCalled(x => x.SubscribeHistoryOrder(
				Arg<XPathNavigator>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything), x => x.Repeat.Once());

			mockSubscriptionHelper.AssertWasCalled(x => x.SubscribeEvent(
				Arg<XPathNavigator>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything), x => x.Repeat.AtLeastOnce());

			mockSubscriptionHelper.AssertWasCalled(x => x.SubscribeInfo(
				Arg<XPathNavigator>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything), x => x.Repeat.AtLeastOnce());

			mockSubscriptionHelper.AssertWasCalled(x => x.DeletePackLinesAndHouseByIDTAndHAWB(
				Arg<XPathNavigator>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything), x => x.Repeat.AtLeastOnce());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_Import_ErrorCodeE00ShouldBeIgnored()
		{
			var input = filePath + "Test3_input.xml";
			var expectedOutput = filePath + "Test3_output.xml";
			var subscriptionInput = ReadResource("AIRERR2UEvent.TestFiles.Test3_Subscription_Input.xml");
			var subscriptionOutput = ReadResource("AIRERR2UEvent.TestFiles.Test3_Subscription_Output.xml");

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockSubscriptionHelper = MockRepository.GeneratePartialMock<SGCustomsSubscriptionHelper>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PRET1.PRET001");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("UPEUPE001");
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2021-03-24T05:30:00").Repeat.AtLeastOnce();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D202103247301I", "@referenceType", "ManifestNumber")).Return("MAN0000781");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAN0000781I")).Return(subscriptionInput);
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SGCMSG", "UPEUPE001", "PRET1.PRET001", "MAN0000781I", subscriptionOutput)).Repeat.Never();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);

			mockCodeMapper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();

			mockSubscriptionHelper.AssertWasCalled(x => x.SubscribeHistoryOrder(
				Arg<XPathNavigator>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything), x => x.Repeat.Once());

			mockSubscriptionHelper.AssertWasCalled(x => x.SubscribeEvent(
				Arg<XPathNavigator>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything), x => x.Repeat.AtLeastOnce());

			mockSubscriptionHelper.AssertWasCalled(x => x.SubscribeInfo(
				Arg<XPathNavigator>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything), x => x.Repeat.AtLeastOnce());

			mockSubscriptionHelper.AssertWasCalled(x => x.DeletePackLinesAndHouseByIDTAndHAWB(
				Arg<XPathNavigator>.Is.Anything,
				Arg<string>.Is.Anything,
				Arg<string>.Is.Anything), x => x.Repeat.AtLeastOnce());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAIRERR2UEvent_EmptySubscriber()
		{
			var input = filePath + "Test1_input.xml";
			var expectedOutput = filePath + "Test1_output.xml";

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("");


			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};
			try
			{
				var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
				mapTester.ExecuteCompiled<AIRERR2UEvent>(input, expectedOutput);
				Assert.Fail("Should throw exception when recipient or sender is Empty");
			}
			catch (Exception e)
			{
				Assert.AreEqual(e.Message, "Unable to resolve the recipient_ Cannot found subscriber at mapping AIRERR");
			}
		}
	}
}
