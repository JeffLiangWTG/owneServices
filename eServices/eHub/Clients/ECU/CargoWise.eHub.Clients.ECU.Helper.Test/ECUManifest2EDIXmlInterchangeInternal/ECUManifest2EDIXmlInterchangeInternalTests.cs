using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Microsoft.BizTalk.TestTools.Schema;
using CargoWise.BizTalk.UnitTestFX;
using System.Reflection;
using CargoWise.eHub.Clients.ECU.Transforms.ECUManifest2EDIConsolsInternal;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;

namespace CargoWise.eHub.Clients.ECU.Helper.Tests
{
	/// <summary>
	/// Summary description for ECUManifest2EDIXmlInterchangeInternalTests
	/// </summary>
	[TestClass]
	public class ECUManifest2EDIXmlInterchangeInternalTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestECUManifest2EDIXmlInterchangeInternal()
		{
			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("/*[local-name()='ConsolsInternal']/*[local-name()='InterchangeInfo']/*[local-name()='Date']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "ECUManifest2EDIXmlInterchangeInternal.TestFiles.single_container_example_ex_Ecu-Line Montreal.xml";
			string expectedFile = "ECUManifest2EDIXmlInterchangeInternal.TestFiles.single_container_example_ex_Ecu-Line Montreal_Output.xml";
			mapTester.ExecuteCompiled<ECUManifest2EDIConsolsInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestECUManifest2EDIXmlInterchangeInternalWithMultipleContainers()
		{
			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("/*[local-name()='ConsolsInternal']/*[local-name()='InterchangeInfo']/*[local-name()='Date']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "ECUManifest2EDIXmlInterchangeInternal.TestFiles.multiple_container_example_ex_Ecu-Line Antwerp.xml";
			string expectedFile = "ECUManifest2EDIXmlInterchangeInternal.TestFiles.multiple_container_example_ex_Ecu-Line Antwerp_Output.xml";
            mapTester.ExecuteCompiled<ECUManifest2EDIConsolsInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTrimContainerNumber()
		{
			Assert.AreEqual<string>("PONU7399201", TrimContainerNumber("PONU 739920/1"));
			Assert.AreEqual<string>("", TrimContainerNumber(""));
		}

		public string TrimContainerNumber(string inputContainerName)
		{
			try
			{
				return inputContainerName.Replace(" ", "").Replace("/", "").Substring(0, 11);
			}
			catch
			{
				return inputContainerName;
			}
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "ECUMELAKL_ECU" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "ECUMELAKL" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "ECU xml-File - Import of Consols", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Container Mode", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Output Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "COL" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "LCL" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
