using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.BizTalk.UnitTestFX;
using System.Reflection;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Clients.DFD.Transforms.EDIWhsRelease_2_LeCreusetORDDEP;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Clients.DFD.Tests
{
    [TestClass]
    public class EDIWhsRelease_2_LeCreusetORDDEPTest
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEDIWhsRelease_2_LeCreusetORDDEP()
        {
            InitialiseCodeMapsTestingContext();

			var ctx = new TestingMessageContext();
			var ca = new ContextAccessor();

            List<string> exclusionXpaths = new List<string>();
            exclusionXpaths.Add("/*[local-name()='LeCreusetWarehouseReleaseORDDEP']/*[local-name()='WarehouseRelease']/*[local-name()='UNB-051']/*[local-name()='DateTimeOfTransfer']");
            ICompare comparer = new ExcludingComparer(exclusionXpaths);
            
            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			ctx.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "DFDSYDPRD_CWR");
			ca.SetTestingMessageContext(ctx);
            string sourceFile = "WarehouseRelease2LeCreusetORDDEP.TestFiles.WhsDocketsInternal_Input.XML";
            string expectedFile = "WarehouseRelease2LeCreusetORDDEP.TestFiles.LeCreusetORDDEP_Output.xml";
            mapTester.Execute<EDIWhsRelease_2_LeCreusetORDDEP>(sourceFile, expectedFile);

			ctx.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "DFDAU2PRD_CWR");
			ca.SetTestingMessageContext(ctx);
			sourceFile = "WarehouseRelease2LeCreusetORDDEP.TestFiles.WhsDocketsInternal_Input.XML";
			expectedFile = "WarehouseRelease2LeCreusetORDDEP.TestFiles.LeCreusetORDDEP_Output_AU2.xml";
			mapTester.Execute<EDIWhsRelease_2_LeCreusetORDDEP>(sourceFile, expectedFile);

            sourceFile = "WarehouseRelease2LeCreusetORDDEP.TestFiles.TransRefShort_Input.xml";
            expectedFile = "WarehouseRelease2LeCreusetORDDEP.TestFiles.TransRefShort_Output.xml";
            mapTester.Execute<EDIWhsRelease_2_LeCreusetORDDEP>(sourceFile, expectedFile);
        }

        private static void InitialiseCodeMapsTestingContext()
        {
            CodeMapsTestingContext ctx = new CodeMapsTestingContext();
            ctx.eHubClients.Add(new eHubClient { CC_ID = "DFDSYDPRD" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "DFDSYDPRD_CWR" });
            ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Le Creuset SAP - Export of Warehouse Releases", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Carrier Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Le Creuset Code" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Sender ID" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = null });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "DFDS001" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Receiver ID" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Lecreuset" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 3, CR_Name = "Message Release" });           
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "DFDS001" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 4, CR_Name = "Transport Product" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "112" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 5, CR_Name = "LWS Owner Number" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "40" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 6, CR_Name = "LWS Multi Owner Number" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "00" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 7, CR_Name = "Location Of Articles" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "OK1" });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Package Type", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Le Creuset Code" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "BX" });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Unit Of Quantity", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Le Creuset Code" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PCE" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Whs. Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Le Creuset Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "DFDSYDPRD_CWR", CK_Key2Value = "BRL" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SYM" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "DFDAU2PRD_CWR", CK_Key2Value = "BRL" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "HPK" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 2 });           

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);
        }
    }
}
