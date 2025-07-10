using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTradeNetPermitNumber()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MainAddress.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.InterbankGIRO, "201101", Core.Constants.CountryCodes.Singapore);
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = Universal.Helper.ShipmentTypeList.Codes.Import23;
			var pack = bill.Packs.AddNew();
			pack.PackedItem.CustomsEntryNumbers.RemoveAndDeleteAll();
			Factory.InvalidateCachedProperties();
			pack.Validation.ValidateAll();
			AssertNoRowMessageError("Should not have the row message error as the bill is not linked an IBG Account.", pack, ValidationConstants.Bill.SGShouldHaveTradeNetPermitNumberWithLinkedIBGAccount);
			bill.ConsigneeOrgPK = consignee.PK;
			Factory.InvalidateCachedProperties();
			pack.Validation.ValidateAll();
			AssertHasRowMessageError("Should have the row message error as there's no valid TNP number on the packline.", pack, ValidationConstants.Bill.SGShouldHaveTradeNetPermitNumberWithLinkedIBGAccount);
			var entryNumber = pack.PackedItem.CustomsEntryNumbers.AddNew();
			entryNumber.CE_EntryType = ASYCUDA.Business.Constants.CustomsEntryType.TradeNetPermit;
			entryNumber.CE_EntryNum = "00001";
			Factory.InvalidateCachedProperties();
			pack.Validation.ValidateAll();
			AssertNoRowMessageError("Should not have the row message error as there's a valid TNP number on the packline.", pack, ValidationConstants.Bill.SGShouldHaveTradeNetPermitNumberWithLinkedIBGAccount);
		}

		public void TestLinePriceCurrencyDoesntComplainWhenEmpty()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.LinePriceCurrency = "";
			pack.LinePrice = 150;
			AssertNoMessageErrors(pack.LinePriceCurrencyInfo);
		}

		public void TestWeightAndVolumeValidationDoesntOccurForSingaporeExclusively()
		{
			var headerZA = (ASYCUDA.Business.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			var billZA = headerZA.Bills.AddNew();
			var contZA = headerZA.Containers.AddNew();
			var packZA = billZA.Packs.AddNew();
			packZA.ContainerPK = contZA.PK;
			packZA.APA_WeightUQ = Core.Constants.Weight.Decitons;
			packZA.APA_Weight = 0;
			AssertHasMessageErrorContaining(packZA.APA_WeightInfo, "cannot be zero");
			packZA.APA_Weight = 10;
			AssertNoMessageErrorContaining(packZA.APA_WeightInfo, "cannot be zero");
			packZA.APA_VolumeUQ = Core.Constants.Volume.CubicCentimeters;
			packZA.APA_Volume = 0;
			AssertHasMessageErrorContaining(packZA.APA_VolumeInfo, "cannot be zero");
			packZA.APA_Volume = 10;
			AssertNoMessageErrorContaining(packZA.APA_VolumeInfo, "cannot be zero");
			var headerSG = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var billSG = headerSG.Bills.AddNew();
			var contSG = headerSG.Containers.AddNew();
			var packSG = billSG.Packs.AddNew();
			packSG.ContainerPK = contSG.PK;
			packSG.APA_Weight = 0;
			packSG.APA_Volume = 0;
			packSG.APA_WeightUQ = Core.Constants.Weight.Grams;
			packSG.APA_VolumeUQ = Core.Constants.Volume.CubicDecimetres;
			AssertNoMessageErrorContaining(packSG.APA_WeightInfo, "cannot be zero");
			AssertNoMessageErrorContaining(packSG.APA_VolumeInfo, "cannot be zero");
		}

		public void TestCheckAPA_PackQty()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 0;
			pack.APA_PackUQ = "XXX";
			pack.Validation.ValidateAPA_PackQty();
			pack.Validation.ValidateAPA_PackUQ();
			AssertNoMessageErrors(pack.APA_PackQtyInfo);
			AssertNoMessageErrors(pack.APA_PackUQInfo);
			pack.APA_VINNumber = "1";
			pack.APA_PackQty = 2;
			pack.Validation.ValidateAPA_PackQty();
			AssertNoMessageErrors(pack.APA_PackQtyInfo);
		}
	}
}
