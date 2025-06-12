using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.LastMileCarrier.Transforms.UniversalShipment2eParcelMessages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cargowise.eHub.Products.LastMileCarrier.Tests
{
	[TestClass]
	public class UniversalShipment2eParcelMessagesTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2eParcelMessages()
		{
			var context = new CodeMapsTestingContext();

			InitializeCodeMapsTestingContext(context);
			InitializeCounters(context);

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(context);

			InitialiseTestingMessageContext("APOMELMEL");

			var exclusionXPaths = new List<string>();
			ICompare compare = new ExcludingComparer(exclusionXPaths);
			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), compare);

			string sourceFile = "UniversalShipment2eParcelMessages.TestFiles.TransportBooking_input.xml";
			string expectedFile = "UniversalShipment2eParcelMessages.TestFiles.TransportBooking_output.xml";
			mapTester.Execute<UniversalShipment2eParcelMessages>(sourceFile, expectedFile);

			sourceFile = "UniversalShipment2eParcelMessages.TestFiles.CB_input.xml";
			expectedFile = "UniversalShipment2eParcelMessages.TestFiles.CB_output.xml";
			mapTester.Execute<UniversalShipment2eParcelMessages>(sourceFile, expectedFile);

			sourceFile = "UniversalShipment2eParcelMessages.TestFiles.CM_input.xml";
			expectedFile = "UniversalShipment2eParcelMessages.TestFiles.CM_output.xml";
			mapTester.Execute<UniversalShipment2eParcelMessages>(sourceFile, expectedFile);
		}

		#region Implementation

		static void InitializeCodeMapsTestingContext(CodeMapsTestingContext ctx)
		{
			ctx.eHubClients.Add(new eHubClient { CC_ID = "LMC_EPR" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "LMC_EPR" });

			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "eParcel Messages - Export Transport Bookings", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Constants", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "ApplicationId" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "MERCHANT" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "ChargeCode" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "DE1" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "InternalChargebackAccount" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SALES" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "ProfileId" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PT_01" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Line Limit" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "3" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "PostChargeToAccount" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "20355936" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Delivery Country Code" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "AU" });

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetStateByPostcode",
				OutputParm = "@state",
				InputParms = new List<string> { 
					"@postcode", "6000"
				},
				Result = "WA "
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetStateByPostcode",
				OutputParm = "@state",
				InputParms = new List<string> { 
					"@postcode", "2567"
				},
				Result = "NSW"
			});
		}

		static void InitializeCounters(CodeMapsTestingContext ctx)
		{
			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCounterInterfaceValue",
				OutputParm = "@StartValue",
				InputParms = new List<string> { 
			        "@TransformatonSetName", "eParcel Messages - Export Transport Bookings",
			        "@Name", "ConsignmentNumber_JDQ",
			        "@MaxValue", "9999999999",
			        "@IncrementValue", "4"
			    },
				Result = "9999999998"
			});
		}

		public void AddStream(Stream reader, Stream writer)
		{
			var buffer = new byte[32 * 1024];
			while (true)
			{
				int read = reader.Read(buffer, 0, buffer.Length);
				writer.Write(buffer, 0, read);
				if (read != buffer.Length) break;
			}
			writer.Flush();
		}

		static TestingMessageContext InitialiseTestingMessageContext(string sourceParty)
		{
			var ctx = new TestingMessageContext();
			ctx.Write("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", sourceParty);
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);
			return ctx;
		}

		#endregion
	}
}

