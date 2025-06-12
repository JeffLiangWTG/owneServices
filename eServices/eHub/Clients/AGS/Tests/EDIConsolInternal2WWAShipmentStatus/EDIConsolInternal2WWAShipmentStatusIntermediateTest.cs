using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.AGS.Transforms.EDIConsolInternal2WWAShipmentStatus;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.AGS.Tests
{
	/// <summary>
	/// Summary description for EDIConsolInternal2WWAShipmentStatusIntermediate
	/// </summary>
	[TestClass]
	public class EDIConsolInternal2WWAShipmentStatusIntermediateTest
	{
		public EDIConsolInternal2WWAShipmentStatusIntermediateTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIConsolInternal2WWAShipmentStatusIntermediate()
		{
			InitialiseCodeMapsTestingContext();

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.ediEnterpriseLCLSampleS00001226.xml";
			string expectedFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.WWAShipmentStatusIntermediate.xml";
			mapTester.Execute<EDIConsolInternal2WWAShipmentStatusIntermediate>(sourceFile, expectedFile);

			sourceFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.Air Export S00001213_20110411095420.xml";
			expectedFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.Air Export S00001213_20110411095420_output.xml";
			mapTester.Execute<EDIConsolInternal2WWAShipmentStatusIntermediate>(sourceFile, expectedFile);

			sourceFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.Air Import S00001276_20110411095903.xml";
			expectedFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.Air Import S00001276_20110411095903_output.xml";
			mapTester.Execute<EDIConsolInternal2WWAShipmentStatusIntermediate>(sourceFile, expectedFile);

			sourceFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.Air Import S00001276_W10_DIM_input.xml";
			expectedFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.Air Import S00001276_W10_DIM_output.xml";
			mapTester.Execute<EDIConsolInternal2WWAShipmentStatusIntermediate>(sourceFile, expectedFile);

			sourceFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.Sea Export S00001218_20110411100731.xml";
			expectedFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.Sea Export S00001218_20110411100731_output.xml";
			mapTester.Execute<EDIConsolInternal2WWAShipmentStatusIntermediate>(sourceFile, expectedFile);

			sourceFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.W30_input.xml";
			expectedFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.W30_output.xml";
			mapTester.Execute<EDIConsolInternal2WWAShipmentStatusIntermediate>(sourceFile, expectedFile);

			sourceFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.W30_FallBackConsol_input.xml";
			expectedFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.W30_FallBackConsol_output.xml";
			mapTester.Execute<EDIConsolInternal2WWAShipmentStatusIntermediate>(sourceFile, expectedFile);

			sourceFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.Input_AULoadingDischargePort.xml";
			expectedFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.Output_AULoadingDischargePort.xml";
			mapTester.Execute<EDIConsolInternal2WWAShipmentStatusIntermediate>(sourceFile, expectedFile);

			sourceFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.W80_input.xml";
			expectedFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.W80_output.xml";
			mapTester.Execute<EDIConsolInternal2WWAShipmentStatusIntermediate>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestWWAShipmentStatusIntermediate2WWAShipmentStatus()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), Comparer);

			string sourceFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.WWAShipmentStatusIntermediate.xml";
			string expectedFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.WWAShipmentStatus.xml";
			mapTester.Execute<WWAShipmentStatusIntermediate2WWAShipmentStatus>(sourceFile, expectedFile);

			sourceFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.WWAShipmentStatusIntermediateArrival.xml";
			expectedFile = "EDIConsolInternal2WWAShipmentStatus.TestFiles.WWAShipmentStatusArrival.xml";
			mapTester.Execute<WWAShipmentStatusIntermediate2WWAShipmentStatus>(sourceFile, expectedFile);
		}

		ICompare Comparer
		{
			get
			{
				if (comparer == null)
				{
					List<string> exclusionXpaths = new List<string>();
					exclusionXpaths.Add("/*[local-name()='ShipmentStatus']/*[local-name()='Envelope']/*[local-name()='EnvelopeID']");
					exclusionXpaths.Add("/*[local-name()='ShipmentStatus']/*[local-name()='ShipmentStatusDetails']/*[local-name()='DocumentationDetails']/*[local-name()='Image']");
					exclusionXpaths.Add("/*[local-name()='ShipmentStatus']/*[local-name()='ShipmentStatusDetails']/*[local-name()='RoutingDetails']/*[local-name()='CutoffReceivingWarehouse']");
					comparer = new ExcludingComparer(exclusionXpaths);
				}

				return comparer;
			}
		}
		ICompare comparer;

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "AGSWORAGS" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "AGSWORAGS_WWA" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "WorldWideAlliance xml-File - Export Shipmnt Status", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Status Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Document Type" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "WWA Status Code" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 3, CR_Name = "Export Shipment Status" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "W10" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "BKC" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "10" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "true" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "W30" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "CFP" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "30" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "true" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "W40" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "40" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "true" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "W50" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "OBL" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "50" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "true" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "W70" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "CAV" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "70" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "false" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "W80" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "CAV" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "80" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "false" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_PassThroughKey = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_PassThroughKey = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = null });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Carrier Booking No." });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "AGS" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "XML Password", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Output Code", });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "bUxr234%678YmtPghgoolGGFsatr" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Release Type", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "WWA Release Type", });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "EBL" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "E" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "O" });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Type of Move", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "TypeOfMove" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "FCL" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "F" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "L" });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Envelope Detail", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Version" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "Shipment" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "1.1.0" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "1.0.0" });

            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Type" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "Shipment" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Shipment_Status_XML_1.1.0" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Shipment_Status_XML_1.0.0" });


            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Member Office Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Member Office Code", });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "AUSYD" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "AUSYD01" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "AUMEL" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "AUMEL01" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "AUXXX01" });

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryInfo",
				OutputParm = "@result",
				InputParms = new List<string> { 
					"@name", "Australia"
				},
				Result = "AU"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryInfo",
				OutputParm = "@result",
				InputParms = new List<string> { 
					"@name", "United States"
				},
				Result = "US"
			});

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
