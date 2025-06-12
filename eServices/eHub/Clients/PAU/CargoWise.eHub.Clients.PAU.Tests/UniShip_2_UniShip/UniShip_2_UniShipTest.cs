using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.PAU.Transforms.UniShip_2_UniShip;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.PAU.Tests
{
	[TestClass]
    public class UniShip_2_UniShipTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniShip_2_UniShip()
		{
			InitialiseCodeMapsTestingContext();

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "UniShip_2_UniShip.TestFiles.Input.xml";
            string expectedFile = "UniShip_2_UniShip.TestFiles.Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_UniShip>(sourceFile, expectedFile);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
            ctx.eHubClients.Add(new eHubClient { CC_ID = "PAUPAUMEL" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "PAUSYDSYD" });
            ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "E2E - Shipments to Declaration", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Declaration Branch" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "JAY" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
