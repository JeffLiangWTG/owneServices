using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.AWL.Transforms.GenentechPartsPsv_2_NativeProduct;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.AWL.Tests
{
	[TestClass]
	public class GenentechPartsPsv_2_NativeProductTests
	{
        MapTester mapTester;

        [TestInitialize]
        public void TestSetup()
        {
            InitialiseCodeMapsTestingContext();

            mapTester = new MapTester(Assembly.GetExecutingAssembly());
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGenentechPartsPsv_2_NativeProduct()
        {
            string sourceFile = "GenentechPartsPsv_2_NativeProduct.TestFiles.Input.xml";
            string expectedFile = "GenentechPartsPsv_2_NativeProduct.TestFiles.Output.xml";
            mapTester.ExecuteCompiled<GenentechPartsPsv_2_NativeProduct>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGenentechPartsPsv_2_NativeProduct_FDA()
        {
            string sourceFile = "GenentechPartsPsv_2_NativeProduct.TestFiles.FDA_Input.xml";
            string expectedFile = "GenentechPartsPsv_2_NativeProduct.TestFiles.FDA_Output.xml";
            mapTester.ExecuteCompiled<GenentechPartsPsv_2_NativeProduct>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGenentechPartsPsv_2_NativeProduct_AllFDA()
        {
            string sourceFile = "GenentechPartsPsv_2_NativeProduct.TestFiles.AllFDA_Input.xml";
            string expectedFile = "GenentechPartsPsv_2_NativeProduct.TestFiles.AllFDA_Output.xml";
            mapTester.ExecuteCompiled<GenentechPartsPsv_2_NativeProduct>(sourceFile, expectedFile);
        }

        private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "AWLORDORD_GPR" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "AWLORDORD" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Genentech Product TXT: Receive Products", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = null });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Stock Unit" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "UNT" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "DG Subs", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "UNDG Substance Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "1090", CK_Key2Value = "ACETONE SOLUTIONS", CK_Key3Value = "%", CK_Key4Value = "%", CK_Key5Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "1090A" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%", CK_Key2Value = "%", CK_Key3Value = "%", CK_Key4Value = "%", CK_Key5Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Unit of Measurement", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Output Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
