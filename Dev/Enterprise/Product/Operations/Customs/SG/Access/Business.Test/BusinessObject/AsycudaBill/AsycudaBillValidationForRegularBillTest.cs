using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.SG.Access.Business.Registry;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.Asycuda;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class AsycudaBillValidationForRegularBillTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCycleNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			bill.CycleNumber = ZString.Empty;
			AssertNoNotifications(bill.CycleNumberInfo);
			bill.CycleNumber = "XXX";
			AssertHasMessageError(bill.CycleNumberInfo, ListValidation.InvalidCodeMessageError);
			bill.CycleNumber = "9999";
			AssertHasMessageError(bill.CycleNumberInfo, ListValidation.InvalidCodeMessageError);
			bill.CycleNumber = "1";
			AssertNoNotifications(bill.CycleNumberInfo);
		}

		public void TestCheckSG_PartyStatus()
		{
			var billCountry = Factory.New<AsycudaBill>();
			billCountry.SG_PartyStatus = "";
			AssertNoMessageErrors(billCountry.SG_PartyStatusInfo);
			billCountry.SG_PartyStatus = "~";
			AssertHasMessageErrorContaining(billCountry.SG_PartyStatusInfo, ListValidation.InvalidCodeMessageError);
			billCountry.SG_PartyStatus = SGPartyStatusList.Codes.A;
			AssertNoMessageErrors(billCountry.SG_PartyStatusInfo);
		}

		public void TestCheckSG_PayeeIndicator()
		{
			var billCountry = Factory.New<AsycudaBill>();
			billCountry.SG_PayeeIndicator = "~";
			AssertHasMessageErrorContaining(billCountry.SG_PayeeIndicatorInfo, ListValidation.InvalidCodeMessageError);
			billCountry.SG_PayeeIndicator = SGPayeeIndicatorList.Codes.Q;
			AssertNoMessageErrors(billCountry.SG_PayeeIndicatorInfo);
		}

		public void TestCheckStatusDescription()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Error;
			var warning = "ERR - R01 Message Error 1 - R02 Message Error 1";
			bill.Logs.AddNew(Events.MessageStatusChange, warning);
			bill.Validation.ValidateAll();
			AssertHasWarningContaining(bill.ShortStatusDescriptionInfo, warning);
			bill.Logs.RemoveAndDeleteAll();
			bill.Validation.ValidateAll();
			AssertNoWarning(bill.ShortStatusDescriptionInfo, warning);
		}

		public void TestCheckCustomsJobNumber()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MainAddress.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.InterbankGIRO, "201101", Core.Constants.CountryCodes.Singapore);
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = Universal.Helper.ShipmentTypeList.Codes.Import23;
			var pack = bill.Packs.AddNew();
			pack.PackedItem.CustomsEntryNumbers.RemoveAndDeleteAll();
			var validation = (AsycudaBillValidationForRegularBill)bill.Validation;
			validation.ValidateCustomsJobNumber();
			AssertNoMessageError("Should not have the message error as the bill is not linked an IBG Account.", bill.CustomsJobNumberInfo, ValidationConstants.Bill.SGShouldHaveTradeNetPermitNumberWithLinkedIBGAccount);
			bill.ConsigneeOrgPK = consignee.PK;
			validation.ValidateCustomsJobNumber();
			AssertHasMessageError("Should have the message error as there's no valid TNP number on the packline.", bill.CustomsJobNumberInfo, ValidationConstants.Bill.SGShouldHaveTradeNetPermitNumberWithLinkedIBGAccount);
			var entryNumber = pack.PackedItem.CustomsEntryNumbers.AddNew();
			entryNumber.CE_EntryType = ASYCUDA.Business.Constants.CustomsEntryType.TradeNetPermit;
			entryNumber.CE_EntryNum = "00001";
			validation.ValidateCustomsJobNumber();
			AssertNoMessageError("Should not have the message error as there's a valid TNP number on the packline.", bill.CustomsJobNumberInfo, ValidationConstants.Bill.SGShouldHaveTradeNetPermitNumberWithLinkedIBGAccount);
			bill.Packs.AddNew();
			validation.ValidateCustomsJobNumber();
			AssertHasMessageError("Should have the message error as there's no valid TNP number on the new packline.", bill.CustomsJobNumberInfo, ValidationConstants.Bill.SGShouldHaveTradeNetPermitNumberWithLinkedIBGAccount);
		}

		public void TestGSTNReferenceNo()
		{
			using (SGAccessRegistry.Instance.OVRLiveEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2022, 1, 1)))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var bill = header.Bills.AddNew();
				var pack = bill.Packs.AddNew();
				var packedItem = pack.PackedItem;
				header.AMA_ManifestType = Constants.ManifestType.Import;
				bill.GSTNReferenceNo = "";
				packedItem.GSTPaid = "";
				var validation = (AsycudaBillValidationForRegularBill)bill.Validation;
				validation.ValidateGSTNReferenceNo();
				AssertNoMessageErrors(bill.GSTNReferenceNoInfo);
				packedItem.GSTPaid = Customs.Business.YesNoList.Codes.Yes;
				validation.ValidateGSTNReferenceNo();
				AssertHasMessageError("The GST Registration No must be entered when GST Paid is entered against any Pack Line.", bill.GSTNReferenceNoInfo, ValidationConstants.Bill.GSTRegistrationNoMustNotBeBlank);
				bill.GSTNReferenceNo = "A23B423";
				validation.ValidateGSTNReferenceNo();
				AssertNoMessageErrors(bill.GSTNReferenceNoInfo);
			}
		}

		public void TestSGBillRowMessageErrors()
		{
			var header = (ASYCUDA.Business.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "X";
			AssertHasMessageError(bill.ABL_BillNumberInfo, "This manifest type requires each Bill to have Packs entered.");
			bill.Packs.AddNew();
			bill.Validation.ValidateABL_BillNumber();
			AssertNoMessageError(bill.ABL_BillNumberInfo, "This manifest type requires each Bill to have Packs entered.");
		}

		public void TestBillsValidationNoWarnings()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_GrossWeight = 0;
			bill.Validation.ValidateABL_GrossWeight();
			AssertNoMessageErrors(bill.ABL_GrossWeightInfo);
			bill.ABL_GrossWeightUQ = ZString.Empty;
			bill.Validation.ValidateABL_GrossWeightUQ();
			AssertNoMessageErrors(bill.ABL_GrossWeightUQInfo);
			bill.ABL_ManifestQty = 0;
			bill.Validation.ValidateABL_ManifestQty();
			AssertNoMessageErrors(bill.ABL_ManifestQtyInfo);
			bill.ABL_ManifestUQ = ZString.Empty;
			bill.Validation.ValidateABL_ManifestUQ();
			AssertNoMessageErrors(bill.ABL_ManifestUQInfo);
			bill.ABL_GrossWeightUQ = "1";
			bill.Validation.ValidateABL_GrossWeightUQ();
			AssertNoMessageErrors(bill.ABL_GrossWeightUQInfo);
			bill.ABL_ManifestUQ = "1";
			bill.Validation.ValidateABL_ManifestUQ();
			AssertNoMessageErrors(bill.ABL_ManifestUQInfo);
			bill.ABL_ConsigneeStreet1 = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeStreet1();
			AssertNoMessageErrors(bill.ABL_ConsigneeStreet1Info);
			bill.ABL_ConsigneeStreet2 = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeStreet2();
			AssertNoMessageErrors(bill.ABL_ConsigneeStreet2Info);
			bill.ABL_ConsigneeCity = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeCity();
			AssertNoMessageErrors(bill.ABL_ConsigneeCityInfo);
			bill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			bill.Validation.ValidateABL_RN_NKConsigneeCountry();
			AssertNoMessageErrors(bill.ABL_RN_NKConsigneeCountryInfo);
			bill.ABL_ConsigneePostcode = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneePostcode();
			AssertNoMessageErrors(bill.ABL_ConsigneePostcodeInfo);
		}

		public void TestCheckABL_ManifestQtyMatchSumOfPacks()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ManifestQty = 10;
			bill.ABL_ManifestUQ = "PKG";
			var pack1 = bill.Packs.AddNew();
			pack1.APA_PackQty = 6;
			var pack2 = bill.Packs.AddNew();
			pack2.APA_PackQty = 6;
			bill.Validation.ValidateABL_ManifestQty();
			AssertNoWarnings(bill.ABL_ManifestQtyInfo);
		}

		public void TestFreightValueCurrency()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.ABL_FreightValue = 10;
			AssertNoMessageErrors(bill.ABL_RX_NKFreightValueCurrencyInfo);
			AssertNoMessageError(bill.ABL_FreightValueInfo, "Goods Value cannot be zero.");
			bill.ABL_RX_NKFreightValueCurrency = "XXX";
			AssertHasError(bill.ABL_RX_NKFreightValueCurrencyInfo, "Enter a valid Freight Currency.");
			bill.ABL_RX_NKFreightValueCurrency = ZString.Empty;
			AssertNoError(bill.ABL_RX_NKFreightValueCurrencyInfo, "Enter a valid Freight Currency.");
			AssertHasMessageError(bill.ABL_RX_NKFreightValueCurrencyInfo, "You have not entered a Freight Currency.");
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.ABL_FreightValue = 0;
			AssertHasMessageError(bill.ABL_FreightValueInfo, "Goods Value cannot be zero.");
		}

		public void TestCheckABL_RL_NKFinalDestination()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = Business.Constants.ManifestType.Export;
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "SGSIN";
			bill.ABL_RL_NKFinalDestination = "SGSIN";
			AssertHasMessageError(bill.ABL_RL_NKFinalDestinationInfo, ValidationConstants.PortCannotSingaporePortForExport("Destination"));
			bill.ABL_RL_NKFinalDestination = "ZACPT";
			AssertNoMessageError(bill.ABL_RL_NKFinalDestinationInfo, ValidationConstants.PortCannotSingaporePortForExport("Destination"));
			header.AMA_ManifestType = Business.Constants.ManifestType.Import;
			bill.ABL_RL_NKFinalDestination = "ZACPT";
			bill.Validation.ValidateABL_RL_NKFinalDestination();
			AssertHasMessageError(bill.ABL_RL_NKFinalDestinationInfo, ValidationConstants.PortMustBeSingaporePortForImport("Destination"));
			bill.ABL_RL_NKFinalDestination = "SGSIN";
			AssertNoMessageError(bill.ABL_RL_NKFinalDestinationInfo, ValidationConstants.PortMustBeSingaporePortForImport("Destination"));
		}

		public void TestCheckABL_RL_NKOrigin()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = Business.Constants.ManifestType.Export;
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKFinalDestination = "SGSIN";
			bill.ABL_RL_NKOrigin = "ZACPT";
			AssertHasMessageError(bill.ABL_RL_NKOriginInfo, ValidationConstants.PortMustBeSingaporePortForExport("Origin"));
			bill.ABL_RL_NKOrigin = "SGSIN";
			AssertNoMessageError(bill.ABL_RL_NKOriginInfo, ValidationConstants.PortMustBeSingaporePortForExport("Origin"));
			header.AMA_ManifestType = Business.Constants.ManifestType.Import;
			bill.ABL_RL_NKOrigin = "SGSIN";
			bill.Validation.ValidateABL_RL_NKOrigin();
			AssertHasMessageError(bill.ABL_RL_NKOriginInfo, ValidationConstants.PortCannotBeSingaporePortForImport("Origin"));
			bill.ABL_RL_NKOrigin = "ZACPT";
			AssertNoMessageError(bill.ABL_RL_NKOriginInfo, ValidationConstants.PortCannotBeSingaporePortForImport("Origin"));
		}

		public void TestCheckFreightValue_CheckTotalPackLinePriceValue()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_FreightValue = 90m;
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.Singapore;
			var pack1 = bill.Packs.AddNew();
			pack1.LinePrice = 80.51m;
			pack1.LinePriceCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.Validation.ValidateABL_FreightValue();
			AssertHasWarning(bill.ABL_FreightValueInfo, ValidationConstants.Bill.SGGoodsValueCurrencyNotSameWithLinePriceCurrency);
			AssertNoMessageError(bill.ABL_FreightValueInfo, ValidationConstants.Bill.SGGoodsValueNotMatchTotalLinePrice);
			var pack2 = bill.Packs.AddNew();
			pack2.LinePrice = 100.49m;
			pack2.LinePriceCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.Validation.ValidateABL_FreightValue();
			AssertHasWarning(bill.ABL_FreightValueInfo, ValidationConstants.Bill.SGGoodsValueCurrencyNotSameWithLinePriceCurrency);
			AssertNoMessageError(bill.ABL_FreightValueInfo, ValidationConstants.Bill.SGGoodsValueNotMatchTotalLinePrice);
			bill.ABL_RX_NKFreightValueCurrency = ZString.Empty;
			bill.Validation.ValidateABL_FreightValue();
			AssertNoWarning(bill.ABL_FreightValueInfo, ValidationConstants.Bill.SGGoodsValueCurrencyNotSameWithLinePriceCurrency);
			AssertNoMessageError(bill.ABL_FreightValueInfo, ValidationConstants.Bill.SGGoodsValueNotMatchTotalLinePrice);
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.Validation.ValidateABL_FreightValue();
			AssertNoWarning(bill.ABL_FreightValueInfo, ValidationConstants.Bill.SGGoodsValueCurrencyNotSameWithLinePriceCurrency);
			AssertHasMessageError(bill.ABL_FreightValueInfo, ValidationConstants.Bill.SGGoodsValueNotMatchTotalLinePrice);
			bill.ABL_FreightValue = 181m;
			AssertNoWarning(bill.ABL_FreightValueInfo, ValidationConstants.Bill.SGGoodsValueCurrencyNotSameWithLinePriceCurrency);
			AssertNoMessageError(bill.ABL_FreightValueInfo, ValidationConstants.Bill.SGGoodsValueNotMatchTotalLinePrice);
		}

		protected override void SetUp()
		{
			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			accessEnableDisposable = accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			base.SetUp();
		}

		protected override void TearDown()
		{
			accessEnableDisposable.Dispose();
			base.TearDown();
		}

		IDisposable accessEnableDisposable;
	}
}
