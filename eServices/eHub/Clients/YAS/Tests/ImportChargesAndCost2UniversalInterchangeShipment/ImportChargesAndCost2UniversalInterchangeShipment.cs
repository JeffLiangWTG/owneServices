using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.YAS.Transforms.ImportChargesAndCost2UniversalInterchangeShipment;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.YAS.Tests
{
	[TestClass]
	public class ImportChargesAndCost2UniversalInterchangeShipmentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestImportChargesAndCost2UniversalInterchangeShipment()
		{
			InitialiseCodeMapsTestingContext();

			string sourceFile = "ImportChargesAndCost2UniversalInterchangeShipment.TestFiles.input.xml";
			string expectedFile = "ImportChargesAndCost2UniversalInterchangeShipment.TestFiles.output.xml";

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<ImportChargesAndCost2UniversalInterchangeShipment>(sourceFile, expectedFile);
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "YASYJPPRD_CHG" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "YASYJPPRD_CHG" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Flat File- Import YJP Air Imp. Ship.Charges+Costs1", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Charge Code Description", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Description" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "29010" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "EINFUHRABGABEN - STEUER" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
