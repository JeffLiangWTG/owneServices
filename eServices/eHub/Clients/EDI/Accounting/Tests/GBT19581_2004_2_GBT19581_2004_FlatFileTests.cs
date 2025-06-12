using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Accounting.Transforms.GBT19581_2004_2_FlatFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using System.Text;
using Microsoft.BizTalk.TestTools.Schema;
using Microsoft.BizTalk.TestTools.Mapper;

namespace CargoWise.eHub.Clients.EDI.Accounting.Tests
{
	[TestClass]
	public class GBT19581_2004_2_GBT19581_2004_FlatFileTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMap()
		{
			var map = new ChinaStandard_GBT19581_2004_2_CN_GBT19581_2004();
			TestMap(map, "TestFiles.DZZB.xml", "TestFiles.DZZB.txt");
			TestMap(map, "TestFiles.JZPZ.xml", "TestFiles.JZPZ.txt");
			TestMap(map, "TestFiles.KJKM.xml", "TestFiles.KJKM.txt");
			TestMap(map, "TestFiles.KMYE.xml", "TestFiles.KMYE.txt");
			TestMap(map, "TestFiles.Q_ZCFZ.xml", "TestFiles.Q_ZCFZ.txt");
			TestMap(map, "TestFiles.Q_LR.xml", "TestFiles.Q_LR.txt");
			TestMap(map, "TestFiles.BMXX.xml", "TestFiles.BMXX.txt");
			TestMap(map, "TestFiles.WLDW.xml", "TestFiles.WLDW.txt");
			TestMap(map, "TestFiles.YGXX.xml", "TestFiles.YGXX.txt");

			TestMap(map, "TestFiles.Q_ZZS.XML", "TestFiles.Q_ZZS.txt");
			TestMap(map, "TestFiles.Q_JZZB.XML", "TestFiles.Q_JZZB.txt");
			TestMap(map, "TestFiles.Q_LRFP.XML", "TestFiles.Q_LRFP.txt");
			TestMap(map, "TestFiles.Q_GDQYBD.XML", "TestFiles.Q_GDQYBD.txt");
			TestMap(map, "TestFiles.Q_XJLL.xml", "TestFiles.Q_XJLL.txt");
		}
	
		#region Implementation

		#region TestMap

		public void TestMap(TestableMapBase map, string inputResourceName, string expectedOutputResourceName)
		{
			InitialiseCodeMapsTestingContext();

			string inputFile = GetFileWithEmbeddedResource(inputResourceName);
			string outputFile = Path.GetTempFileName();

			try
			{
				string expectedFileContent = GetResourceAsString(expectedOutputResourceName);

				map.TestMap(inputFile, GetInputInstanceType(inputResourceName), outputFile, GetOutputInstanceType(expectedOutputResourceName));
				string actualFileContent = File.ReadAllText(outputFile);

				Assert.AreEqual<string>(expectedFileContent, actualFileContent);
			}
			finally
			{
				FileDelete(inputFile);
				FileDelete(outputFile);
			}
		}

		private static void InitialiseCodeMapsTestingContext()
		{
		}

		#endregion

		#region Get Instance Type

		InputInstanceType GetInputInstanceType(string resourceName)
		{
			return IsXmlResource(resourceName) ? InputInstanceType.Xml : InputInstanceType.Native;
		}

		OutputInstanceType GetOutputInstanceType(string resourceName)
		{
			return IsXmlResource(resourceName) ? OutputInstanceType.XML : OutputInstanceType.Native;
		}

		bool IsXmlResource(string resourceName)
		{
			return resourceName.ToLower().EndsWith(".xml");
		}

		#endregion

		#region Embedded Resource and File

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

		string GetResourceAsString(string resourceName)
		{
			using (var stream = GetEmbeddedResource(resourceName))
			{
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}

		void FileDelete(string fileName)
		{
			try
			{
				File.Delete(fileName);
			}
			catch { }
		}

		#endregion

		#endregion
	}
}
