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
	[TestedType(typeof(CargoguideCredentialsRegistryItem))]
	class CargoGuideCredentialsRegistryItemTest : StronglyTypedRegistryItemTestCase<CargoGuideCredentials>
	{
		protected override StronglyTypedRegistryItem<CargoGuideCredentials, CargoGuideCredentials> GetNewRegistryItem()
		{
			return new CargoguideCredentialsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}

		public void TestDeserializing()
		{
			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				var root = new XElement("CargoGuideCredentials",
					new XElement("Login", "WTGDevelopment@cargoguide.com"),
					new XElement("EncryptedPassword", "u8EfB56kRZs543TXUVPbyA=="));
				root.Save(writer, SaveOptions.DisableFormatting);
				var actualSerializedValue = Encoding.Unicode.GetBytes(writer.ToString());

				var expectedCargoGuideCredentials = GetNewRegistryItem().DataType.Deserialise(actualSerializedValue) as CargoGuideCredentials;

				CombineAssertions(() =>
				{
					AssertEquals(expectedCargoGuideCredentials.Login, "WTGDevelopment@cargoguide.com");
					AssertEquals(expectedCargoGuideCredentials.Password, "pass");
				});
			}
		}

		public void TestSerializing()
		{
			var registryItem = GetNewRegistryItem() as CargoguideCredentialsRegistryItem;
			registryItem.Value.Login = "invalid";
			registryItem.Value.Password = "invalid";

			var serialisedValue = registryItem.DataType.Serialise(new CargoGuideCredentials
			{
				Login = "WTGDevelopment@cargoguide.com",
				Password = "pass"
			});
			var actualXml = Encoding.Unicode.GetString(serialisedValue);

			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				var root = new XElement("CargoGuideCredentials",
					new XElement("Login", "WTGDevelopment@cargoguide.com"),
					new XElement("EncryptedPassword", "u8EfB56kRZs543TXUVPbyA=="));
				root.Save(writer, SaveOptions.DisableFormatting);
				var expectedXml = writer.ToString();

				AssertXMLEquals(expectedXml, actualXml);
			}
		}
	}

	[TestedType(typeof(CargoGuideCredentialsRegistryDataType))]
	class CargoGuideCredentialsDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CargoGuideCredentialsRegistryDataType>
	{
		#region Implementation

		protected override CargoGuideCredentialsRegistryDataType GetNewDataType()
		{
			return new CargoGuideCredentialsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CargoGuideCredentialsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var item1 = new CargoGuideCredentials { Login = "testLogin", Password = "testPassword" };

			byte[] byteArrayValue1 =
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,67,0,97,0,114,0,103,0,111,0,71,0,117,0,105,0,100,0,101,0,67,0,114,0,101,0,100,0,101,0,110,0,116,0,105,0,97,0,108,0,115,0,62,0,60,0,76,
				0,111,0,103,0,105,0,110,0,62,0,116,0,101,0,115,0,116,0,76,0,111,0,103,0,105,0,110,0,60,0,47,0,76,0,111,0,103,0,105,0,110,0,62,0,60,0,69,0,110,0,99,0,114,0,121,0,112,0,116,0,101,
				0,100,0,80,0,97,0,115,0,115,0,119,0,111,0,114,0,100,0,62,0,97,0,49,0,99,0,53,0,85,0,75,0,72,0,111,0,48,0,87,0,48,0,72,0,120,0,98,0,56,0,76,0,57,0,77,0,117,0,43,0,90,0,85,0,69,0,
				78,0,69,0,75,0,48,0,117,0,82,0,79,0,54,0,116,0,111,0,100,0,75,0,67,0,47,0,72,0,48,0,105,0,48,0,89,0,85,0,61,0,60,0,47,0,69,0,110,0,99,0,114,0,121,0,112,0,116,0,101,0,100,0,80,0,
				97,0,115,0,115,0,119,0,111,0,114,0,100,0,62,0,60,0,47,0,67,0,97,0,114,0,103,0,111,0,71,0,117,0,105,0,100,0,101,0,67,0,114,0,101,0,100,0,101,0,110,0,116,0,105,0,97,0,108,0,115,0,
				62,0
			};

			var item2 = new CargoGuideCredentials();

			byte[] byteArrayValue2 =
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,67,0,97,0,114,0,103,0,111,0,71,0,117,0,105,0,100,0,101,0,67,0,114,0,101,0,100,0,101,0,110,0,116,0,105,0,97,0,108,0,115,0,62,0,60,0,76,
				0,111,0,103,0,105,0,110,0,32,0,47,0,62,0,60,0,69,0,110,0,99,0,114,0,121,0,112,0,116,0,101,0,100,0,80,0,97,0,115,0,115,0,119,0,111,0,114,0,100,0,62,0,76,0,107,0,55,0,83,0,88,0,97,
				0,97,0,52,0,82,0,103,0,120,0,67,0,51,0,50,0,117,0,50,0,114,0,43,0,122,0,122,0,72,0,103,0,61,0,61,0,60,0,47,0,69,0,110,0,99,0,114,0,121,0,112,0,116,0,101,0,100,0,80,0,97,0,115,0,
				115,0,119,0,111,0,114,0,100,0,62,0,60,0,47,0,67,0,97,0,114,0,103,0,111,0,71,0,117,0,105,0,100,0,101,0,67,0,114,0,101,0,100,0,101,0,110,0,116,0,105,0,97,0,108,0,115,0,62,0
			};

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(item1, byteArrayValue1),
				new ValidSampleAndBinaryValueInDB(item2, byteArrayValue2)
			};
		}

		#endregion
	}
}
