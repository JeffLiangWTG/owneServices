using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidate_ActualWeight_ErrorIfNegative()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			var item = consignment.Items.AddNew();

			item.HVI_ActualWeight = -1;
			AssertHasError(item.HVI_ActualWeightInfo, "Please enter an 'Actual Weight' greater than or equal to 0.");

			item.HVI_ActualWeight = 0;
			AssertNoWarnings(item.HVI_ActualVolumeInfo);

			item.HVI_ActualWeight = 1;
			AssertNoWarnings(item.HVI_ActualVolumeInfo);
		}

		public void TestValidate_ManifestedWeight_ErrorIfNegative()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			var item = consignment.Items.AddNew();

			Factory.Save();

			item.HVI_ManifestedWeight = -1;
			AssertHasError(item.HVI_ManifestedWeightInfo, "Please enter a 'Manifested Weight' greater than or equal to 0.");

			item.HVI_ManifestedWeight = 0;
			AssertNoWarnings(item.HVI_ManifestedWeightInfo);

			item.HVI_ManifestedWeight = 1;
			AssertNoWarnings(item.HVI_ManifestedWeightInfo);
		}

		public void TestValidate_ManifestedVolume_ErrorIfNegative()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			var item = consignment.Items.AddNew();

			Factory.Save();

			item.HVI_ManifestedVolume = -1;
			AssertHasError(item.HVI_ManifestedVolumeInfo, "Please enter a 'Manifested Volume' greater than or equal to 0.");

			item.HVI_ManifestedVolume = 0;
			AssertNoWarnings(item.HVI_ManifestedVolumeInfo);

			item.HVI_ManifestedVolume = 1;
			AssertNoWarnings(item.HVI_ManifestedVolumeInfo);
		}

		public void TestContainerNumberValidation_Error_ShouldNotLongerThan12()
		{
			var item = Factory.New<HVLVItem>();
			AssertNoErrors("pre condition", item);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			item.HVI_ContainerNumber = "IAMLONGERTHAN12CHARS";
			AssertNoErrors("Container number validation should be skipped if shipment is not sea", item);

			shipment.JS_TransportMode = TransportModes.Sea;
			item.Validation.ValidateHVI_ContainerNumber();
			AssertHasError(item.HVI_ContainerNumberInfo, "The container number can be no more than 12 characters long.");

			item.HVI_ContainerNumber = "CTNR1234567";
			AssertNoErrors(item);
		}

		public void TestContainerNumberValidation_Warning_DoesNotExistOnAnyRelatedConsols()
		{
			var item = Factory.New<HVLVItem>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;

			item.HVI_JS_LoadedOnShipment = shipment.PK;
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "CONSOL001";
			consol2.JK_UniqueConsignRef = "CONSOL002";
			consol1.Containers.AddNew().JC_ContainerNum = "CONT1234565";
			consol2.Containers.AddNew().JC_ContainerNum = "CONT4567890";

			AssertNoWarnings("pre condition", item);

			item.HVI_ContainerNumber = "CTNR1234567";
			AssertNoWarnings("Container number validation should be skipped if shipment is not sea", item);

			shipment.JS_TransportMode = TransportModes.Sea;
			item.Validation.ValidateHVI_ContainerNumber();
			AssertHasWarning(item.HVI_ContainerNumberInfo, "The container number does not exist on any related consols.");

			item.HVI_ContainerNumber = "CONT1234565";
			AssertNoWarnings(item);
		}

		public void TestContainerNumberValidation_Warning_ShouldNotBeEmptyWhenShipmentTransportModeIsSea()
		{
			var item = Factory.New<HVLVItem>();
			var shipment = Factory.New<ForwardingShipment>();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			AssertNotNull("pre-condition", item.Shipment);

			foreach (var mode in typeof(TransportModes).GetFields().Select(f => f.GetValue(null)).Cast<string>())
			{
				shipment.JS_TransportMode = mode;
				item.Validation.ValidateHVI_ContainerNumber();
				if (mode == TransportModes.Sea || mode == TransportModes.SeaAir)
				{
					Assert("pre-condition", shipment.IsSea);
					AssertHasWarning(item.HVI_ContainerNumberInfo, "The container number should not be empty.");
				}
				else
				{
					AssertNoWarnings(item);
				}
			}
		}

		#region HVI_ItemId

		public void TestValidate_ItemId()
		{
			HVLVTestHelper.SetGS1FountainOnOrgProxy(new BusinessObjectFactory(), "1234567");

			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item = header.Consignments.AddNew().Items.AddNew();
			item.HVI_ItemId = ZString.Empty;
			AssertNoErrors(item.HVI_ItemIdInfo);

			Factory.Save();
			item.HVI_ItemId = "1234";
			AssertNoErrors(item.HVI_ItemIdInfo);

			item.HVI_ItemId = ZString.Empty;
			AssertHasError(item.HVI_ItemIdInfo, "Please enter an Item ID.");
		}

		public void TestValidate_ItemId_AUExport_IsNotMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;

				Factory.Save();

				item.HVI_ItemId = "1234";
				AssertNoErrors(item.HVI_ItemIdInfo);

				item.HVI_ItemId = ZString.Empty;
				AssertNoErrors("Item Id is not mandatory for AU Export", item.HVI_ItemIdInfo);
			}
		}

		public void TestValidate_ItemId_WarnIfAnyDiacritics()
		{
			var item = Factory.New<HVLVItem>();

			item.HVI_ItemId = "Dïácrïtïcs";
			item.Validation.ValidateHVI_ItemId();
			AssertHasWarning(item.HVI_ItemIdInfo, "Item ID should not contain any characters with diacritics.");

			item.HVI_ItemId = "ITEMID";
			item.Validation.ValidateHVI_ItemId();
			AssertNoWarnings(item.HVI_ItemIdInfo);
		}

		#endregion

		public void TestValidate_PackType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			var item = Factory.New<HVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			shipment.JS_TransportMode = "SEA";
			item.Validation.ValidateHVI_F3_NKPackType();
			AssertHasError(item.HVI_F3_NKPackTypeInfo, "Please enter a Pack Type.");

			shipment.JS_TransportMode = "AIR";
			item.Validation.ValidateHVI_F3_NKPackType();
			AssertNoErrors(item.HVI_F3_NKPackTypeInfo);

			item.HVI_F3_NKPackType = "XXX";
			AssertHasError(item.HVI_F3_NKPackTypeInfo, "Enter a valid Pack Type.");

			shipment.JS_TransportMode = "SEA";
			item.HVI_IsUnmanifestedAtDestination = true;
			item.HVI_F3_NKPackType = "";
			item.Validation.ValidateHVI_F3_NKPackType();
			AssertNoErrors("Expect allow empty for fields that can be empty when unmanifested.", item.HVI_F3_NKPackTypeInfo);

			item.HVI_IsUnmanifestedAtDestination = false;
			item.HVI_F3_NKPackType = "";
			AssertHasError(item.HVI_F3_NKPackTypeInfo, "Please enter a Pack Type.");

			item.HVI_F3_NKPackType = "PLT";
			AssertNoErrors(item.HVI_F3_NKPackTypeInfo);
		}

		public void TestValidate_PackType_AUExport_IsMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				shipment.JS_TransportMode = TransportModes.Sea;

				item.HVI_F3_NKPackType = "PLT";
				AssertNoErrors(item.HVI_F3_NKPackTypeInfo);

				item.HVI_F3_NKPackType = ZString.Empty;
				AssertHasError("Pack Type is mandatory for AU Export", item.HVI_F3_NKPackTypeInfo, "Please enter a Pack Type.");
			}
		}

		public void TestValidate_ManifestedWeight_MandatoryForAirAndSea()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			var item = Factory.New<HVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Action<string, bool> assertWeightIsMandatoryForTransportMode = (transportMode, expectedToBeMandatory) =>
			{
				shipment.JS_TransportMode = transportMode;
				item.HVI_ManifestedWeight = 0;

				if (expectedToBeMandatory)
				{
					AssertHasError(item.HVI_ManifestedWeightInfo, "Please enter a Manifested Weight.");
					item.HVI_ManifestedWeight = 20;
					AssertNoErrors(item.HVI_ManifestedWeightInfo);
				}
				else
				{
					AssertNoErrors(item.HVI_ManifestedWeightInfo);
				}
			};

			assertWeightIsMandatoryForTransportMode("SEA", true);
			assertWeightIsMandatoryForTransportMode("AIR", true);
			assertWeightIsMandatoryForTransportMode("ROA", false);
		}

		public void TestValidate_ManifestedWeight_AUExport_IsMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				shipment.JS_TransportMode = TransportModes.Sea;

				item.HVI_ManifestedWeight = 1.0;
				AssertNoErrors(item.HVI_ManifestedWeightInfo);

				item.HVI_ManifestedWeight = 0.0;
				AssertHasError("Manifested Weight is mandatory for AU Export", item.HVI_ManifestedWeightInfo, "Please enter a Manifested Weight.");
			}
		}

		public void TestValidate_ManifestedWeight_WarningCalculateWithUnitConverting()
		{
			var (item, itemLine1, itemLine2) = PrepareHVLVItemTestDataWithManifestedWeight(100.1M, 100M, 100M);
			AssertHasWarning("Should show warning", item.HVI_ManifestedWeightInfo, "Manifested weight does not match Item Line Gross Weights.");

			item.Consignment.HVC_WeightUQ = Weight.Kilograms;
			itemLine1.HVS_WeightUnit = Weight.Kilograms;
			itemLine2.HVS_WeightUnit = Weight.Grams;
			AssertNoWarning("Should show no warning", item.HVI_ManifestedWeightInfo, "Manifested weight does not match Item Line Gross Weights.");
		}

		public void TestValidate_ManifestedWeight_ManifestedWeightDoesNotMatchItemLineGrossWeights_WhenTransportIsNotAirOrSea()
		{
			var (item, _, _) = PrepareHVLVItemTestDataWithManifestedWeight(100.1M, 100M, 100M, TransportModes.Road);
			AssertHasWarning("Should show warning", item.HVI_ManifestedWeightInfo, "Manifested weight does not match Item Line Gross Weights.");
		}

		public void TestValidate_ManifestedWeight_ManifestedWeightDoesNotMatchItemLineGrossWeights_WhenHVI_IsUnmanifestedAtDestinationIsTrueOrFalse()
		{
			var (item, _, _) = PrepareHVLVItemTestDataWithManifestedWeight(100.1M, 100M, 100M);

			item.HVI_IsUnmanifestedAtDestination = true;
			AssertHasWarning("Should show warning", item.HVI_ManifestedWeightInfo, "Manifested weight does not match Item Line Gross Weights.");

			item.HVI_IsUnmanifestedAtDestination = false;
			AssertHasWarning("Should show warning", item.HVI_ManifestedWeightInfo, "Manifested weight does not match Item Line Gross Weights.");
		}

		(HVLVItem, HVLVItemLine, HVLVItemLine) PrepareHVLVItemTestDataWithManifestedWeight(decimal itemWeight, decimal itemLine1Weight, decimal itemLine2Weight, string transportMode = "AIR")
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;
			var consignment = Factory.New<HVLVConsignment>();
			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			var itemLine1 = item.Lines.AddNew();
			var itemLine2 = item.Lines.AddNew();

			itemLine1.HVS_GrossWeight = itemLine1Weight;
			itemLine2.HVS_GrossWeight = itemLine2Weight;
			item.HVI_ManifestedWeight = itemWeight;

			return (item, itemLine1, itemLine2);
		}

		public void TestValidate_ManifestedVolume()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			var item = Factory.New<HVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Action<string, bool> assertVolumeIsMandatoryForTransportMode = (transportMode, expectedToBeMandatory) =>
			{
				shipment.JS_TransportMode = transportMode;
				item.HVI_ManifestedVolume = 0;

				if (expectedToBeMandatory)
				{
					AssertHasError(item.HVI_ManifestedVolumeInfo, "Please enter a Manifested Volume.");
					item.HVI_ManifestedVolume = 20;
					AssertNoErrors(item.HVI_ManifestedVolumeInfo);
				}
				else
				{
					AssertNoErrors(item.HVI_ManifestedVolumeInfo);
				}
			};

			assertVolumeIsMandatoryForTransportMode("SEA", true);
			assertVolumeIsMandatoryForTransportMode("AIR", false);
			assertVolumeIsMandatoryForTransportMode("ROA", false);
		}

		public void TestValidate_ManifestedVolume_AUExport_IsMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				shipment.JS_TransportMode = TransportModes.Sea;

				item.HVI_ManifestedVolume = 1.0;
				AssertNoErrors(item.HVI_ManifestedVolumeInfo);

				item.HVI_ManifestedVolume = 0.0;
				AssertHasError("Manifested Weight is mandatory for AU Export", item.HVI_ManifestedVolumeInfo, "Please enter a Manifested Volume.");
			}
		}

		public void TestValidate_ShipperReference()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			var item3 = consignment.Items.AddNew();
			var item4 = consignment.Items.AddNew();

			item3.HVI_IsActive = false;
			Factory.Save();

			CombineAssertions("Pre-condition: no errors", () =>
			{
				AssertNoErrors(item1.HVI_ShipperReferenceInfo);
				AssertNoErrors(item2.HVI_ShipperReferenceInfo);
				AssertNoErrors(item3.HVI_ShipperReferenceInfo);
				AssertNoErrors(item4.HVI_ShipperReferenceInfo);
			});

			item1.HVI_ShipperReference = "1";
			item2.HVI_ShipperReference = "1";
			item3.HVI_ShipperReference = "2";
			item4.HVI_ShipperReference = "2";

			item1.Validation.ValidateHVI_ShipperReference();
			item2.Validation.ValidateHVI_ShipperReference();
			item3.Validation.ValidateHVI_ShipperReference();
			item4.Validation.ValidateHVI_ShipperReference();

			CombineAssertions("Error if duplicate on active items", () =>
			{
				AssertHasError(item1.HVI_ShipperReferenceInfo, "The Shipper Reference has been duplicated and must be unique.");
				AssertHasError(item2.HVI_ShipperReferenceInfo, "The Shipper Reference has been duplicated and must be unique.");
				AssertNoErrors(item3.HVI_ShipperReferenceInfo);
				AssertNoErrors(item4.HVI_ShipperReferenceInfo);
			});
		}

		public void TestValidate_ShipperReference_WarnIfAnyDiacritics()
		{
			var item = Factory.New<HVLVItem>();

			item.HVI_ShipperReference = "Dïácrïtïcs";
			item.Validation.ValidateHVI_ShipperReference();
			AssertHasWarning(item.HVI_ShipperReferenceInfo, "Shipper Reference should not contain any characters with diacritics.");

			item.HVI_ShipperReference = "ShipperRef";
			item.Validation.ValidateHVI_ShipperReference();
			AssertNoWarnings(item.HVI_ShipperReferenceInfo);
		}

		public void TestValidate_ActualVolume_WarningIfNotEqualToCalculation()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			var item = consignment.Items.AddNew();

			Factory.Save();

			item.HVI_Length = 1m;
			item.HVI_Height = 1m;
			item.HVI_Width = 1m;
			item.HVI_UnitOfDimension = "M";
			item.HVI_ActualVolume = 1m;

			item.Validation.ValidateHVI_ActualVolume();

			AssertNoWarning(item.HVI_ActualVolumeInfo, "The actual volume does not match the volume calculated by the length x width x height.");

			item.HVI_ActualVolume = 2m;
			item.Validation.ValidateHVI_ActualVolume();

			AssertHasWarning(item.HVI_ActualVolumeInfo, "The actual volume does not match the volume calculated by the length x width x height.");
		}

		public void TestValidate_ActualVolume_ErrorIfNegative()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			var item = consignment.Items.AddNew();

			Factory.Save();

			item.HVI_ActualVolume = -1;
			AssertHasError(item.HVI_ActualVolumeInfo, "Please enter an 'Actual Volume' greater than or equal to 0.");

			item.HVI_ActualWeight = 0;
			AssertNoWarnings(item.HVI_ActualVolumeInfo);

			item.HVI_ActualWeight = 1;
			AssertNoWarnings(item.HVI_ActualVolumeInfo);
		}

		public void TestValidate_Height()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item = header.Consignments.AddNew().Items.AddNew();

			Factory.Save();

			item.HVI_Height = -1;
			AssertHasError(item.HVI_HeightInfo, "Please enter a 'Height' greater than or equal to 0.");

			item.HVI_Height = 0;
			AssertNoWarnings(item.HVI_HeightInfo);

			item.HVI_Height = 1;
			AssertNoWarnings(item.HVI_HeightInfo);
		}

		public void TestValidate_Width()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item = header.Consignments.AddNew().Items.AddNew();

			Factory.Save();

			item.HVI_Width = -1;
			AssertHasError(item.HVI_WidthInfo, "Please enter a 'Width' greater than or equal to 0.");

			item.HVI_Width = 0;
			AssertNoWarnings(item.HVI_WidthInfo);

			item.HVI_Width = 1;
			AssertNoWarnings(item.HVI_WidthInfo);
		}

		public void TestValidate_Length()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item = header.Consignments.AddNew().Items.AddNew();

			Factory.Save();

			item.HVI_Length = -1;
			AssertHasError(item.HVI_LengthInfo, "Please enter a 'Length' greater than or equal to 0.");

			item.HVI_Length = 0;
			AssertNoWarnings(item.HVI_LengthInfo);

			item.HVI_Length = 1;
			AssertNoWarnings(item.HVI_LengthInfo);
		}

		public void TestValidate_UnitOfDimension()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			var item = consignment.Items.AddNew();

			Factory.Save();

			item.HVI_Length = 1m;
			item.HVI_Width = 1m;
			item.HVI_Height = 1.5m;
			item.HVI_UnitOfDimension = Length.Metres;

			AssertEquals("Pre-condition:", 1.5m, item.HVI_ActualVolume);

			item.HVI_UnitOfDimension = "ZZ";

			AssertHasError(item.HVI_UnitOfDimensionInfo, "Enter a valid Unit.");

			item.HVI_UnitOfDimension = string.Empty;

			AssertHasError(item.HVI_UnitOfDimensionInfo, "Please enter a Unit.");
		}

		public void TestValidate_UnitOfDimension_AUExport_IsMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				shipment.JS_TransportMode = TransportModes.Sea;

				item.HVI_Length = 1m;
				item.HVI_Width = 1m;
				item.HVI_Height = 1.5m;
				item.HVI_UnitOfDimension = Length.Metres;
				AssertNoErrors(item.HVI_UnitOfDimensionInfo);

				item.HVI_UnitOfDimension = ZString.Empty;
				AssertHasError(item.HVI_UnitOfDimensionInfo, "Please enter a Unit.");
			}
		}

		public void TestValidate_GoodsDescription_WarnIfAnyDiacritics()
		{
			var item = Factory.New<HVLVItem>();

			item.HVI_GoodsDescription = "Dïácrïtïcs";
			item.Validation.ValidateHVI_GoodsDescription();
			AssertHasWarning(item.HVI_GoodsDescriptionInfo, "Goods Description should not contain any characters with diacritics.");

			item.HVI_GoodsDescription = "ShipperRef";
			item.Validation.ValidateHVI_GoodsDescription();
			AssertNoWarnings(item.HVI_GoodsDescriptionInfo);
		}

		public void TestValidate_CurrentBarcode_WarnIfAnyDiacritics()
		{
			var item = Factory.New<HVLVItem>();

			item.HVI_CurrentBarcode = "Dïácrïtïcs123";
			item.Validation.ValidateHVI_CurrentBarcode();
			AssertHasWarning(item.HVI_CurrentBarcodeInfo, "Barcode should not contain any characters with diacritics.");

			item.HVI_CurrentBarcode = "BARCODE123";
			item.Validation.ValidateHVI_CurrentBarcode();
			AssertNoWarnings(item.HVI_CurrentBarcodeInfo);
		}

		public void TestValidate_IsActive_ErrorIfActiveItemsOnInactiveConsignments()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_IsActive = false;
			item.HVI_IsActive = false;

			AssertNoError("Precondition:", item.HVI_IsActiveInfo, "Item cannot be active as it is attached to an inactive consignment.");

			item.HVI_IsActive = true;
			AssertHasError("Should show error", item.HVI_IsActiveInfo, "Item cannot be active as it is attached to an inactive consignment.");

			item.HVI_IsActive = false;
			AssertNoError("Error should be removed", item.HVI_IsActiveInfo, "Item cannot be active as it is attached to an inactive consignment.");
		}

		public void TestValidate_IsActive_ErrorIfNoActiveItemsOnActiveConsignment()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			var item3 = consignment.Items.AddNew();

			consignment.HVC_IsActive = true;
			item1.HVI_IsActive = true;
			item2.HVI_IsActive = false;
			item3.HVI_IsActive = false;

			CombineAssertions("Precondition: No Errors", () =>
			{
				AssertNoError("Item 1:", item1.HVI_IsActiveInfo, "An active Consignment requires at least one active item.");
				AssertNoError("Item 2:", item2.HVI_IsActiveInfo, "An active Consignment requires at least one active item.");
				AssertNoError("Item 3:", item3.HVI_IsActiveInfo, "An active Consignment requires at least one active item.");
			});

			item1.HVI_IsActive = false;

			CombineAssertions("All items should have error", () =>
			{
				AssertHasError("Item 1:", item1.HVI_IsActiveInfo, "An active Consignment requires at least one active item.");
				AssertHasError("Item 2:", item2.HVI_IsActiveInfo, "An active Consignment requires at least one active item.");
				AssertHasError("Item 3:", item3.HVI_IsActiveInfo, "An active Consignment requires at least one active item.");
			});

			item1.HVI_IsActive = true;

			CombineAssertions("All items should have error removed", () =>
			{
				AssertNoError("Item 1:", item1.HVI_IsActiveInfo, "An active Consignment requires at least one active item.");
				AssertNoError("Item 2:", item2.HVI_IsActiveInfo, "An active Consignment requires at least one active item.");
				AssertNoError("Item 3:", item3.HVI_IsActiveInfo, "An active Consignment requires at least one active item.");
			});
		}

		#region Test ValidateAll

		public void TestValidateAll_WithNoChanges_DoesNotValidate()
		{
			var item = Factory.New<HVLVItem_WithGoodsDescriptionError_ForTest>();
			AssertEquals("Precondition: No changes on item", false, item.HasChanges);

			item.RunPreSaveValidation();
			AssertNoErrors("Validation should not have run, so no errors", item);
		}

		public void TestValidateAll_WithChanges_DoesValidate()
		{
			var item = Factory.New<HVLVItem_WithGoodsDescriptionError_ForTest>();
			item.HasChanges = true;

			item.RunPreSaveValidation();
			AssertHasError("Validation should have run, resulting in error", item.HVI_GoodsDescriptionInfo, HVLVItem_WithGoodsDescriptionError_ForTest.TestErrorMesssage);
		}

		public void TestValidateAll_WithChangesToChildren_DoesValidate()
		{
			var item = Factory.New<HVLVItem_WithGoodsDescriptionError_ForTest>();
			AssertEquals("Precondition: No changes on item", false, item.HasChanges);
			var line = item.Lines.AddNew();
			line.HVS_GoodsDescription = "ABC";

			CombineAssertions("Precondition: Changing child should set has changes on parent", () =>
			{
				AssertEquals("Line has changes", true, line.HasChanges);
				AssertEquals("Item also has changes", true, item.HasChanges);
			});

			item.RunPreSaveValidation();
			AssertHasError("Validation should have run, resulting in error", item.HVI_GoodsDescriptionInfo, HVLVItem_WithGoodsDescriptionError_ForTest.TestErrorMesssage);
		}

		public void TestValidateAll_WithChangesToParent_DoesValidate()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = Factory.New<HVLVItem_WithGoodsDescriptionError_ForTest>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HasChanges = false;

			consignment.HVC_ConsigneeName = "ABC";

			CombineAssertions("Precondition:", () =>
			{
				AssertEquals("Parent Consignment has changes", true, consignment.HasChanges);
				AssertEquals("No changes on item", false, item.HasChanges);
			});

			item.RunPreSaveValidation();
			AssertHasError("Validation should have run, resulting in error", item.HVI_GoodsDescriptionInfo, HVLVItem_WithGoodsDescriptionError_ForTest.TestErrorMesssage);
		}

		#endregion

		class HVLVItem_WithGoodsDescriptionError_ForTest : HVLVItem
		{
			public HVLVItem_WithGoodsDescriptionError_ForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override HVLVItemValidation GetNewValidation()
			{
				return new HVLVItemValidation_ForTest(this);
			}

			public static string TestErrorMesssage => "This is an error that always appears when validating for tests";

			class HVLVItemValidation_ForTest : HVLVItemValidation
			{
				public HVLVItemValidation_ForTest(AutoHVLVItem parent)
					: base(parent)
				{
				}

				protected override void CheckHVI_GoodsDescription()
				{
					Parent.HVI_GoodsDescriptionInfo.AddError(TestErrorMesssage);
				}
			}
		}
	}
}
