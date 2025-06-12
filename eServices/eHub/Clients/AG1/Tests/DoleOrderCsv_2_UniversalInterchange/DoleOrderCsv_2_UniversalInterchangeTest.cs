using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.AG1.Transforms.DoleOrderCsv_2_UniversalInterchange;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.AG1.Tests
{
	[TestClass]
	public class DoleOrderCsv_2_UniversalInterchangeTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestDoleOrderCsv_2_UniversalInterchange()
		{
			InitialiseCodeMapsTestingContext();
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "DoleOrderCsv_2_UniversalInterchange.TestFiles.DoleOrderCsv_2_UniversalInterchange_input.xml";
			string expectedFile = "DoleOrderCsv_2_UniversalInterchange.TestFiles.DoleOrderCsv_2_UniversalInterchange_output.xml";
			mapTester.Execute<DoleOrderCsv_2_UniversalInterchange>(sourceFile, expectedFile);
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "AG1PQCSIN_DOR" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "AG1PQCSIN" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Dole Order csv-file: Receive OrderManager Orders", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Data Provider" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "DOLE" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Unit of Measurement", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Output Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "PIECE" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PCE" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetColumnValue",
				OutputParm = "@result",
				InputParms = new List<string> { 
			        "@tableName", "RefCountry", "@inputColumn", "RN_Desc", "@outputColumn", "RN_Code", "@inputValue", "Philippines"
			    },
				Result = "PH"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetColumnValue",
				OutputParm = "@result",
				InputParms = new List<string> { 
			        "@tableName", "RefCountry", "@inputColumn", "RN_Desc", "@outputColumn", "RN_Code", "@inputValue", "Thailand"
			    },
				Result = ""
			});

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
