using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Enterprise.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Registry.Business.Testing
{
	[TestedType(typeof(CargoSphereCredentialsRegistryItem))]
	class CargoSphereCredentialsRegistryItemTest : StronglyTypedRegistryItemTestCase<CargoSphereCredentials>
	{
		protected override StronglyTypedRegistryItem<CargoSphereCredentials, CargoSphereCredentials> GetNewRegistryItem()
		{
			return new CargoSphereCredentialsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}

		public void TestDeserializing()
		{
			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				var root = new XElement("CargoSphereCredentials",
					new XElement("Login", "dev.rating"),
					new XElement("EncryptedPassword", "q948VyfUUep5nu7AvB6Nnw=="),
					new XElement("SystemCode", "HYE"));
				root.Save(writer, SaveOptions.DisableFormatting);
				var actualSerializedValue = Encoding.Unicode.GetBytes(writer.ToString());

				var expectedCargoSphereCredentials = GetNewRegistryItem().DataType.Deserialise(actualSerializedValue) as CargoSphereCredentials;

				CombineAssertions(() =>
				{
					AssertEquals(expectedCargoSphereCredentials.Login, "dev.rating");
					AssertEquals(expectedCargoSphereCredentials.Password, "pass");
					AssertEquals(expectedCargoSphereCredentials.SystemCode, "HYE");
				});
			}
		}

		public void TestSerializing()
		{
			var registryItem = GetNewRegistryItem() as CargoSphereCredentialsRegistryItem;
			registryItem.Value.Login = "invalid";
			registryItem.Value.Password = "invalid";
			registryItem.Value.SystemCode = "invalid";

			var serialisedValue = registryItem.DataType.Serialise(new CargoSphereCredentials
			{
				Login = "dev.rating",
				SystemCode = "HYE",
				Password = "pass"
			});
			var actualXml = Encoding.Unicode.GetString(serialisedValue);

			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				var root = new XElement("CargoSphereCredentials",
					new XElement("Login", "dev.rating"),
					new XElement("EncryptedPassword", "q948VyfUUep5nu7AvB6Nnw=="),
					new XElement("SystemCode", "HYE"));
				root.Save(writer, SaveOptions.DisableFormatting);
				var expectedXml = writer.ToString();

				AssertXMLEquals(expectedXml, actualXml);
			}
		}
	}

	[TestedType(typeof(CargoSphereCredentialsRegistryDataType))]
	class CargoSphereCredentialsDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CargoSphereCredentialsRegistryDataType>
	{
		#region Implementation

		protected override CargoSphereCredentialsRegistryDataType GetNewDataType()
		{
			return new CargoSphereCredentialsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CargoSphereCredentialsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var item1 = new CargoSphereCredentials() { Login = "testLogin", Password = "testPassword", SystemCode = "testSystemCode" };

			byte[] byteArrayValue1 = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,
				105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,67,0,97,0,114,0,103,0,111,0,83,0,112,0,104,0,101,0,114,0,
				101,0,67,0,114,0,101,0,100,0,101,0,110,0,116,0,105,0,97,0,108,0,115,0,62,0,60,0,76,0,111,0,103,0,105,0,110,0,62,0,116,0,101,0,115,0,116,0,
				76,0,111,0,103,0,105,0,110,0,60,0,47,0,76,0,111,0,103,0,105,0,110,0,62,0,60,0,69,0,110,0,99,0,114,0,121,0,112,0,116,0,101,0,100,0,80,0,97,
				0,115,0,115,0,119,0,111,0,114,0,100,0,62,0,78,0,70,0,50,0,116,0,87,0,116,0,118,0,118,0,54,0,110,0,71,0,72,0,50,0,72,0,74,0,88,0,49,0,80,0,
				90,0,73,0,80,0,82,0,111,0,81,0,74,0,72,0,55,0,97,0,100,0,76,0,121,0,83,0,48,0,49,0,89,0,119,0,100,0,49,0,74,0,52,0,78,0,107,0,89,0,61,0,60,
				0,47,0,69,0,110,0,99,0,114,0,121,0,112,0,116,0,101,0,100,0,80,0,97,0,115,0,115,0,119,0,111,0,114,0,100,0,62,0,60,0,83,0,121,0,115,0,116,0,101,
				0,109,0,67,0,111,0,100,0,101,0,62,0,116,0,101,0,115,0,116,0,83,0,121,0,115,0,116,0,101,0,109,0,67,0,111,0,100,0,101,0,60,0,47,0,83,0,121,0,
				115,0,116,0,101,0,109,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,67,0,97,0,114,0,103,0,111,0,83,0,112,0,104,0,101,0,114,0,101,0,67,0,114,0,101,
				0,100,0,101,0,110,0,116,0,105,0,97,0,108,0,115,0,62,0
			};

			var item2 = new CargoSphereCredentials();

			byte[] byteArrayValue2 = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,
				105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,67,0,97,0,114,0,103,0,111,0,83,0,112,0,104,0,101,0,114,0,
				101,0,67,0,114,0,101,0,100,0,101,0,110,0,116,0,105,0,97,0,108,0,115,0,62,0,60,0,76,0,111,0,103,0,105,0,110,0,32,0,47,0,62,0,60,0,69,0,110,0,
				99,0,114,0,121,0,112,0,116,0,101,0,100,0,80,0,97,0,115,0,115,0,119,0,111,0,114,0,100,0,62,0,97,0,114,0,122,0,77,0,83,0,66,0,74,0,99,0,83,0,
				117,0,87,0,70,0,111,0,86,0,70,0,118,0,56,0,104,0,73,0,72,0,121,0,103,0,61,0,61,0,60,0,47,0,69,0,110,0,99,0,114,0,121,0,112,0,116,0,101,0,
				100,0,80,0,97,0,115,0,115,0,119,0,111,0,114,0,100,0,62,0,60,0,83,0,121,0,115,0,116,0,101,0,109,0,67,0,111,0,100,0,101,0,32,0,47,0,62,0,60,
				0,47,0,67,0,97,0,114,0,103,0,111,0,83,0,112,0,104,0,101,0,114,0,101,0,67,0,114,0,101,0,100,0,101,0,110,0,116,0,105,0,97,0,108,0,115,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(item1, byteArrayValue1),
				new ValidSampleAndBinaryValueInDB(item2, byteArrayValue2)
			};
		}

		#endregion
	}
}
