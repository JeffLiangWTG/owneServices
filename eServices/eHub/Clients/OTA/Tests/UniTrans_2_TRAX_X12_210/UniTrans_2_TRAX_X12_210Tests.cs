using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.OTA.Transforms.UniTrans_2_TRAX_X12_210;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.Clients.Common.UnitTestHelperFramework;

namespace CargoWise.eHub.Clients.OTA.Tests
{
    [TestClass]
    public class UniTrans_2_TRAX_X12_210Tests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniTrans_2_TRAX_X12_210()
        {
            InitialiseCodeMapsTestingContext();

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "UniTrans_2_TRAX_X12_210.TestFiles.Consol_Input.xml";
            string expectedFile = "UniTrans_2_TRAX_X12_210.TestFiles.Consol_Output.xml";
            mapTester.ExecuteCompiled<UniTrans_2_TRAX_X12_210>(sourceFile, expectedFile);

            sourceFile = "UniTrans_2_TRAX_X12_210.TestFiles.Customs_Declaration_Input.xml";
            expectedFile = "UniTrans_2_TRAX_X12_210.TestFiles.Customs_Declaration_Output.xml";
            mapTester.ExecuteCompiled<UniTrans_2_TRAX_X12_210>(sourceFile, expectedFile);

            sourceFile = "UniTrans_2_TRAX_X12_210.TestFiles.Job_Invoice_with_ShippingLine_Input.xml";
            expectedFile = "UniTrans_2_TRAX_X12_210.TestFiles.Job_Invoice_with_ShippingLine_Output.xml";
            mapTester.ExecuteCompiled<UniTrans_2_TRAX_X12_210>(sourceFile, expectedFile);

            sourceFile = "UniTrans_2_TRAX_X12_210.TestFiles.Shipment_Road_Invoice_Input.xml";
            expectedFile = "UniTrans_2_TRAX_X12_210.TestFiles.Shipment_Road_Invoice_Output.xml";
            mapTester.ExecuteCompiled<UniTrans_2_TRAX_X12_210>(sourceFile, expectedFile);
        }

