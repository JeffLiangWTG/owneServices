using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public sealed class PackingLineDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateDataObject()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			PopulatePackLine(packLine);

			var writer = new PackingLineDataObjectWriter<PackLine>(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packLine)));
			var packLineDataObject = writer.GetDataObject(packLine);
			AssertPackLine(packLineDataObject);
			AssertNull("packLineDataObject.ContainerNumber", packLineDataObject.ContainerNumber);

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "XXX";
			commodity.RH_Description = "Camel toes";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA0000001";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB0000002";

			packLine.SetContainer(container2.PK);

			writer = new PackingLineDataObjectWriter<PackLine>(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packLine)));
			packLineDataObject = writer.GetDataObject(packLine);
			AssertPackLine(packLineDataObject);
			AssertEquals("packLineDataObject.ContainerNumber", "BBBB0000002", packLineDataObject.ContainerNumber);
		}

		public void TestPopulateDataObject_ROROMode()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			var packLine = shipment.OuterPackLines.AddNew();
			PopulatePackLine(packLine);

			var writer = new PackingLineDataObjectWriter<PackLine>(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packLine)));
			var packLineDataObject = writer.GetDataObject(packLine);
			AssertPackLine(packLineDataObject, true);
			AssertNull("packLineDataObject.ContainerNumber", packLineDataObject.ContainerNumber);
		}

		public void TestPopulateDataObject_RequiredTemperatureUnit()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			PopulatePackLine(packLine);

			var writer = new PackingLineDataObjectWriter<PackLine>(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packLine)));
			var packLineDataObject = writer.GetDataObject(packLine);

			CombineAssertions(() =>
			{
				AssertEquals("C", packLineDataObject.RequiredTemperatureUnit.Code);
				AssertEquals("Centigrade", packLineDataObject.RequiredTemperatureUnit.Description);
			});

			packLine.JL_RequiredTemperatureUnit = Core.Constants.Temperature.Fahrenheit;

			writer = new PackingLineDataObjectWriter<PackLine>(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packLine)));
			packLineDataObject = writer.GetDataObject(packLine);

			CombineAssertions(() =>
			{
				AssertEquals("F", packLineDataObject.RequiredTemperatureUnit.Code);
				AssertEquals("Fahrenheit", packLineDataObject.RequiredTemperatureUnit.Description);
			});
		}

		public void TestPopulateCustomValues()
		{
			Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "CustomAttrib1";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute2Caption = "CustomAttrib2";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute3Caption = "CustomAttrib3";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute4Caption = "CustomAttrib4";

			Env.Registry.Freight.PackLine.PackLineCustomDecimal1Caption = "CustomDecimal1";
			Env.Registry.Freight.PackLine.PackLineCustomDecimal2Caption = "CustomDecimal2";

			Env.Registry.Freight.PackLine.PackLineCustomDate1Caption = "CustomDate1";
			Env.Registry.Freight.PackLine.PackLineCustomDate2Caption = "CustomDate2";

			Env.Registry.Freight.PackLine.PackLineCustomFlag1Caption = "CustomFlag1";
			Env.Registry.Freight.PackLine.PackLineCustomFlag2Caption = "CustomFlag2";

			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			PopulatePackLine(packLine);

			packLine.JL_CustomDate1 = new ZDateTime(2012, 1, 1);
			packLine.JL_CustomDate2 = new ZDateTime(2012, 2, 2);
			packLine.JL_CustomDecimal1 = 66m;
			packLine.JL_CustomDecimal2 = 99m;
			packLine.JL_CustomFlag1 = true;
			packLine.JL_CustomFlag2 = false;
			packLine.JL_CustomAttrib1 = "custom 1";
			packLine.JL_CustomAttrib2 = "custom 2";
			packLine.JL_CustomAttrib3 = "custom 3";
			packLine.JL_CustomAttrib4 = "custom 4";

			var writer = new PackingLineDataObjectWriter<PackLine>(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packLine)));
			var packLineDataObject = writer.GetDataObject(packLine);
			AssertPackLine(packLineDataObject);

			AssertContainsExactElementsInAnyOrder("packingLineData.CustomizedFieldCollection", new[]
			{
				"CustomAttrib1|custom 1",
				"CustomAttrib2|custom 2",
				"CustomAttrib3|custom 3",
				"CustomAttrib4|custom 4",
				"CustomDate1|2012-01-01T00:00:00",
				"CustomDate2|2012-02-02T00:00:00",
				"CustomDecimal1|66",
				"CustomDecimal2|99",
				"CustomFlag1|true",
				"CustomFlag2|false"
			},
			packLineDataObject.CustomizedFieldCollection.Select(customFieldData => string.Format("{0}|{1}", customFieldData.Key, customFieldData.Value)));
		}

		public void TestOnlyValuesSetUpInTheRegistryArePopulated()
		{
			Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "CustomAttrib1";
			Env.Registry.Freight.PackLine.PackLineCustomDecimal2Caption = "CustomDecimal2";

			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			PopulatePackLine(packLine);

			packLine.JL_CustomDate1 = new ZDateTime(2012, 1, 1);
			packLine.JL_CustomDate2 = new ZDateTime(2012, 2, 2);
			packLine.JL_CustomDecimal1 = 66m;
			packLine.JL_CustomDecimal2 = 99m;
			packLine.JL_CustomFlag1 = true;
			packLine.JL_CustomFlag2 = false;
			packLine.JL_CustomAttrib1 = "custom 1";
			packLine.JL_CustomAttrib2 = "custom 2";
			packLine.JL_CustomAttrib3 = "custom 3";
			packLine.JL_CustomAttrib4 = "custom 4";

			var writer = new PackingLineDataObjectWriter<PackLine>(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packLine)));
			var packLineDataObject = writer.GetDataObject(packLine);
			AssertPackLine(packLineDataObject);

			AssertContainsExactElementsInAnyOrder("packingLineData.CustomizedFieldCollection", new[]
			{
				"CustomAttrib1|custom 1",
				"CustomDecimal2|99"
			},
			packLineDataObject.CustomizedFieldCollection.Select(customFieldData => string.Format("{0}|{1}", customFieldData.Key, customFieldData.Value)));
		}

		public static void PopulatePackLine(PackLine packLine)
		{
			packLine.JL_PackLineId = "1234";
			packLine.JL_F3_NKPackType = "PAI"; // Pail
			packLine.JL_PackageCount = 12;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualWeight = 2.34m;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ContainerPackingOrder = 1;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Inches;
			packLine.JL_Length = 66m;
			packLine.JL_Width = 77m;
			packLine.JL_Height = 88m;
			packLine.JL_RN_NKOrigin = Core.Constants.CountryCodes.DemocraticRepublicOfCongo;
			packLine.JL_HarmonisedCode = "2103.45.34.12A";
			packLine.JL_ItemNo = 1;
			packLine.JL_MarksAndNumbers = "MORE ART RIPPING KNOT";
			packLine.JL_RefNumber = "FORYOURREFERENCE";
			packLine.JL_ExportRefNumber = "ExportRefNumber";
			packLine.JL_ImportRefNumber = "ImportRefNumber";

			packLine.JL_Outturn = 50;
			packLine.JL_Damaged = 2;
			packLine.JL_Pillaged = 48;
			packLine.JL_OutturnComment = "AHARRRRR!!!";
			packLine.JL_OutturnedHeight = 23.34m;
			packLine.JL_OutturnedLength = 34.45m;
			packLine.JL_OutturnedWeight = 6.78m;
			packLine.JL_OutturnedWidth = 56.98m;
			packLine.JL_Description = "longer than 36 chars to test that data object length is correct";
			packLine.JL_LoadingMeters = 4.32m;
			packLine.JL_EndItemNo = 1;
			packLine.JL_LinePrice = 5.43m;
			packLine.JL_DetailedDescription = "This is a detailed description that will be mapped to a field.";

			packLine.JL_RequiresTemperatureControl = true;
			packLine.JL_RequiredTemperatureMinimum = 1.00;
			packLine.JL_RequiredTemperatureMaximum = 100.00;
			packLine.JL_RequiredTemperatureUnit = Core.Constants.Temperature.Centigrade;

			packLine.JL_VehicleColor = "White";
			packLine.JL_VehicleMake = "Make 1";
			packLine.JL_VehicleModel = "Model 1";
			packLine.JL_VehicleNumberOfDoors = 4;
			packLine.JL_VehicleTransmission = "MAN";
			packLine.JL_VehicleYear = 2015;

			var substance = packLine.Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "3000";
			substance.DG_Variant = "c";
			substance.DG_FlashPoint = "100 C";
			substance.DG_Class = "Clas";
			substance.DG_PG = "Gr1";
			substance.DG_PSN = "Name1";
			substance.DG_TechName = "T";
			substance.DG_MP = "Y";
			substance.DG_SubLabel1 = "TEST";
			substance.DG_SubLabel2 = "AAA";

			var undg1 = packLine.UNDGs.AddNew();
			var contact1 = packLine.Factory.New<OrgContact>();
			undg1.DI_OC_DGContact = contact1.PK;
			contact1.OC_ContactName = "Contact1";
			contact1.OC_Phone = "123456";
			undg1.DI_DG = substance.PK;
			undg1.DI_DGFlashPoint = 0.1m;
			undg1.DI_DGVolume = 1m;
			undg1.DI_DGWeight = 2m;
			undg1.DI_MPMarinePollutant = "Y";
			undg1.DI_UnitOfWeight = "kg";
			undg1.DI_UnitOfVolume = "m3";
			undg1.DI_TechnicalName = "Tech1";
			undg1.DI_IsLimitedQuantity = true;

			var undg2 = packLine.UNDGs.AddNew();
			var contact2 = packLine.Factory.New<OrgContact>();
			undg2.DI_OC_DGContact = contact2.PK;
			undg2.DI_DG = substance.PK;

			var harmonisedCode = packLine.HarmonisedCodes.AddNew();
			harmonisedCode.JLH_Code = "HS1";
			harmonisedCode.JLH_RN_NKCountry = "AU";

			packLine.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
			packLine.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2013, 7, 6);

			var lastKnownWarehouseAddress = packLine.Factory.NewWithValidTestData<OrgAddress>();
			lastKnownWarehouseAddress.OA_Address1 = "AD1";
			packLine.JL_OA_LastKnownTransitWarehouseAddress = lastKnownWarehouseAddress.PK;
		}

		public static void AssertPackLine(PackingLine packingLineData, bool isVehicleFilledIn = false)
		{
			AssertEquals("packingLineID", "1234", packingLineData.PackingLineID);
			AssertEquals("packingLineData.ContainerPackingOrder", 1, packingLineData.ContainerPackingOrder);
			AssertEquals("packingLineData.HarmonisedCode", "2103.45.34.12A", packingLineData.HarmonisedCode);
			AssertEquals("packingLineData.Height", 88m, packingLineData.Height);
			AssertEquals("packingLineData.ItemNo", new ZShort(1), packingLineData.ItemNo);
			AssertEquals("packingLineData.Length", 66m, packingLineData.Length);
			AssertEquals("packingLineData.MarksAndNos", "MORE ART RIPPING KNOT", packingLineData.MarksAndNos);
			AssertEquals("packingLineData.CountryOfOrigin.Code", "CD", packingLineData.CountryOfOrigin.Code);
			AssertEquals("packingLineData.CountryOfOrigin.Name", "Congo, The Democratic Republic of t", packingLineData.CountryOfOrigin.Name);
			AssertEquals("packingLineData.OutturnQty", 50, packingLineData.OutturnQty);
			AssertEquals("packingLineData.OutturnDamagedQty", 2, packingLineData.OutturnDamagedQty);
			AssertEquals("packingLineData.OutturnPillagedQty", 48, packingLineData.OutturnPillagedQty);
			AssertEquals("packingLineData.OutturnComment", "AHARRRRR!!!", packingLineData.OutturnComment);
			AssertEquals("packingLineData.OutturnedHeight", 23.34m, packingLineData.OutturnedHeight);
			AssertEquals("packingLineData.OutturnedLength", 34.45m, packingLineData.OutturnedLength);
			AssertEquals("packingLineData.OutturnedVolume", 37.539m, packingLineData.OutturnedVolume);
			AssertEquals("packingLineData.OutturnedWeight", 6.78m, packingLineData.OutturnedWeight);
			AssertEquals("packingLineData.OutturnedWidth", 56.98m, packingLineData.OutturnedWidth);
			AssertEquals("packingLineData.PackQty", 12L, packingLineData.PackQty);
			AssertEquals("packingLineData.PackType.Code", "PAI", packingLineData.PackType.Code);
			AssertEquals("packingLineData.PackType.Description", "Pail", packingLineData.PackType.Description);
			AssertEquals("packingLineData.ReferenceNumber", "FORYOURREFERENCE", packingLineData.ReferenceNumber);
			AssertEquals("packingLineData.ExportReferenceNumber", "ExportRefNumber", packingLineData.ExportReferenceNumber);
			AssertEquals("packingLineData.ImportReferenceNumber", "ImportRefNumber", packingLineData.ImportReferenceNumber);
			AssertEquals("packingLineData.LengthUnit.Code", "IN", packingLineData.LengthUnit.Code);
			AssertEquals("packingLineData.LengthUnit.Description", "Inches", packingLineData.LengthUnit.Description);
			AssertEquals("packingLineData.Volume", 87.943m, packingLineData.Volume);
			AssertEquals("packingLineData.VolumeUnit.Code", "M3", packingLineData.VolumeUnit.Code);
			AssertEquals("packingLineData.VolumeUnit.Description", "Cubic Meters", packingLineData.VolumeUnit.Description);
			AssertEquals("packingLineData.Weight", 2.34m, packingLineData.Weight);
			AssertEquals("packingLineData.WeightUnit.Code", "T", packingLineData.WeightUnit.Code);
			AssertEquals("packingLineData.WeightUnit.Description", "Tonnes", packingLineData.WeightUnit.Description);
			AssertEquals("packingLineData.Width", 77m, packingLineData.Width);
			AssertEquals("packingLineData.GoodsDescription", "longer than 36 chars to test that data object length is correct", packingLineData.GoodsDescription);
			AssertEquals("packingLineData.LoadingMeters", 4.32m, packingLineData.LoadingMeters);
			AssertEquals("packingLineData.EndItemNo", new ZShort(1), packingLineData.EndItemNo);
			AssertEquals("packingLineData.LinePrice", 5.43m, packingLineData.LinePrice);
			AssertEquals("packingLineData.DetailedDescription", "This is a detailed description that will be mapped to a field.", packingLineData.DetailedDescription);
			AssertEquals("packingLineData.LastKnownCFSStatusDate", new ZDateTime(2013, 7, 6), packingLineData.LastKnownCFSStatusDate);
			AssertEquals("packingLineData.LastKnownCFSStatus.Code", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, packingLineData.LastKnownCFSStatus.Code);

			AssertEquals("packingLineData.OrganizationAddressCollection.Count", 1, packingLineData.OrganizationAddressCollection.Count);

			var lastKnownTransitWarehouseAddress = packingLineData.OrganizationAddressCollection.FirstOrDefault(AddressTypes.LastKnownCFSFacility);
			AssertEquals("packingLineData.OrganizationAddressCollection: LastKnownTransitWarehouseAddress", "AD1", lastKnownTransitWarehouseAddress.Address1);

			if (isVehicleFilledIn)
			{
				AssertNotNull("packingLineData.Vehicle", packingLineData.Vehicle);
				AssertEquals("packingLineData.Vehicle.Color", "White", packingLineData.Vehicle.Color);
				AssertEquals("packingLineData.Vehicle.Make", "Make 1", packingLineData.Vehicle.Make);
				AssertEquals("packingLineData.Vehicle.Model", "Model 1", packingLineData.Vehicle.Model);
				AssertEquals("packingLineData.Vehicle.NumberOfDoors", (ZByte)4, packingLineData.Vehicle.NumberOfDoors);
				AssertEquals("packingLineData.Vehicle.Transmission", "MAN", packingLineData.Vehicle.Transmission.Code);
				AssertEquals("packingLineData.Vehicle.Year", (ZShort)2015, packingLineData.Vehicle.Year);
			}
			else
			{
				AssertNull("packingLineData.Vehicle", packingLineData.Vehicle);
			}

			AssertEquals("packingLineData.UNDGCollection count", 2, packingLineData.UNDGCollection.Count);
			AssertEquals("Contact.FullName", "Contact1", packingLineData.UNDGCollection[0].Contact.FullName);
			AssertEquals("Contact.Phone", "123456", packingLineData.UNDGCollection[0].Contact.Phone);
			AssertEquals("FlashPoint", "0.1", packingLineData.UNDGCollection[0].FlashPoint);
			AssertEquals("IMOClass", "Clas", packingLineData.UNDGCollection[0].IMOClass);
			AssertEquals("MarinePollutant.Code", "Y", packingLineData.UNDGCollection[0].MarinePollutant.Code);
			AssertEquals("PackedInLimitedQuantity", true, packingLineData.UNDGCollection[0].PackedInLimitedQuantity);
			AssertEquals("PackingGroup", "Gr1", packingLineData.UNDGCollection[0].PackingGroup);
			AssertEquals("ProperShippingName", "Name1", packingLineData.UNDGCollection[0].ProperShippingName);
			AssertEquals("TechicalName", "Tech1", packingLineData.UNDGCollection[0].TechicalName);
			AssertEquals("UNDGCode", "3000c", packingLineData.UNDGCollection[0].UNDGCode);
			AssertEquals("Volume", 1m, packingLineData.UNDGCollection[0].Volume);
			AssertEquals("Weight", 2m, packingLineData.UNDGCollection[0].Weight);
			AssertEquals("WeightUQ", "kg", packingLineData.UNDGCollection[0].WeightUQ.Code);
			AssertEquals("VolumeUQ", "m3", packingLineData.UNDGCollection[0].VolumeUQ.Code);
			AssertEquals("SubLabel1", "TEST", packingLineData.UNDGCollection[0].SubLabel1);
			AssertEquals("SubLabel2", "AAA", packingLineData.UNDGCollection[0].SubLabel2);

			AssertEquals("Contact.FullName", "", packingLineData.UNDGCollection[1].Contact.FullName);
			AssertEquals("Contact.Phone", "", packingLineData.UNDGCollection[1].Contact.Phone);
			AssertEquals("FlashPoint", "100.0", packingLineData.UNDGCollection[1].FlashPoint);
			AssertEquals("IMOClass", "Clas", packingLineData.UNDGCollection[1].IMOClass);
			AssertEquals("MarinePollutant.Code", "Y", packingLineData.UNDGCollection[1].MarinePollutant.Code);
			AssertEquals("PackedInLimitedQuantity", false, packingLineData.UNDGCollection[1].PackedInLimitedQuantity);
			AssertEquals("PackingGroup", "Gr1", packingLineData.UNDGCollection[1].PackingGroup);
			AssertEquals("ProperShippingName", "Name1", packingLineData.UNDGCollection[1].ProperShippingName);
			AssertEquals("TechicalName", "", packingLineData.UNDGCollection[1].TechicalName);
			AssertEquals("UNDGCode", "3000c", packingLineData.UNDGCollection[1].UNDGCode);
			AssertEquals("Volume", 0m, packingLineData.UNDGCollection[1].Volume);
			AssertEquals("Weight", 0m, packingLineData.UNDGCollection[1].Weight);
			AssertEquals("WeightUQ", "", packingLineData.UNDGCollection[1].WeightUQ.Code);
			AssertEquals("VolumeUQ", "", packingLineData.UNDGCollection[1].VolumeUQ.Code);

			AssertEquals("Classification.Code", "HS1", packingLineData.ClassificationCollection[0].Code);
			AssertEquals("Classification.Type.Code", "HSC", packingLineData.ClassificationCollection[0].Type.Code);
			AssertEquals("Classification.Type.Description", "Harmonized Code", packingLineData.ClassificationCollection[0].Type.Description);
			AssertEquals("Classification.Country.Code", "AU", packingLineData.ClassificationCollection[0].Country.Code);
			AssertEquals("Classification.Country.Name", "Australia", packingLineData.ClassificationCollection[0].Country.Name);
		}

		public void TestAviationSecurityFieldMappings()
		{
			// Arrange
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "SEA"; // Sea Freight
			shipment.JS_PackingMode = "LCL";
			shipment.JS_ShipmentType = "STD"; // Standard House
			var packLine = shipment.OuterPackLines.AddNew();
			PopulatePackLine(packLine);

			// Act & Assert
			var writer = new PackingLineDataObjectWriter<PackLine>(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packLine)));
			var packLineDataObject = writer.GetDataObject(packLine);

			packLine.JL_AdditionalInspectionTypeCode = "XRY";
			packLine.JL_InspectionTypeCode = "PHS";
			packLine.JL_IsHighRisk = true;
			packLineDataObject = new PackingLineDataObjectWriter<PackLine>(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packLine))).GetDataObject(packLine);
			AssertNotNull("packLineDataObject", packLineDataObject);
			AssertNull("Aviation Security Additional Inspection Type is not added because it is not a Air Shipment", packLineDataObject.AviationSecurityAdditionalInspectionType);
			AssertNull("Aviation Security Inspection Type is not added because it is not a Air Shipment", packLineDataObject.AviationSecurityInspectionType);
			AssertNull("PackLine Is High Risk is null because it is not a Air Shipment", packLineDataObject.IsHighRisk);

			shipment.JS_TransportMode = "AIR"; // Air Freight
			packLineDataObject = new PackingLineDataObjectWriter<PackLine>(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packLine))).GetDataObject(packLine);
			AssertNotNull("packLineDataObject", packLineDataObject);
			AssertEquals("XRY", packLineDataObject.AviationSecurityAdditionalInspectionType.Code);
			AssertEquals("PHS", packLineDataObject.AviationSecurityInspectionType.Code);
			AssertEquals(true, packLineDataObject.IsHighRisk);

			packLine.JL_AdditionalInspectionTypeCode = "UNK";
			packLine.JL_InspectionTypeCode = "UNK";
			packLineDataObject = new PackingLineDataObjectWriter<PackLine>(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packLine))).GetDataObject(packLine);
			AssertNotNull("packLineDataObject", packLineDataObject);
			AssertEquals("UNK", packLineDataObject.AviationSecurityAdditionalInspectionType.Code);
			AssertEquals("UNK", packLineDataObject.AviationSecurityInspectionType.Code);
			AssertEquals(true, packLineDataObject.IsHighRisk);

			packLine.JL_AdditionalInspectionTypeCode = ZString.Empty;
			packLine.JL_InspectionTypeCode = ZString.Empty;
			packLineDataObject = new PackingLineDataObjectWriter<PackLine>(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packLine))).GetDataObject(packLine);
			AssertNotNull("packLineDataObject", packLineDataObject);
			AssertEquals(ZString.Empty, packLineDataObject.AviationSecurityAdditionalInspectionType.Code);
			AssertEquals(ZString.Empty, packLineDataObject.AviationSecurityInspectionType.Code);
			AssertEquals(true, packLineDataObject.IsHighRisk);
		}
	}
}
