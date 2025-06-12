using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.CACustoms.Schemas;
using System.IO;
using Winterdom.BizTalk.PipelineTesting.Simple;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.eHub.Products.CACustoms.Tests
{
	[TestClass]
	public class SchemaTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Usage here is safe")]
		public void GOVCBRFlatFileSchema_Test1()
		{
			string inputXMLResourceName = "SchemaTests.Input.input_1.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests.Output.output_1.txt";

            using (var inputMsg = GetEmbeddedResource(inputXMLResourceName))
            using (var outputMsg = SchemaTester<EFACT_D13A_GOVCBR>.AssembleFF(inputMsg))
            using (var sr = new StreamReader(outputMsg))
                Assert.AreEqual(GetResourceAsString(expectedOutputFlatFileResourceName), sr.ReadToEnd());
		}

        public Stream GetEmbeddedResource(string resourceName)
        {
            string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
            if (resource == null)
            {
                throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
            }
            return resource;
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Usage here is safe")]
        public string GetResourceAsString(string resourceName)
        {
            using (var stream = GetEmbeddedResource(resourceName))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

	}
}
