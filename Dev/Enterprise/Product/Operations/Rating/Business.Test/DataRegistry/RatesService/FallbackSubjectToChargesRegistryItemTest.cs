using System;
using System.Text;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using WiseRates.Constants;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(FallbackSubjectToChargesRegistryItem))]
	class FallbackSubjectToChargesRegistryItemTest : StronglyTypedRegistryItemTestCase<FallbackSubjectToChargesCollection>
	{
		public void TestGetDefaults()
		{
			var registryItem = GetNewRegistryItem() as FallbackSubjectToChargesRegistryItem;
			var fallbackCollection = registryItem.DefaultValue;
			AssertEquals(0, fallbackCollection.Count);
		}

		public void TestGetInvalid()
		{
			var registryItem = GetNewRegistryItem() as FallbackSubjectToChargesRegistryItem;

			AssertEquals(false, registryItem.GetIsFallbackAllowed("CGSP", "SEA", "???"));
			AssertEquals(false, registryItem.GetIsFallbackAllowed("CGSP", "???", "FCL"));
			AssertEquals(false, registryItem.GetIsFallbackAllowed("??", "SEA", "LCL"));
			AssertEquals(false, registryItem.GetIsFallbackAllowed("", "", ""));
		}

		public void TestSettingAndGetting()
		{
			var registryItem = GetNewRegistryItem() as FallbackSubjectToChargesRegistryItem;
			var fallbackCollection = new FallbackSubjectToChargesCollection();
			var fallbackSubjectToCharge = new FallbackSubjectToCharges();

			fallbackCollection.Add(fallbackSubjectToCharge);
			fallbackSubjectToCharge.RatesProviderCode = WRConstants.RateProviders.CargoSphere;
			fallbackSubjectToCharge.TransportMode = Core.Constants.TransportModes.Sea;
			fallbackSubjectToCharge.ContainerMode = Core.Constants.ContainerModes.FCL;
			fallbackSubjectToCharge.IsFallbackEnabled = true;

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackCollection);

			AssertEquals(true, registryItem.GetIsFallbackAllowed(WRConstants.RateProviders.CargoSphere, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL));
		}

		#region Implementation

		protected override StronglyTypedRegistryItem<FallbackSubjectToChargesCollection, FallbackSubjectToChargesCollection> GetNewRegistryItem()
		{
			return new FallbackSubjectToChargesRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}

		#endregion
	}

	[TestedType(typeof(FallbackSubjectToChargesRegistryDataType))]
	class FallbackSubjectToChargesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<FallbackSubjectToChargesRegistryDataType>
	{
		#region Implementation

		protected override FallbackSubjectToChargesRegistryDataType GetNewDataType()
		{
			return new FallbackSubjectToChargesRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "FallbackSubjectToChargesRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var first = new FallbackSubjectToChargesCollection();

			var fallbackSubjectToCharge1 = new FallbackSubjectToCharges();
			first.Add(fallbackSubjectToCharge1);
			fallbackSubjectToCharge1.RatesProviderCode = WRConstants.RateProviders.CargoSphere;
			fallbackSubjectToCharge1.TransportMode = Core.Constants.TransportModes.Sea;
			fallbackSubjectToCharge1.ContainerMode = Core.Constants.ContainerModes.LCL;

			var fallbackSubjectToCharge2 = new FallbackSubjectToCharges();
			first.Add(fallbackSubjectToCharge2);
			fallbackSubjectToCharge2.RatesProviderCode = WRConstants.RateProviders.CargoSphere;
			fallbackSubjectToCharge2.TransportMode = Core.Constants.TransportModes.Sea;
			fallbackSubjectToCharge2.ContainerMode = Core.Constants.ContainerModes.FCL;

			var firstSerialised = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfFallbackSubjectToCharges xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><FallbackSubjectToCharges><RatesProviderCode>CGSP</RatesProviderCode><TransportMode>SEA</TransportMode><ContainerMode>LCL</ContainerMode><IsFallbackEnabled>N</IsFallbackEnabled></FallbackSubjectToCharges><FallbackSubjectToCharges><RatesProviderCode>CGSP</RatesProviderCode><TransportMode>SEA</TransportMode><ContainerMode>FCL</ContainerMode><IsFallbackEnabled>N</IsFallbackEnabled></FallbackSubjectToCharges></ArrayOfFallbackSubjectToCharges>";

			var second = new FallbackSubjectToChargesCollection();

			var fallbackSubjectToCharge3 = new FallbackSubjectToCharges();
			second.Add(fallbackSubjectToCharge3);
			fallbackSubjectToCharge3.RatesProviderCode = WRConstants.RateProviders.CargoGuide;
			fallbackSubjectToCharge3.TransportMode = Core.Constants.TransportModes.Air;
			fallbackSubjectToCharge3.ContainerMode = Core.Constants.ContainerModes.Loose;
			fallbackSubjectToCharge3.IsFallbackEnabled = true;

			var fallbackSubjectToCharge4 = new FallbackSubjectToCharges();
			second.Add(fallbackSubjectToCharge4);
			fallbackSubjectToCharge4.RatesProviderCode = WRConstants.RateProviders.CargoSphere;
			fallbackSubjectToCharge4.TransportMode = Core.Constants.TransportModes.Sea;
			fallbackSubjectToCharge4.ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			fallbackSubjectToCharge4.IsFallbackEnabled = true;

			var secondSerialised = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfFallbackSubjectToCharges xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><FallbackSubjectToCharges><RatesProviderCode>CGGD</RatesProviderCode><TransportMode>AIR</TransportMode><ContainerMode>LSE</ContainerMode><IsFallbackEnabled>Y</IsFallbackEnabled></FallbackSubjectToCharges><FallbackSubjectToCharges><RatesProviderCode>CGSP</RatesProviderCode><TransportMode>SEA</TransportMode><ContainerMode>BCN</ContainerMode><IsFallbackEnabled>Y</IsFallbackEnabled></FallbackSubjectToCharges></ArrayOfFallbackSubjectToCharges>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(first, Encoding.Unicode.GetBytes(firstSerialised)),
				new ValidSampleAndBinaryValueInDB(second, Encoding.Unicode.GetBytes(secondSerialised)),
			};
		}
		#endregion
	}
}
