using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Clients.Common.UnitTestHelperFramework;
using CargoWise.eHub.Clients.Common.Transforms.CFXML_ShipDet_Incoming_Template;


namespace CargoWise.eHub.Clients.Common.Tests
{	
	[TestClass]
	public class CFXML_ShipDet_Incoming_TemplateTests
	{	
		private string    path;
		private MapTester mapTester;
		
		
		private void doTest( string sourceFile, string expectedFile )
		{	
			mapTester.ExecuteCompiled<CFXML_ShipDet_Incoming_Template>( path + sourceFile,
			                                                            path + expectedFile );
		}
		
		
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCFXML_ShipDet_Incoming_Template()
		{	
			InitialiseCodeMapsTestingContext();
			
			mapTester = new MapTester( Assembly.GetExecutingAssembly() );
			
			path = "CFXML_ShipDet_Incoming_Template.TestFiles.";
			
			doTest("01_ShipmentDetails_Input.xml",
			       "01_ShipmentDetails_Output.xml");
			
			doTest("02_All_Fields_Input.xml",
			       "02_All_Fields_Output.xml");
		}
		
		
		private static void InitialiseCodeMapsTestingContext()
		{	
			string Sender    = "SSSSSSSSS_SSS";
			string Recipient = "RRRRRRRRR";
			string TS_Name   = "TS Name for this Interface";
			
			
			UnitTestHelper helper = new UnitTestHelper();
			helper.SetActiveTS( Sender, Recipient, TS_Name );
			CodeSet cs;
			
			
			//-------------------------------------------------------------------------------------
			
			cs = helper.NewCodeSet("Defaults", isDefault: true );
			
			cs.AddFields("Data Provider", "Company Code", "Enterprise ID", "Server ID", "Transport Mode");
			cs.AddRecord("HAP"          , ""            , ""             , ""         , "SEA"           );
			
			
			//-------------------------------------------------------------------------------------
			
			cs = helper.NewCodeSet("Branch Codes");
			
			cs.AddFields("Incoming Branch Code", "CW1 Branch Code" );
			cs.AddRecord("%"                   , "<PassThroughKey>");
			
			
			//-------------------------------------------------------------------------------------
			
			cs = helper.NewCodeSet("Transport Modes");
			
			cs.AddFields("Incoming Transport Mode", "CW1 Transport Mode");
			cs.AddRecord("RAIL"                   , "RAI"               );
			cs.AddRecord("ROAD"                   , "ROA"               );
			cs.AddRecord("%"                      , "<PassThroughKey>"  );
			
			
			//-------------------------------------------------------------------------------------
			
			cs = helper.NewCodeSet("Pack Types");
			
			cs.AddFields("Incoming Pack Type Code", "CW1 Pack Type Code");
			cs.AddRecord("%"                      , "<PassThroughKey>"  );
			
			
			//-------------------------------------------------------------------------------------
			
			cs = helper.NewCodeSet("Additional Ref Codes");
			
			cs.AddFields("Reference Type"  , "CW1 Add Ref Type Code");
			cs.AddRecord("ShipperRefNo"    , "BKG"                  );
			cs.AddRecord("ConsigneeRefNo"  , "UCR"                  );
			cs.AddRecord("NotifyPartyRefNo", "OAG"                  );
			cs.AddRecord("%"               , ""                     );
			
			
			//-------------------------------------------------------------------------------------
			
			cs = helper.NewCodeSet("Container Mode");
			
			cs.AddFields("Container Shipment Type", "CW1 Container Mode");
            cs.AddRecord("LCL"                    , "LCL"               );
			cs.AddRecord("%"                      , "FCL"               );
			
			
			//-------------------------------------------------------------------------------------
			
			cs = helper.NewCodeSet("Container Type");
			
			cs.AddFields("Container Size", "CW1 Container Type Code");
			cs.AddRecord("40feet"        , "40GP"                   );
			cs.AddRecord("%"             , "<PassThroughKey>"       );
			
			
			//-------------------------------------------------------------------------------------
			
			helper.SetActiveTS("eHub", "eHub", "Common Code Mappings");
			
			cs = helper.NewCodeSet("Transport Mode");
			
			cs.AddFields("Code", "Description");
			cs.AddRecord("AIR" , "Air"        );
			cs.AddRecord("SEA" , "Sea"        );
			cs.AddRecord("ROA" , "Road"       );
			cs.AddRecord("RAI" , "Rail"       );
			cs.AddRecord("STO" , "Storage"    );
			cs.AddRecord("%"   , ""           );
			
			
			//-------------------------------------------------------------------------------------
			
			CodeMapsTestingContext ctx = helper.GetCodeMapsTestingContext();
			
			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext( ctx );
		}
	}
}
