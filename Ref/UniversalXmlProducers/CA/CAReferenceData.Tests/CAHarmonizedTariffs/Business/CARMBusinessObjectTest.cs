using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	abstract class CARMBusinessObjectTest<T>
	{
		[Test]
		public void TestDeserializeXML()
		{
			var serializer = new XmlSerializer(typeof(CARMResponseFeed<T>));
			foreach (var item in TestCasesForDeserializeXML)
			{
				using (var stream = new StringReader(item.xml))
				using (var reader = XmlReader.Create(stream))
				{
					var responseFeed = ((CARMResponseFeed<T>)serializer.Deserialize(reader));
					foreach (var entry in responseFeed.Entry)
					{
						AssertDeserializedEntry(entry.Content.Properties, item.caseID);
					}
				}
			}
		}

		protected abstract void AssertDeserializedEntry(T entryProperties, int caseID);

		protected abstract System.Collections.Generic.IEnumerable<(int caseID, string xml)> TestCasesForDeserializeXML { get; }
	}
}
