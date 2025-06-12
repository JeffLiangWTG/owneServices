using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.eHub.Clients.GEO.Transforms.USCustomsACS2IBMUSCustomsABI;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.BizTalk.TestTools.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.GEO.Tests
{
	[TestClass]
	public class USCustomsACS2IBMUSCustomsABITests
	{
		CodeMapsTestingContext ctx;

		public USCustomsACS2IBMUSCustomsABITests()
		{
			ctx = new CodeMapsTestingContext();
			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}

		//[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUSCustomsACS2IBMUSCustomsABI()
		{
			string sourceFile = GetEmbeddedResourceFile("USCustomsACS2IBMUSCustomsABI.TestFiles.ABIexample.xml");
			string expectedFile = GetEmbeddedResourceFile("USCustomsACS2IBMUSCustomsABI.TestFiles.ABIexample_output.xml");
			string outputFile = Path.GetTempFileName();

			try
			{
				ctx.ActionProcedures.Clear();
				ctx.ActionProcedures.Add(new ActionProcedure { Procedure = "SelectSubscribedReference", OutputParm = "@reference", InputParms = new List<string> { "@senderId", "DDDEEEFFF", "@recipientId", "USC", "@ST_ID", "USCHAB", "@value", "00000057021" }, Result = "" });

				var map = new USCustomsACS2IBMUSCustomsABI();
				//map.TestMap(sourceFile, InputInstanceType.Xml, outputFile, OutputInstanceType.XML);

				string expectedFileContent = File.ReadAllText(expectedFile);
				string outputFileContent = File.ReadAllText(outputFile);

				Assert.AreEqual(expectedFileContent, outputFileContent.Remove(217,26).Insert(217,"2012-06-25-10.56.51.000000"));
			}
			finally
			{
				File.Delete(sourceFile);
				File.Delete(expectedFile);
				File.Delete(outputFile);
			}


			sourceFile = GetEmbeddedResourceFile("USCustomsACS2IBMUSCustomsABI.TestFiles.ABIexample4.xml");
			expectedFile = GetEmbeddedResourceFile("USCustomsACS2IBMUSCustomsABI.TestFiles.ABIexample4_output.xml");
			outputFile = Path.GetTempFileName();

			try
			{
				ctx.ActionProcedures.Clear();
				ctx.ActionProcedures.Add(new ActionProcedure { Procedure = "SelectSubscribedReference", OutputParm = "@reference", InputParms = new List<string> { "@senderId", "DDDEEEFFF", "@recipientId", "USC", "@ST_ID", "USCHAB", "@value", "00000057021" }, Result = "H000000001" });

				var map = new USCustomsACS2IBMUSCustomsABI();
				//map.TestMap(sourceFile, InputInstanceType.Xml, outputFile, OutputInstanceType.XML);

				string expectedFileContent = File.ReadAllText(expectedFile);
				string outputFileContent = File.ReadAllText(outputFile);

				Assert.AreEqual(expectedFileContent, outputFileContent.Remove(217, 26).Insert(217, "2012-06-25-10.57.41.000000"));
			}
			finally
			{
				File.Delete(sourceFile);
				File.Delete(expectedFile);
				File.Delete(outputFile);
			}
		}

		string GetEmbeddedResourceFile(string resourceName)
		{
			string tempFileName = Path.GetTempFileName();
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			using (Stream reader = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName))
			{
				using (Stream writer = new FileStream(tempFileName, FileMode.Create))
				{
					reader.CopyTo(writer);
					writer.Flush();
				}
			}
			return tempFileName;
		}
	}
}
