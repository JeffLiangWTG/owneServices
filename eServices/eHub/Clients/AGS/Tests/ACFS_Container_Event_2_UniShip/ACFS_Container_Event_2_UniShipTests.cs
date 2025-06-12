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
using CargoWise.eHub.Clients.AGS.Transforms.ACFS_Container_Event_2_UniShip;


namespace CargoWise.eHub.Clients.AGS.Tests
{	
	[TestClass]
	public class ACFS_Container_Event_2_UniShipTests
	{	
		private string    path;
		private MapTester mapTester;
		
		
		private void doTest( string sourceFile, string expectedFile )
		{	
			mapTester.ExecuteCompiled<ACFS_Container_Event_2_UniShip>( path + sourceFile,
			                                                           path + expectedFile );
		}
		
		
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestACFS_Container_Event_2_UniShip()
		{	
			InitialiseCodeMapsTestingContext();
			
			mapTester = new MapTester( Assembly.GetExecutingAssembly() );
			
			path = "ACFS_Container_Event_2_UniShip.TestFiles.";
			
			doTest("01_Slot_Date_Input.xml",
			       "01_Slot_Date_Output.xml");
			
			doTest("02_Wharf_Gate_Out_Input.xml",
			       "02_Wharf_Gate_Out_Output.xml");
			
			doTest("03_Actual_Full_Delivery_Input.xml",
			       "03_Actual_Full_Delivery_Output.xml");
		}
		
		
		private static void InitialiseCodeMapsTestingContext()
		{	
			string Sender    = "AGSWORAGS_ACF";
			string Recipient = "AGSWORAGS";
			string TS_Name   = "ACFS Container Event XML - Receive as Events";
			
			
			UnitTestHelper helper = new UnitTestHelper();
			helper.SetActiveTS( Sender, Recipient, TS_Name );
			CodeSet cs;
			
			
			//-------------------------------------------------------------------------------------
			
			cs = helper.NewCodeSet("Defaults", isDefault: true );
			
			cs.AddFields("Data Provider", "Company Code", "Enterprise ID", "Server ID");
			cs.AddRecord("ACF"          , ""            , ""             , ""         );
			
			
			//-------------------------------------------------------------------------------------
			
			cs = helper.NewCodeSet("Event Types");
			
			cs.AddFields("Event Type"              , "Field Name"            );
			cs.AddRecord("____"                    , "FCLAvailable"          );
			cs.AddRecord("DepartureSlotTimeBooked" , "ArrivalSlotDateTime"   );
			cs.AddRecord("DepartureCartageComplete", "FCLWharfGateOut"       );
			cs.AddRecord("IntoTransportYard"       , "ArrivalCartageComplete");
			cs.AddRecord("%"                       , ""                      );
			
			
			//-------------------------------------------------------------------------------------
			
			CodeMapsTestingContext ctx = helper.GetCodeMapsTestingContext();
			
			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext( ctx );
		}
	}
}
