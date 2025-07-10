using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class TopLevelPackPackingLineDataObjectWriterTest : TestCaseWithFactory
	{
		public static void PopulateContainer(AgencyShipmentContainer container, RefCommodityCode commodity, BusinessObjectFactory factory)
		{
			container.JC_RH_NKContainerCommodityCode = commodity.RH_Code;
			container.JC_ContainerNum = "IGORCAR";
			container.JC_TotalHeight = 123.45m;
			container.JC_TotalLength = 234.56m;
			container.JC_TotalWidth = 345.67m;
			container.JC_TotalUnitOfMeasure = Constants.Length.Inches;
			container.JC_GrossVolume = 456.78m;
			container.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;
			container.JC_GrossWeight = 567.89m;
			container.JC_GrossWeightUQ = Constants.Weight.Tonnes;
			container.JC_Description = "DESC";
			container.JC_MarksAndNumbers = "MORE ART RIPPING KNOT";
			container.JC_HarmonisedCode = "2103.45.34.12A";
			container.JC_GoodsValue = 111.22M;
			container.JC_RX_NKGoodsCurrency = Constants.CurrencyCodes.Ukraine;
			container.JC_VehicleColor = "RED";
			container.JC_VehicleMake = "IGOR";
			container.JC_VehicleModel = "GORICAR";
			container.JC_VehicleNumberOfDoors = 1;
			container.JC_VehicleTransmission = Constants.VehicleTransmissionType.Automatic;
			container.JC_VehicleYear = 2012;
			var substance = container.Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "3000";
			substance.DG_Variant = "c";
			substance.DG_FlashPoint = "100 C";
			substance.DG_Class = "Clas";
			substance.DG_PG = "Gr1";
			substance.DG_PSN = "Name1";
			substance.DG_TechName = "T";
			substance.DG_MP = "Y";
			var undg1 = container.UNDGs.AddNew();
			var contact1 = container.Factory.New<OrgContact>();
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
			var undg2 = container.UNDGs.AddNew();
			var contact2 = container.Factory.New<OrgContact>();
			undg2.DI_OC_DGContact = contact2.PK;
			undg2.DI_DG = substance.PK;
		}

		public static void AssertPackLine(PackingLine packingLineData, bool isVehicleFilledIn)
		{
			AssertEquals("packingLineData.Commodity.Code", "FGFG", packingLineData.Commodity.Code);
			AssertEquals("packingLineData.Commodity.Description", "Fudge Guts Fingers Gone", packingLineData.Commodity.Description);
			AssertEquals("packingLineData.HarmonisedCode", "2103.45.34.12A", packingLineData.HarmonisedCode);
			AssertEquals("packingLineData.Height", 123.45m, packingLineData.Height);
			AssertEquals("packingLineData.Length", 234.56m, packingLineData.Length);
			AssertEquals("packingLineData.MarksAndNos", "MORE ART RIPPING KNOT", packingLineData.MarksAndNos);
			AssertEquals("packingLineData.PackQty", 1L, packingLineData.PackQty);
			AssertEquals("packingLineData.ReferenceNumber", "IGORCAR", packingLineData.ReferenceNumber);
			AssertEquals("packingLineData.LengthUnit.Code", "IN", packingLineData.LengthUnit.Code);
			AssertEquals("packingLineData.LengthUnit.Description", "Inches", packingLineData.LengthUnit.Description);
			AssertEquals("packingLineData.Volume", 456.78m, packingLineData.Volume);
			AssertEquals("packingLineData.VolumeUnit.Code", "M3", packingLineData.VolumeUnit.Code);
			AssertEquals("packingLineData.VolumeUnit.Description", "Cubic Meters", packingLineData.VolumeUnit.Description);
			AssertEquals("packingLineData.Weight", 567.89m, packingLineData.Weight);
			AssertEquals("packingLineData.WeightUnit.Code", "T", packingLineData.WeightUnit.Code);
			AssertEquals("packingLineData.WeightUnit.Description", "Tonnes", packingLineData.WeightUnit.Description);
			AssertEquals("packingLineData.Width", 345.67m, packingLineData.Width);
			AssertEquals("packingLineData.GoodsDescription", "DESC", packingLineData.GoodsDescription);
			AssertEquals("packingLineData.LinePrice", 111.22m, packingLineData.LinePrice);
			AssertEquals("packingLineData.LinePriceCurrency.Code", "UAH", packingLineData.LinePriceCurrency.Code);
			AssertEquals("packingLineData.LinePriceCurrency.Description", "Ukrainian Hryvnia", packingLineData.LinePriceCurrency.Description);
			if (isVehicleFilledIn)
			{
				AssertNotNull("packingLineData.Vehicle", packingLineData.Vehicle);
				AssertEquals("Vehicle.Color", "RED", packingLineData.Vehicle.Color);
				AssertEquals("Vehicle.Make", "IGOR", packingLineData.Vehicle.Make);
				AssertEquals("Vehicle.Model", "GORICAR", packingLineData.Vehicle.Model);
				AssertEquals("Vehicle.NumberOfDoors", (byte)1, packingLineData.Vehicle.NumberOfDoors);
				AssertEquals("Vehicle.Transmission.Code", "AUT", packingLineData.Vehicle.Transmission.Code);
				AssertEquals("Vehicle.Transmission.Description", "Automatic", packingLineData.Vehicle.Transmission.Description);
				AssertEquals("Vehicle.Year", (short)2012, packingLineData.Vehicle.Year);
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
		}

		public void TestPopulateRORContainerDataObject()
		{
			var shipmentBO = Factory.New<BillOfLading>();
			shipmentBO.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			var containerBO = shipmentBO.ShippingContainers.AddNew();
			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "FGFG";
			commodity.RH_Description = "Fudge Guts Fingers Gone";
			commodity.RH_IsShipping = true;
			commodity.RH_IsForwarding = true;
			commodity.RH_IsLandTransport = false;
			PopulateContainer(containerBO, commodity, Factory);
			var writer = new TopLevelPackPackingLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)));
			var dataObject = writer.GetDataObject(containerBO);
			AssertPackLine(dataObject, true);
		}

		public void TestPopulateBBKContainerDataObject()
		{
			var shipmentBO = Factory.New<BillOfLading>();
			shipmentBO.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			var containerBO = shipmentBO.ShippingContainers.AddNew();
			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "FGFG";
			commodity.RH_Description = "Fudge Guts Fingers Gone";
			commodity.RH_IsShipping = true;
			commodity.RH_IsForwarding = true;
			commodity.RH_IsLandTransport = false;
			PopulateContainer(containerBO, commodity, Factory);
			var writer = new TopLevelPackPackingLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)));
			var dataObject = writer.GetDataObject(containerBO);
			AssertPackLine(dataObject, false);
		}
	}
}
