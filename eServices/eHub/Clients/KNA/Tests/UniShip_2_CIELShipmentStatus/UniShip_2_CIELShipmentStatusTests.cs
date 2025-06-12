using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.KNA.Transforms.UniShip_2_CIELShipmentStatus;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;

namespace CargoWise.eHub.Clients.KNA.Tests
{
	[TestClass]
	public class UniShip_2_CIELShipmentStatusTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_CIELShipmentStatus()
		{
            InitialiseCodeMapsTestingContext("{|EnTRYnumbER_TriGGerDescriPTion)},1510,1600,1690");

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='ShipmentStatusExt']/*[local-name()='Envelope']/*[local-name()='TransmissionDateTime']/*[local-name()='Date']");
			exclusionXpaths.Add("//*[local-name()='ShipmentStatusExt']/*[local-name()='Envelope']/*[local-name()='TransmissionDateTime']/*[local-name()='Time']");

			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniShip_2_CIELShipmentStatus.TestFiles.AIR_Shipment_Input.xml";
			string expectedFile = "UniShip_2_CIELShipmentStatus.TestFiles.AIR_Shipment_Output.xml";
			mapTester.ExecuteCompiled<UniShip_2_CIELShipmentStatus>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_CIELShipmentStatus.TestFiles.SEA_CusDec_Input.xml";
			expectedFile = "UniShip_2_CIELShipmentStatus.TestFiles.SEA_CusDec_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_CIELShipmentStatus>(sourceFile, expectedFile);

            sourceFile = "UniShip_2_CIELShipmentStatus.TestFiles.SEA_CusDec_OAG_Input.xml";
            expectedFile = "UniShip_2_CIELShipmentStatus.TestFiles.SEA_CusDec_OAG_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_CIELShipmentStatus>(sourceFile, expectedFile);

            sourceFile = "UniShip_2_CIELShipmentStatus.TestFiles.SEA_CusDec_AA_Input.xml";
            expectedFile = "UniShip_2_CIELShipmentStatus.TestFiles.SEA_CusDec_AA_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_CIELShipmentStatus>(sourceFile, expectedFile);
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniShip_2_CIELShipmentStatus_Wildcard()
        {
            InitialiseCodeMapsTestingContext("{|EnTRYnumbER_TriGGerDescriPTion)},%");

            List<string> exclusionXpaths = new List<string>();
            exclusionXpaths.Add("//*[local-name()='ShipmentStatusExt']/*[local-name()='Envelope']/*[local-name()='TransmissionDateTime']/*[local-name()='Date']");
            exclusionXpaths.Add("//*[local-name()='ShipmentStatusExt']/*[local-name()='Envelope']/*[local-name()='TransmissionDateTime']/*[local-name()='Time']");

            ICompare comparer = new ExcludingComparer(exclusionXpaths);

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

            string sourceFile = "UniShip_2_CIELShipmentStatus.TestFiles.AIR_Shipment_Input.xml";
            string expectedFile = "UniShip_2_CIELShipmentStatus.TestFiles.AIR_Shipment_Output.xml";
            mapTester.ExecuteCompiled<UniShip_2_CIELShipmentStatus>(sourceFile, expectedFile);
        }

		static void InitialiseCodeMapsTestingContext(string generateEntryNumber)
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "KNASYDSYD" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "KNASYDSYD_CSS" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "CIEL Status xml - Send Shipment Status", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Envelope Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Sender Identification" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "CARW" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Receiver Identification" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "KNTEST" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Message Type" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SHPSTE" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Message Version" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "01.00" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Envelope Identification" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "String" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Message Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 }); ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 }); 
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Carrier SCAC" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "CARW" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Entry Number (Remark)" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = generateEntryNumber });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Status Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Status Code" });
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "AU", CK_Key2Value = "HK", CK_Key3Value = "STC", CK_Key4Value = "CFL" , CK_Key5Value = "%"});
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "1588" });
            
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "AU", CK_Key2Value = "AU", CK_Key3Value = "STC", CK_Key4Value = "CFL", CK_Key5Value = "%" });
			    ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "1510" });
            
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "NZ", CK_Key2Value = "DE", CK_Key3Value = "CES", CK_Key4Value = "CLR", CK_Key5Value = "%" });
			    ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "1622" });
            
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "NZ", CK_Key2Value = "DE", CK_Key3Value = "CES", CK_Key4Value = "%", CK_Key5Value = "KN" });
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "1601" });
                
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "NZ", CK_Key2Value = "DE", CK_Key3Value = "CES", CK_Key4Value = "%", CK_Key5Value = "OAG" });
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "1602" });

                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "NZ", CK_Key2Value = "DE", CK_Key3Value = "CES", CK_Key4Value = "%", CK_Key5Value = "*" });
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "1603" });
                
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%", CK_Key2Value = "%", CK_Key3Value = "%", CK_Key4Value = "%", CK_Key5Value = "%" });
			    ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "1600" });

			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Reference No. Type" });
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "AU", CK_Key2Value = "HK", CK_Key3Value = "STC", CK_Key4Value = "CFL", CK_Key5Value = "%" });
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "ABC" });
                
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "AU", CK_Key2Value = "AU", CK_Key3Value = "STC", CK_Key4Value = "CFL", CK_Key5Value = "%" });
			    ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "OAG" });
                
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "NZ", CK_Key2Value = "DE", CK_Key3Value = "CES", CK_Key4Value = "CLR", CK_Key5Value = "%" });
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "BN" });
                
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "NZ", CK_Key2Value = "DE", CK_Key3Value = "CES", CK_Key4Value = "%", CK_Key5Value = "KN" });
			    ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "KN" });

                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "NZ", CK_Key2Value = "DE", CK_Key3Value = "CES", CK_Key4Value = "%", CK_Key5Value = "OAG" });
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "OAG" });

                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "NZ", CK_Key2Value = "DE", CK_Key3Value = "CES", CK_Key4Value = "%", CK_Key5Value = "%" });
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "KK" });
                
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%", CK_Key2Value = "%", CK_Key3Value = "%", CK_Key4Value = "%", CK_Key5Value = "%" });
			    ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "DN" });

                ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Reference Type", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
                ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Output Code" });
                
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "OAG" });
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "OAG" });
                
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "KN" });
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "KN" });
                
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%" });
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Sequence" });
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "OAG" });
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "3" });
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "KN" });
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "1" });
                ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%" });
                ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "99" });
			
            var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
