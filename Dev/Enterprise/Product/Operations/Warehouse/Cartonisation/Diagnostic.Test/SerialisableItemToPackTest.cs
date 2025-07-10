using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic.Testing
{
	public class SerialisableItemToPackTest : TestCase
	{
		public void TestIsSerialisable()
		{
			var item = new SerialisableItemToPack();
			var serialiser = new XmlSerializer(typeof(SerialisableItemToPack));

			using (var outStream = new StringWriter())
			{
				AssertNoExceptionThrown(() => serialiser.Serialize(outStream, item));
				using (var inStream = new StringReader(outStream.ToString()))
				{
					AssertNoExceptionThrown(() => serialiser.Deserialize(inStream));
				}
			}
		}
	}
}
