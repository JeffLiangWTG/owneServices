using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.ForwardAir.Transforms.X12_997_2_UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.ForwardAir.Tests
{
	[TestClass]
	public class X12_997_2_UniversalEventTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestX12_997_2_UniversalEvent()
		{
			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='EventTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "X12_997_2_UniversalEvent.TestFiles.Test1_input.xml";
			string expectedFile = "X12_997_2_UniversalEvent.TestFiles.Test1_output.xml";
			mapTester.Execute<X12_997_2_UniversalEvent>(sourceFile, expectedFile);

			sourceFile = "X12_997_2_UniversalEvent.TestFiles.Test2_input.xml";
			expectedFile = "X12_997_2_UniversalEvent.TestFiles.Test2_output.xml";
			mapTester.Execute<X12_997_2_UniversalEvent>(sourceFile, expectedFile);

			sourceFile = "X12_997_2_UniversalEvent.TestFiles.Test3_input.xml";
			expectedFile = "X12_997_2_UniversalEvent.TestFiles.Test3_output.xml";
			mapTester.Execute<X12_997_2_UniversalEvent>(sourceFile, expectedFile);
		}

		void InitialiseTestingMessageContext()
		{
			var ctx = new TestingMessageContext();
			ctx.Write("InterchangeControlNo", "http://schemas.microsoft.com/Edi/PropertySchema", "000000014");
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();

			ctx.eHubClients.Add(new eHubClient { CC_ID = "FORAIRCMH" });

			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Forward Air - Receive 997", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Event Type", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Enterprise Code" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Event Reference" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "A" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "MAA" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "Load Tender Accepted|DEP=Forward Air|" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "R" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "MRJ" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "Load Tender Rejected|DEP=Forward Air|" });

			ctx.ActionProcedures.Add(new ActionProcedure()
			{
				Procedure = "SelectSubscribedReference",
				OutputParm = "@reference",
				InputParms = new List<string> 
						 { 
							 "@senderId", "FORAIRCMH",
							 "@recipientId", "",
							 "@ST_ID", "FORTRN",
							 "@value", "51630001"
						 },
				Result = "C00676426"
			});

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
