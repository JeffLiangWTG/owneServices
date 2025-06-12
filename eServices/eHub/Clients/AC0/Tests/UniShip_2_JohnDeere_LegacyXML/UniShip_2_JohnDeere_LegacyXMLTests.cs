using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.AC0.Transforms.UniShip_2_JohnDeere_LegacyXML;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;

namespace CargoWise.eHub.Clients.AC0.Tests
{
	[TestClass]
	public class UniShip_2_JohnDeere_LegacyXMLTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_JohnDeere_LegacyXML()
		{
			var ctx = new TestingMessageContext();
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);
			
			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("/*[local-name()='XmlInterchange']/*[local-name()='InterchangeInfo']/*[local-name()='Date']");

			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);


			string sourceFile = "UniShip_2_JohnDeere_LegacyXML.TestFiles.01_Consol_with_Shipment_and_Declaration_Input.xml";
			string expectedFile = "UniShip_2_JohnDeere_LegacyXML.TestFiles.01_Consol_with_Shipment_and_Declaration_Ouput.xml";
			mapTester.ExecuteCompiled<UniShip_2_JohnDeere_LegacyXML>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_JohnDeere_LegacyXML.TestFiles.02_Standalone_Declaration_CountryOfIssue_Input.xml";
			expectedFile = "UniShip_2_JohnDeere_LegacyXML.TestFiles.02_Standalone_Declaration_CountryOfIssue_Output.xml";
			mapTester.ExecuteCompiled<UniShip_2_JohnDeere_LegacyXML>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_JohnDeere_LegacyXML.TestFiles.03_Standalone_Declaration_Input.xml";
			expectedFile = "UniShip_2_JohnDeere_LegacyXML.TestFiles.03_Standalone_Declaration_Output.xml";
			mapTester.ExecuteCompiled<UniShip_2_JohnDeere_LegacyXML>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_JohnDeere_LegacyXML.TestFiles.04_Standalone_Declaration_Insurance_Input.xml";
			expectedFile = "UniShip_2_JohnDeere_LegacyXML.TestFiles.04_Standalone_Declaration_Insurance_Output.xml";
			mapTester.ExecuteCompiled<UniShip_2_JohnDeere_LegacyXML>(sourceFile, expectedFile);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();

			ctx.eHubClients.Add(new eHubClient { CC_ID = "AC0AKLAKL" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "AC0AKLAKL_JDE" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Send Legacy XML files to John Deere via B2B", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Entry Currency Code" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "NZD" });
			
			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetColumnValue",
				OutputParm = "@result",
				InputParms = new List<string> { 
					"@tableName", "RefCountry", "@inputColumn", "RN_Code", "@outputColumn", "RN_Desc", "@inputValue", "AU"
				},
				Result = "Australia"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetColumnValue",
				OutputParm = "@result",
				InputParms = new List<string> { 
					"@tableName", "RefCountry", "@inputColumn", "RN_Code", "@outputColumn", "RN_Desc", "@inputValue", "NZ"
				},
				Result = "New Zealand"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetColumnValue",
				OutputParm = "@result",
				InputParms = new List<string> { 
					"@tableName", "RefCountry", "@inputColumn", "RN_Code", "@outputColumn", "RN_Desc", "@inputValue", "US"
				},
				Result = "United States"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetColumnValue",
				OutputParm = "@result",
				InputParms = new List<string> { 
					"@tableName", "RefCountry", "@inputColumn", "RN_Code", "@outputColumn", "RN_Desc", "@inputValue", "BE"
				},
				Result = "Belgium"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetColumnValue",
				OutputParm = "@result",
				InputParms = new List<string> { 
					"@tableName", "RefCountry", "@inputColumn", "RN_Code", "@outputColumn", "RN_Desc", "@inputValue", "DE"
				},
				Result = "Germany"
			});

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}

	}
}
