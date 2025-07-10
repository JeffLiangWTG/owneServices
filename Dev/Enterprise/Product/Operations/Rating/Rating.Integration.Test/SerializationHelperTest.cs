using System.Globalization;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Rating.Integration.Test
{
	internal class SerializationHelperTest : TestCase
	{
		public void TestSerialize()
		{
			SerializableForTest serializable = new SerializableForTest("One", "Two");
			AssertEquals("Serialized as expected", XML, serializable.SerializeToString());
		}

		public void TestDeserialize()
		{
			SerializableForTest serializable = new SerializableForTest();
			AssertEquals("Precondition", "", serializable.Property1);
			AssertEquals("Precondition", "", serializable.Property2);

			serializable.DeserializeFromString(XML);
			AssertEquals("Deserialized value", "One", serializable.Property1);
			AssertEquals("Deserialized value", "Two", serializable.Property2);

			AssertExceptionThrown("Exceptions are thrown", typeof(XmlException), delegate
			{
				serializable.DeserializeFromString("not a proper xml");
			});
		}

		public void TestDecimalToInvariantString()
		{
			var originalCultureInfo = Thread.CurrentThread.CurrentCulture;

			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
				var decimalValue = 20.14m;

				AssertEquals("Can NOT convert correctly", "20,14", decimalValue.ToString());
				AssertEquals("Convert correctly when use invariant culture", "20.14", SerializationHelper.DecimalToInvariantString(decimalValue));
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = originalCultureInfo;
			}
		}

		public void TestInvariantStringToDecimal()
		{
			var originalCultureInfo = Thread.CurrentThread.CurrentCulture;

			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
				string decimalString = "20.14";

				decimal decimalOut;
				decimal.TryParse(decimalString, out decimalOut);

				AssertEquals("Can NOT convert correctly", 0m, decimalOut);
				AssertEquals("Convert correctly when use invariant culture", 20.14m, SerializationHelper.InvariantStringToDecimal(decimalString));
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = originalCultureInfo;
			}
		}

		#region Implementation

		const string XML = "<Dummy><Hello>One</Hello><World>Two</World></Dummy>";

		class SerializableForTest : IXmlSerializable
		{
			public SerializableForTest()
			{
			}

			public SerializableForTest(ZString property1, ZString property2)
			{
				Property1 = property1;
				Property2 = property2;
			}

			public ZString Property1 { get; set; }
			public ZString Property2 { get; set; }

			System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
			{
				return null;
			}

			void IXmlSerializable.WriteXml(XmlWriter writer)
			{
				writer.WriteStartElement("Dummy");
				writer.WriteElementString("Hello", Property1);
				writer.WriteElementString("World", Property2);
				writer.WriteEndElement();
				writer.Flush();
			}

			void IXmlSerializable.ReadXml(XmlReader reader)
			{
				reader.ReadStartElement();
				Property1 = reader.ReadElementString("Hello");
				Property2 = reader.ReadElementString("World");
				reader.ReadEndElement();
			}
		}

		#endregion
	}
}
