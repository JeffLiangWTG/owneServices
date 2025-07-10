using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT.Testing
{
	class CusCarEntryNumTest : TestCaseWithFactory
	{
		public void TestICusCarLine_ShipmentType()
		{
			var bill = Bill;
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			var abcEntryNum = bill.CustomsEntryNumbers.AddNew();
			var cusCarEntryNum = new CusCarEntryNum(abcEntryNum);
			AssertEquals(ShipmentTypeList.Codes.Export22, ((ICusCarLine)cusCarEntryNum).ShipmentType);
		}

		public void TestICusCarLine_BillNumberWithHyphen()
		{
			var bill = Bill;
			bill.ABL_BillNumber = "!ABL--1234 56789";
			var abcEntryNum = bill.CustomsEntryNumbers.AddNew();
			var cusCarEntryNum = new CusCarEntryNum(abcEntryNum);
			AssertEquals("ABL-12345678", ((ICusCarLine)cusCarEntryNum).BillNumberWithHyphen);
		}

		public void TestICusCarLine_CargoReleaseStatus()
		{
			var bill = Bill;
			var header = Bill.Header;
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var abcEntryNum = bill.CustomsEntryNumbers.AddNew();
			var cusCarEntryNum = new CusCarEntryNum(abcEntryNum);
			bill.CargoReleaseStatus = "1";
			AssertEquals("1", ((ICusCarLine)cusCarEntryNum).CargoReleaseStatus);
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
			var bill = Bill;
			var header = bill.Header;
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var abcEntryNum = bill.CustomsEntryNumbers.AddNew();
			var cusCarEntryNum = new CusCarEntryNum(abcEntryNum);
			bill.CargoReleaseStatus = "1";
			AssertEquals("Goods released", ((ICusCarLine)cusCarEntryNum).CargoReleaseStatusDescription);
			bill.CargoReleaseStatus = "2";
			AssertEquals("Goods stopped / detained", ((ICusCarLine)cusCarEntryNum).CargoReleaseStatusDescription);
			bill.CargoReleaseStatus = "3";
			AssertEquals("Goods may move under Customs transfer (Customs Intervention) - Conditional Release", ((ICusCarLine)cusCarEntryNum).CargoReleaseStatusDescription);
			bill.CargoReleaseStatus = "50";
			AssertEquals("Released to States Warehouse", ((ICusCarLine)cusCarEntryNum).CargoReleaseStatusDescription);
			bill.CargoReleaseStatus = "51";
			bill.CargoReleaseStatusOtherDescription = "Other Desc.";
			AssertEquals("Other Desc.", ((ICusCarLine)cusCarEntryNum).CargoReleaseStatusDescription);
		}

		public void TestICusCarLine_DepotOfUnpack()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "VWG", Core.Constants.CountryCodes.SouthAfrica);
			var bill = Bill;
			bill.Header.AMA_OA_DeconsolidateAddress = org.PK;
			var abcEntryNum = bill.CustomsEntryNumbers.AddNew();
			var cusCarEntryNum = new CusCarEntryNum(abcEntryNum);
			AssertEquals("VWG", ((ICusCarLine)cusCarEntryNum).DepotOfUnpack);
		}

		public void TestICusCarLine_TerminalOfDischarge()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "VWG", Core.Constants.CountryCodes.SouthAfrica);
			var bill = Bill;
			bill.Header.AMA_OA_DischargeTerminalAddress = org.PK;
			var abcEntryNum = bill.CustomsEntryNumbers.AddNew();
			var cusCarEntryNum = new CusCarEntryNum(abcEntryNum);
			AssertEquals("VWG", ((ICusCarLine)cusCarEntryNum).TerminalOfDischarge);
		}

		public void TestICusCarLine_UCRNumber()
		{
			var bill = Bill;
			bill.ABL_UCRNumber = "123";
			var abcEntryNum = bill.CustomsEntryNumbers.AddNew();
			var cusCarEntryNum = new CusCarEntryNum(abcEntryNum);
			AssertEquals("123", ((ICusCarLine)cusCarEntryNum).UCRNumber);
		}

		public void TestICusCarLine_LocationOfGoods()
		{
			var bill = Bill;
			bill.ABL_GoodsLocation = "123";
			var abcEntryNum = bill.CustomsEntryNumbers.AddNew();
			var cusCarEntryNum = new CusCarEntryNum(abcEntryNum);
			AssertEquals("123", ((ICusCarLine)cusCarEntryNum).LocationOfGoods);
		}

		public void TestICusCarLine_PlaceOfDispatch()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "123";
			var bill = Bill;
			bill.ABL_OA_Shipper = address.PK;
			bill.Header.AMA_RL_NKPortOfLoading = "123";
			bill.ABL_RL_NKOrigin = bill.Header.AMA_RL_NKPortOfLoading;
			var abcEntryNum = bill.CustomsEntryNumbers.AddNew();
			var cusCarEntryNum = new CusCarEntryNum(abcEntryNum);
			AssertEquals("123", ((ICusCarLine)cusCarEntryNum).PlaceOfDispatch);
		}

		public void TestICusCarLine_Packages()
		{
			var bill = Bill;
			bill.ABL_ManifestQty = 1;
			bill.ABL_ManifestUQ = "BAG";
			bill.ABL_Volume = 100M;
			bill.ABL_VolumeUQ = "M3";
			bill.ABL_GrossWeight = 200M;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_MarksAndNumbers = "VIC";
			bill.ABL_CargoStatus = ASYCUDA.Business.CargoStatusList.Codes.LastPartShipment;
			var abcEntryNum = Bill.CustomsEntryNumbers.AddNew();
			abcEntryNum.PackPivots.AddNew(); // add null pack to test filtering out null packages
			abcEntryNum.PackPivots.AddNew(); // add null pack to test filtering out null packages
			abcEntryNum.PackPivots.AddNew(); // add null pack to test filtering out null packages
			var cusCarEntryNum = new CusCarEntryNum(abcEntryNum);
			AssertEquals(1, ((ICusCarLine)cusCarEntryNum).Packages.Count());
			var package = ((ICusCarLine)cusCarEntryNum).Packages.First();
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

		AsycudaBill Bill
		{
			get
			{
				if (bill == null)
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_TransportMode = Core.Constants.TransportModes.Road;
					bill = header.Bills.AddNew();
				}

				return bill;
			}
		}

		AsycudaBill bill;
	}
}
