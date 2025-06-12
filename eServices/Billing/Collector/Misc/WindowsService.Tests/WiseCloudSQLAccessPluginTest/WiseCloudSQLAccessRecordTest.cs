using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WiseCloudSQLAccess;
using CargoWise.eServices.Billing.Tests.Common;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Tests.WiseCloudSQLAccessPluginTest
{
	[TestFixture]
	public class WiseCloudSQLAccessRecordTest
	{
		[TestCaseSource(nameof(QueryResultXmlFiles))]
		public void TestDeserializationFromXmlElement(string resourceFileName)
		{
			var document = TestHelper.LoadFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, resourceFileName);
			var rows = document.GetElementsByTagName("row");
			Assert.That(rows, Is.Not.Null);

			var xmlSerializer = new XmlSerializer(typeof(WiseCloudSQLAccessRecord), Helper.SolarwindsNamespace);
			foreach (XmlElement row in rows)
			{
				using (var reader = new StringReader(row.OuterXml))
				{
					var record = xmlSerializer.Deserialize(reader);
					Assert.That(record, Is.Not.Null);
				}
			}
		}

		static readonly IList<TestCaseData> QueryResultXmlFiles = new[]
		{
			new TestCaseData("SolarWindQueryResult-1.xml")
		};
	}
}
