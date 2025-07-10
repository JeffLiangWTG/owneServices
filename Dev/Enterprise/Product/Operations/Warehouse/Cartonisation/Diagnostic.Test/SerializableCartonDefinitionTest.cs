using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic.Testing
{
	public class SerializableCartonDefinitionTest : TestCase
	{
		public void TestIsSerialisable()
		{
			var item = new SerializableCartonDefinition();
			var serialiser = new XmlSerializer(typeof(SerializableCartonDefinition));

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
