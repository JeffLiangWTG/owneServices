using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.AWL.Transforms.UniShip_2_XPO_214;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CargoWise.eHub.Clients.AWL.Tests
{
	[TestClass]
	public class UniShip_2_XPO_214Tests
	{
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniShip_2_XPO_214_AF()
        {
            InitialiseCodeMapsTestingContext();
            
            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "UniShip_2_XPO_214.TestFiles.AF_Input.xml";
            string expectedFile = "UniShip_2_XPO_214.TestFiles.AF_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_XPO_214>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniShip_2_XPO_214_X3()
        {
            InitialiseCodeMapsTestingContext();

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "UniShip_2_XPO_214.TestFiles.X3_Input.xml";
            string expectedFile = "UniShip_2_XPO_214.TestFiles.X3_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_XPO_214>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniShip_2_XPO_214_B6()
        {
            InitialiseCodeMapsTestingContext();

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "UniShip_2_XPO_214.TestFiles.B6_Input.xml";
            string expectedFile = "UniShip_2_XPO_214.TestFiles.B6_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_XPO_214>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniShip_2_XPO_214_D1()
        {
            InitialiseCodeMapsTestingContext();

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "UniShip_2_XPO_214.TestFiles.D1_Input.xml";
            string expectedFile = "UniShip_2_XPO_214.TestFiles.D1_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_XPO_214>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniShip_2_XPO_214_X1()
        {
            InitialiseCodeMapsTestingContext();

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "UniShip_2_XPO_214.TestFiles.X1_Input.xml";
            string expectedFile = "UniShip_2_XPO_214.TestFiles.X1_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_XPO_214>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniShip_2_XPO_214_P1_ATD()
        {
            InitialiseCodeMapsTestingContext();

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "UniShip_2_XPO_214.TestFiles.P1_ATD_Input.xml";
            string expectedFile = "UniShip_2_XPO_214.TestFiles.P1_ATD_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_XPO_214>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniShip_2_XPO_214_P1_ETD()
        {
            InitialiseCodeMapsTestingContext();

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "UniShip_2_XPO_214.TestFiles.P1_ETD_Input.xml";
            string expectedFile = "UniShip_2_XPO_214.TestFiles.P1_ETD_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_XPO_214>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniShip_2_XPO_214_CT()
        {
            InitialiseCodeMapsTestingContext();

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "UniShip_2_XPO_214.TestFiles.CT_Input.xml";
            string expectedFile = "UniShip_2_XPO_214.TestFiles.CT_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_XPO_214>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniShip_2_XPO_214_PA()
        {
            InitialiseCodeMapsTestingContext();

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "UniShip_2_XPO_214.TestFiles.PA_Input.xml";
            string expectedFile = "UniShip_2_XPO_214.TestFiles.PA_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_XPO_214>(sourceFile, expectedFile);
        }

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
            ctx.eHubClients.Add(new eHubClient { CC_ID = "AWLORDORD" });
            ctx.eHubClients.Add(new eHubClient { CC_ID = "AWLORDORD_XP2" });
            ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "XPO 214 - Send Shipment Status", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = null });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "SCAC" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "AIIH" });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Shipment Status", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Shipment Status - AT701" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Unit of Measurement", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "X12 Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "T" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "E" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "G" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "G" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "KG" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "K" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "LB" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "L" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "TN" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "S" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 6, CK_Key1Value = "TL" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "T" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });
            
            var pk_CNNCA = Guid.NewGuid();
            var pk_AUSYD = Guid.NewGuid();
            var pk_USLAX = Guid.NewGuid();
            var pk_NZAKL = Guid.NewGuid();

            ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "CNNCA", StateRef = pk_CNNCA });
            ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "AUSYD", StateRef = pk_AUSYD });
            ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "USLAX", StateRef = pk_USLAX });
            ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "NZAKL", StateRef = pk_NZAKL });

            ctx.eHubStateList.Add(new eHubState { StatePK = pk_CNNCA, Code = "34" });
            ctx.eHubStateList.Add(new eHubState { StatePK = pk_AUSYD, Code = "NSW" });
            ctx.eHubStateList.Add(new eHubState { StatePK = pk_USLAX, Code = "CA" });
            ctx.eHubStateList.Add(new eHubState { StatePK = pk_NZAKL, Code = "AUK" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
