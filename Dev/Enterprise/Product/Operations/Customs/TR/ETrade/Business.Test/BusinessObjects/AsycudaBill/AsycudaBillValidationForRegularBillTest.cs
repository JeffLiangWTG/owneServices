using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_Procedure()
		{
			Manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			Bill.ABL_BolType = ASYCUDA.Business.AsycudaBill.ChildBolCode;

			Bill.ABL_Procedure = ZString.Empty;
			AssertHasMessageErrorContaining(Bill.ABL_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.ABL_Procedure = "XYZ";
			AssertHasMessageErrorContaining(Bill.ABL_ProcedureInfo, ListValidation.InvalidCodeMessageError);
			Bill.ABL_Procedure = "12";
			AssertHasMessageErrorContaining(Bill.ABL_ProcedureInfo, ListValidation.InvalidCodeMessageError);
			Bill.ABL_Procedure = "8100";
			AssertNoMessageErrorContaining(Bill.ABL_ProcedureInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckGuaranteeFields()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			var bill = Manifest.Bills.AddNew();
			bill.GuaranteeType = "";
			bill.GuaranteeAmount = ZDecimal.Zero;
			bill.GuaranteeRefNo = ZString.Empty;
			bill.Validation.ValidateAll();
			AssertNoNotifications(bill.GuaranteeAmountInfo);
			AssertNoNotifications(bill.GuaranteeRefNoInfo);

			bill.GuaranteeType = bill.Lookups.BondTypeList[1].Code;
			bill.GuaranteeAmount = ZDecimal.Zero;
			bill.GuaranteeRefNo = ZString.Empty;
			bill.Validation.ValidateAll();
			AssertHasMessageErrorContaining(bill.GuaranteeAmountInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.GuaranteeRefNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.GuaranteeType = "ABC";
			bill.GuaranteeAmount = 1234.56m;
			bill.GuaranteeRefNo = "Test";
			bill.Validation.ValidateAll();
			AssertNoNotifications(bill.GuaranteeAmountInfo);
			AssertNoNotifications(bill.GuaranteeRefNoInfo);

			bill.GuaranteeAmount = -1234.56m;
			bill.Validation.ValidateAll();
			AssertHasMessageError(bill.GuaranteeAmountInfo, "Guarantee Amount must be greater or equal to zero.");

			bill.GuaranteeAmount = 100m;
			bill.Validation.ValidateAll();
			AssertNoMessageError(bill.GuaranteeAmountInfo, "Guarantee Amount must be greater or equal to zero.");

			bill.GuaranteeType = bill.Lookups.BondTypeList[0].Code;
			AssertNoMessageErrorContaining(bill.GuaranteeTypeInfo, "list");
			bill.GuaranteeType = "";
			AssertNoMessageErrorContaining(bill.GuaranteeTypeInfo, "list");
			bill.GuaranteeType = "X";
			AssertHasMessageErrorContaining(bill.GuaranteeTypeInfo, "list");
		}

		public void TestCheckDepartureCountry()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

			bill.DepartureCountry = "12";
			AssertHasMessageErrorContaining(bill.DepartureCountryInfo, ListValidation.InvalidCodeMessageError);

			bill.DepartureCountry = "TR";
			AssertNoMessageErrorContaining(bill.DepartureCountryInfo, ListValidation.InvalidCodeMessageError);

			bill.DepartureCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.DepartureCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTradeCountry()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

			bill.TradeCountry = "12";
			AssertHasMessageErrorContaining(bill.TradeCountryInfo, ListValidation.InvalidCodeMessageError);

			bill.TradeCountry = "TR";
			AssertNoMessageErrorContaining(bill.TradeCountryInfo, ListValidation.InvalidCodeMessageError);

			bill.TradeCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.TradeCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckExportCountry()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

			bill.ExportCountry = "12";
			AssertHasMessageErrorContaining(bill.ExportCountryInfo, ListValidation.InvalidCodeMessageError);

			bill.ExportCountry = "TR";
			AssertNoMessageErrorContaining(bill.ExportCountryInfo, ListValidation.InvalidCodeMessageError);

			bill.ExportCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ExportCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckArrivalCountry()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

			bill.ArrivalCountry = "12";
			AssertHasMessageErrorContaining(bill.ArrivalCountryInfo, ListValidation.InvalidCodeMessageError);

			bill.ArrivalCountry = "TR";
			AssertNoMessageErrorContaining(bill.ArrivalCountryInfo, ListValidation.InvalidCodeMessageError);

			bill.ArrivalCountry = ZString.Empty;
			AssertNoMessageErrors(bill.ArrivalCountryInfo);
		}

		public void TestCheckABL_Incoterm()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

			bill.ABL_Incoterm = "XYZ";
			AssertHasMessageError(bill.ABL_IncotermInfo, ListValidation.InvalidCodeMessageError);

			bill.ABL_Incoterm = bill.Lookups.IncotermList.GetCodeFromDescription(Core.Constants.IncoTerms.FreeAlongsideShip);
			AssertNoMessageErrorContaining(bill.ABL_IncotermInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckPaymentMethod()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

			bill.PaymentMethod = "X";
			AssertHasMessageError(bill.PaymentMethodInfo, ListValidation.InvalidCodeMessageError);

			bill.PaymentMethod = "1";
			AssertNoMessageErrorContaining(bill.PaymentMethodInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckABL_OtherValue()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

			bill.ABL_OtherValue = -123;
			AssertHasError(bill.ABL_OtherValueInfo, "Domestic Expenditure Value must be greater or equal to zero.");

			bill.ABL_OtherValue = 0;
			AssertNoError(bill.ABL_OtherValueInfo, "Domestic Expenditure Value must be greater or equal to zero.");

			bill.ABL_OtherValue = 123;
			AssertNoError(bill.ABL_OtherValueInfo, "Domestic Expenditure Value must be greater or equal to zero.");
		}

		public void TestCheckABL_GrossWeight()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;

			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_GrossWeight = 32;
			AssertHasMessageErrorContaining(bill.ABL_GrossWeightInfo, "Gross Weight value must be lower or equal to 30.00 Kg.");

			bill.ABL_GrossWeight = 30;
			AssertNoError(bill.ABL_GrossWeightInfo, "Gross Weight value must be lower or equal to 30.00 Kg.");

			bill.ABL_GrossWeight = 29;
			AssertNoError(bill.ABL_GrossWeightInfo, "Gross Weight value must be lower or equal to 30.00 Kg.");

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;

			bill.ABL_GrossWeight = 302;
			AssertHasMessageErrorContaining(bill.ABL_GrossWeightInfo, "Gross Weight value must be lower or equal to 300.00 Kg.");

			bill.ABL_GrossWeight = 300;
			AssertNoError(bill.ABL_GrossWeightInfo, "Gross Weight value must be lower or equal to 300.00 Kg.");

			bill.ABL_GrossWeight = 299;
			AssertNoError(bill.ABL_GrossWeightInfo, "Gross Weight value must be lower or equal to 300.00 Kg.");
		}

		public void TestCheckABL_GrossWeightUQ()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_GrossWeight = 32;
			AssertHasMessageErrorContaining(bill.ABL_GrossWeightInfo, "Gross Weight value must be lower or equal to 30.00 Kg.");

			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_GrossWeight = 30;
			AssertNoMessageErrorContaining(bill.ABL_GrossWeightInfo, "Gross Weight value must be lower or equal to 30.00 Kg.");

			bill.ABL_GrossWeightUQ = "G";
			bill.ABL_GrossWeight = 30001;
			AssertHasMessageErrorContaining(bill.ABL_GrossWeightInfo, "Gross Weight value must be lower or equal to 30.00 Kg.");

			bill.ABL_GrossWeightUQ = "G";
			bill.ABL_GrossWeight = 32;
			AssertNoMessageErrorContaining(bill.ABL_GrossWeightInfo, "Gross Weight value must be lower or equal to 30.00 Kg.");
		}

		public void TestCheckABL_CustomsValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				CurrencyTestHelper helper = new CurrencyTestHelper(Factory);
				helper.SetExchangeRate(helper.EURCurrency, 8.748600m, ZDateTime.Today);

				Factory.Save();

				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;

				header.Branch.Company.GC_IsReciprocal = true;
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;

				var bill1 = header.Bills.AddNew();

				bill1.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				bill1.ABL_CustomsValue = 1502;
				AssertHasMessageErrorContaining(bill1.ABL_CustomsValueInfo, "Customs Value must be lower or equal to 1500.00 EUR.");

				var bill2 = header.Bills.AddNew();
				bill2.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				bill2.ABL_CustomsValue = 1500;
				AssertNoError(bill2.ABL_CustomsValueInfo, "Customs Value must be lower or equal to 1500.00 EUR.");

				var bill3 = header.Bills.AddNew();
				bill3.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				bill3.ABL_CustomsValue = 1499;
				AssertNoError(bill3.ABL_CustomsValueInfo, "Customs Value must be lower or equal to 1500.00 EUR.");

				header.AMA_Nature = ShipmentTypeList.Codes.Export22;

				var bill1export = header.Bills.AddNew();
				bill1export.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				bill1export.ABL_CustomsValue = 15002;
				AssertHasMessageErrorContaining(bill1export.ABL_CustomsValueInfo, "Customs Value must be lower or equal to 15000.00 EUR.");

				var bill2export = header.Bills.AddNew();
				bill2export.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				bill2export.ABL_CustomsValue = 15000;
				AssertNoError(bill2export.ABL_CustomsValueInfo, "Customs Value must be lower or equal to 15000.00 EUR.");

				var bill3export = header.Bills.AddNew();
				bill3export.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				bill3export.ABL_CustomsValue = 14999;
				AssertNoError(bill3export.ABL_CustomsValueInfo, "Customs Value must be lower or equal to 15000.00 EUR.");

				helper.SetExchangeRate(helper.USDCurrency, 9.23433m, ZDateTime.Today);
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;

				var bill4 = header.Bills.AddNew();
				bill4.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				bill4.ABL_CustomsValue = 1502;
				AssertHasMessageErrorContaining(bill4.ABL_CustomsValueInfo, "Customs Value must be lower or equal to 1500.00 EUR.");

				var bill5 = header.Bills.AddNew();
				bill5.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				bill5.ABL_CustomsValue = 1500;
				AssertNoError(bill5.ABL_CustomsValueInfo, "Customs Value must be lower or equal to 1500.00 EUR.");

				var bill6 = header.Bills.AddNew();
				bill6.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				bill6.ABL_CustomsValue = 1499;
				AssertNoError(bill6.ABL_CustomsValueInfo, "Customs Value must be lower or equal to 1500.00 EUR.");

				header.AMA_Nature = ShipmentTypeList.Codes.Export22;

				var bill4export = header.Bills.AddNew();
				bill4export.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				bill4export.ABL_CustomsValue = 15002;
				AssertHasMessageErrorContaining(bill4export.ABL_CustomsValueInfo, "Customs Value must be lower or equal to 15000.00 EUR.");

				var bill5export = header.Bills.AddNew();
				bill5export.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				bill5export.ABL_CustomsValue = 15000;
				AssertNoError(bill5export.ABL_CustomsValueInfo, "Customs Value must be lower or equal to 15000.00 EUR.");

				var bill6export = header.Bills.AddNew();
				bill6export.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				bill6export.ABL_CustomsValue = 14999;
				AssertNoError(bill6export.ABL_CustomsValueInfo, "Customs Value must be lower or equal to 15000.00 EUR.");
			}
		}

		public void TestCheckNatureOfBusiness()
		{
			Bill.NatureOfBusiness = "XX";
			AssertHasMessageError(Bill.NatureOfBusinessInfo, ListValidation.InvalidCodeMessageError);

			Bill.NatureOfBusiness = "22";
			AssertNoMessageErrorContaining(Bill.NatureOfBusinessInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckExemptionCode1()
		{
			Bill.ExportCountry = "DE";
			Bill.ExemptionCode1 = ZString.Empty;
			AssertHasMessageErrorContaining(Bill.ExemptionCode1Info, MandatoryValidation.YouHaveNotEntered);

			Bill.ExemptionCode1 = "ABXYZ";
			AssertHasMessageError(Bill.ExemptionCode1Info, ListValidation.InvalidCodeMessageError);

			Manifest.AMA_DateAtCustomsOffice = ZDateTime.Now;
			Bill.ExemptionCode1 = "AFET";
			AssertNoMessageErrorContaining(Bill.ExemptionCode1Info, ListValidation.InvalidCodeMessageError);

			Bill.ExemptionCode2 = "AFET";
			Bill.ExemptionCode1 = "AFET";
			AssertHasMessageError(Bill.ExemptionCode1Info, "Exemption Code 1 and Exemption Code 2 cannot be same.");

			Bill.ExemptionCode1 = "ATOM";
			AssertNoMessageErrorContaining(Bill.ExemptionCode1Info, "Exemption Code 1 and Exemption Code 2 cannot be same.");

			Bill.ExportCountry = "ZA";
			Bill.ExemptionCode2 = ZString.Empty;
			Bill.ExemptionCode1 = "TOHUM";
			AssertNoMessageErrorContaining(Bill.ExemptionCode1Info, ListValidation.InvalidCodeMessageError);

			Bill.ExemptionCode2 = "TOHUM";
			Bill.ExemptionCode1 = "TOHUM";
			AssertHasMessageError(Bill.ExemptionCode1Info, "Exemption Code 1 and Exemption Code 2 cannot be same.");

			Bill.ExemptionCode1 = "ATOM";
			AssertNoMessageErrorContaining(Bill.ExemptionCode1Info, "Exemption Code 1 and Exemption Code 2 cannot be same.");
		}

		public void TestCheckExemptionCode2()
		{
			Bill.ExportCountry = "DE";
			Manifest.AMA_DateAtCustomsOffice = ZDateTime.Now;
			Bill.ExemptionCode1 = ZString.Empty;
			Bill.ExemptionCode2 = ZString.Empty;
			AssertNoMessageErrorContaining(Bill.ExemptionCode2Info, ListValidation.InvalidCodeMessageError);

			Bill.ExemptionCode2 = "ABXYZ";
			AssertHasMessageErrorContaining(Bill.ExemptionCode2Info, ListValidation.InvalidCodeMessageError);

			Bill.ExemptionCode1 = "ATOM";
			Bill.ExemptionCode2 = "ATOM";
			AssertHasMessageError(Bill.ExemptionCode2Info, "Exemption Code 1 and Exemption Code 2 cannot be same.");

			Bill.ExemptionCode2 = "TOHUM";
			AssertNoMessageErrorContaining(Bill.ExemptionCode2Info, "Exemption Code 1 and Exemption Code 2 cannot be same.");

			Bill.ExportCountry = "ZA";
			Bill.ExemptionCode1 = ZString.Empty;
			Bill.ExemptionCode2 = "ATOM";
			AssertHasMessageErrorContaining(Bill.ExemptionCode2Info, ListValidation.InvalidCodeMessageError);

			Bill.ExemptionCode2 = "AFET";
			AssertNoMessageErrorContaining(Bill.ExemptionCode2Info, ListValidation.InvalidCodeMessageError);

			Bill.ExemptionCode1 = "TOHUM";
			Bill.ExemptionCode2 = "TOHUM";
			AssertHasMessageError(Bill.ExemptionCode2Info, "Exemption Code 1 and Exemption Code 2 cannot be same.");

			Bill.ExemptionCode2 = "AFET";
			AssertNoMessageErrorContaining(Bill.ExemptionCode2Info, "Exemption Code 1 and Exemption Code 2 cannot be same.");
		}

		public void TestContainerNumber()
		{
			Bill.ContainerNumber = "XXX";
			AssertNoMessageErrorContaining(Bill.ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.ContainerNumber = "";
			AssertHasMessageErrorContaining(Bill.ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckSupplementaryDeclarationRegNoIdNo()
		{
			Bill.SupplementaryDeclarationRegNoIdNo = "20201224104";
			AssertNoMessageErrorContaining(Bill.SupplementaryDeclarationRegNoIdNoInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.SupplementaryDeclarationRegNoIdNo = "";
			AssertHasMessageErrorContaining(Bill.SupplementaryDeclarationRegNoIdNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckSupplementaryDeclarationDeliveryDate()
		{
			Bill.SupplementaryDeclarationDeliveryDate = ZDate.BrettsBirthday;
			AssertNoMessageErrorContaining(Bill.SupplementaryDeclarationDeliveryDateInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.SupplementaryDeclarationDeliveryDate = ZDate.Empty;
			AssertHasMessageErrorContaining(Bill.SupplementaryDeclarationDeliveryDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckABL_GoodsValue()
		{
			Bill.ABL_GoodsValue = 100m;
			AssertNoMessageErrorContaining(Bill.ABL_GoodsValueInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.ABL_GoodsValue = ZDecimal.Zero;
			AssertHasMessageErrorContaining(Bill.ABL_GoodsValueInfo, MandatoryValidation.YouHaveNotEntered);

			var pack1 = Bill.Packs.AddNew();
			var pack2 = Bill.Packs.AddNew();
			pack1.PackedItem.API_GoodsValue = 61m;
			pack2.PackedItem.API_GoodsValue = 39m;

			Bill.ABL_GoodsValue = 60m;
			AssertHasWarningContaining(Bill.ABL_GoodsValueInfo, "Out of balance, total goods value on packs is not equal goods value on bill.");

			Bill.ABL_GoodsValue = 100m;
			AssertNoWarnings(Bill.ABL_GoodsValueInfo);

			pack1.PackedItem.API_GoodsValue = 63m;
			AssertHasWarningContaining(Bill.ABL_GoodsValueInfo, "Out of balance, total goods value on packs is not equal goods value on bill.");

			pack2.PackedItem.API_GoodsValue = 37m;
			AssertNoWarnings(Bill.ABL_GoodsValueInfo);
		}

		public void TestCheckABL_RX_NKGoodsValueCurrency()
		{
			Bill.ABL_RX_NKGoodsValueCurrency = "USD";
			AssertNoMessageErrorContaining(Bill.ABL_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.ABL_RX_NKGoodsValueCurrency = ZString.Empty;
			AssertHasMessageErrorContaining(Bill.ABL_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckABL_SpecialCargoCode()
		{
			CombineAssertions("List Validation Test", () =>
			{
				Bill.ABL_SpecialCargoCode = ZString.Empty;
				AssertHasMessageErrorContaining(Bill.ABL_SpecialCargoCodeInfo, MandatoryValidation.YouHaveNotEntered);
				Bill.ABL_SpecialCargoCode = "ABC";
				AssertHasMessageErrorContaining(Bill.ABL_SpecialCargoCodeInfo, ListValidation.InvalidCodeMessageError);
				Bill.ABL_SpecialCargoCode = "123";
				AssertHasMessageErrorContaining(Bill.ABL_SpecialCargoCodeInfo, ListValidation.InvalidCodeMessageError);
				Bill.ABL_SpecialCargoCode = SpecialCargoCodes.Codes.ET;
				AssertNoMessageErrorContaining(Bill.ABL_SpecialCargoCodeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckABL_BillNumberIsUnique()
		{
			Bill.ABL_BillNumber = "X123456";
			AssertNoMessageErrorContaining(Bill.ABL_BillNumberInfo, "Bill number must be unique");
			var bill2 = Manifest.Bills.AddNew();
			bill2.ABL_BillNumber = "X123456";
			AssertHasMessageErrorContaining(bill2.ABL_BillNumberInfo, "Bill number must be unique");
			bill2.ABL_BillNumber = "Y123456";
			AssertNoMessageErrorContaining(bill2.ABL_BillNumberInfo, "Bill number must be unique");
		}

		public void TestCheckABL_OA_NotifyParty()
		{
			Bill.ABL_OA_NotifyParty = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertNoMessageErrorContaining(Bill.ABL_OA_NotifyPartyInfo, "Please fill in the Market Place name.");
			Bill.ABL_OA_NotifyParty = ZGuid.Empty;
			AssertHasMessageErrorContaining(Bill.ABL_OA_NotifyPartyInfo, "Please fill in the Market Place name.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "Nature Of Business");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "11", @"Eşyanın Mülkiyetinin Parasal veya Başka Bir Karşılıkla Devredildiği İşlemler - Doğrudan Satın Alma veya Satma", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "22", @"İade Edilen Eşyanın Değiştirilmesi", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "33", @"Eşyanın Mülkiyetinin Parasal veya Başka Bir Karşılık Alınmaksızın Devredildiği İşlemler-Diğer Yardım Programları (Özel hukuk tüzel kişileri, gerçek kişiler ve sivil toplum kuruluşları)", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "99", @"Diğer Ticari İşlemler", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			var trImport = helper.CreateRefCusProcedure("TR", "A", "40", "10", "", "Import", "IMP");
			var trExport = helper.CreateRefCusProcedure("TR", "H", "81", "00", "", "Export", "EXP");

			var refsysconfigtype = helper.CreateRefSysConfigType("TRETIGWMAX", "TR E-Trade Maximum Import Gross Weight Limit", "There is a maximum import weight limit for TR Customs E-Trade.");
			var refsysconfigtype2 = helper.CreateRefSysConfigType("TRETEGWMAX", "TR E-Trade Maximum Export Gross Weight Limit", "There is a maximum export weight limit for TR Customs E-Trade.");
			var refsysconfigtype3 = helper.CreateRefSysConfigType("TRETIGVMAX", "TR E-Trade Maximum Import Goods Value", "There is a maximum import Goods Value limit for TR Customs E-Trade. The limit currency code is EUR");
			var refsysconfigtype4 = helper.CreateRefSysConfigType("TRETEGVMAX", "TR E-Trade Maximum Export Goods Value", "There is a maximum Export Goods Value limit for TR Customs E-Trade. The limit currency code is EUR");

			var maxexportgross = helper.CreateRefSysConfig(refsysconfigtype.ZRT_ConfigCode, 30, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var maxexportcustoms = helper.CreateRefSysConfig(refsysconfigtype2.ZRT_ConfigCode, 300, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var maximportgross = helper.CreateRefSysConfig(refsysconfigtype3.ZRT_ConfigCode, 1500, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var maximportcustoms = helper.CreateRefSysConfig(refsysconfigtype4.ZRT_ConfigCode, 15000, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, "ETR");
			Factory.Save();
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "AFET", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), @"27.3.1992-92/2879 BKK Afetler için tanınan muafiyet");
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "ATOM", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), @"285 Sayılı OHAL ilişkin KHK.");
			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "TOHUM", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), @"2008/13262sayı.İthalat Rejimi Kararı");

			var tradegroupAll = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "All Countries", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var tradegroupEu = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "EU", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var tradegroupNonEu = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "Non-EU", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();

			var tradeGroupCountryDEAll = helper.AddCountry(tradegroupAll, Core.Constants.CountryCodes.Germany);
			var tradeGroupCountryDEEu = helper.AddCountry(tradegroupEu, Core.Constants.CountryCodes.Germany);
			var tradeGroupCountryZAAll = helper.AddCountry(tradegroupNonEu, Core.Constants.CountryCodes.SouthAfrica);
			var tradeGroupCountryZANeu = helper.AddCountry(tradegroupAll, Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Turkey, "ETR");
			Factory.Save();
			var ratecode1 = helper.LoadOrCreateNewCusRateCode(Factory, "10", rateType.PK);
			Factory.Save();
			var ratedeall = helper.CreateRate(tariff1, ratecode1.PK, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var appAll = helper.CreateCusApplicability(ratedeall, tradegroupAll, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			var ratezaall = helper.CreateRate(tariff1, ratecode1.PK, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusApplicability(ratezaall, tradegroupAll, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			var ratedeeu = helper.CreateRate(tariff2, ratecode1.PK, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusApplicability(ratedeeu, tradegroupEu, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			var ratezanoneu = helper.CreateRate(tariff3, ratecode1.PK, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusApplicability(ratezanoneu, tradegroupNonEu, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			Manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			Manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
			Manifest.AMA_ManifestType = ShipmentTypeList.Codes.Import23;
			Bill = Manifest.Bills.AddNew();
			Bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

			Factory.Save();
		}

		public AsycudaManifestHeader Manifest { get; set; }
		public AsycudaBill Bill { get; set; }
	}
}