        private static void InitialiseCodeMapsTestingContext()
        {
            string Sender = "OTACLTCLT";
            string Recipient = "OTACLTCLT_TRA";
            string TS_Name = "Send ANSI X12 210 for Bell & Howell to TRAX";

            UnitTestHelper helper = new UnitTestHelper();
            helper.SetActiveTS(Sender, Recipient, TS_Name);
            CodeSet cs;

            cs = helper.NewCodeSet("Defaults", isDefault: true);
            cs.AddFields("SCAC", "Freight Charge Code", "Routing Sequence", "POD03 Fallback");
            cs.AddRecord("XXXXXXX", "YYYYYYY", "ZZZZZZZ", "QQQQQQQ");

            cs = helper.NewCodeSet("Service Level");
            cs.AddFields("InputCode", "X12 Code");
            cs.AddRecord("XXXXXXX", "YYYYYYY");
            cs.AddRecord("%", "<PassThroughKey>");

            cs = helper.NewCodeSet("Pack Type");
            cs.AddFields("InputCode", "X12 Code");
            cs.AddRecord("XXXXXXX", "YYYYYYY");
            cs.AddRecord("%", "<PassThroughKey>");

            cs = helper.NewCodeSet("Transport Mode");
            cs.AddFields("InputCode", "X12 Code");
            cs.AddRecord("XXXXXXX", "YYYYYYY");
            cs.AddRecord("%", "<PassThroughKey>");

            cs = helper.NewCodeSet("Container Type");
            cs.AddFields("InputCode", "X12 Code", "X12 Length");
            cs.AddRecord("XXXXXXX", "YYYYYYY", "ZZZZZZZ");
            cs.AddRecord("%", "20", "0000");

            cs = helper.NewCodeSet("Charge Code");
            cs.AddFields("InputCode", "X12 Code", "Qualifier", "X12 Description");
            cs.AddRecord("AES", "370", "", "Export Declarations - U.S. Shippers");
            cs.AddRecord("AMS", "MFC", "", "Manifest Charge");
            cs.AddRecord("BAF", "BUA", "", "Bunker Adjustment");
            cs.AddRecord("BCLR", "CSE", "", "Customs Entry");
            cs.AddRecord("BND", "BND", "", "Bond Charges");
            cs.AddRecord("BOBTL", "BOB", "", "Bobtail Charges");
            cs.AddRecord("CACLR", "145", "", "Canadian C.Q.Customs Clearance");
            cs.AddRecord("CAF", "CUF", "", "Currency Adjustment Factor");
            cs.AddRecord("CCC", "CSE", "", "Customs Entry");
            cs.AddRecord("CCLR", "CSE", "", "Customs Entry");
            cs.AddRecord("CDEST", "055", "", "Agent Disbursement Fee - Destination");
            cs.AddRecord("CDEST2", "055", "", "Agent Disbursement Fee - Destination");
            cs.AddRecord("CFS", "730", "", "Terminal Service Fee");
            cs.AddRecord("CLNTK", "260", "", "Delivery Surcharge");
            cs.AddRecord("COD", "COL", "", "Fee for Collecting COD Charge");
            cs.AddRecord("COO", "170", "", "Certificate of Origin");
            cs.AddRecord("COUR", "CRS", "", "Courier Services");
            cs.AddRecord("CRAT", "027", "", "Special Packaging");
            cs.AddRecord("CUC", "CHE", "", "Chassis Equipment Lease Charge");
            cs.AddRecord("DAD", "BLC", "", "Bill of Lading Charge");
            cs.AddRecord("DBILL", "BLC", "", "Bill of Lading Charge");
            cs.AddRecord("DDU", "010", "", "Add on - Destination");
            cs.AddRecord("DET", "DTV", "", "Detention (Vehicle)");
            cs.AddRecord("DFSC", "260", "", "Delivery Surcharge");
            cs.AddRecord("DFT", "120", "", "Banking Drafts");
            cs.AddRecord("DGF", "DGS", "", "Dangerous Goods Surcharge");
            cs.AddRecord("DHG", "DEM", "", "Demurrage");
            cs.AddRecord("DISADM", "300", "", "Distribution Fee");
            cs.AddRecord("DISB", "SER", "", "Service Charge");
            cs.AddRecord("DIST", "300", "", "Distribution Fee");
            cs.AddRecord("DITRK", "DIS", "", "Distribution Service");
            cs.AddRecord("DIV", "DIC", "", "Diversion Charge");
            cs.AddRecord("DLV", "DEL", "", "Delivery Charge");
            cs.AddRecord("DMG", "DEM", "", "Demurrage");
            cs.AddRecord("DOC", "DOC", "", "Documentation Charge");
            cs.AddRecord("DOCTRF", "DOC", "", "Documentation Charge");
            cs.AddRecord("DOF", "BLC", "", "Bill of Lading Charge");
            cs.AddRecord("DRAY", "DRC", "", "Drayage");
            cs.AddRecord("DRP", "ETR", "", "Empty Trailer Returned Charge");
            cs.AddRecord("DUTY", "315", "", "Duty Charge");
            cs.AddRecord("EFAF", "ENS", "", "Energy Surcharge (Fuel Adjustment Factor)");
            cs.AddRecord("EUFEE", "ECX", "", "European Charge Code");
            cs.AddRecord("EXCC", "360", "", "Export Customs Clearance");
            cs.AddRecord("EXM", "CTX", "", "Customs Exams (Intensive, Tailgate)");
            cs.AddRecord("EXPU", "PUC", "", "Pick-up Charge");
            cs.AddRecord("FDA", "CSE", "", "Customs Entry");
            cs.AddRecord("FOB", "015", "", "Add on - Origin");
            cs.AddRecord("FOR", "FWC", "", "Forwarding Charge");
            cs.AddRecord("FRT", "400", "", "Freight");
            cs.AddRecord("FSC", "405", "", "Fuel Surcharge");
            cs.AddRecord("FUMI", "OFU", "", "Fumigation");
            cs.AddRecord("GATEFEE", "LFT", "", "Lift Gate (Truck) or Forklift Service at Pick-up/Delivery");
            cs.AddRecord("GOFEE", "MSG", "", "Miscellaneous Charge");
            cs.AddRecord("GRI", "400", "", "Freight");
            cs.AddRecord("HLS", "SER", "", "Service Charge");
            cs.AddRecord("HND", "HHB", "", "Handling");
            cs.AddRecord("HON", "HHB", "", "Handling");
            cs.AddRecord("INL", "DTF", "", "Destination Inland Freight");
            cs.AddRecord("INSUR", "INS", "", "Insurance");
            cs.AddRecord("ISC", "440", "", "Import Service Fee");
            cs.AddRecord("ISF", "ISF", "", "ISF Filing Fee");
            cs.AddRecord("ISFBND", "BND", "", "Bond Charges");
            cs.AddRecord("ISPS", "440", "", "Import Service Fee");
            cs.AddRecord("ITCC", "CSE", "", "Customs Entry");
            cs.AddRecord("ITFEE", "CSE", "", "Customs Entry");
            cs.AddRecord("LFTGATE", "LFT", "", "Lift Gate (Truck) or Forklift Service at Pick-up/Delivery");
            cs.AddRecord("LOC", "470", "", "Letter of Credit Processing");
            cs.AddRecord("LOSUL", "SUR", "", "Surcharge");
            cs.AddRecord("MSG", "485", "", "Messenger Service");
            cs.AddRecord("OAWB", "550", "", "Preparation of Air Waybill - Origin");
            cs.AddRecord("OSC", "POS", "", "Positioning at Origin");
            cs.AddRecord("OSTR", "SSC", "", "Stripping, Sorting, and Consolidation");
            cs.AddRecord("OVW", "EXW", "", "Excess Weight");
            cs.AddRecord("PLTS", "PPN", "", "Pallet Charge");
            cs.AddRecord("PORCON", "CON", "", "Congestion Surcharge");
            cs.AddRecord("PPASS", "PBL", "", "Pier Charges Other Than Wharfage");
            cs.AddRecord("PRD", "DET", "", "Detention of Trailers");
            cs.AddRecord("PRPL", "PDC", "", "Preloading Charge");
            cs.AddRecord("PS", "IDP", "", "Interdivision Profit");
            cs.AddRecord("PSS", "SUR", "", "Surcharge");
            cs.AddRecord("QUA", "HBD", "", "Harbor Dues");
            cs.AddRecord("SCR", "IAC", "", "Inspection Fee");
            cs.AddRecord("SEC", "SUR", "", "Surcharge");
            cs.AddRecord("SPDLV", "685", "", "Special Delivery");
            cs.AddRecord("SPEQ", "SEC", "", "Special Equipment Charge");
            cs.AddRecord("STOP", "SOC", "", "Stop-off Charge");
            cs.AddRecord("STOR", "SRG", "", "Storage");
            cs.AddRecord("STR", "SSC", "", "Stripping, Sorting, and Consolidation");
            cs.AddRecord("STUF", "STF", "", "Container Stuffing");
            cs.AddRecord("THC", "TER", "", "Terminal Charge");
            cs.AddRecord("TRF", "TRF", "", "Transfer Charge");
            cs.AddRecord("UNL", "UNL", "", "Unloading (Labor Charges)");
            cs.AddRecord("VAT", "VAT", "", "Value Added Tax");
            cs.AddRecord("VGM", "WTV", "", "Weight Verification Charge");
            cs.AddRecord("WAIT", "WTG", "", "Waiting Time");
            cs.AddRecord("WAR", "WAR", "", "War Risk Surcharge");
            cs.AddRecord("WRF", "WFG", "", "Wharfage");
            cs.AddRecord("%", "<PassThroughKey>", "<PassThroughKey>", "<PassThroughKey>");

            cs = helper.NewCodeSet("Unit of Measurement");
            cs.AddFields("InputCode", "X12 Code");
            cs.AddRecord("XXXXXXX", "YYYYYYY");
            cs.AddRecord("%", "<PassThroughKey>");

            helper.SetActiveTS("eHub", "eHub", "Common Code Mappings");
            cs = helper.NewCodeSet("X12 B304 - Payment Method");
            cs.AddFields("ShipmentIncoTerm", "Payment Method - B304");
            cs.AddRecord("FOB", "CC");
            cs.AddRecord("%", "<PassThroughKey>");

            CodeMapsTestingContext ctx = helper.GetCodeMapsTestingContext();

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);
        }
    }
}



