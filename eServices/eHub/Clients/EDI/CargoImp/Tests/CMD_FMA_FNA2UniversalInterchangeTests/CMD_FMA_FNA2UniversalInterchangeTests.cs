using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Transforms.CargoImp.CMD_FMA_FNA2UniversalInterchange;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Clients.EDI.Schemas.CargoImp.Tests.CMD_FMA_FNA2UniversalInterchangeTests
{
	[TestClass]
	public class CMD_FMA_FNA2UniversalInterchangeTests
	{

		public CMD_FMA_FNA2UniversalInterchangeTests()
		{
			InitialiseCodeMapsTestingContext();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void CMD_FMA_FNA2UniversalInterchange_FNACMDLong()
        {
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CCN");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("WTLDSGSGC");
			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor }
			};
			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "CMD_FMA_FNA_XSDTests.TestFiles.FNACMDLong.xml";
            string expectedFile = "CMD_FMA_FNA2UniversalInterchangeTests.TestFiles.FNACMDLong_UniversalInterchange.xml";

            mapTester.ExecuteCompiled<CMD_FMA_FNA2UniversalInterchange>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void CMD_FMA_FNA2UniversalInterchange_FMACMDShort()
        {
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CCN");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("WTLDSGSGC");
			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor }
			};
			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "CMD_FMA_FNA_XSDTests.TestFiles.FMACMDShort.xml";
            string expectedFile = "CMD_FMA_FNA2UniversalInterchangeTests.TestFiles.FMACMDShort_UniversalInterchange.xml";

            mapTester.ExecuteCompiled<CMD_FMA_FNA2UniversalInterchange>(sourceFile, expectedFile);
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
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "CMD_FMA_FNA to UniversalInterchange", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

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
