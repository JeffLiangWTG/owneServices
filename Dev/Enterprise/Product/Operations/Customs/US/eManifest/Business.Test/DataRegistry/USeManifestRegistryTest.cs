using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(USeManifestDataRegistry))]
	sealed class USeManifestRegistryTest : RegistryItemSetTestCaseWithFactory<USeManifestDataRegistry>
	{
		public void TestNumberOfShipmentsInOneMessage()
		{
			TestGenericRegistryItem(ItemSet.MaximumShipmentsToSendInOneMessage,
				"eManifestMaximumShipmentsToSendInOneMessage",
				USeManifestDataRegistry.Categories.Customs_UnitedStatesofAmerica_eManifest,
				"Maximum Shipments To Send in One Message",
				"Override this value to customize the maximum shipments to be sent in one message.",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForSupport,
				3000);

			var intRegistryDataType = (IntRegistryDataType)ItemSet.MaximumShipmentsToSendInOneMessage.DataType;
			AssertEquals("Minimum value", 1, (int)intRegistryDataType.LowerBound);
			AssertEquals("Maximum value", 5000, (int)intRegistryDataType.UpperBound);

			var newValue = 131;
			ItemSet.MaximumShipmentsToSendInOneMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals(131, ItemSet.MaximumShipmentsToSendInOneMessage.Value);
		}

		public void TestNumberCustomisation()
		{
			TestGenericRegistryItem(
				ItemSet.NumberCustomisation,
				"eManifestNumberCustomisation",
				USeManifestDataRegistry.Categories.Customs_UnitedStatesofAmerica_eManifest,
				"e-Manifest Number Customization",
				"Override this value to customize how e-Manifest numbers are formatted",
				RegistryStorageFlags.All);

			var dataType = (BillCustomisationRegistryDataType)ItemSet.NumberCustomisation.DataType;
			AssertEquals("GeneratedNumberName", "e-Manifest Number", dataType.GeneratedNumberName);
			AssertEquals("MaxLength", CusInBondHeaderSchema.BH_JobReference.MaxLength, dataType.MaxLength);
			AssertEquals("7", dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationIATA]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationUNLOCO]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginIATA]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginUNLOCO]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.Direction]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.TransportMode]);
		}
	}
}
