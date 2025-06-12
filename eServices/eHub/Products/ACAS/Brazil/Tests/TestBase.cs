using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Schema;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.ACAS.BR.Tests
{
	public class TestBase
	{
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

		public string GetResourceAsString(string resourceName)
		{
			using (var stream = GetEmbeddedResource(resourceName))
			{
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}

        public void ValidateSchema<SchemaUnderTest>(string inputFileName) where SchemaUnderTest : SchemaBase, new()
        {
            var document = XDocument.Load(GetEmbeddedResource(inputFileName));
            var report = String.Empty;
            document.Validate(new SchemaUnderTest().SchemaSet, (_, validationEventArgs) =>
                {
                    report = validationEventArgs.Message;
                }
            );
            Assert.AreEqual(string.Empty, report, typeof(SchemaUnderTest).Name + "is not valid: \r\n" + report);
        }
	}
}