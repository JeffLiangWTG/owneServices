using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.MC4.Transforms.NichiboTsv_2_UniShip;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Clients.MC4.Tests
{
    [TestClass]
    public class NichiboTsv_2_UniShipTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestNichiboTsv_2_UniShip()
        {
            InitialiseCodeMapsTestingContext();

            List<string> exclusionXpaths = new List<string>();
            exclusionXpaths.Add("//*[local-name()='WayBillNumber'][../*[local-name()='WayBillType']/*[local-name()='Code'] = 'MWB']");
            ICompare comparer = new ExcludingComparer(exclusionXpaths);

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

            string sourceFile = "NichiboTsv_2_UniShip.TestFiles.New_Input.xml";
            string expectedFile = "NichiboTsv_2_UniShip.TestFiles.New_Output.xml";
            mapTester.ExecuteCompiled<NichiboTsv_2_UniShip>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestNichiboTsv_2_UniShip_Fallback()
        {
            InitialiseCodeMapsTestingContext();

            List<string> exclusionXpaths = new List<string>();
            exclusionXpaths.Add("//*[local-name()='WayBillNumber'][../*[local-name()='WayBillType']/*[local-name()='Code'] = 'MWB']");
            ICompare comparer = new ExcludingComparer(exclusionXpaths);

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

            string sourceFile = "NichiboTsv_2_UniShip.TestFiles.Fallback_Input.xml";
            string expectedFile = "NichiboTsv_2_UniShip.TestFiles.Fallback_Output.xml";
            mapTester.ExecuteCompiled<NichiboTsv_2_UniShip>(sourceFile, expectedFile);
        }

        private static void InitialiseCodeMapsTestingContext()
        {
            CodeMapsTestingContext ctx = new CodeMapsTestingContext();
            ctx.eHubClients.Add(new eHubClient { CC_ID = "MC4AKLAKL_NIN" });
            ctx.eHubClients.Add(new eHubClient { CC_ID = "MC4AKLAKL" });
            ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Nichibo tab-file - Receive Consols CusDec&Invoices", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Consol Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Transport Mode" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SEA" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Sending Agent" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "NICHIBA" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Receiving Agent" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "MCCULLOUGH" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Master Bill Number Prefix" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "MAIN" });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Data Provider" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "NIJA" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Transport Mode" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SEA" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Commodity" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "MVEH" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Trailing Description" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = " X USED VEHICLES" });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Brokerage Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Entry Type" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "IMP" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Entry Style" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "NOR" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Broker" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "GG3" });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Inv Header Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Pref Duty" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "N" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Group Charge Currency" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "NZD" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Group Charge Dist. By" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "QTY" });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Unit Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Weight Unit" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "KG" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Volume Unit" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "M3" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Pack Type" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "UNT" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Inv Line Qty Unit" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "NMB" });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Origin", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Origin" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "%BMW%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "DE" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "%AUDI%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "DE" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%MERCEDES%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "DE" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "%VOLVO%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SE" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "%CHEVROLET%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "US" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 999, CK_Key1Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "JP" });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Lookup Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Lookup Code" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "12%", CK_Key2Value = "PETROL", CK_Key3Value = "SEDAN", CK_Key4Value = "1050" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "C" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "2%", CK_Key2Value = "%", CK_Key3Value = "TRUCK", CK_Key4Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "T" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%", CK_Key2Value = "%", CK_Key3Value = "TRUCK", CK_Key4Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "T+" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "%", CK_Key2Value = "PETROL", CK_Key3Value = "VAN", CK_Key4Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "VN" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "%", CK_Key2Value = "DIESEL", CK_Key3Value = "VAN", CK_Key4Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "VDN" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 999, CK_Key1Value = "%", CK_Key2Value = "%", CK_Key3Value = "%", CK_Key4Value = "%" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "UNKNOWN" });

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);
        }
    }
}
