using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.INCustoms.Schema;
using CargoWise.eHub.Products.INCustoms.Schema.Manifest;
using CargoWise.eHub.Products.INCustoms.Transform.CMCHI21AUniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting.Simple;
using System.IO;
using System.Text;
using System;

namespace CargoWise.eHub.Products.INCustoms.Tests
{
	[TestClass]
	public class CMCHI21AUniversalEventTest
	{
        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CMCHI21AUniversalEventTest_Reject()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "TestFiles.CMCHI21AUniversalEventInput_1.xml";
			string outputFile = "TestFiles.CMCHI21AUniversalEventOutput_1.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<CHCMI21AToUniversalEvent>(sourceFile, outputFile);
		}

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CMCHI21AUniversalEventTest_Accept()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "TestFiles.CMCHI21AUniversalEventInput_2.xml";
			string outputFile = "TestFiles.CMCHI21AUniversalEventOutput_2.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<CHCMI21AToUniversalEvent>(sourceFile, outputFile);
		}

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CMCHI21AFlatFileSchema_Test1()
		{
			using (var inputMsg = GetEmbeddedResource("Sample.CMCHI21A.txt"))
			using (var outputMsg = SchemaTester<CMCHI21ASchema>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg))
			{
				Assert.AreEqual(GetResourceAsString("SchemaValidation.CMCHI21ASample.xml"), sr.ReadToEnd());
			}
		}

		static Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
			}
			return resource;
		}

		static string GetResourceAsString(string resourceName)
		{
			using (var stream = GetEmbeddedResource(resourceName))
			{
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}

		static void InitialiseMessageTestingContext() { }

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "INCustoms" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "ICES - Sea Consol Manifest", eHubClient_Sender = ctx.eHubClients[0] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "ErrorDetail", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "ErrorDescription" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "000 "});
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "IGM Submitted" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "129" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Vessel Name cannot be Amended" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "270" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "HBL No has Special Characters" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "282"});
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Destination Code is invalid" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "325" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Seal No. Cannot be NULL" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 6, CK_Key1Value = "453"});
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "BL No./ BL Date Not matching with corresponding IGM" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 7, CK_Key1Value = "601"});
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Data Length Error or Value Large Error" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 8, CK_Key1Value = "%"});
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "No detailed description for this error" });

			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "ErrorSolution" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "129" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Do not amend Vessel Name" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "270" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Remove Special Characters From HBL No" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "No detailed description for this error" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}