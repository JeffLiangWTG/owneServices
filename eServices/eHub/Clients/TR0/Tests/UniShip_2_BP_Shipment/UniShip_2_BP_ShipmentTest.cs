using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.TR0.Transforms.UniShip_2_BP_Shipment;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.TR0.Tests.UniShip_2_BP_ShipmentTest
{
	[TestClass]
	public class UniversalShipment_2_X12_214Test
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_BP_Shipment()
		{
            var ctx = new TestingMessageContext();
            ctx.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "263TG_JobNumber_20191211");
            var ca = new ContextAccessor();
            ca.SetTestingMessageContext(ctx);

			var testAssembly = Assembly.GetExecutingAssembly();
			MapTester mapTester = new MapTester(testAssembly);

            string sourceFile = "UniShip_2_BP_Shipment.TestFiles.BP_Shipments_SEA_input.xml";
            string expectedFile = "UniShip_2_BP_Shipment.TestFiles.BP_Shipments_SEA_output.xml";
			mapTester.ExecuteCompiled<UniShip_2_BP_Shipment>(sourceFile, expectedFile);

            sourceFile = "UniShip_2_BP_Shipment.TestFiles.BP_Shipments_Multi_input.xml";
            expectedFile = "UniShip_2_BP_Shipment.TestFiles.BP_Shipments_Multi_output.xml";
            mapTester.ExecuteCompiled<UniShip_2_BP_Shipment>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_BP_Shipment.TestFiles.BP_Shipments_Empty_input.xml";
			using (Stream input = ResourceHelper.GetEmbeddedResource(testAssembly, sourceFile))
			{
				using (Stream expectedOutput = ResourceHelper.GetEmbeddedResource(testAssembly, expectedFile))
				{
					MapResult result = mapTester.MapCompiled<UniShip_2_BP_Shipment>(input, expectedOutput);
					Assert.IsFalse(result.Success);
					Assert.AreEqual(result.MapOutput, string.Empty);
				}
			}
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGenericEnvelope_2_BP_Shipment()
        {
            InitialiseCodeMapsTestingContext();

            var ctx = new TestingMessageContext();
            var ca = new ContextAccessor();
            ca.SetTestingMessageContext(ctx);

            List<string> exclusionXpaths = new List<string>();
            exclusionXpaths.Add("/*[local-name()='BP_Shipments']/*[local-name()='Shipment'][1]/*[local-name()='PROJECT']");

            ICompare comparer = new ExcludingComparer(exclusionXpaths);

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

            string sourceFile = "UniShip_2_BP_Shipment.TestFiles.GenericEnvelope_2_BP_Shipment_Input.xml";
            string expectedFile = "UniShip_2_BP_Shipment.TestFiles.GenericEnvelope_2_BP_Shipment_Output.xml";
            mapTester.ExecuteCompiled<GenericEnvelope_2_BP_Shipment>(sourceFile, expectedFile);

            Assert.AreEqual("263TG0006", ctx.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
        }

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();

            ctx.ActionProcedures.Add(new ActionProcedure
            {
                Procedure = "GetCounterInterfaceValue",
                OutputParm = "@StartValue",
                InputParms = new List<string> { 
			        "@TransformatonSetName", "BP To Project - Send Shipment Data",
			        "@Name", "FilenameCounter",
			        "@MaxValue", "9999",
			        "@IncrementValue", "1"
			    },
                Result = "6"
            });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
