using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalShipment2EventMessages = CargoWise.eHub.Clients.APO.Transforms.UniversalShipment2EventMessages;

namespace CargoWise.eHub.Clients.APO.Tests
{
	[TestClass]
	public class UniversalShipment2EventMessagesTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2EventMessages()
		{
			InitialiseCodeMapsTestingContext();
			var ctx = InitialiseTestingMessageContext("APOMELMEL_TRK");

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='EventMessageTransmissionDateTime']");
			exclusionXpaths.Add("//*[local-name()='RecordModificationDateTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniversalShipment2EventMessages.TestFiles.Test1_Input.xml";
			string expectedFile = "UniversalShipment2EventMessages.TestFiles.Test1_output.xml";
			string expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.Test1_output_batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.Test2_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.Test2_output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.Test2_output_batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.Test3_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.Test3_output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.Test3_output_batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.Test4_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.Test4_output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.Test4_output_batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.Test5_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.Test5_output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.Test5_output_batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.Test6_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.Test6_output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.Test6_output_batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.9347678_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.9347678_Output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.9347678_Output_Batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.0009347678_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.0009347678_Output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.0009347678_Output_Batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.Q8S_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.Q8S_Output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.Q8S_Output_Batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.0009349304_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.0009349304_Output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.0009349304_Output_Batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.9349304_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.9349304_Output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.9349304_Output_Batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.ZUY_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.ZUY_Output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.ZUY_Output_Batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.6K2_7K2_8K2_9K2_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.6K2_7K2_8K2_9K2_Output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.6K2_7K2_8K2_9K2_Output_Batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.2QQ_2QR_2QS_2QT_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.2QQ_2QR_2QS_2QT_Output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.2QQ_2QR_2QS_2QT_Output_Batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.1Level_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.1Level_Output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.1Level_Output_Batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2EventMessages_APOMELTSH()
		{
			InitialiseCodeMapsTestingContext();
			var ctx = InitialiseTestingMessageContext("APOMELTSH_TRK");

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='EventMessageTransmissionDateTime']");
			exclusionXpaths.Add("//*[local-name()='RecordModificationDateTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniversalShipment2EventMessages.TestFiles.Test.Test1_Input.xml";
			string expectedFile = "UniversalShipment2EventMessages.TestFiles.Test.Test1_output.xml";
			string expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.Test.Test1_output_batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.Test.Test2_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.Test.Test2_output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.Test.Test2_output_batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.Test.Test3_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.Test.Test3_output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.Test.Test3_output_batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.Test.Test4_Input.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.Test.Test4_output.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.Test.Test4_output_batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2EventMessages_APOAKL()
		{
			InitialiseCodeMapsTestingContext();
			var ctx = InitialiseTestingMessageContext("APOAKLMEL_TRK");

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='EventMessageTransmissionDateTime']");
			exclusionXpaths.Add("//*[local-name()='RecordModificationDateTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniversalShipment2EventMessages.TestFiles.Test.Test1_Input.xml";
			string expectedFile = "UniversalShipment2EventMessages.TestFiles.Test.Test1_output.xml";
			string expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.Test.Test1_output_batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);

			sourceFile = "UniversalShipment2EventMessages.TestFiles.Test6_Input_CMS.xml";
			expectedFile = "UniversalShipment2EventMessages.TestFiles.Test6_Output_CMS.xml";
			expectedBatchFile = "UniversalShipment2EventMessages.TestFiles.Test6_Output_CMS_batch.xml";
			mapTester.Execute<UniversalShipment2EventMessages.UniversalShipment2EventMessages>(sourceFile, expectedFile);
			mapTester.Execute<UniversalShipment2EventMessages.EventMessagesEnvelope2EventMessages>(expectedFile, expectedBatchFile);
		}

		#region Implementation

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "APOMELMEL_TRK" });

			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Australia Post Send to SAP PI", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Event Type", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "SAP Event" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Held Event" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "CAD", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "DOM-0033" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "DOM-0034" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "DEP", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "DOM-0032" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "CMS", CK_Key2Value = "HLD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "DOM-0034" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "MSF", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "DOM-0030" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "CAO", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "DOM-0031" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Batching", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Batch Size" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "100" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Location", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Pseudo XPath" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "DEP" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PortOfLoading" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "CAO" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PortOfLoading" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "CAD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PortOfDischarge" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "CVD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PortOfDischarge" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "MSF" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "OrganizationAddressCollection/OrganizationAddress[AddressType = 'ConsignorDocumentaryAddress']/Port" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 6, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PortOfDischarge" });

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "CalculateTimeZoneOffset",
				OutputParm = "@offset",
				InputParms = new List<string> { 
			        "@UNLOCO", "AUMEL", 
			        "@localtime", "2013-01-29T17:47:00"
			    },
				Result = "+11:00"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "CalculateTimeZoneOffset",
				OutputParm = "@offset",
				InputParms = new List<string> { 
			        "@UNLOCO", "USLAX", 
			        "@localtime", "2013-02-21T16:51:00"
			    },
				Result = "-08:00"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "CalculateTimeZoneOffset",
				OutputParm = "@offset",
				InputParms = new List<string> { 
					"@UNLOCO", "AUMEL", 
					"@localtime", "2013-03-26T11:26:00"
				},
				Result = "+11:00"
			});
			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "CalculateTimeZoneOffset",
				OutputParm = "@offset",
				InputParms = new List<string> { 
					"@UNLOCO", "USLAX", 
					"@localtime", "2013-07-06T23:27:00"
				},
				Result = "-08:00"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "CalculateTimeZoneOffset",
				OutputParm = "@offset",
				InputParms = new List<string> { 
					"@UNLOCO", "HKHKG", 
					"@localtime", "2014-03-31T01:39:00"
				},
				Result = "+08:00"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "CalculateTimeZoneOffset",
				OutputParm = "@offset",
				InputParms = new List<string> { 
					"@UNLOCO", "", 
					"@localtime", "2016-04-18T14:37:59.077"
				},
				Result = ""
			});

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}

		static TestingMessageContext InitialiseTestingMessageContext(string destinationParty)
		{
			var ctx = new TestingMessageContext();
			ctx.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", destinationParty);
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);
			return ctx;
		}

		#endregion
	}
}

