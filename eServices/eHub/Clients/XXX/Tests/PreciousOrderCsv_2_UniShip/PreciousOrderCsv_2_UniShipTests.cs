using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.XXX.Transforms.PreciousOrderCsv_2_UniShip;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.XXX.Tests
{
	[TestClass]
	public class PreciousOrderCsv_2_UniShipTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPreciousOrderCsv_2_UniShip_Counter()
		{
			InitialiseCodeMapsTestingContext();

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "PreciousOrderCsv_2_UniShip.TestFiles.Client_Input.xml";
			string expectedFile = "PreciousOrderCsv_2_UniShip.TestFiles.Client_Output.xml";
			mapTester.Execute<PreciousOrderCsv_2_UniShip>(sourceFile, expectedFile);

			sourceFile = "PreciousOrderCsv_2_UniShip.TestFiles.Test_Input.xml";
			expectedFile = "PreciousOrderCsv_2_UniShip.TestFiles.Test_Output.xml";
			mapTester.Execute<PreciousOrderCsv_2_UniShip>(sourceFile, expectedFile);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "XXXUSAOR1_PPO" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "XXXUSAOR1" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Precious Order CSV - Receive OrderManager Orders", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Data Provider/Ordered By" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "EDICUS" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Sending Agent" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "THITAIKHH" });
			
			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
