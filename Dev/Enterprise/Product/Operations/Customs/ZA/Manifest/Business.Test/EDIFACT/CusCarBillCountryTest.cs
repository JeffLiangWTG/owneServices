using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.MasterFiles.Business;
using CargoStatusList = Enterprise.Customs.ASYCUDA.Business.CargoStatusList;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT.Testing
{
	class CusCarBillCountryTest : TestCaseWithFactory
	{
		public void TestICusCarLine_ShipmentType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			var carBillCountry = new CusCarBill(bill);
			AssertEquals(ShipmentTypeList.Codes.Export22, ((ICusCarLine)carBillCountry).ShipmentType);
		}

		public void TestICusCarLine_BillNumberWithHyphen()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "!ABL--1234 56789";
			var carBillCountry = new CusCarBill(bill);
			AssertEquals("ABL-12345678", ((ICusCarLine)carBillCountry).BillNumberWithHyphen);
		}

		public void TestICusCarLine_CargoReleaseStatus()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.CargoReleaseStatus = "1";
			var carBillCountry = new CusCarBill(bill);
			AssertEquals("1", ((ICusCarLine)carBillCountry).CargoReleaseStatus);
		}

		public void TestICusCarLine_CargoReleaseStatusDescription()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			var goodsReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "1", "Goods released", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsReleased.PK, "AQM", "desc.");
			var goodsStoppedDetained = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "2", "Goods stopped / detained", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsStoppedDetained.PK, "AQM", "desc.");
			var conditionalReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "3", "Goods may move under Customs transfer (Customs Intervention) - Conditional Release", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(conditionalReleased.PK, "AQM", "desc.");
			var releasedToStatesWarehouse = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "50", "Released to States Warehouse", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(releasedToStatesWarehouse.PK, "AQM", "desc.");
			var other = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "51", "Other (Overboard, destroyed, lost etc)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(other.PK, "AQM", "desc.");
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.CargoReleaseStatus = "1";
			var carBillCountry = new CusCarBill(bill);
			AssertEquals("Goods released", ((ICusCarLine)carBillCountry).CargoReleaseStatusDescription);
			bill.CargoReleaseStatus = "2";
			carBillCountry = new CusCarBill(bill);
			AssertEquals("Goods stopped / detained", ((ICusCarLine)carBillCountry).CargoReleaseStatusDescription);
			bill.CargoReleaseStatus = "3";
			carBillCountry = new CusCarBill(bill);
			AssertEquals("Goods may move under Customs transfer (Customs Intervention) - Conditional Release", ((ICusCarLine)carBillCountry).CargoReleaseStatusDescription);
			bill.CargoReleaseStatus = "50";
			carBillCountry = new CusCarBill(bill);
			AssertEquals("Released to States Warehouse", ((ICusCarLine)carBillCountry).CargoReleaseStatusDescription);
			bill.CargoReleaseStatus = "51";
			bill.CargoReleaseStatusOtherDescription = "Other Desc.";
			carBillCountry = new CusCarBill(bill);
			AssertEquals("Other Desc.", ((ICusCarLine)carBillCountry).CargoReleaseStatusDescription);
		}

		public void TestICusCarLine_DepotOfUnpack()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "VWG", Core.Constants.CountryCodes.SouthAfrica);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_DeconsolidateAddress = org.PK;
			var bill = header.Bills.AddNew();
			var billCountry = new CusCarBill(bill);
			AssertEquals("VWG", ((ICusCarLine)billCountry).DepotOfUnpack);
		}

		public void TestICusCarLine_TerminalOfDischarge()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "VWG", Core.Constants.CountryCodes.SouthAfrica);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_DischargeTerminalAddress = org.PK;
			var bill = header.Bills.AddNew();
			var billCountry = new CusCarBill(bill);
			AssertEquals("VWG", ((ICusCarLine)billCountry).TerminalOfDischarge);
		}

		public void TestICusCarPackage_GrossVolume()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var billCountry = new CusCarBill(bill);
			bill.ABL_VolumeUQ = Core.Constants.Volume.Litre;
			bill.ABL_Volume = 12.22M;
			AssertEquals(12.22M, ((ICusCarPackage)billCountry).GrossVolume);
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
			bill.ABL_Volume = 12.22M;
			AssertEquals(12M, ((ICusCarPackage)billCountry).GrossVolume);
			bill.ABL_Volume = 12.52M;
			AssertEquals(13M, ((ICusCarPackage)billCountry).GrossVolume);
		}

		public void TestICusCarPackage_GrossVolumeUnitCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var billCountry = new CusCarBill(bill);
			AssertEquals("MTQ", ((ICusCarPackage)billCountry).GrossVolumeUnitCode);
			bill.ABL_VolumeUQ = Core.Constants.Volume.Litre;
			AssertEquals("LTR", ((ICusCarPackage)billCountry).GrossVolumeUnitCode);
		}

		public void TestICusCarLine_UCRNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_UCRNumber = "123";
			var billCountry = new CusCarBill(bill);
			AssertEquals("123", ((ICusCarLine)billCountry).UCRNumber);
		}

		public void TestICusCarLine_LocationOfGoods()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_GoodsLocation = "123";
			var billCountry = new CusCarBill(bill);
			AssertEquals("123", ((ICusCarLine)billCountry).LocationOfGoods);
		}

		public void TestICusCarLine_Origin()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "ITGOA";
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "ITMIL";
			var billCountry = new CusCarBill(bill);
			AssertEquals("ITGOA", ((ICusCarLine)billCountry).Origin);
		}

		public void TestICusCarLine_PlaceOfDispatch()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "123";
			bill.ABL_OA_Shipper = address.PK;
			header.AMA_RL_NKPortOfLoading = "123";
			bill.ABL_RL_NKOrigin = header.AMA_RL_NKPortOfLoading;
			var billCountry = new CusCarBill(bill);
			AssertEquals("123", ((ICusCarLine)billCountry).PlaceOfDispatch);
		}

		public void TestICusCarLine_Packages()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ManifestQty = 1;
			bill.ABL_ManifestUQ = "BAG";
			bill.ABL_Volume = 100M;
			bill.ABL_VolumeUQ = "M3";
			bill.ABL_GrossWeight = 200M;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_MarksAndNumbers = "VIC";
			bill.ABL_CargoStatus = CargoStatusList.Codes.LastPartShipment;
			var billCountry = new CusCarBill(bill);
			AssertEquals(1, ((ICusCarLine)billCountry).Packages.Count());
			var package = ((ICusCarLine)billCountry).Packages.First();
			CombineAssertions(() =>
			{
				AssertEquals(1, package.NumberOfPacks);
				AssertEquals("BAG", package.PackUQ);
				AssertEquals("", package.Description);
				AssertEquals(100M, package.GrossVolumeInM3);
				AssertEquals(200M, package.GrossMassInKilos);
				AssertEquals("VIC", package.MarksAndNumbers);
				AssertEquals(CargoStatusIndicator.LastPartShipment, package.CargoStatusIndicator);
				AssertEquals("", package.UNDGClass);
				AssertEquals("", package.UNDGNumber);
				AssertEquals("", package.CommodityCode);
				AssertEquals("", package.ContainerNumber);
			});
		}

		public void TestICusCarPackage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ManifestQty = 1;
			bill.ABL_ManifestUQ = "BAG";
			bill.ABL_Volume = 100M;
			bill.ABL_VolumeUQ = "M3";
			bill.ABL_GrossWeight = 200M;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_MarksAndNumbers = "VIC";
			bill.ABL_CargoStatus = CargoStatusList.Codes.LastPartShipment;
			var billCountry = new CusCarBill(bill);
			CombineAssertions(() =>
			{
				AssertEquals(1, ((ICusCarPackage)billCountry).NumberOfPacks);
				AssertEquals("BAG", ((ICusCarPackage)billCountry).PackUQ);
				AssertEquals("", ((ICusCarPackage)billCountry).Description);
				AssertEquals(100M, ((ICusCarPackage)billCountry).GrossVolumeInM3);
				AssertEquals(200M, ((ICusCarPackage)billCountry).GrossMassInKilos);
				AssertEquals("VIC", ((ICusCarPackage)billCountry).MarksAndNumbers);
				AssertEquals(CargoStatusIndicator.LastPartShipment, ((ICusCarPackage)billCountry).CargoStatusIndicator);
				AssertEquals("", ((ICusCarPackage)billCountry).UNDGClass);
				AssertEquals("", ((ICusCarPackage)billCountry).UNDGNumber);
				AssertEquals("", ((ICusCarPackage)billCountry).CommodityCode);
				AssertEquals("", ((ICusCarPackage)billCountry).ContainerNumber);
			});
		}
	}
}
