using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Transforms.CargoImp.FMA_FNA2UniversalEventInternal;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Clients.EDI.Schemas.CargoImp.Tests.FMA_FNA2UniversalEventInternalTests
{
	[TestClass]
	public class FMA_FNA2UniversalEventInternalTests
	{

		public FMA_FNA2UniversalEventInternalTests()
		{
			InitialiseCodeMapsTestingContext();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FMAFHLLong()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA_XSDTests.TestFiles.FMAFHLLong.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FMAFHLLong_NativeEventInternal.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FMAFWBLong()
        {
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA_XSDTests.TestFiles.FMAFWBLong.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FMAFWBLong_NativeEventInternal.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA2UniversalEventInternal_FMAFWBWithoutOriginDestination()
        {
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
            {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
            };

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

            string sourceFile = "FMA_FNA_XSDTests.TestFiles.FMAFWBWithoutOriginDestination.xml";
            string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FMAFWBWithoutOriginDestination_NativeEventInternal.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FNAFHLLong()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA_XSDTests.TestFiles.FNAFHLLong.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAFHLLong_NativeEventInternal.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FNAFWBLong()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA_XSDTests.TestFiles.FNAFWBLong.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAFWBLong_NativeEventInternal.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FMAISAC()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA_XSDTests.TestFiles.FMAISAC.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FMAISAC_NativeEventInternal.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FNAISAC()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA_XSDTests.TestFiles.FNAISAC.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAISAC_NativeEventInternal.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FMA()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FMA_inputfile.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FMA_FMA_FNA2UniversalEventInternal_output.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FNA_WithNoRecognizedReason()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_00_inputfile.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_00_FMA_FNA2UniversalEventInternal_output.xml";

			mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FNA_WithRecognizedReason_01()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_01_inputfile.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_01_FMA_FNA2UniversalEventInternal_output.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FNA_WithRecognizedReason_02()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_02_inputfile.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_02_FMA_FNA2UniversalEventInternal_output.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FNA_WithRecognizedReason_03()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_03_inputfile.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_03_FMA_FNA2UniversalEventInternal_output.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FNA_WithRecognizedReason_04()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_04_inputfile.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_04_FMA_FNA2UniversalEventInternal_output.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FNA_WithRecognizedReason_05()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};


            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_05_inputfile.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_05_FMA_FNA2UniversalEventInternal_output.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FNA_WithRecognizedReason_06()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};


			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);

			string sourceFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_06_inputfile.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_06_FMA_FNA2UniversalEventInternal_output.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA2UniversalEventInternal_FNA_EventType_IRJ()
		{
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10").Repeat.Any();
            var extensionObjects = new Dictionary<string, object>()
			{
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer, extensionObjects);
			string sourceFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_IRJ_input.xml";
			string expectedFile = "FMA_FNA2UniversalEventInternalTests.TestFiles.FNAWithDetails.FNA_IRJ_output.xml";

            mapTester.ExecuteCompiled<FMA_FNA2UniversalEventInternal>(sourceFile, expectedFile);
		}

		ICompare Comparer
		{
			get
			{
				if (comparer == null)
				{
					List<string> exclusionXpaths = new List<string>();
					exclusionXpaths.Add("/*[local-name()='UniversalEventsInternal']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='EventTime']");
					comparer = new ExcludingComparer(exclusionXpaths);
				}

				return comparer;
			}
		}

		ICompare comparer;

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "eHubAirService" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "eHubAirService" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "FMA_FNA to UniversalEvent", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "ACK Message", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Reason for Rejection" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "ZCSTXXH%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "You are not a registered EDI Customer with Continental Airlines. Please Contact COA Sales Person and request EDI Connectivity." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "MESSAGE NOT FORWARDED - AIRLINE DOES NOT ACCEPT F%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Airline does accept FHL only for following airports: SYD, MEL, BNE, PER, ADL, DRW, CNS, OOL, SHA, PVG and LAX." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "CF01 RECIPIENT NOT ABLE TO HANDLE THIS MESSAGE OR%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Airline is currently not supported by the service provider." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "GLS USER ID NOT FOUND%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Client is not registered in GLS/HK system, please Contact Cargowise." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "AWB REJECTED FWB0015%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "FWB was already created on the airline system and locked.  Once the FWB gets locked, sending another FWB causes FNA." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 6, CK_Key1Value = "MESSAGE NOT FORWARDED - AIRLINE HAS REQUESTED NOT%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Airline is currently not supported by the service provider." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 7, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}

		string GetEmbeddedResourceFile(string resourceName)
		{
			string tempFileName = Path.GetTempFileName();
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			using (Stream reader = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName))
			{
				using (Stream writer = new FileStream(tempFileName, FileMode.Create))
				{
					reader.CopyTo(writer);
					writer.Flush();
				}
			}
			return tempFileName;
		}
	}
}
