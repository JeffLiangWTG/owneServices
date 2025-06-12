using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.PML.Transforms.EDIShipmentInternal2X12_00401_214;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using Microsoft.BizTalk.TestTools.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.DataAccess.Sql;

namespace Tests
{
	[TestClass]
	public class EDIShipmentInternal2X12_00401_214Test
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIShipmentInternal2X12_00401_214_LeftTrimsForL11()
		{
			InitialiseCodeMapsTestingContext();

			string sourceFile = GetFileWithEmbeddedResource("EDIShipmentInternal2X12_00401_214.TestFiles.EDIShipmentsInternal_WithLeadingSpaceInReference.xml");
			string expectedFile = GetFileWithEmbeddedResource("EDIShipmentInternal2X12_00401_214.TestFiles.EDIShipmentInternal2X12_00401_214_output.xml");
			string outputFile = Path.GetTempFileName();

			try
			{
				EDIShipmentInternal2X12_00401_214 map = new EDIShipmentInternal2X12_00401_214();
				map.TestMap(sourceFile, InputInstanceType.Xml, outputFile, OutputInstanceType.XML);

				string outputFileContent = File.ReadAllText(outputFile);

				string expectedFileContent = File.ReadAllText(expectedFile);
				Assert.AreEqual<string>(expectedFileContent, outputFileContent);
			}
			finally
			{
				FileDelete(sourceFile);
				FileDelete(expectedFile);
				FileDelete(outputFile);
			}

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIShipmentInternal2X12_00401_214()
		{
			InitialiseCodeMapsTestingContext();

			string sourceFile = GetFileWithEmbeddedResource("EDIShipmentInternal2X12_00401_214.TestFiles.EDIShipmentsInternal_WithoutTriggeredBy_input.xml");
			string expectedFile = GetFileWithEmbeddedResource("EDIShipmentInternal2X12_00401_214.TestFiles.EDIShipmentInternal_WithoutTriggeredBy_output.xml");
			string outputFile = Path.GetTempFileName();

			try
			{
				EDIShipmentInternal2X12_00401_214 map = new EDIShipmentInternal2X12_00401_214();
				map.TestMap(sourceFile, InputInstanceType.Xml, outputFile, OutputInstanceType.XML);

				string outputFileContent = File.ReadAllText(outputFile);

				string expectedFileContent = File.ReadAllText(expectedFile);
				Assert.AreEqual<string>(expectedFileContent, outputFileContent);
			}
			finally
			{
				FileDelete(sourceFile);
				FileDelete(expectedFile);
				FileDelete(outputFile);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIShipmentInternal2X12_00401_214WhenInformationTagIsMissingFromEvent()
		{
			InitialiseCodeMapsTestingContext();

			string sourceFile = "EDIShipmentInternal2X12_00401_214.TestFiles.EDIShipmentsInternalWithMissingInfoTag.xml";
			string expectedFile = "EDIShipmentInternal2X12_00401_214.TestFiles.X12_00401_214_HandlingMissingInfoTag.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<EDIShipmentInternal2X12_00401_214>(sourceFile, expectedFile);
		}

		ICompare Comparer
		{
			get
			{
				if (comparer == null)
				{
					List<string> exclusionXpaths = new List<string>();
					exclusionXpaths.Add("/*[local-name()='ShipmentStatus']/*[local-name()='Envelope']/*[local-name()='EnvelopeID']");
					exclusionXpaths.Add("/*[local-name()='ShipmentStatus']/*[local-name()='ShipmentStatusDetails']/*[local-name()='DocumentationDetails']/*[local-name()='Image']");
					comparer = new ExcludingComparer(exclusionXpaths);
				}

				return comparer;
			}
		}

		ICompare comparer;

		string GetFileWithEmbeddedResource(string resourceName)
		{
			string tempFileName = Path.GetTempFileName();
			using (Stream reader = GetEmbeddedResource(resourceName))
			{
				using (Stream writer = new FileStream(tempFileName, FileMode.Create))
				{
					AddStream(reader, writer);
				}
			}

			return tempFileName;
		}

		void AddStream(Stream reader, Stream writer)
		{
			byte[] buffer = new byte[32 * 1024];
			while (true)
			{
				int read = reader.Read(buffer, 0, buffer.Length);
				writer.Write(buffer, 0, read);
				if (read != buffer.Length) break;
			}
			writer.Flush();
		}

		Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
			}
			return resource;
		}

		void FileDelete(string fileName)
		{
			try
			{
				File.Delete(fileName);
			}
			catch { }
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "PMLDFWCAX" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "PMLDFWCAX_PIE" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Pier 1 ANSI 214 - Export Domestic Shipment Status", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Status Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Pier1 Status Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "PSD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SD" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}