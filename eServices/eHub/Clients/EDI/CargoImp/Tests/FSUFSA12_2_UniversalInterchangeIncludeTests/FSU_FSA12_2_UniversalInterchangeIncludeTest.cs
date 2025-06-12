using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Transforms.CargoImp.FSU_FSA122UniversalInterchangeInclude;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Clients.EDI.Transforms.Native.FSU_FSA122NativeEventInternal;

namespace CargoWise.eHub.Clients.EDI.Schemas.CargoImp.Tests.FSUFSA12_2_UniversalInterchangeIncludeTest
{
	[TestClass]
	public class FSUFSA12_2_UniversalInterchangeIncludeTests
	{
		public FSUFSA12_2_UniversalInterchangeIncludeTests()
		{
			InitialiseCodeMapsTestingContext();
		}

		TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSU_RCSOnly()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "eHubAirService" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "FSU_FSA to UniversalEvent", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Event Codes", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Event Type" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "RCS" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "HNV" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU12_RCSOnly.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU12_RCSOnly_UniversalInterchangeInclude.xml";

			var expectedStream = TestHelper.GetEmbeddedResource(expectedFile);
			HelperCSharpScripts helper = new HelperCSharpScripts();

			var eventYear = Int32.Parse(helper.FormatDateTime("20", "NOV", "").Substring(7, 4));
			var eventTime1 = XmlConvert.ToString(new DateTime(eventYear, 11, 20, 11, 13, 0), XmlDateTimeSerializationMode.Unspecified);
			var formattedResource = string.Format(new StreamReader(expectedStream).ReadToEnd(), eventTime1);

			using (var formattedStream = new MemoryStream(Encoding.ASCII.GetBytes(formattedResource)))
			{
				mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, formattedStream);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSU_ARR_ArriveAtNextday()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_ARR_ArriveAtNextday.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_ARR_ArriveAtNextday_UniversalInterchangeInclude.xml";
			mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSU_ARR_Fallback_EventTime()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_ARR_Fallback_EventTime.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_ARR_Fallback_EventTime_UniversalInterchangeInclude.xml";
			mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSU_RCF()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_RCF_NoFlight_NoTOD.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_RCF_NoFlight_NoTOD_UniversalInterchangeInclude.xml";
			mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSU_RCF_FallbackTime()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_RCF_FallbackTime.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_RCF_FallbackTime_UniversalInterchangeInclude.xml";
			mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSU_RCF_DefaultTime()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_RCF_DefaultTime.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_RCF_DefaultTime_UniversalInterchangeInclude.xml";
			mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSU_BKD_OSI()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "eHubAirService" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "FSU_FSA to UniversalEvent", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Event Codes", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Event Type" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "BKD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "BKC" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_BKD_OSI.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_BKD_OSI_UniversalInterchangeInclude.xml";
			mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSU_MANOnly()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "eHubAirService" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "FSU_FSA to UniversalEvent", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Event Codes", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Event Type" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "MAN" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "STU" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_MANOnly.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_MANOnly_UniversalInterchangeInclude.xml";
			mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSU12_MultipleCarrierAndDuplicateStatusRecords()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "eHubAirService" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "FSU_FSA to UniversalEvent", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Event Codes", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Event Type" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "RCT" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "RCV" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU12_MultipleCarrierAndDuplicateStatusRecords.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU12_MultipleCarrierAndDuplicateStatusRecords_UniversalInterchangeInclude.xml";
			mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSU12_OneCarrierAndAllStatusRecords()
		{
			// Add client infromation
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "eHubAirService" });

