using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.GTL.Transforms.EDIConsolsInternal2EDIConsolsWithXmlInterchange;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Client.GTL.Tests
{
    [TestClass]
    public class EDIConsolsInternal2EDIConsolsWithXmlInterchangeTest
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestTransformConsolsInternal2DeclarationInternal()
        {
            InitialiseCodeMapsTestingContext();

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "EDIConsolsInternal2EDIConsolsWithXmlInterchange.TestFiles.ConsolsInternal.xml";
            string expectedFile = "EDIConsolsInternal2EDIConsolsWithXmlInterchange.TestFiles.ConsolsInternalWithXMLInterchange.xml";
            mapTester.Execute<EDIConsolsInternal2EDIConsolsWithXmlInterchange>(sourceFile, expectedFile);

        }

        static void InitialiseCodeMapsTestingContext()
        {
            CodeMapsTestingContext ctx = new CodeMapsTestingContext();
            ctx.eHubClients.Add(new eHubClient { CC_ID = "GTLPRDSYD_FWD" });
            ctx.eHubClients.Add(new eHubClient { CC_ID = "GTLSYDSYD" });
            ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "ediEnterprise - Import Shipment Declaration xml", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Declaration Branch" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SYD" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
        }
    }
}
