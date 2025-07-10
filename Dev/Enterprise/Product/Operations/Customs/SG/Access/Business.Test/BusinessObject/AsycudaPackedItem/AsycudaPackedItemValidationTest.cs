using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.Access.Business.Registry;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class AsycudaPackedItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckGoodsType()
		{
			var helper = new ZZDataTestHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsType, "GoodsType");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.DutiableGoods, "Dutiable Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.ControlledGoods, "Controlled Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.GoodsType, "KD", "KD DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			packedItem.GoodsType = "KD";
			AssertHasMessageErrorContaining(packedItem.GoodsTypeInfo, ListValidation.InvalidCodeMessageError);
			packedItem.GoodsType = Constants.GoodsType.DutiableGoods;
			AssertNoMessageErrorContaining(packedItem.GoodsTypeInfo, ListValidation.InvalidCodeMessageError);
			packedItem.GoodsType = Constants.GoodsType.ControlledGoods;
			AssertNoMessageErrorContaining(packedItem.GoodsTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2018, 06, 01)]
		public void TestCheckGoodsTypeWithTariff()
		{
			var consignee = CreateOrg("SNOWSILL, SONYA", "CGNTEST1", "8AU0495926");
			var helper = new ZZDataTestHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsType, "GoodsType");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.DutiableGoods, "Dutiable Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.ControlledGoods, "Controlled Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.NormalGoods, "Normal Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.MajorExporter, "Major Exporter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.GoodsType, "KD", "KD DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_E_DEP = new ZDateTime(2017, 12, 2);
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			header.AMA_ManifestType = Constants.ManifestType.Import;
			AssertEquals(4, packedItem.Lookups.GoodsTypeList.Count);
			AssertEquals(true, packedItem.Lookups.GoodsTypeList.ContainsCode(Constants.GoodsType.ControlledGoods));
			header.AMA_ManifestType = Constants.ManifestType.Export;
			AssertEquals(2, packedItem.Lookups.GoodsTypeList.Count);
			packedItem.API_Tariff = "25161100";
			AssertEquals(true, packedItem.Tariff.IsUnderImportControl(ZDateTime.Now));
			packedItem.GoodsType = Constants.GoodsType.NormalGoods;
			AssertHasMessageError(packedItem.GoodsTypeInfo, "'CT' must be used when the Tariff is not Blank and is a controlled type.");
			packedItem.API_Tariff = "93063019";
			AssertEquals(true, packedItem.Tariff.IsUnderExportControl(ZDateTime.Now));
			packedItem.GoodsType = Constants.GoodsType.NormalGoods;
			AssertHasMessageError(packedItem.GoodsTypeInfo, "'CT' must be used when the Tariff is not Blank and is a controlled type.");
			packedItem.API_Tariff = "01029010";
			packedItem.GoodsType = Constants.GoodsType.NormalGoods;
			AssertNoMessageError(packedItem.GoodsTypeInfo, "'CT' must be used when the Tariff is not Blank and is a controlled type.");
			header.AMA_ManifestType = Constants.ManifestType.Import;
			packedItem.API_DutyAmount = 1;
			packedItem.GoodsType = Constants.GoodsType.DutiableGoods;
			AssertNoMessageError(packedItem.GoodsTypeInfo, "'DT' must be used when the Duty amount is not 0");
			packedItem.API_DutyAmount = 1;
			packedItem.GoodsType = Constants.GoodsType.NormalGoods;
			AssertHasMessageError(packedItem.GoodsTypeInfo, "'DT' must be used when the Duty amount is not 0");
			packedItem.API_DutyAmount = 0;
			packedItem.GoodsType = Constants.GoodsType.NormalGoods;
			AssertNoMessageError(packedItem.GoodsTypeInfo, "'DT' must be used when the Duty amount is not 0");
			bill.ABL_OA_Consignee = consignee.MainAddress.PK;
			packedItem.API_Tariff = "01029010";
			packedItem.GoodsType = Constants.GoodsType.NormalGoods;
			AssertHasMessageError(packedItem.GoodsTypeInfo, "'ME' must be used when the Tariff is not Blank and is a controlled type.");
			packedItem.GoodsType = Constants.GoodsType.MajorExporter;
			AssertNoMessageError(packedItem.GoodsTypeInfo, "'ME' must be used when the Tariff is not Blank and is a controlled type.");

			OrgHeader CreateOrg(ZString companyName, ZString orgCode, ZString accountNumber)
			{
				var org = Factory.New<OrgHeader>();
				org.OH_FullName = companyName;
				org.OH_Code = orgCode;
				org.MainAddress.OA_Address1 = companyName + " STR 1";
				org.MainAddress.OA_Address2 = companyName + " STR 2";
				org.MainAddress.OA_City = companyName + " CITY";
				org.MainAddress.OA_State = companyName + " STATE";
				org.MainAddress.OA_PostCode = accountNumber.Left(6) + "POST";
				org.CustomsCodes.AddNew("UAN", accountNumber, Core.Constants.CountryCodes.Singapore);
				org.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.PartyStatusType, SGPartyStatusList.Codes.Y, Core.Constants.CountryCodes.Singapore);
				return org;
			}
		}

		public void TestMandatoryFieldsWhenManifestShowPackItemAttributeApplied()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.PackageTypes, "PK1", "PACKS1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			packedItem.API_TaxAmount = 1.0; //Must change to not the same value
			packedItem.API_GoodsDescription = "";
			packedItem.API_CustomsQty = 0.0;
			packedItem.API_CustomsUQ = "";
			packedItem.API_TaxAmount = 0.0;
			packedItem.API_RN_NKGoodsOrigin = "";
			AssertHasMessageErrorContaining(packedItem.API_GoodsDescriptionInfo, "Goods Description is a required field for Singapore");
			AssertHasMessageErrorContaining(packedItem.API_CustomsQtyInfo, "Customs Qty is a required field for Singapore");
			AssertHasMessageErrorContaining(packedItem.API_CustomsUQInfo, "You have not entered a Customs UQ");
			AssertHasMessageErrorContaining(packedItem.API_TaxAmountInfo, "Tax Amount is a required field for Singapore");
			AssertHasMessageErrorContaining(packedItem.API_RN_NKGoodsOriginInfo, "Goods Origin is a required field for Singapore");
			packedItem.API_GoodsDescription = "Description";
			packedItem.API_CustomsQty = 5.0;
			packedItem.API_CustomsUQ = "PK1";
			packedItem.API_TaxAmount = 5.0;
			packedItem.API_RN_NKGoodsOrigin = "AU";
			AssertNoMessageErrorContaining(packedItem.API_GoodsDescriptionInfo, "Goods Description is a required field for Singapore");
			AssertNoMessageErrorContaining(packedItem.API_CustomsQtyInfo, "Customs Qty is a required field for Singapore");
			AssertNoMessageErrorContaining(packedItem.API_CustomsUQInfo, "You have not entered a Customs UQ");
			AssertNoMessageErrorContaining(packedItem.API_CustomsUQInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(packedItem.API_TaxAmountInfo, "Tax Amount is a required field for Singapore");
			AssertNoMessageErrorContaining(packedItem.API_RN_NKGoodsOriginInfo, "Goods Origin is a required field for Singapore");
			packedItem.API_CustomsUQ = "KG";
			AssertHasMessageErrorContaining(packedItem.API_CustomsUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestGoodsTypeForSG()
		{
			var helper = new ZZDataTestHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsType, "GoodsType");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.DutiableGoods, "Dutiable Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.ControlledGoods, "Controlled Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.NormalGoods, "Normal Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.MajorExporter, "Major Exporter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.GoodsType, "KD", "KD DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			packedItem.GoodsType = "KD";
			AssertHasMessageErrorContaining(packedItem.GoodsTypeInfo, ListValidation.InvalidCodeMessageError);
			packedItem.GoodsType = Constants.GoodsType.DutiableGoods;
			AssertNoMessageErrorContaining(packedItem.GoodsTypeInfo, ListValidation.InvalidCodeMessageError);
			packedItem.GoodsType = Constants.GoodsType.ControlledGoods;
			AssertNoMessageErrorContaining(packedItem.GoodsTypeInfo, ListValidation.InvalidCodeMessageError);
			packedItem.GoodsType = ZString.Empty;
			AssertNoMessageErrors("For SG Manifest Goods type will always default back to 'NT' or 'ME' when blanked out", packedItem.GoodsTypeInfo);
			AssertEquals(Constants.GoodsType.NormalGoods, packedItem.GoodsType);
			packedItem.Pack.Bill.SG_PartyStatus = YesNoList.Codes.Yes;
			packedItem.GoodsType = ZString.Empty;
			AssertNoMessageErrors("For SG Manifest Goods type will always default back to 'NT' or 'ME' when blanked out", packedItem.GoodsTypeInfo);
			AssertEquals(Constants.GoodsType.MajorExporter, packedItem.GoodsType);
		}

		[TestDate(2015, 1, 1)]
		public void TestCheckAPI_DutyAmount()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			header.AMA_ManifestType = Constants.ManifestType.Import;
			packedItem.API_Tariff = "22030090";
			packedItem.API_CustomsValue = 401m;
			packedItem.API_DutyAmount = 0m;
			AssertHasMessageError(packedItem.API_DutyAmountInfo, "Duty is applicable and cannot be calculated. Please supply the duty amount");
			packedItem.API_DutyAmount = 12m;
			AssertNoMessageErrors(packedItem.API_DutyAmountInfo);
			packedItem.API_Tariff = "22030090";
			packedItem.API_CustomsValue = 399m;
			packedItem.API_DutyAmount = 0m;
			AssertNoMessageErrors(packedItem.API_DutyAmountInfo);
			packedItem.API_DutyAmount = 12m;
			AssertNoMessageErrors(packedItem.API_DutyAmountInfo);
			header.AMA_ManifestType = Constants.ManifestType.Export;
			packedItem.API_DutyAmount = 0m;
			AssertNoMessageErrors(packedItem.API_DutyAmountInfo);
		}

		public void TestAPI_TaxAmount()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BAG", "BAG 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var messageError = ASYCUDA.Business.ValidationConstants.FieldIsMandatory("Tax Amount is a required field", "Singapore");
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			packedItem.API_CustomsValue = ZDecimal.Zero;
			packedItem.API_TaxAmount = ZDecimal.Zero;
			packedItem.Validation.ValidateAPI_TaxAmount();
			AssertHasMessageError(packedItem.API_TaxAmountInfo, messageError);
			packedItem.API_CustomsValue = -1m;
			packedItem.API_TaxAmount = ZDecimal.Zero;
			packedItem.Validation.ValidateAPI_TaxAmount();
			AssertHasMessageError(packedItem.API_TaxAmountInfo, messageError);
			packedItem.API_CustomsValue = Constants.Deminimis;
			packedItem.API_TaxAmount = ZDecimal.Zero;
			packedItem.Validation.ValidateAPI_TaxAmount();
			AssertHasMessageError(packedItem.API_TaxAmountInfo, messageError);
			packedItem.API_CustomsValue = Constants.Deminimis + 1;
			packedItem.API_TaxAmount = ZDecimal.Zero;
			packedItem.Validation.ValidateAPI_TaxAmount();
			AssertHasMessageError(packedItem.API_TaxAmountInfo, messageError);
			packedItem.API_CustomsValue = Constants.Deminimis - 1;
			packedItem.API_TaxAmount = ZDecimal.Zero;
			packedItem.Validation.ValidateAPI_TaxAmount();
			AssertNoMessageError(packedItem.API_TaxAmountInfo, messageError);
		}

		public void TestCheckGSTPaid()
		{
			using (SGAccessRegistry.Instance.OVRLiveEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2022, 1, 1)))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var bill = header.Bills.AddNew();
				var pack = bill.Packs.AddNew();
				var packedItem = pack.PackedItem;
				header.AMA_ManifestType = Constants.ManifestType.Import;
				packedItem.GSTPaid = "";
				var validation = packedItem.Validation;
				validation.ValidateGSTPaid();
				AssertNoMessageErrors(packedItem.GSTPaidInfo);
				bill.GSTNReferenceNo = "1A2B3C4F";
				validation.ValidateGSTPaid();
				AssertHasMessageError("The GST Paid indicator must be entered when GST Registration No is entered against the Bill.", packedItem.GSTPaidInfo, SG.Access.Business.ValidationConstants.Bill.GSTPaidFlagMustNotBeBlank);
				packedItem.GSTPaid = YesNoList.Codes.Yes;
				validation.ValidateGSTPaid();
				AssertNoMessageErrors(packedItem.GSTPaidInfo);
				packedItem.GSTPaid = "A";
				AssertHasMessageErrorContaining(packedItem.GSTPaidInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestGSTLowValueThresholdValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("DEM", 400m, "SG", new ZDateTime(2022, 1, 1), new ZDateTime(2079, 6, 6), "Low Value Threshold");
			Factory.Save();

			using (SGAccessRegistry.Instance.OVRLiveEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2022, 1, 1)))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_ManifestType = Constants.ManifestType.Import;
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;

				var bill = header.Bills.AddNew();
				bill.ABL_CustomsValue = 399m;

				var pack = bill.Packs.AddNew();
				var packedItem = pack.PackedItem;
				packedItem.GSTPaid = "Y";

				var validation = packedItem.Validation;
				validation.ValidateGSTPaid();
				AssertHasMessageError("The GST Paid indicator must be blank when the Customs Value is less than or equal to the deminimus value and the Transport Mode is Air Freight.", packedItem.GSTPaidInfo, "The GST Paid indicator must be blank when the Bill Customs Value is less than or equal to $400 and the Transport Mode is Air Freight.");

				bill.GSTNReferenceNo = "1A2B3C4F";
				validation.ValidateGSTPaid();
				AssertHasMessageError("The GST Paid indicator must be blank when the  Customs Value is less than or equal to the deminimus value and the Transport Mode is Air Freight.", packedItem.GSTPaidInfo, "The GST Paid indicator must be blank when the Bill Customs Value is less than or equal to $400 and the Transport Mode is Air Freight.");

				packedItem.GSTPaid = "";
				validation.ValidateGSTPaid();
				AssertHasMessageError("The GST Paid indicator must be entered when GST Registration No is entered against the Bill.", packedItem.GSTPaidInfo, SG.Access.Business.ValidationConstants.Bill.GSTPaidFlagMustNotBeBlank);

				header.AMA_TransportMode = Core.Constants.TransportModes.Road;
				packedItem.GSTPaid = "Y";
				bill.GSTNReferenceNo = "";
				validation.ValidateGSTPaid();
				AssertHasMessageError("The GST Paid indicator and the GTNReference, (on the Manifest Header), must be blank when the Bill Customs Value is less than or equal to the deminimus value and the Transport Mode is Road Freight.", packedItem.GSTPaidInfo, "The GST Paid indicator must be blank when the Bill Customs Value is less than or equal to $400 and the Transport Mode is Road Freight and the GSTN Reference, (on the Bill Details), is blank.");

				bill.GSTNReferenceNo = "1A2B3C4F";
				validation.ValidateGSTPaid();
				AssertNoMessageErrors(packedItem.GSTPaidInfo);

				packedItem.GSTPaid = "";
				validation.ValidateGSTPaid();
				AssertHasMessageError("The GST Paid indicator must be entered when GST Registration No is entered against the Bill.", packedItem.GSTPaidInfo, SG.Access.Business.ValidationConstants.Bill.GSTPaidFlagMustNotBeBlank);

				bill.ABL_CustomsValue = 401m;
				bill.GSTNReferenceNo = "";
				packedItem.GSTPaid = "Y";
				validation.ValidateGSTPaid();
				AssertHasMessageError("The GST Paid indicator must be blank when the Customs Value is more than the deminimus value and the GSTN is blank.", packedItem.GSTPaidInfo, "The GST Paid indicator must be blank when the Bill Customs Value is more than $400 and the GSTN Reference, (on the Bill Details), is blank.");

				packedItem.GSTPaid = "";
				validation.ValidateGSTPaid();
				AssertNoMessageErrors(packedItem.GSTPaidInfo);

				bill.GSTNReferenceNo = "1A2B3C4F";
				packedItem.GSTPaid = "Y";
				validation.ValidateGSTPaid();
				AssertNoMessageErrors(packedItem.GSTPaidInfo);

				bill.ABL_CustomsValue = 150m;
				bill.GSTNReferenceNo = "1A2B3C4F";
				packedItem.GSTPaid = "Y";
				validation.ValidateGSTPaid();
				AssertNoMessageErrors(packedItem.GSTPaidInfo);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(tariffType, "22030090");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Alcohol, tariff);
			var tariff1 = helper.LoadOrCreateNewTariff(tariffType, "25161100");
			var com1 = helper.CreateCommodity(tariff1, "com1");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISIMPORTCONTROL, "Y", com1);
			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "93063019");
			var com2 = helper.CreateCommodity(tariff2, "com2");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISEXPORTCONTROL, "Y", com2);
			var tariff3 = helper.LoadOrCreateNewTariff(tariffType, "01029010");
			var com3 = helper.CreateCommodity(tariff3, "com3");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISIMPORTCONTROL, "N", com3);
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISEXPORTCONTROL, "N", com3);
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISTRANSHIPMENTCONTROL, "N", com3);
			Factory.Save();
		}
	}
}