			// Create transformation
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "FSU_FSA to UniversalEvent", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0] });

			// Add code set for discrepancy codes
			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Discrepancy Description", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Discrepancy Description" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "FDAW" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Found Air Waybill" });

			// Add code set for Event Code
			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Event Codes", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });

			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Event Type" });

			// Add the code and value pairs
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "DIS" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "STU" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "AWD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "STU" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "NFD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "STU" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "BKD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "BKC" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "MAN" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "STU" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 6, CK_Key1Value = "CCD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SCM" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 7, CK_Key1Value = "DLV" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "HNV" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 8, CK_Key1Value = "RCS" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "HNV" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 9, CK_Key1Value = "CRC" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "STU" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 10, CK_Key1Value = "RCF" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "FUL" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 11, CK_Key1Value = "PRE" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "STU" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 12, CK_Key1Value = "TFD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "HNV" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 13, CK_Key1Value = "TRM" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "STU" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 14, CK_Key1Value = "RCT" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "RCV" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 15, CK_Key1Value = "DDL" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "DLV" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 16, CK_Key1Value = "TGC" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SHL" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 17, CK_Key1Value = "AWR" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "STU" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 18, CK_Key1Value = "FOH" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "STU" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU12_OneCarrierAndAllStatusRecords.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU12_OneCarrierAndAllStatusRecords_UniversalInterchangeInclude.xml";

			var expectedStream = TestHelper.GetEmbeddedResource(expectedFile);
			HelperCSharpScripts helper = new HelperCSharpScripts();

			var eventYear = Int32.Parse(helper.FormatDateTime("20", "NOV", "").Substring(7, 4));
			var eventTime1 = XmlConvert.ToString(new DateTime(eventYear, 11, 20, 11, 13, 0), XmlDateTimeSerializationMode.Unspecified);
			var eventTime2 = XmlConvert.ToString(new DateTime(eventYear, 11, 20, 11, 14, 0), XmlDateTimeSerializationMode.Unspecified);
			var eventTime3 = XmlConvert.ToString(new DateTime(eventYear, 11, 18, 12, 53, 0), XmlDateTimeSerializationMode.Unspecified);
			var formattedResource = string.Format(new StreamReader(expectedStream).ReadToEnd(), eventTime1, eventTime2, eventTime3);

			using (var formattedStream = new MemoryStream(Encoding.ASCII.GetBytes(formattedResource)))
			{
				mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, formattedStream);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSUTRM()
		{
            CodeMapsTestingContext ctx = new CodeMapsTestingContext();
            ctx.eHubClients.Add(new eHubClient { CC_ID = "eHubAirService" });
            ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "FSU_FSA to UniversalEvent", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0] });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Event Codes", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Event Type" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "RCS" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "HNV" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "BKD" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "BKC" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "TRM" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "STU" });

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSUTRM.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSUTRM_UniversalInterchangeInclude.xml";

            var expectedStream = TestHelper.GetEmbeddedResource(expectedFile);
            HelperCSharpScripts helper = new HelperCSharpScripts();

            var eventYear = helper.FormatDateTime("15", "FEB", "").Substring(7,4);
            var formattedResource = string.Format(new StreamReader(expectedStream).ReadToEnd(), eventYear);

            using (var formattedStream = new MemoryStream(Encoding.ASCII.GetBytes(formattedResource)))
            {
                mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, formattedStream);
            }

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_DoNotMapAdditionalFieldsToUpdateCollectionWhenPartial()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU12_ShipmentDescriptionCodeIsPartial.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU12_ShipmentDescriptionCodeIsPartial_UniversalInterchangeInclude.xml";
			mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSUDEPDates()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "eHubAirService" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "FSU_FSA to UniversalEvent", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Event Codes", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Event Type" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "MAN" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "STU" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_DEPDates.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_DEPDates_UniversalInterchangeInclude.xml";
			mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSUDEP_Full()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_DEP_Full.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_DEP_Full_UniversalInterchangeInclude.xml";
			mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSU12_OSI_Only()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_OSI.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_OSI_UniversalInterchangeInclude.xml";
			mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA122UniversalInterchangeInclude_FSU_ARR_DEP_RCF_Partial()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_ARR_DEP_RCF_Partial.xml";
			string expectedFile = "FSUFSA12_2_UniversalInterchangeIncludeTests.TestFiles.FSU_ARR_DEP_RCF_Partial_UniversalInterchangeInclude.xml";
			mapTester.Execute<FSU_FSA12_2_UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		ICompare Comparer
		{
			get
			{
				if (comparer == null)
				{
					List<string> exclusionXpaths = GetExclusionXpathsList();
					comparer = new ExcludingComparer(exclusionXpaths);
				}

				return comparer;
			}
		}

		ICompare comparer;

		private static List<string> GetExclusionXpathsList()
		{
			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='TRF']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='STU']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='OCR']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='MAN']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='BKC']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='ARV']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='DEP']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='CAD']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='ADR']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='CAV']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='RLS']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='CCD']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='CRC']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='DLV']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='TGC']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='FOH']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='FLO']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='FUL']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='RCV']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='SHL']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event' and *[local-name()='EventType']/text()='SCM']/*[local-name()='EventTime']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='EventParameters']/*[local-name()='FlightDate']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='ContextCollection']/*[local-name()='Context' and *[local-name()='Type']/text()='TimeOfDeparture']/*[local-name()='Value']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='ContextCollection']/*[local-name()='Context' and *[local-name()='Type']/text()='TimeOfArrival']/*[local-name()='Value']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='ContextCollection']/*[local-name()='Context' and *[local-name()='Type']/text()='FlightDate']/*[local-name()='Value']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='AdditionalFieldsToUpdateCollection']/*[local-name()='AdditionalFieldsToUpdate' and *[local-name()='Type']/text()='JobConsolTransport.JW_ATD']/*[local-name()='Value']");
			exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='AdditionalFieldsToUpdateCollection']/*[local-name()='AdditionalFieldsToUpdate' and *[local-name()='Type']/text()='JobConsolTransport.JW_ATA']/*[local-name()='Value']");
			return exclusionXpaths;
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "XXCCS", IATACode = "CCS" });
			ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "XXCDG", IATACode = "CDG" });
			ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "XXMRU", IATACode = "MRU" });
			ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "XXNRT", IATACode = "NRT" });
			ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "XXFRA", IATACode = "FRA" });
			ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "XXORD", IATACode = "ORD" });
			ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "XXSTR", IATACode = "STR" });
			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}

	}
}
