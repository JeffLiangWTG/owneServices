using System;
using System.IO;
using System.Reflection;
using CargoWise.eHub.Clients.YAS.Transforms.EDIFHL2EDIFHL;
using Microsoft.BizTalk.TestTools.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.YAS.Tests
{
	[TestClass]
	public class EDIFHL2EDIFHLTests
	{

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIFHL2EDIFHL()
		{
			string sourceFile = GetFileWithEmbeddedResource("EDIFHL2EDIFHL.TestFiles.StandardFHL.txt");
			string expectedFile = GetFileWithEmbeddedResource("EDIFHL2EDIFHL.TestFiles.DeltaFHL.txt");
			string outputFile = Path.GetTempFileName();

			try
			{
				EDIFHL2EDIFHL map = new EDIFHL2EDIFHL();
				map.TestMap(sourceFile, InputInstanceType.Native, outputFile, OutputInstanceType.Native);

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

        string GetFileWithEmbeddedResource(string resourceName)
        {
            string tempFileName = Path.GetTempFileName();
            using (Stream reader = GetEmbeddedResource(resourceName))
            {
                using( Stream writer = new FileStream(tempFileName, FileMode.Create))
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
                if( read != buffer.Length) break;
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

	}
}
