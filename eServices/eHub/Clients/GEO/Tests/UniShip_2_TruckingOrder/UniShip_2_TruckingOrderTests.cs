using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.Clients.GEO.Transforms.UniShip_2_TruckingOrder;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;

namespace CargoWise.eHub.Clients.GEO.Tests
{
	[TestClass]
	public class UniShip_2_TruckingOrderTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_TruckingOrder()
		{
			var ctx = new TestingMessageContext();
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);

			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("/*[local-name()='TruckingOrder']/*[local-name()='TransactionHeader']/@*[local-name()='transactionDateTime']");

			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniShip_2_TruckingOrder.TestFiles.Input.xml";
			string expectedFile = "UniShip_2_TruckingOrder.TestFiles.Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_TruckingOrder>(sourceFile, expectedFile);
            Assert.IsTrue(Regex.IsMatch((string)ctx.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"), "GEON_EXW_ORDER_........_0000666.xml"));

            sourceFile = "UniShip_2_TruckingOrder.TestFiles.Fallback_Input.xml";
            expectedFile = "UniShip_2_TruckingOrder.TestFiles.Fallback_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_TruckingOrder>(sourceFile, expectedFile);

            sourceFile = "UniShip_2_TruckingOrder.TestFiles.Fallback2_Input.xml";
            expectedFile = "UniShip_2_TruckingOrder.TestFiles.Fallback2_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_TruckingOrder>(sourceFile, expectedFile);

            sourceFile = "UniShip_2_TruckingOrder.TestFiles.AdditionalReference_Input.xml";
            expectedFile = "UniShip_2_TruckingOrder.TestFiles.AdditionalReference_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_TruckingOrder>(sourceFile, expectedFile);

            sourceFile = "UniShip_2_TruckingOrder.TestFiles.UserDefined_Input.xml";
            expectedFile = "UniShip_2_TruckingOrder.TestFiles.UserDefined_output.xml";
            mapTester.ExecuteCompiled<UniShip_2_TruckingOrder>(sourceFile, expectedFile);

            sourceFile = "UniShip_2_TruckingOrder.TestFiles.UserDefined2_Input.xml";
            expectedFile = "UniShip_2_TruckingOrder.TestFiles.UserDefined2_output.xml";
            mapTester.ExecuteCompiled<UniShip_2_TruckingOrder>(sourceFile, expectedFile);

            sourceFile = "UniShip_2_TruckingOrder.TestFiles.UserDefined3_Input.xml";
            expectedFile = "UniShip_2_TruckingOrder.TestFiles.UserDefined3_output.xml";
            mapTester.ExecuteCompiled<UniShip_2_TruckingOrder>(sourceFile, expectedFile);

		    sourceFile = "UniShip_2_TruckingOrder.TestFiles.UserDefined4_Input.xml";
		    expectedFile = "UniShip_2_TruckingOrder.TestFiles.UserDefined4_output.xml";
		    mapTester.ExecuteCompiled<UniShip_2_TruckingOrder>(sourceFile, expectedFile);

            sourceFile = "UniShip_2_TruckingOrder.TestFiles.BPRShipment_Input.xml";
            expectedFile = "UniShip_2_TruckingOrder.TestFiles.BPRShipment_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_TruckingOrder>(sourceFile, expectedFile);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "GEOGFRPRO_EXW" });
            ctx.eHubClients.Add(new eHubClient { CC_ID = "GEOGFRPRO_EXW" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "ExWorks XML GEO - Send Transport Booking", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Purpose" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "add" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "SCAC" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SCAC" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Time Zone" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Australia/Sydney" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Booked Price" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "0.01" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Booked Currency" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "USD" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Booked Scac" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SODH" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Cargo Type", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "CargoType" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "LSE" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "LOOSE" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "CNT" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "FCL" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Pack Type", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Packaging" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "CTN" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "CARTONS" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "SKD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SKIDS" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "BOX" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "BOXES" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "PK" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PK" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Shipment Type", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "ShipmentType" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "ABC" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "DE" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PU" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Match Loading with Origin" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "ABC" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "t" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "whatever" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Direction", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Is Pickup" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "DST" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "falsee" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "T" });

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCounterValue",
				OutputParm = "@value",
				InputParms = new List<string> { 
					"@name", "CargoWise.eHub.Clients.GEO.Transforms.UniShip_2_TruckingOrder.transactionId",
					"@maxlength", "7",
				},
				Result = "666"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetIATAfromUNLOCO",
				OutputParm = "@IATACode",
				InputParms = new List<string> { 
					"@UNLOCOCode", ""
				},
				Result = ""
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetIATAfromUNLOCO",
				OutputParm = "@IATACode",
				InputParms = new List<string> { 
					"@UNLOCOCode", "USLAX"
				},
				Result = "LAX"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetIATAfromUNLOCO",
				OutputParm = "@IATACode",
				InputParms = new List<string> { 
					"@UNLOCOCode", "CNNCA"
				},
				Result = "   "
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetIATAfromUNLOCO",
				OutputParm = "@IATACode",
				InputParms = new List<string> { 
					"@UNLOCOCode", "CAYYZ"
				},
				Result = "YYZ"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetIATAfromUNLOCO",
				OutputParm = "@IATACode",
				InputParms = new List<string> { 
					"@UNLOCOCode", "TWTPE"
				},
				Result = "TPE"
			});

            ctx.ActionProcedures.Add(new ActionProcedure
            {
                Procedure = "GetIATAfromUNLOCO",
                OutputParm = "@IATACode",
                InputParms = new List<string> { 
					"@UNLOCOCode", "USCPM"
				},
                Result = "CPM"
            });

            ctx.ActionProcedures.Add(new ActionProcedure
            {
                Procedure = "GetIATAfromUNLOCO",
                OutputParm = "@IATACode",
                InputParms = new List<string> { 
					"@UNLOCOCode", "USSYY"
				},
                Result = "SYY"
            });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
