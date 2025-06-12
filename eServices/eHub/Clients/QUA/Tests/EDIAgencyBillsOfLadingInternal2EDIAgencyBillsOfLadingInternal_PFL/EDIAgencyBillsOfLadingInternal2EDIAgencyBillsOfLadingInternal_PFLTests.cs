using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.QUA.Transforms.EDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFL;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.QUA.Tests
{
	[TestClass]
	public class UnitEDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFLTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UnitEDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFL_MultiAgencyBillOfLading()
		{
			InitialiseCodeMapsTestingContext("Y");
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "EDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFL.TestFiles.EDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFLTests_input.XML";
			string expectedFile = "EDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFL.TestFiles.EDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFLTests_output.XML";
			mapTester.Execute<EDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFL>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UnitEDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFL_SingleAgencyBillOfLading()
		{
			InitialiseCodeMapsTestingContext("N");
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "EDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFL.TestFiles.EDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFLTests_single_input.XML";
			string expectedFile = "EDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFL.TestFiles.EDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFLTests_single_output.XML";
			mapTester.Execute<EDIAgencyBillsOfLadingInternal2EDIAgencyBillsOfLadingInternal_PFL>(sourceFile, expectedFile);
		}

		ICompare Comparer
		{
			get
			{
				if (comparer == null)
				{
					List<string> exclusionXpaths = new List<string>();
					comparer = new ExcludingComparer(exclusionXpaths);
				}
				return comparer;
			}
		}
		ICompare comparer;

		private static void InitialiseCodeMapsTestingContext(string RemoveDirection)
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "QUABNEBNE" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "PFLPNZHST" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "E2E - Import of Ships Agency Bills of Lading", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Billing Branch", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "PFL Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "PFL" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "LFP" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Remove Voyage Direction" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = RemoveDirection });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
