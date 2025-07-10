using System;
using System.Text;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(SameChargeCodeDifferentProviderRegistryItem))]
	class SameChargeCodeDifferentProviderRegistryItemTest : StronglyTypedRegistryItemTestCase<SameChargeCodeDifferentProviderCollection>
	{
		public void TestGetDefaults()
		{
			var registryItem = GetNewRegistryItem() as SameChargeCodeDifferentProviderRegistryItem;
			var fallbackCollection = registryItem.DefaultValue;
			AssertEquals(0, fallbackCollection.Count);
		}

		public void TestGetInvalid()
		{
			var registryItem = GetNewRegistryItem() as SameChargeCodeDifferentProviderRegistryItem;

			AssertEquals(false, registryItem.GetIsEnabled("SHP", "SEA", "???"));
			AssertEquals(false, registryItem.GetIsEnabled("SHP", "???", "IMP"));
			AssertEquals(false, registryItem.GetIsEnabled("??", "SEA", "IMP"));
			AssertEquals(false, registryItem.GetIsEnabled("", "", ""));
		}

		public void TestSettingAndGetting()
		{
			var registryItem = GetNewRegistryItem() as SameChargeCodeDifferentProviderRegistryItem;
			var fallbackCollection = new SameChargeCodeDifferentProviderCollection();
			var fallbackSubjectToCharge = new SameChargeCodeDifferentProvider();

			fallbackCollection.Add(fallbackSubjectToCharge);
			fallbackSubjectToCharge.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			fallbackSubjectToCharge.TransportMode = Core.Constants.TransportModes.Sea;
			fallbackSubjectToCharge.Direction = Core.Constants.FreightShipmentDirection.Code.Export;
			fallbackSubjectToCharge.IsEnabled = true;

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackCollection);

			Assert(registryItem.GetIsEnabled(JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.TransportModes.Sea, Core.Constants.FreightShipmentDirection.Code.Export));
		}

		#region Implementation

		protected override StronglyTypedRegistryItem<SameChargeCodeDifferentProviderCollection, SameChargeCodeDifferentProviderCollection> GetNewRegistryItem()
		{
			return new SameChargeCodeDifferentProviderRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}

		#endregion
	}

	[TestedType(typeof(SameChargeCodeDifferentProviderRegistryDataType))]
	class SameChargeCodeDifferentProviderRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SameChargeCodeDifferentProviderRegistryDataType>
	{
		#region Implementation

		protected override SameChargeCodeDifferentProviderRegistryDataType GetNewDataType()
		{
			return new SameChargeCodeDifferentProviderRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "SameChargeCodeDifferentProviderRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var first = new SameChargeCodeDifferentProviderCollection();

			var fallbackSubjectToCharge1 = new SameChargeCodeDifferentProvider();
			first.Add(fallbackSubjectToCharge1);
			fallbackSubjectToCharge1.JobType = JobInvoicingConsumerTypes.ShipmentCode;
			fallbackSubjectToCharge1.TransportMode = Core.Constants.TransportModes.Sea;
			fallbackSubjectToCharge1.Direction = Core.Constants.FreightShipmentDirection.Code.Export;

			var fallbackSubjectToCharge2 = new SameChargeCodeDifferentProvider();
			first.Add(fallbackSubjectToCharge2);
			fallbackSubjectToCharge2.JobType = JobInvoicingConsumerTypes.ShipmentCode;
			fallbackSubjectToCharge2.TransportMode = Core.Constants.TransportModes.Sea;
			fallbackSubjectToCharge2.Direction = Core.Constants.FreightShipmentDirection.Code.Import;

			var firstSerialised = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfSameChargeCodeDifferentProvider xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><SameChargeCodeDifferentProvider><JobType>SHP</JobType><TransportMode>SEA</TransportMode><Direction>EXP</Direction><IsEnabled>N</IsEnabled></SameChargeCodeDifferentProvider><SameChargeCodeDifferentProvider><JobType>SHP</JobType><TransportMode>SEA</TransportMode><Direction>IMP</Direction><IsEnabled>N</IsEnabled></SameChargeCodeDifferentProvider></ArrayOfSameChargeCodeDifferentProvider>";

			var second = new SameChargeCodeDifferentProviderCollection();

			var fallbackSubjectToCharge3 = new SameChargeCodeDifferentProvider();
			second.Add(fallbackSubjectToCharge3);
			fallbackSubjectToCharge3.JobType = JobInvoicingConsumerTypes.ShipmentCode;
			fallbackSubjectToCharge3.TransportMode = Core.Constants.TransportModes.Air;
			fallbackSubjectToCharge3.Direction = Core.Constants.FreightShipmentDirection.Code.Export;
			fallbackSubjectToCharge3.IsEnabled = true;

			var fallbackSubjectToCharge4 = new SameChargeCodeDifferentProvider();
			second.Add(fallbackSubjectToCharge4);
			fallbackSubjectToCharge4.JobType = JobInvoicingConsumerTypes.ShipmentCode;
			fallbackSubjectToCharge4.TransportMode = Core.Constants.TransportModes.Sea;
			fallbackSubjectToCharge4.Direction = Core.Constants.FreightShipmentDirection.Code.Import;
			fallbackSubjectToCharge4.IsEnabled = true;

			var secondSerialised = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfSameChargeCodeDifferentProvider xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><SameChargeCodeDifferentProvider><JobType>SHP</JobType><TransportMode>AIR</TransportMode><Direction>EXP</Direction><IsEnabled>Y</IsEnabled></SameChargeCodeDifferentProvider><SameChargeCodeDifferentProvider><JobType>SHP</JobType><TransportMode>SEA</TransportMode><Direction>IMP</Direction><IsEnabled>Y</IsEnabled></SameChargeCodeDifferentProvider></ArrayOfSameChargeCodeDifferentProvider>";

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(first, Encoding.Unicode.GetBytes(firstSerialised)),
				new ValidSampleAndBinaryValueInDB(second, Encoding.Unicode.GetBytes(secondSerialised)),
			};
		}
		#endregion
	}
}
