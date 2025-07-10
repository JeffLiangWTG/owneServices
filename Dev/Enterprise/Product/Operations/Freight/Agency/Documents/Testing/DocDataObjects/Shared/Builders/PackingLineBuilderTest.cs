using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	class PackingLineBuilderTest : TestCaseWithFactory
	{
		#region PopulateGeneralInfo

		public void TestPopulateGeneralInfo()
		{
			var packLineBO = GetPackLine();
			packLineBO.UNDGs.AddNew();
			packLineBO.UNDGs.AddNew();

			var packLine = new PackingLineBuilder().Build(packLineBO);

			AssertEquals("AAAA0000007", packLine.ContainerNumber);
			AssertEquals("V0001", packLine.ShipmentID);
			AssertEquals("BKG000001", packLine.ShippersRef);
			AssertEquals("SLDNO002", packLine.ImportReferenceNumber);
			AssertEquals("SLDNO001", packLine.ExportReferenceNumber);

			AssertEquals((ZShort)12, packLine.ItemNumber);
			AssertEquals((ZShort)13, packLine.EndItemNumber);
			AssertEquals(0m, packLine.LoadingMeters);
			AssertEquals(1, packLine.PackingOrder);
			AssertEquals("2", packLine.PackingLineID);

			AssertEquals(2, packLine.DangerousGoods.Count);
			AssertEquals(3, packLine.HarmonizedCodes.Count);
			AssertEquals("HS (AU): AUCode", packLine.ExportHarmonizedCode.ToString());
			AssertEquals("HS (NZ): NZCode", packLine.ImportHarmonizedCode.ToString());
			AssertEquals("HSCodeData", packLine.HarmonizedCode.ToString());
			AssertEquals("5.00 C", packLine.TemperatureMinimum.ToString());
			AssertEquals("6.00 C", packLine.TemperatureMaximum.ToString());
			Assert(packLine.RequiresTemperatureControl);
		}

		#endregion

		#region PopulateGoodsDetails

		public void TestPopulateGoodsDetails()
		{
			var packLineBO = GetPackLine();
			var packLine = new PackingLineBuilder().Build(packLineBO);

			AssertEquals(CargoTypes.General, packLine.Commodity.Code);
			AssertEquals("Goods 1", packLine.ShortGoodsDescription);
			AssertEquals("DDD", packLine.DetailedGoodsDescription);
			AssertEquals("DDD", packLine.GoodsDescription);
			AssertEquals("AU", packLine.Origin.Code);
			AssertEquals("JL_RefNumber", packLine.ReferenceNumber);
		}

		#endregion

		#region PopulateWeightAndMeasures

		public void TestPopulateWeightAndMeasures()
		{
			var packLineBO = GetPackLine();
			var packLine = new PackingLineBuilder().Build(packLineBO);

			AssertEquals(3, packLine.Quantity);
			AssertEquals("PTL", packLine.PackageType.Code);
			AssertEquals("15.00 KG", packLine.Weight.ToString());
			AssertEquals("18.00 M3", packLine.Volume.ToString());
			AssertEquals("3.00 M", packLine.Height.ToString());
			AssertEquals("1.00 M", packLine.Length.ToString());
			AssertEquals("2.00 M", packLine.Width.ToString());
		}

		#endregion

		#region PopulateOutturnDetails

		public void TestPopulateOutturnDetails()
		{
			var packLineBO = GetPackLine();

			packLineBO.JL_Outturn = 1;
			packLineBO.JL_Damaged = 2;
			packLineBO.JL_Pillaged = 3;
			packLineBO.JL_OutturnComment = "111";

			packLineBO.JL_OutturnedWeight = 151;
			packLineBO.JL_OutturnedVolume = 0;

			packLineBO.JL_OutturnedLength = 11;
			packLineBO.JL_OutturnedHeight = 22;
			packLineBO.JL_OutturnedWidth = 33;

			var packLine = new PackingLineBuilder().Build(packLineBO);

			AssertEquals(1, packLine.Outturn);
			AssertEquals(2, packLine.Damaged);
			AssertEquals(3, packLine.Pillaged);
			AssertEquals("111", packLine.OutturnComment);
			AssertEquals("Marks 1", packLine.MarksAndNumbers);
			AssertEquals("11.00 M", packLine.OutturnLength.ToString());
			AssertEquals("22.00 M", packLine.OutturnHeight.ToString());
			AssertEquals("33.00 M", packLine.OutturnWidth.ToString());
			AssertEquals("151.00 KG", packLine.OutturnWeight.ToString());
			AssertEquals(packLineBO.JL_OutturnedVolume, packLine.OutturnVolume.Value);
		}

		#endregion

		#region PopulateRORODetails

		public void TestPopulateRORODetails()
		{
			var packLineBO = GetPackLine(ContainerModes.RollOnRollOff);

			packLineBO.JL_VehicleColor = "White";
			packLineBO.JL_VehicleMake = "Volga";
			packLineBO.JL_VehicleModel = "GAZ-24";
			packLineBO.JL_VehicleNumberOfDoors = 4;
			packLineBO.JL_VehicleTransmission = VehicleTransmissionType.Automatic;
			packLineBO.JL_VehicleYear = 1953;
			packLineBO.JL_RefNumber = "1234567890";

			var packLine = new PackingLineBuilder().Build(packLineBO);

			AssertEquals("White", packLine.VehicleColor);
			AssertEquals("Volga", packLine.VehicleMake);
			AssertEquals("GAZ-24", packLine.VehicleModel);
			AssertEquals(4, packLine.VehicleNumberOfDoors);
			AssertEquals(VehicleTransmissionType.Automatic, packLine.VehicleTransmission.Code);
			AssertEquals(1953, packLine.VehicleYear);
			AssertEquals("1234567890", packLine.VIN);
		}

		#endregion

		#region PopulateFromAgencyShipmentContainer

		public void TestPopulateFromAgencyShipmentContainer()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_PackingMode = ContainerModes.RollOnRollOff;

			var container = billOfLading.ShippingContainers.AddNew();
			container.JC_ContainerNum = "C0001";
			container.JC_F3_NKPackType = Core.Constants.PkgUnit.Box;

			container.JC_VehicleColor = "Green";
			container.JC_VehicleMake = "AC";
			container.JC_VehicleModel = "Cobra";
			container.JC_VehicleNumberOfDoors = 2;
			container.JC_VehicleTransmission = VehicleTransmissionType.Manual;
			container.JC_VehicleYear = 1966;

			container.JC_ContainerCount = 2;
			container.JC_Description = "GoodsDescription";
			container.JC_MarksAndNumbers = "MarksAndNumbers";

			container.JC_GrossWeight = 1000;
			container.JC_GrossWeightUQ = Weight.Grams;

			container.JC_GrossVolume = 1000;
			container.JC_GrossVolumeUQ = Volume.MegaLitre;

			container.JC_TotalHeight = 100;
			container.JC_TotalLength = 1000;
			container.JC_TotalWidth = 10000;
			container.JC_TotalUnitOfMeasure = Length.Centimetres;

			container.JC_RH_NKContainerCommodityCode = "ZZZ";
			container.JC_HarmonisedCode = "0405";

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "8000";
			substance.DG_Variant = "12";

			var undg = container.UNDGs.AddNew();
			undg.DI_DG = substance.PK;

			var packLine = new PackingLineBuilder().Build(container);

			AssertEquals("C0001", packLine.ReferenceNumber);
			AssertEquals(Core.Constants.PkgUnit.Box, packLine.PackageType.Code);

			AssertEquals("Green", packLine.VehicleColor);
			AssertEquals("AC", packLine.VehicleMake);
			AssertEquals("Cobra", packLine.VehicleModel);
			AssertEquals(2, packLine.VehicleNumberOfDoors);
			AssertEquals(VehicleTransmissionType.Manual, packLine.VehicleTransmission.Code);
			AssertEquals(1966, packLine.VehicleYear);

			AssertEquals(2, packLine.Quantity);
			AssertEquals("GoodsDescription", packLine.GoodsDescription);
			AssertEquals("MarksAndNumbers", packLine.MarksAndNumbers);

			AssertEquals((ZDecimal)1, packLine.Weight.Value);
			AssertEquals(Weight.Kilograms, packLine.Weight.Unit.Code);
			AssertEquals((ZDecimal)2000, packLine.Volume.Value);
			AssertEquals(Volume.CubicMetres, packLine.Volume.Unit.Code);
			AssertEquals((ZDecimal)100, packLine.Height.Value);
			AssertEquals(Length.Centimetres, packLine.Height.Unit.Code);
			AssertEquals((ZDecimal)1000, packLine.Length.Value);
			AssertEquals(Length.Centimetres, packLine.Length.Unit.Code);
			AssertEquals((ZDecimal)10000, packLine.Width.Value);
			AssertEquals(Length.Centimetres, packLine.Width.Unit.Code);

			AssertEquals("ZZZ", packLine.Commodity.Code);
			AssertEquals("0405", packLine.HarmonizedCode.Code);
			AssertEquals(ZString.Empty, packLine.HarmonizedCode.Country.Code);

			AssertEquals(1, packLine.DangerousGoods.Count);
			AssertEquals("800012", packLine.DangerousGoods.First().Code);
		}

		#endregion

		#region Implementation

		AgencyShipmentPackLine GetPackLine(string packingMode = ContainerModes.FCL)
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_UniqueConsignRef = "V0001";
			billOfLading.JS_HouseBill = "HOUSEBILL001";
			billOfLading.JS_PackingMode = packingMode;
			billOfLading.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			billOfLading.JS_RL_NKOrigin = "AUSYD";
			billOfLading.JS_RL_NKDestination = "NZAKL";
			billOfLading.JS_HouseBillIssueDate = new ZDateTime(2024, 1, 1);
			billOfLading.JS_RL_NKHouseBillIssuePlace = "AUMEL";
			billOfLading.JS_HBLContainerPackModeOverride = HBLDeliveryModes.Codes.CY_CY;
			billOfLading.JS_INCO = DomesticPaymentTerms.Prepaid;
			billOfLading.JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Shipped;
			billOfLading.JS_ShippedOnBoardDate = new ZDateTime(2024, 1, 1);
			billOfLading.JS_E_DEP = new ZDateTime(2024, 1, 2);
			billOfLading.JS_E_ARV = new ZDateTime(2024, 1, 3);
			billOfLading.JS_GoodsDescription = "goods description";
			billOfLading.JS_MarksAndNumbers = "marks & numbers";
			billOfLading.JS_BookingReference = "BKG000001";
			billOfLading.JS_HouseBillOfLadingType = "FIA";

			billOfLading.CustomsEntryNumber = "T7HRTXGXT";
			billOfLading.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;

			var mainTransport = billOfLading.Transports.AddNew();
			mainTransport.JW_LegOrder = 1;
			mainTransport.JW_TransportMode = TransportModes.Sea;
			mainTransport.JW_TransportType = TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = "AUSYD";
			mainTransport.JW_RL_NKDiscPort = "NZAKL";
			mainTransport.JW_Vessel = "MAIN.Vessel";
			mainTransport.JW_VoyageFlight = "MAINVoyage";

			var container = billOfLading.FCLContainers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_ContainerMode = ContainerModes.FCL;

			var container2 = billOfLading.FCLContainers.AddNew();
			container2.JC_ContainerNum = "BBBB0000007";
			container2.JC_ContainerMode = ContainerModes.FCL;

			billOfLading.OuterPackLines.RemoveAndDeleteAll();

			var packLine = billOfLading.OuterPackLines.AddNew();
			packLine.JL_RH_NKCommodityCode = CargoTypes.General;
			packLine.JL_RN_NKOrigin = "AU";
			packLine.JL_ItemNo = 12;
			packLine.JL_EndItemNo = 13;
			packLine.JL_ContainerPackingOrder = 1;
			packLine.JL_PackLineId = "2";
			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			packLine.JL_PackageCount = 3;
			packLine.JL_ActualWeight = 15;
			packLine.JL_ActualWeightUQ = Weight.Kilograms;
			packLine.JL_ActualVolume = 2000;
			packLine.JL_ActualVolumeUQ = Volume.CubicDecimetres;
			packLine.JL_Length = 1;
			packLine.JL_Width = 2;
			packLine.JL_Height = 3;
			packLine.JL_UnitOfDimension = Length.Metres;
			packLine.JL_ExportRefNumber = "SLDNO001";
			packLine.JL_ImportRefNumber = "SLDNO002";
			packLine.JL_Description = "Goods 1";
			packLine.JL_MarksAndNumbers = "Marks 1";
			packLine.JL_F3_NKPackType = "PTL";
			packLine.JL_DetailedDescription = "DDD";
			packLine.JL_HarmonisedCode = "HSCodeData";
			packLine.JL_RefNumber = "JL_RefNumber";
			packLine.JL_RequiresTemperatureControl = true;
			packLine.JL_RequiredTemperatureMinimum = 5;
			packLine.JL_RequiredTemperatureMaximum = 6;
			packLine.JL_RequiredTemperatureUnit = Temperature.Centigrade;

			var harmonisedCode1 = packLine.HarmonisedCodes.AddNew();
			harmonisedCode1.JLH_RN_NKCountry = "AU";
			harmonisedCode1.JLH_Code = "AUCode";

			var harmonisedCode2 = packLine.HarmonisedCodes.AddNew();
			harmonisedCode2.JLH_RN_NKCountry = "NZ";
			harmonisedCode2.JLH_Code = "NZCode";

			var harmonisedCode3 = packLine.HarmonisedCodes.AddNew();
			harmonisedCode3.JLH_RN_NKCountry = "CN";
			harmonisedCode3.JLH_Code = "CNCode";

			packLine.JL_JS = billOfLading.PK;
			container.PackLines.Add(packLine);

			Factory.Save();

			return packLine;
		}

		#endregion
	}
}
