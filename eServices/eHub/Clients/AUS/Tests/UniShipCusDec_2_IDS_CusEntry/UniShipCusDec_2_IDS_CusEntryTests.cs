using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.AUS.Transforms.UniShipCusDec_2_IDS_CusEntry;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.AUS.Tests
{
	[TestClass]
	public class UniShipCusDec_2_IDS_CusEntryTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShipCusDec_2_IDS_CusEntry()
		{
			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='ProcessedDate']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniShipCusDec_2_IDS_CusEntry.TestFiles.N10_N20_Input.xml";
			string expectedFile = "UniShipCusDec_2_IDS_CusEntry.TestFiles.N10_N20_Output.xml";
			mapTester.ExecuteCompiled<UniShipCusDec_2_IDS_CusEntry>(sourceFile, expectedFile);

			sourceFile = "UniShipCusDec_2_IDS_CusEntry.TestFiles.N30_Input.xml";
			expectedFile = "UniShipCusDec_2_IDS_CusEntry.TestFiles.N30_Output.xml";
			mapTester.ExecuteCompiled<UniShipCusDec_2_IDS_CusEntry>(sourceFile, expectedFile);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "AUSSYDSYD" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "AUSSYDSYD_ICE" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "IDS Customs Entry 1 - Send Customs Entry Data", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Invoice Number" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "EX-BOND" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
