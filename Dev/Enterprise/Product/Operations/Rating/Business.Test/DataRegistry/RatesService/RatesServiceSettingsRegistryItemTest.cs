using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RatesServiceSettingsRegistryItem))]
	class RatesServiceSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<RatesServiceRegistrySettingsCollection>
	{
		public void TestGetDefaultsForRatesServiceSubscription()
		{
			var registryItem = GetNewRegistryItem() as RatesServiceSettingsRegistryItem;
			var ratesServiceSubscriptionCollection = registryItem.DefaultValue;
			AssertEquals(2, ratesServiceSubscriptionCollection.Count);
			AssertEquals(true, registryItem.IsEnabled("SEA", "FCL"));
			AssertEquals(true, registryItem.IsEnabled("AIR", "LSE"));
			AssertEquals(true, registryItem.IsEnabled("SEA"));
			AssertEquals(true, registryItem.IsEnabled("AIR"));
		}

		public void TestEnablednessForTransportMode_OneRecordOnly()
		{
			var newValue = new RatesServiceRegistrySettingsCollection();
			newValue.Add(
				new RatesServiceRegistrySettings
				{
					TransportMode = Constants.TransportModes.Sea,
					ContainerMode = Constants.ContainerModes.FCL,
					IsSubscriptionEnabled = true
				}
			);
			var registryItem = GetNewRegistryItem() as RatesServiceSettingsRegistryItem;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			AssertEquals(1, registryItem.Value.Count);
			AssertEquals(true, registryItem.IsEnabled("SEA", "FCL"));
			AssertEquals(false, registryItem.IsEnabled("AIR", "LSE"));
			AssertEquals(true, registryItem.IsEnabled("SEA"));
			AssertEquals(false, registryItem.IsEnabled("AIR"));
		}

		public void TestEnablednessForTransportMode_ManyRecords()
		{
			var newValue = new RatesServiceRegistrySettingsCollection();
			newValue.AddRange(
				new RatesServiceRegistrySettings
				{
					TransportMode = Constants.TransportModes.Sea,
					ContainerMode = Constants.ContainerModes.FCL,
					IsSubscriptionEnabled = false
				},
				new RatesServiceRegistrySettings
				{
					TransportMode = Constants.TransportModes.Sea,
					ContainerMode = Constants.ContainerModes.BuyersConsol,
					IsSubscriptionEnabled = true
				},
				new RatesServiceRegistrySettings
				{
					TransportMode = Constants.TransportModes.Air,
					ContainerMode = Constants.ContainerModes.Loose,
					IsSubscriptionEnabled = false
				}
			);
			var registryItem = GetNewRegistryItem() as RatesServiceSettingsRegistryItem;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			AssertEquals(3, registryItem.Value.Count);
			AssertEquals(false, registryItem.IsEnabled("SEA", "FCL"));
			AssertEquals(false, registryItem.IsEnabled("AIR", "LSE"));
			AssertEquals(true, registryItem.IsEnabled("SEA"));
			AssertEquals(false, registryItem.IsEnabled("AIR"));
		}

		public void TestGetDefaultsForRatesServiceRateSelector()
		{
			var registryItem = GetNewRegistryItemForRatesServiceRateSelector() as RatesServiceSettingsRegistryItem;
			var ratesServiceRateSelectorCollection = registryItem.DefaultValue;
			AssertEquals(2, ratesServiceRateSelectorCollection.Count);
			AssertEquals(false, registryItem.IsEnabled("SEA", "FCL"));
			AssertEquals(false, registryItem.IsEnabled("AIR", "LSE"));
			AssertEquals(false, registryItem.IsEnabled("SEA"));
			AssertEquals(false, registryItem.IsEnabled("AIR"));
		}

		public void TestGetInvalid()
		{
			var registryItem = GetNewRegistryItem() as RatesServiceSettingsRegistryItem;

			AssertEquals(false, registryItem.IsEnabled("SEA", "???"));
			AssertEquals(false, registryItem.IsEnabled("???", "FCL"));
			AssertEquals(false, registryItem.IsEnabled("SEA", "LCL"));
			AssertEquals(false, registryItem.IsEnabled("", ""));
			AssertEquals(false, registryItem.IsEnabled("???"));
			AssertEquals(false, registryItem.IsEnabled("???"));
		}

		public void TestSettingAndGetting()
		{
			var registryItem = GetNewRegistryItem() as RatesServiceSettingsRegistryItem;
			var ratesServiceRegistrySettingsCollection = new RatesServiceRegistrySettingsCollection();

			var ratesServiceRegistrySettings = new RatesServiceRegistrySettings();
			ratesServiceRegistrySettingsCollection.Add(ratesServiceRegistrySettings);
			ratesServiceRegistrySettings.TransportMode = Core.Constants.TransportModes.Sea;
			ratesServiceRegistrySettings.ContainerMode = Core.Constants.ContainerModes.FCL;
			ratesServiceRegistrySettings.IsSubscriptionEnabled = true;

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ratesServiceRegistrySettingsCollection);

			AssertEquals(true, registryItem.IsEnabled(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL));
			AssertEquals(true, registryItem.IsAllowed());
		}

		public void TestDeserialise()
		{
			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				var root = new XElement("ArrayOfRatesServiceRegistrySettings",
					new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
					new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
					new XElement("RatesServiceRegistrySettings",
						new XElement("TransportMode", "SEA"),
						new XElement("ContainerMode", "FCL"),
						new XElement("IsSubscriptionEnabled", ZBool.True.ToString())),
					new XElement("RatesServiceRegistrySettings",
						new XElement("TransportMode", "AIR"),
						new XElement("ContainerMode", "LSE"),
						new XElement("IsSubscriptionEnabled", ZBool.False.ToString())));
				root.Save(writer, SaveOptions.DisableFormatting);
				var newValue = Encoding.Unicode.GetBytes(writer.ToString());

				var registryItem = GetNewRegistryItem() as RatesServiceSettingsRegistryItem;
				var ratesServiceRegistrySettingsCollection = registryItem.DataType.Deserialise(newValue) as RatesServiceRegistrySettingsCollection;

				CombineAssertions("Deserialise for RateServiceSubscription", () =>
				{
					AssertEquals("Transport Mode should be SEA", ratesServiceRegistrySettingsCollection[0].TransportMode, "SEA");
					AssertEquals("Container Mode should be FCL", ratesServiceRegistrySettingsCollection[0].ContainerMode, "FCL");
					AssertEquals("Subscription should be enabled", ratesServiceRegistrySettingsCollection[0].IsSubscriptionEnabled, true);

					AssertEquals("Transport Mode should be SEA", ratesServiceRegistrySettingsCollection[1].TransportMode, "AIR");
					AssertEquals("Container Mode should be FCL", ratesServiceRegistrySettingsCollection[1].ContainerMode, "LSE");
					AssertEquals("Subscription should be disabled", ratesServiceRegistrySettingsCollection[1].IsSubscriptionEnabled, false);
				});
			}

			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				var enabled = ZBool.False.ToString();

				var root = new XElement("ArrayOfRatesServiceRegistrySettings",
					new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
					new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
					new XElement("RatesServiceRegistrySettings",
						new XElement("TransportMode", "SEA"),
						new XElement("ContainerMode", "FCL"),
						new XElement("IsSubscriptionEnabled", enabled)),
					new XElement("RatesServiceRegistrySettings",
						new XElement("TransportMode", "AIR"),
						new XElement("ContainerMode", "LSE"),
						new XElement("IsSubscriptionEnabled", enabled)));
				root.Save(writer, SaveOptions.DisableFormatting);
				var newValue = Encoding.Unicode.GetBytes(writer.ToString());

				var registryItem = GetNewRegistryItem() as RatesServiceSettingsRegistryItem;
				var ratesServiceRegistrySettingsCollection = registryItem.DataType.Deserialise(newValue) as RatesServiceRegistrySettingsCollection;

				CombineAssertions("Deserialise for RateSelectorRegistry", () =>
				{
					AssertEquals("Transport Mode should be SEA", ratesServiceRegistrySettingsCollection[0].TransportMode, "SEA");
					AssertEquals("Container Mode should be FCL", ratesServiceRegistrySettingsCollection[0].ContainerMode, "FCL");
					AssertEquals("Subscription should be disabled", ratesServiceRegistrySettingsCollection[0].IsSubscriptionEnabled, false);

					AssertEquals("Transport Mode should be SEA", ratesServiceRegistrySettingsCollection[1].TransportMode, "AIR");
					AssertEquals("Container Mode should be FCL", ratesServiceRegistrySettingsCollection[1].ContainerMode, "LSE");
					AssertEquals("Subscription should be disabled", ratesServiceRegistrySettingsCollection[1].IsSubscriptionEnabled, false);
				});
			}
		}

		#region Implementation

		protected override StronglyTypedRegistryItem<RatesServiceRegistrySettingsCollection, RatesServiceRegistrySettingsCollection> GetNewRegistryItem()
		{
			return new RatesServiceSettingsRegistryItem(nameof(DataRegistryRating.Instance.RatesServiceSubscription), null, null, null, RegistryStorageFlags.Company);
		}

		protected StronglyTypedRegistryItem<RatesServiceRegistrySettingsCollection, RatesServiceRegistrySettingsCollection> GetNewRegistryItemForRatesServiceRateSelector()
		{
			return new RatesServiceSettingsRegistryItem(nameof(DataRegistryRating.Instance.RatesServiceRateSelector), null, null, null, RegistryStorageFlags.Company);
		}

		#endregion
	}

	[TestedType(typeof(RatesServiceSettingsRegistryDataType))]
	class RatesServiceSettingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<RatesServiceSettingsRegistryDataType>
	{
		#region Implementation

		protected override RatesServiceSettingsRegistryDataType GetNewDataType()
		{
			return new RatesServiceSettingsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "RatesServiceSettingsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var first = new RatesServiceRegistrySettingsCollection();

			var ratesServiceSubscription1 = new RatesServiceRegistrySettings();
			first.Add(ratesServiceSubscription1);
			ratesServiceSubscription1.TransportMode = Core.Constants.TransportModes.Sea;
			ratesServiceSubscription1.ContainerMode = Core.Constants.ContainerModes.FCL;
			ratesServiceSubscription1.IsSubscriptionEnabled = true;

			var ratesServiceSubscription2 = new RatesServiceRegistrySettings();
			first.Add(ratesServiceSubscription2);
			ratesServiceSubscription2.TransportMode = Core.Constants.TransportModes.Air;
			ratesServiceSubscription2.ContainerMode = Core.Constants.ContainerModes.Loose;
			ratesServiceSubscription2.IsSubscriptionEnabled = true;

			var firstSerialised = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfRatesServiceRegistrySettings xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><RatesServiceRegistrySettings><TransportMode>SEA</TransportMode><ContainerMode>FCL</ContainerMode><IsSubscriptionEnabled>Y</IsSubscriptionEnabled></RatesServiceRegistrySettings><RatesServiceRegistrySettings><TransportMode>AIR</TransportMode><ContainerMode>LSE</ContainerMode><IsSubscriptionEnabled>Y</IsSubscriptionEnabled></RatesServiceRegistrySettings></ArrayOfRatesServiceRegistrySettings>";

			var second = new RatesServiceRegistrySettingsCollection();

			var ratesServiceSubscription3 = new RatesServiceRegistrySettings();
			second.Add(ratesServiceSubscription3);
			ratesServiceSubscription3.TransportMode = Core.Constants.TransportModes.Sea;
			ratesServiceSubscription3.ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			ratesServiceSubscription3.IsSubscriptionEnabled = false;

			var ratesServiceSubscription4 = new RatesServiceRegistrySettings();
			second.Add(ratesServiceSubscription4);
			ratesServiceSubscription4.TransportMode = Core.Constants.TransportModes.Air;
			ratesServiceSubscription4.ContainerMode = Core.Constants.ContainerModes.Loose;
			ratesServiceSubscription4.IsSubscriptionEnabled = true;

			var secondSerialised = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfRatesServiceRegistrySettings xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><RatesServiceRegistrySettings><TransportMode>SEA</TransportMode><ContainerMode>BCN</ContainerMode><IsSubscriptionEnabled>N</IsSubscriptionEnabled></RatesServiceRegistrySettings><RatesServiceRegistrySettings><TransportMode>AIR</TransportMode><ContainerMode>LSE</ContainerMode><IsSubscriptionEnabled>Y</IsSubscriptionEnabled></RatesServiceRegistrySettings></ArrayOfRatesServiceRegistrySettings>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(first, Encoding.Unicode.GetBytes(firstSerialised)),
				new ValidSampleAndBinaryValueInDB(second, Encoding.Unicode.GetBytes(secondSerialised)),
			};
		}
		#endregion
	}
}
