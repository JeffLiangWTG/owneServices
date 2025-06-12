using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.TRX.Transforms.ShipmentsInternal_2_Cisco3B3;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.TRX.Tests
{
	[TestClass]
	public class ShipmentsInternal_2_Cisco3B3Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestShipmentsInternal_2_Cisco3B3()
		{
			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("/*[local-name()='ShipmentStatusDistributionEnvelope']/*[local-name()='ShipmentStatusDistribution']/*[local-name()='DocumentHeader']/*[local-name()='DocumentInformation']/*[local-name()='Creation']");
			exclusionXpaths.Add("/*[local-name()='ShipmentStatusDistributionEnvelope']/*[local-name()='ShipmentStatusDistribution']/*[local-name()='ShipmentStatus']/*[local-name()='ShipmentInformation']/*[local-name()='DateTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "ShipmentsInternal_2_Cisco3B3.TestFiles.input1.xml";
			string expectedFile = "ShipmentsInternal_2_Cisco3B3.TestFiles.output1.xml";
			mapTester.Execute<ShipmentsInternal_2_Cisco3B3>(sourceFile, expectedFile);

			sourceFile = "ShipmentsInternal_2_Cisco3B3.TestFiles.3B3_Input.xml";
			expectedFile = "ShipmentsInternal_2_Cisco3B3.TestFiles.3B3_Output.xml";
			mapTester.Execute<ShipmentsInternal_2_Cisco3B3>(sourceFile, expectedFile);

			sourceFile = "ShipmentsInternal_2_Cisco3B3.TestFiles.AAV_Input.xml";
			expectedFile = "ShipmentsInternal_2_Cisco3B3.TestFiles.AAV_Output.xml";
			mapTester.Execute<ShipmentsInternal_2_Cisco3B3>(sourceFile, expectedFile);
		}


		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCisco3B3Reply_2_UniversalEvent()
		{
			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='EventTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "ShipmentsInternal_2_Cisco3B3.TestFiles.Cisco3B3Reply_input1_Success.xml";
			string expectedFile = "ShipmentsInternal_2_Cisco3B3.TestFiles.Cisco3B3Reply_output1_Success.xml";
			mapTester.Execute<Cisco3B3Reply_2_UniversalEvent>(sourceFile, expectedFile);

			sourceFile = "ShipmentsInternal_2_Cisco3B3.TestFiles.Cisco3B3Reply_input1_503Error.xml";
			expectedFile = "ShipmentsInternal_2_Cisco3B3.TestFiles.Cisco3B3Reply_output1_503Error.xml";
			mapTester.Execute<Cisco3B3Reply_2_UniversalEvent>(sourceFile, expectedFile);
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "TRXELPELP" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "TRXELPELP_CIS" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Cisco 3B3 - Export Shipment Milestones", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Message ID Prefix" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "TRX" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Sender Name" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Trans-Expedite, Inc." });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Sender DUNS" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "123456789" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Carrier Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "TRX6" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Shipment Status", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Shipment Status" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "3B3" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "DAS" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "AAV" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "AAV" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "CRF" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "CRF" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "XXX" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Customs Type" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "3B3" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "IMP" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "AAV" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "EXP" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "CRF" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "EXP" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "IMP" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Shipment Mode", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Cisco Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "MOT" });

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCounterValue",
				OutputParm = "@value",
				InputParms = new List<string> { 
					"@name", "CargoWise.eHub.Clients.TRX.Transforms.ShipmentsInternal_2_Cisco3B3.DocumentIdentification.Identifier",
					"@padlength", "18" 
				},
				Result = "000000000000000066"
			});

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
