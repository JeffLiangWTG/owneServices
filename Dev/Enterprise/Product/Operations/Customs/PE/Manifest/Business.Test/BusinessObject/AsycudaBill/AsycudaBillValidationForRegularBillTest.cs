using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	sealed class AsycudaBillValidationForRegularBillTest : BusinessObjectLookupsTestCase
	{
		public void TestCheckCargoNature()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(bill.CargoNatureInfo, "15", "14");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			ValidationTestHelper.AssertFieldIsNotMandatory(bill.CargoNatureInfo);

			bill.CargoNature = "15";
			AssertNoMessageErrorContaining(header.AMA_NatureInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCargoCondition()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(bill.CargoConditionInfo, "15", "14");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			ValidationTestHelper.AssertFieldIsNotMandatory(bill.CargoConditionInfo);

			bill.CargoCondition = "15";
			AssertNoMessageErrorContaining(bill.CargoConditionInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestGrossWeightIsEqualBetweenBillAndPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Weight = 4;
			pack1.APA_WeightUQ = Core.Constants.Weight.Kilograms;

			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_GrossWeight = 5;

			AssertHasMessageError(bill.ABL_GrossWeightInfo, "Weight of packages (4) is not equal to manifest Gross Weight (5) in (KG)");

			var pack2 = bill.Packs.AddNew();
			pack2.APA_Weight = 3000;
			pack2.APA_WeightUQ = Core.Constants.Weight.Grams;

			bill.ABL_GrossWeight = 7;

			AssertNoMessageErrors(bill.ABL_GrossWeightInfo);
		}

		public void TestVolumeIsEqualBetweenBillAndPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Volume = 4;
			pack1.APA_VolumeUQ = Core.Constants.Volume.Litre;

			bill.ABL_VolumeUQ = Core.Constants.Volume.Litre;
			bill.ABL_Volume = 5;

			AssertHasMessageError(bill.ABL_VolumeInfo, "Volume of packages (4) is not equal to manifest Volume (5) in (L)");

			var pack2 = bill.Packs.AddNew();
			pack2.APA_Volume = 4000;
			pack2.APA_VolumeUQ = Core.Constants.Volume.CubicCentimeters;

			bill.ABL_Volume = 8;

			AssertNoMessageErrors(bill.ABL_VolumeInfo);
		}

		public void TestCheckABL_ManifestQtyMatchSumOfPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_ManifestQty = 11;

			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 6;

			pack = bill.Packs.AddNew();
			pack.APA_PackQty = 5;

			bill.Validation.ValidateABL_ManifestQty();
			AssertNoMessageErrors(bill.ABL_ManifestQtyInfo);

			pack = bill.Packs.AddNew();
			pack.APA_PackQty = 1;

			bill.Validation.ValidateABL_ManifestQty();
			AssertHasMessageError(bill.ABL_ManifestQtyInfo, "Sum of packages' package counts (12) is not equal to manifest quantity");
		}

		public void TestCheckABL_GoodsDescription()
		{
			bill.ABL_GoodsDescription = ZString.Empty;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(bill.ABL_GoodsDescriptionInfo);
		}

		public void TestCheckABL_OA_Shipper()
		{
			bill.ABL_OA_Shipper = ZGuid.Empty;
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, MandatoryValidation.YouHaveNotEntered);

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "SHIPPER NAME";
			var orgAddress = org.MainAddress;

			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertNoNotifications(bill.ABL_OA_ShipperInfo);
		}

		public void TestCheckABL_Volume()
		{
			bill.ABL_Volume = 0;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(bill.ABL_VolumeInfo);
		}

		public void TestCheckABL_VolumeUQ()
		{
			bill.ABL_VolumeUQ = ZString.Empty;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(bill.ABL_VolumeUQInfo, "XX", Core.Constants.Volume.CubicFeet);

			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicFeet;
			AssertNoNotifications(bill.ABL_VolumeUQInfo);
		}

		public void TestCheckABL_OA_NotifyParty()
		{
			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, MandatoryValidation.YouHaveNotEntered);

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "NOTIFY NAME";
			var orgAddress = org.MainAddress;

			bill.ABL_OA_NotifyParty = orgAddress.PK;
			AssertNoNotifications(bill.ABL_OA_NotifyPartyInfo);
		}

		public void TestCheckABL_BillIssueDate()
		{
			bill.ABL_BillIssueDate = ZDate.Empty;
			AssertHasMessageErrorContaining(bill.ABL_BillIssueDateInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_BillIssueDate = ZDate.Today;
			AssertNoNotifications(bill.ABL_BillIssueDateInfo);
		}

		public void TestCheckABL_BillNumber()
		{
			bill.ABL_BillNumber = "123";
			AssertNoNotifications(bill.ABL_BillNumberInfo);

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "123";
			AssertHasErrorContaining(bill2.ABL_BillNumberInfo, "Bill number must be unique");
			bill2.ABL_BillNumber = "234";
			AssertNoErrorContaining(bill2.ABL_BillNumberInfo, "Bill number must be unique");
		}

		public void TestCheckABL_ShipperRegNo()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ShipperRegNo = ZString.Empty;
			AssertNoNotifications(bill.ABL_ShipperRegNoInfo);

			bill.ABL_RN_NKShipperCountry = "PE";
			bill.ABL_ShipperRegNo = ZString.Empty;
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, "You have not entered a value.");

			bill.ABL_ShipperRegNo = "PE";
			AssertNoNotifications(bill.ABL_ShipperRegNoInfo);
		}

		public void TestCheckABL_ConsigneeRegNo()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ConsigneeRegNo = ZString.Empty;
			AssertNoNotifications(bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_RN_NKConsigneeCountry = "PE";
			bill.ABL_ConsigneeRegNo = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, "You have not entered a value.");

			bill.ABL_ConsigneeRegNo = "13245";
			AssertNoNotifications(bill.ABL_ConsigneeRegNoInfo);
		}

		public void TestCheckABL_NotifyPartyRegNo()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_NotifyPartyRegNo = ZString.Empty;
			AssertNoNotifications(bill.ABL_NotifyPartyRegNoInfo);
			
			bill.ABL_RN_NKNotifyPartyCountry = "PE";
			bill.ABL_NotifyPartyRegNo = ZString.Empty;
			AssertHasMessageError(bill.ABL_NotifyPartyRegNoInfo, "You have not entered a value.");

			bill.ABL_NotifyPartyRegNo = "13245";
			AssertNoNotifications(bill.ABL_NotifyPartyRegNoInfo);
		}

		public void TestCheckABL_ConsigneeRegNoType()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.ABL_RN_NKConsigneeCountry = "PE";
			bill.ABL_ConsigneeRegNoType = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoTypeInfo, "You have not entered a value.");

			bill.ABL_RN_NKConsigneeCountry = "";
			bill.ABL_ConsigneeRegNoType = "VAT";
			AssertHasMessageError(bill.ABL_ConsigneeRegNoTypeInfo, "The code you have selected is not in the list.");

			bill.ABL_ConsigneeRegNoType = OrgCusCode.CodeTypes.PassportID;
			AssertNoNotifications(bill.ABL_ConsigneeRegNoTypeInfo);

			bill.ABL_RN_NKConsigneeCountry = "PE";
			bill.ABL_ConsigneeRegNoType = OrgCusCode.CodeTypes.PassportID;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoTypeInfo, "RUC or DNI type should be selected");

			bill.ABL_ConsigneeRegNoType = OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode;
			AssertNoNotifications(bill.ABL_ConsigneeRegNoTypeInfo);
		}

		public void TestCheckABL_ShipperRegNoType()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.ABL_RN_NKShipperCountry = "PE";
			bill.ABL_ShipperRegNoType = ZString.Empty;
			AssertHasMessageError(bill.ABL_ShipperRegNoTypeInfo, "You have not entered a value.");

			bill.ABL_RN_NKShipperCountry = "";
			bill.ABL_ShipperRegNoType = "VAT";
			AssertHasMessageError(bill.ABL_ShipperRegNoTypeInfo, "The code you have selected is not in the list.");

			bill.ABL_ShipperRegNoType = OrgCusCode.CodeTypes.PassportID;
			AssertNoNotifications(bill.ABL_ShipperRegNoTypeInfo);

			bill.ABL_RN_NKShipperCountry = "PE";
			bill.ABL_ShipperRegNoType = OrgCusCode.CodeTypes.PassportID;
			AssertHasMessageError(bill.ABL_ShipperRegNoTypeInfo, "RUC or DNI type should be selected");

			bill.ABL_ShipperRegNoType = OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode;
			AssertNoNotifications(bill.ABL_ShipperRegNoTypeInfo);
		}

		public void TestCheckABL_NotifyPartyRegNoType()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.ABL_RN_NKNotifyPartyCountry = "PE";
			bill.ABL_NotifyPartyRegNoType = ZString.Empty;
			AssertHasMessageError(bill.ABL_NotifyPartyRegNoTypeInfo, "You have not entered a value.");

			bill.ABL_RN_NKNotifyPartyCountry = "";
			bill.ABL_NotifyPartyRegNoType = "VAT";
			AssertHasMessageError(bill.ABL_NotifyPartyRegNoTypeInfo, "The code you have selected is not in the list.");

			bill.ABL_NotifyPartyRegNoType = OrgCusCode.CodeTypes.PassportID;
			AssertNoNotifications(bill.ABL_NotifyPartyRegNoTypeInfo);

			bill.ABL_RN_NKNotifyPartyCountry = "PE";
			bill.ABL_NotifyPartyRegNoType = OrgCusCode.CodeTypes.PassportID;
			AssertHasMessageError(bill.ABL_NotifyPartyRegNoTypeInfo, "RUC or DNI type should be selected");

			bill.ABL_NotifyPartyRegNoType = OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode;
			AssertNoNotifications(bill.ABL_NotifyPartyRegNoTypeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			bill = header.Bills.AddNew();
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
