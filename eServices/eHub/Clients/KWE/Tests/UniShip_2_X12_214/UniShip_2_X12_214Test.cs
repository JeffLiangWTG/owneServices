using System;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.KWE.Transforms.UniShip_2_X12_214;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.KWE.Tests
{
	[TestClass]
	public class UniShip_2_X12_214Test
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_X12_214_TimeZone()
		{
			InitialiseCodeMapsTestingContext();
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniShip_2_X12_214.TestFiles.P1_Input1.xml";
			string expectedFile = "UniShip_2_X12_214.TestFiles.P1_Output1.xml";
			mapTester.Execute<UniShip_2_X12_214>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_X12_214()
		{
			InitialiseCodeMapsTestingContext();
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniShip_2_X12_214.TestFiles.P1_Input2.xml";
			string expectedFile = "UniShip_2_X12_214.TestFiles.P1_Output2.xml";
			mapTester.Execute<UniShip_2_X12_214>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_X12_214.TestFiles.O5_Input1.xml";
			expectedFile = "UniShip_2_X12_214.TestFiles.O5_Output1.xml";
			mapTester.Execute<UniShip_2_X12_214>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_X12_214.TestFiles.O5_Input2.xml";
			expectedFile = "UniShip_2_X12_214.TestFiles.O5_Output2.xml";
			mapTester.Execute<UniShip_2_X12_214>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_X12_214.TestFiles.C6_Input1.xml";
			expectedFile = "UniShip_2_X12_214.TestFiles.C6_Output1.xml";
			mapTester.Execute<UniShip_2_X12_214>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_X12_214.TestFiles.C6_Input2.xml";
			expectedFile = "UniShip_2_X12_214.TestFiles.C6_Output2.xml";
			mapTester.Execute<UniShip_2_X12_214>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_X12_214.TestFiles.D5_Input1.xml";
			expectedFile = "UniShip_2_X12_214.TestFiles.D5_Output1.xml";
			mapTester.Execute<UniShip_2_X12_214>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_X12_214.TestFiles.D5_Input2.xml";
			expectedFile = "UniShip_2_X12_214.TestFiles.D5_Output2.xml";
			mapTester.Execute<UniShip_2_X12_214>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_X12_214.TestFiles.X1_Input1.xml";
			expectedFile = "UniShip_2_X12_214.TestFiles.X1_Output1.xml";
			mapTester.Execute<UniShip_2_X12_214>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_X12_214.TestFiles.X1_Input2.xml";
			expectedFile = "UniShip_2_X12_214.TestFiles.X1_Output2.xml";
			mapTester.Execute<UniShip_2_X12_214>(sourceFile, expectedFile);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "KWESYDSYD" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "KWESYDSYD_214" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Emerson 214 - Send Status Messages", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = null });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "SCAC" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "KWEI" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Shipment Status", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "X12 Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Stop Number" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "P1" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "2" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "O5" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "3" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "X1" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "4" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Unit of Measurement", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "X12 Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "KG" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "K" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "LB" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "L" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "CF" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "E" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "M3" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "X" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "T" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "E" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 6, CK_Key1Value = "CI" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "N" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 7, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });


			var SIStateGuid = Guid.NewGuid();
			var AUStateGuid = Guid.NewGuid();
			var CAStateGuid = Guid.NewGuid();

			ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "SGSIN", StateRef = SIStateGuid });
			ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "AUBNE", StateRef = AUStateGuid });
			ctx.eHubUNLOCOList.Add(new eHubUNLOCO { UNLOCOCode = "CAYYZ", StateRef = CAStateGuid });

			ctx.eHubStateList.Add(new eHubState { StatePK = SIStateGuid, Code = "SI" });
			ctx.eHubStateList.Add(new eHubState { StatePK = AUStateGuid, Code = "QLD" });
			ctx.eHubStateList.Add(new eHubState { StatePK = CAStateGuid, Code = "ON" });


			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
