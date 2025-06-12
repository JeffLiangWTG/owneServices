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
using CargoWise.eHub.Clients.Common.Transforms.CFXML_BOE_Outgoing_Template;


namespace CargoWise.eHub.Clients.Common.Tests
{	
	[TestClass]
	public class CFXML_BOE_Outgoing_TemplateTests
	{	
		private string    path;
		private MapTester mapTester;
		
		
		private void doTest( string sourceFile, string expectedFile )
		{
			mapTester.ExecuteCompiled<CFXML_BOE_Outgoing_Template>(path + sourceFile,
			                                                            path + expectedFile );
		}
		
		
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCFXML_BOE_Outgoing_Template()
		{	
			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = PrepareExclusionXpaths();
			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			path = "CFXML_BOE_Outgoing_Template.TestFiles.";

			doTest("01_CFXML_BOE_Outgoing_Input.xml",
			       "01_CFXML_BOE_Outgoing_Output.xml");
		}

		private static List<string> PrepareExclusionXpaths()
		{
			List<string> arr = new List<string>();

			string root = "/*[local-name()='CFXML_BOE_DETAILS']";
			string hdr = "/*[local-name()='MessageHeader']";


			arr.Add(root + hdr + "/*[local-name()='Timestamp']");

			return arr;
		}
		
		private static void InitialiseCodeMapsTestingContext()
		{	
			string Sender    = "SSSSSSSSS";
			string Recipient = "RRRRRRRRR_RRR";
			string TS_Name = "CFXML BOE Outgoing Template";
			
			
			UnitTestHelper helper = new UnitTestHelper();
			helper.SetActiveTS( Sender, Recipient, TS_Name );
			CodeSet cs;

			cs = helper.NewCodeSet("Transport Mode");

			cs.AddFields("CW1 Code", "CFXML Code");
			cs.AddRecord("AIR", "AIR");
			cs.AddRecord("SEA", "SEA");
			cs.AddRecord("%"   , "<PassThroughKey>");


			CodeMapsTestingContext ctx = helper.GetCodeMapsTestingContext();

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCounterInterfaceValue",
				OutputParm = "@StartValue",
				InputParms = new List<string> { 
					"@TransformatonSetName", "CFXML BOE Outgoing Template",
					"@Name", "NewMessageID",
					"@MaxValue", "999999999",
					"@IncrementValue", "1"
				},
				Result = "3"
			});
			

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext( ctx );
		}
	}
}
