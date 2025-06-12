using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.JWT.Transforms.PWC_OrderCsv_2_UniInterOrder;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.JWT.Tests
{
	[TestClass]
	public class PWC_OrderCsv_2_UniInterOrderTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPWC_OrderCsv_2_UniInterOrder()
		{
			InitialiseCodeMapsTestingContext();
			InitialiseBizTalkTestingContext();

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "PWC_OrderCsv_2_UniInterOrder.TestFiles.Input.xml";
			string expectedFile = "PWC_OrderCsv_2_UniInterOrder.TestFiles.Output.xml";
			mapTester.ExecuteCompiled<PWC_OrderCsv_2_UniInterOrder>(sourceFile, expectedFile);

			sourceFile = "PWC_OrderCsv_2_UniInterOrder.TestFiles.ItemDescription_Input.xml";
			expectedFile = "PWC_OrderCsv_2_UniInterOrder.TestFiles.ItemDescription_Output.xml";
			mapTester.ExecuteCompiled<PWC_OrderCsv_2_UniInterOrder>(sourceFile, expectedFile);

			sourceFile = "PWC_OrderCsv_2_UniInterOrder.TestFiles.NoLine_Input.xml";
			expectedFile = "PWC_OrderCsv_2_UniInterOrder.TestFiles.NoLine_Output.xml";
			mapTester.ExecuteCompiled<PWC_OrderCsv_2_UniInterOrder>(sourceFile, expectedFile);
		}

		static void InitialiseBizTalkTestingContext()
		{
			var ctx = new TestingMessageContext();
			ctx.Write("InboundTransportLocation", "http://schemas.microsoft.com/BizTalk/2003/system-properties", @"ftp://libertyftp.libertyint.com/PWC - CW/CWPRODUCTION");
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "JWTJFKJFK_LIB" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "JWTJFKJFK" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Liberty csv-file - Import OrderManager Orders", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Unit of Measurement", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "ediEnterprise Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Data Provider", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Data Provider" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "%KM%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "K&M" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "%PWC%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PWC" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryInfo",
				OutputParm = "@result",
				InputParms = new List<string> { 
			        "@name", "TAIWAN"
			    },
				Result = "TW"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryInfo",
				OutputParm = "@result",
				InputParms = new List<string> { 
			        "@name", "AUS"
			    },
				Result = ""
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryInfo",
				OutputParm = "@result",
				InputParms = new List<string> { 
			        "@name", "USA"
			    },
				Result = "US"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryInfo",
				OutputParm = "@result",
				InputParms = new List<string> { 
			        "@name", "INDIA"
			    },
				Result = "IN"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryInfo",
				OutputParm = "@result",
				InputParms = new List<string> { 
			        "@name", "CHINA"
			    },
				Result = "CN"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryInfo",
				OutputParm = "@result",
				InputParms = new List<string> { 
			        "@name", ""
			    },
				Result = ""
			});

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
