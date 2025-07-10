using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	public partial class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestPacks()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertType<AsycudaPackCollection>(bill.Packs);
		}

		public void TestABL_RX_NKGoodsValueCurrency()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			bill.ABL_RX_NKGoodsValueCurrency = "TRY";
			AssertEquals("TRY", pack.PackedItem.API_RX_NKGoodsValueCurrency);

			bill.ABL_RX_NKGoodsValueCurrency = "EUR";
			AssertEquals("EUR", pack.PackedItem.API_RX_NKGoodsValueCurrency);
		}

		public void TestDoNotCreateBOInGetter()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			_ = bill.GuaranteeType;
			AssertEquals(0, bill.Guarantees.Count);
			bill.GuaranteeType = "TEST";
			AssertEquals(1, bill.Guarantees.Count);

			bill.Guarantees.RemoveAndDeleteAll();
			_ = bill.GuaranteeRefNo;
			AssertEquals(0, bill.Guarantees.Count);
			bill.GuaranteeRefNo = "1";
			AssertEquals(1, bill.Guarantees.Count);

			bill.Guarantees.RemoveAndDeleteAll();
			_ = bill.GuaranteeAmount;
			AssertEquals(0, bill.Guarantees.Count);
			bill.GuaranteeAmount = 10m;
			AssertEquals(1, bill.Guarantees.Count);

			_ = bill.SupplementaryDeclarationName;
			AssertEquals(0, bill.ETradeBillDatas.Count);
			bill.SupplementaryDeclarationName = "NAME";
			AssertEquals(1, bill.ETradeBillDatas.Cast<ETradeData>().Count(x => x.CY_Type == "DPD"));

			bill.ETradeBillDatas.RemoveAndDeleteAll();
			_ = bill.SupplementaryDeclarationRegNoIdNo;
			AssertEquals(0, bill.ETradeBillDatas.Count);
			bill.SupplementaryDeclarationRegNoIdNo = "1";
			AssertEquals(1, bill.ETradeBillDatas.Cast<ETradeData>().Count(x => x.CY_Type == "DPD"));

			bill.ETradeBillDatas.RemoveAndDeleteAll();
			_ = bill.SupplementaryDeclarationDeliveryDate;
			AssertEquals(0, bill.ETradeBillDatas.Count);
			bill.SupplementaryDeclarationDeliveryDate = new ZDateTime(2021, 2, 23, 9, 30, 15);
			AssertEquals(1, bill.ETradeBillDatas.Cast<ETradeData>().Count(x => x.CY_Type == "DPD"));
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var bill = header.Bills.AddNew();
			AssertEquals(Core.Constants.CurrencyCodes.Turkey, bill.ABL_RX_NKOtherValueCurrency);
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, bill.ABL_RX_NKCustomsValueCurrency);
			AssertEquals(SpecialCargoCodes.Codes.ET, bill.ABL_SpecialCargoCode);
		}

		public void TestDecimalPlaces()
		{
			var bizObj = GetNewBusinessObject();
			AssertHasCustomAttribute<DecimalPlacesAttribute>(bizObj.GetType(), "ABL_GoodsValue", false, attr => attr.DecimalPlaces == 2);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(bizObj.GetType(), "ABL_InsuranceValue", false, attr => attr.DecimalPlaces == 2);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(bizObj.GetType(), "ABL_TransportValue", false, attr => attr.DecimalPlaces == 2);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(bizObj.GetType(), "ABL_CustomsValue", false, attr => attr.DecimalPlaces == 2);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(bizObj.GetType(), "ABL_OtherValue", false, attr => attr.DecimalPlaces == 2);
		}

		public void TestGetEffectiveValues()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertGetEffectiveValueWithCountryCode(() => bill.DepartureCountry,
				(ZString countryCode) => bill.DepartureCountry = countryCode,
				(ZString countryCode) => header.DepartureCountryCode = countryCode);

			AssertGetEffectiveValueWithCountryCode(() => bill.TradeCountry,
				(ZString countryCode) => bill.TradeCountry = countryCode,
				(ZString countryCode) => header.DepartureCountryCode = countryCode);

			AssertGetEffectiveValueWithCountryCode(() => bill.ExportCountry,
				(ZString countryCode) => bill.ExportCountry = countryCode,
				(ZString countryCode) => header.DepartureCountryCode = countryCode);

			AssertGetEffectiveValueWithCountryCode(() => bill.ArrivalCountry,
				(ZString countryCode) => bill.ArrivalCountry = countryCode,
				(ZString countryCode) => header.AMA_RL_NKPortOfFirstArrival = countryCode);

			void AssertGetEffectiveValueWithCountryCode(Func<ZString> getValue, Action<ZString> setValue, Action<ZString> setFallbackValue)
			{
				setFallbackValue(ZString.Empty);
				setValue(ZString.Empty);
				AssertEquals(ZString.Empty, getValue());

				setFallbackValue(Constants.CountryCodes.Germany);
				setValue(Constants.CountryCodes.France);
				AssertEquals(Constants.CountryCodes.France, getValue());

				setValue(ZString.Empty);
				AssertEquals(Constants.CountryCodes.Germany, getValue());
			}
		}

		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.TRETrade.IAsycudaBill>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaBill>(bizObj.PK).GetType());
		}

		public void TestGuarantee()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.GuaranteeAmount = 1234.56m;
			bill.GuaranteeRefNo = "testRefNo";
			bill.GuaranteeType = "testType";
			AssertEquals("CusBondDetails should contain 1 records", 1, bill.Guarantees.Count);
			AssertEquals(1234.56m, bill.GuaranteeAmount);
			AssertEquals("testRefNo", bill.GuaranteeRefNo);
			AssertEquals("testType", bill.GuaranteeType);

			bill.GuaranteeAmount = ZDecimal.Zero;
			bill.GuaranteeRefNo = ZString.Empty;
			bill.GuaranteeType = ZString.Empty;
			Factory.Save();
			var guarantees = bill.Guarantees.Cast<CusBondDetail>().FirstOrDefault(e => e.PW_ParentID == bill.PK && !e.IsDeleted);
			AssertEquals("CusBondDetails should not contain records", 0, bill.Guarantees.Count);
			AssertNull("CusBondDetails should not exist", guarantees);
		}

		public void TestCusBondDetailCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cusBondInfo = bill.Guarantees.AddNew();
			cusBondInfo.PW_BondNumber = "testRefNo";

			Factory.Save();

			AssertEquals("CusBondDetails should contain 1 records", 1, bill.Guarantees.Count);
			AssertNotNull("CusBondInfo should exist", new BusinessObjectFactory().Load<CusBondDetail>(cusBondInfo.PK));

			bill.Guarantees.RemoveAndDeleteAll();
			Factory.Save();

			AssertNull("CusBondDetails should be deleted", new BusinessObjectFactory().Load<CusBondDetail>(cusBondInfo.PK));
		}

		public void TestCustomsValueWithEUR()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;

				CurrencyTestHelper helper = new CurrencyTestHelper(Factory);
				helper.SetExchangeRate(helper.EURCurrency, 7.7448m, ZDateTime.Today);
				helper.SetExchangeRate(helper.USDCurrency, 6.7344m, ZDateTime.Today);
				helper.SetExchangeRate(helper.TRYCurrency, 1m, ZDateTime.Today);

				header.Branch.Company.GC_IsReciprocal = true;
				Factory.Save();
				var bill = header.Bills.AddNew();
				bill.ABL_GoodsValue = 100;
				bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

				var bill2 = header.Bills.AddNew();
				bill2.ABL_GoodsValue = 200;
				bill2.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.Turkey;

				var bill3 = header.Bills.AddNew();
				bill3.ABL_GoodsValue = 300;
				bill3.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;

				CombineAssertions("CustomsValueWithEUR", () =>
				{
					AssertEquals("Pre condition", (ZDecimal)7.7448, helper.EURCurrency.CurrentCustomsRate);
					AssertEquals("Pre condition", (ZDecimal)6.7344, helper.USDCurrency.CurrentCustomsRate);
					AssertEquals("Pre condition", (ZDecimal)1, helper.TRYCurrency.CurrentCustomsRate);

					AssertEquals("Pre condition", Core.Constants.CurrencyCodes.EuropeanUnion, bill.ABL_RX_NKCustomsValueCurrency);
					AssertEquals("EUR", (ZDecimal)100.00, bill.ABL_CustomsValue);

					AssertEquals("Pre condition", Core.Constants.CurrencyCodes.EuropeanUnion, bill2.ABL_RX_NKCustomsValueCurrency);
					AssertEquals("TRY", (ZDecimal)25.82, bill2.ABL_CustomsValue);

					AssertEquals("Pre condition", Core.Constants.CurrencyCodes.EuropeanUnion, bill3.ABL_RX_NKCustomsValueCurrency);
					AssertEquals("USD", (ZDecimal)260.86, bill3.ABL_CustomsValue);
				});
			}
		}

		public void TestBillStatus()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "RED";
			AssertEquals(bill.ABL_BillStatus, "RED");
		}

		public void TestCargoStatus()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_CargoStatus = "NAP";
			AssertEquals(bill.ABL_CargoStatus, "NAP");
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			Integration.Customs.ICusCodeDataTypeSupporter supporter = bill;
			supporter.AssertType(typeof(ETradeData), AsycudaBill.BillCYType);
			var etradeDataInfo = bill.ETradeBillDatas.AddNew();
			etradeDataInfo.CY_Data = "testCYData";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(etradeDataInfo.PK);
			AssertEquals(typeof(ETradeData), codeData.GetType());
		}

		public void TestETradeBillDataCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var etradeDataInfo = bill.ETradeBillDatas.AddNew();
			etradeDataInfo.CY_Data = "testCYData";

			Factory.Save();

			AssertEquals("ETradeDatas should contain 1 records", 1, bill.ETradeBillDatas.Count);
			AssertNotNull("etradeDataInfo should exist", new BusinessObjectFactory().Load<ETradeData>(etradeDataInfo.PK));

			bill.ETradeBillDatas.RemoveAndDeleteAll();
			Factory.Save();

			AssertNull("ETradeDatas should be deleted", new BusinessObjectFactory().Load<ETradeData>(etradeDataInfo.PK));
		}

		public void TestPropertiesForTaxAndDuty()
		{
			PrepareReferenceTestData();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;
				header.Branch.Company.GC_IsReciprocal = true;
				var bill = header.Bills.AddNew();
				bill.ExemptionCode1 = "HK18";
				bill.ExemptionCode2 = "DOC";
				bill.ExportCountry = Core.Constants.CountryCodes.Germany;
				bill.ABL_CustomsValue = 500;
				bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				var pack1 = bill.Packs.AddNew();
				var pack2 = bill.Packs.AddNew();
				pack1.PackedItem.API_GoodsValue = (ZDecimal)300;
				pack1.PackedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				pack1.PackedItem.API_Tariff = "1000";
				pack2.PackedItem.API_GoodsValue = (ZDecimal)300;
				pack2.PackedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				pack2.PackedItem.API_Tariff = "2000";
				Factory.Save();

				header.CalculateDuties();
				var tax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);
				var taxCount = bill.AsycudaTaxes.Cast<AsycudaTax>().Count(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);

				CombineAssertions(() =>
				{
					AssertEquals(taxCount, 1);
					AssertEquals(tax.AET_ChargeAmount, 1425m);
					AssertEquals(tax.AET_ChargeType, TaxCodeList.Codes.CustomsDuty);
					AssertEquals(tax.AET_Rate, 38m);
					AssertEquals(tax.AET_MethodOfPayment, MethodOfPaymentList.Codes.Cash);
					AssertEquals(tax.AET_BaseValue, 3750m);
				});
			}
		}

		public void TestPropertiesForContainer()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ContainerNumber = "XXX";
			CombineAssertions(() =>
			{
				AssertEquals(bill.Container.ACN_ContainerNumber, "XXX");
				AssertEquals(bill.Pivot.APC_ACN_Container, bill.ContainerPK);
				AssertEquals(bill.Pivot.APC_ABL_Bill, bill.PK);
			});
			var bill2 = header.Bills.AddNew();
			Factory.Save();
			AssertEquals(bill2.ContainerNumber, "XXX");
			CombineAssertions(() =>
			{
				AssertEquals(bill2.Container.ACN_ContainerNumber, "XXX");
				AssertEquals(bill2.Pivot.APC_ACN_Container, bill2.ContainerPK);
				AssertEquals(bill2.Pivot.APC_ABL_Bill, bill2.PK);
			});
			bill.ContainerNumber = "SXX";
			CombineAssertions(() =>
			{
				AssertEquals(bill.Container.ACN_ContainerNumber, "SXX");
				AssertEquals(bill2.ContainerNumber, "XXX");
				AssertEquals(bill2.Container.ACN_ContainerNumber, "XXX");
			});
			bill.ContainerNumber = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertNull(bill.Container);
				AssertNull(bill.Pivot);
				AssertEquals(bill2.ContainerNumber, "XXX");
				AssertEquals(bill2.Pivot.APC_ACN_Container, bill2.ContainerPK);
				AssertEquals(bill2.Pivot.APC_ABL_Bill, bill2.PK);
			});
		}

		public void TestDelete()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ContainerNumber = "XXX";
			AssertEquals(1, header.Containers.Count);

			bill.Delete();
			AssertEquals(0, header.Containers.Count);
		}

		public void TestGoodsValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;

				CurrencyTestHelper helper = new CurrencyTestHelper(Factory);
				helper.SetExchangeRate(helper.EURCurrency, 7.7448m, ZDateTime.Today);
				helper.SetExchangeRate(helper.USDCurrency, 6.7344m, ZDateTime.Today);
				helper.SetExchangeRate(helper.TRYCurrency, 1m, ZDateTime.Today);

				header.Branch.Company.GC_IsReciprocal = true;
				Factory.Save();
				var bill = header.Bills.AddNew();
				bill.ABL_GoodsValue = 100;
				bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertEquals("EUR", (ZDecimal)100.00, bill.ABL_CustomsValue);
				AssertEquals("EUR", (ZDecimal)100.00, bill.ABL_GoodsValue);
				AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, bill.ABL_RX_NKGoodsValueCurrency);

				bill.ABL_GoodsValue = 200;
				bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.Turkey;
				AssertEquals("TRY", (ZDecimal)25.82, bill.ABL_CustomsValue);
				AssertEquals("TRY", (ZDecimal)200.00, bill.ABL_GoodsValue);
				AssertEquals(Core.Constants.CurrencyCodes.Turkey, bill.ABL_RX_NKGoodsValueCurrency);

				bill.ABL_GoodsValue = 100;
				bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertEquals("USD", (ZDecimal)86.95, bill.ABL_CustomsValue);
				AssertEquals("USD", (ZDecimal)100.00, bill.ABL_GoodsValue);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, bill.ABL_RX_NKGoodsValueCurrency);

				CombineAssertions("ExchangeRates", () =>
				{
					AssertEquals("Pre condition", (ZDecimal)7.7448, helper.EURCurrency.CurrentCustomsRate);
					AssertEquals("Pre condition", (ZDecimal)6.7344, helper.USDCurrency.CurrentCustomsRate);
					AssertEquals("Pre condition", (ZDecimal)1, helper.TRYCurrency.CurrentCustomsRate);
				});
			}
		}

		public void TestCalculateBanderolDuty()
		{
			var helper = new CurrencyTestHelper(Factory);
			helper.SetExchangeRate(helper.EURCurrency, 9m, ZDateTime.Today);
			helper.SetExchangeRate(helper.TRYCurrency, 1m, ZDateTime.Today);

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDateTime.Today;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.Branch.Company.GC_IsReciprocal = true;
			Factory.Save();
			var bill = header.Bills.AddNew();
			bill.ABL_GoodsValue = 16;
			bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			bill.ABL_ManifestQty = 2;
			bill.CalculateBanderolDuty();
			var banderolTax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.TRTBandrol);
			if (banderolTax != null)
			{
				CombineAssertions(() =>
				{
					AssertEquals("Arrival Date", ZDateTime.Today, header.AMA_DateAtCustomsOffice);
					AssertEquals("Banderol Type", TaxCodeList.Codes.TRTBandrol, banderolTax.AET_ChargeType);
					AssertEquals("Charge Amount", 144.00m, banderolTax.AET_ChargeAmount);
					AssertEquals("Rate", 2.00m, banderolTax.AET_Rate);
					AssertEquals("Base Value", 288.00m, banderolTax.AET_BaseValue);
				});
			}

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			bill.CalculateBanderolDuty();
			banderolTax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.TRTBandrol);
			AssertEquals(null, banderolTax);
		}

		public void TestExistenceofABL_SpecialCargoCode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_SpecialCargoCode = SpecialCargoCodes.Codes.ET;
			AssertEquals("Just to see if the field is exist", bill.ABL_SpecialCargoCode, SpecialCargoCodes.Codes.ET);
		}

		public void TestCaptionandShortCaptionforABL_SpecialCargoCode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertCaption(bill.ABL_SpecialCargoCodeInfo, "Trade Type", "Trade Type");
		}

		void AssertCaption(ZPropertyInfo info, string shortCaption, string caption)
		{
			var propertyData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals($"{info.Name} Caption", caption, propertyData.Caption);
			AssertEquals($"{info.Name} ShortCaption", shortCaption, propertyData.ShortCaption);
		}

		public void TestABL_BillNumberReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("BillNumber Number", false, bill.ABL_BillNumberInfo.ReadOnly);

			bill.ABL_MessageStatus = "AWA";
			AssertEquals("BillNumber Number", true, bill.ABL_BillNumberInfo.ReadOnly);

			bill.ABL_MessageStatus = "ACC";
			AssertEquals("BillNumber Number", false, bill.ABL_BillNumberInfo.ReadOnly);

			bill.ABL_MessageStatus = "ERR";
			AssertEquals("BillNumber Number", false, bill.ABL_BillNumberInfo.ReadOnly);
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { AsycudaBill.Schema.ABL_SpecialCargoCode };
		}

		public void TestCalculateDutyWithAdditionalCodes()
		{
			PrepareReferenceTestData();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;
				header.Branch.Company.GC_IsReciprocal = true;
				var bill = header.Bills.AddNew();
				bill.ExportCountry = Core.Constants.CountryCodes.Germany;
				bill.ABL_CustomsValue = 500;
				bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				var pack1 = bill.Packs.AddNew();
				var pack2 = bill.Packs.AddNew();
				pack1.PackedItem.API_GoodsValue = (ZDecimal)300;
				pack1.PackedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				pack1.PackedItem.API_Tariff = "5000";
				pack1.PackedItem.API_ChemicalSubstanceCode = "AC1";

				Factory.Save();

				header.CalculateDuties();
				var tax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);

				CombineAssertions(() =>
				{
					AssertEquals(tax.AET_Rate, 30m);
					AssertEquals(tax.AET_ChargeAmount, 1125m);
					AssertEquals(tax.AET_ChargeType, TaxCodeList.Codes.CustomsDuty);
					AssertEquals(tax.AET_MethodOfPayment, MethodOfPaymentList.Codes.Cash);
					AssertEquals(tax.AET_BaseValue, 3750m);
				});

				pack1.PackedItem.API_ChemicalSubstanceCode = "AC2";
				header.CalculateDuties();
				tax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);

				CombineAssertions(() =>
				{
					AssertEquals(tax.AET_Rate, 50m);
					AssertEquals(tax.AET_ChargeAmount, 1875m);
					AssertEquals(tax.AET_ChargeType, TaxCodeList.Codes.CustomsDuty);
					AssertEquals(tax.AET_MethodOfPayment, MethodOfPaymentList.Codes.Cash);
					AssertEquals(tax.AET_BaseValue, 3750m);
				});
			}
		}

		public void TestCalculateDutyWithAdditionalCodesWithSCDFix()
		{
			PrepareTestDataForRefSys();
			PrepareReferenceTestData();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;
				header.Branch.Company.GC_IsReciprocal = true;
				var bill = header.Bills.AddNew();
				bill.ExportCountry = Core.Constants.CountryCodes.Germany;
				bill.ABL_CustomsValue = 500;
				bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				var pack1 = bill.Packs.AddNew();
				pack1.PackedItem.API_GoodsValue = (ZDecimal)300;
				pack1.PackedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				pack1.PackedItem.API_Tariff = "5000";
				pack1.PackedItem.API_ChemicalSubstanceCode = "AC1";

				Factory.Save();

				header.CalculateDuties();
				var tax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);

				CombineAssertions(() =>
				{
					AssertEquals(20m, tax.AET_Rate);
					AssertEquals(750m, tax.AET_ChargeAmount);
					AssertEquals(TaxCodeList.Codes.CustomsDuty, tax.AET_ChargeType);
					AssertEquals(MethodOfPaymentList.Codes.Cash , tax.AET_MethodOfPayment);
					AssertEquals(3750m, tax.AET_BaseValue);
				});
			}
		}

		public void TestExemptionCode1Attribute()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<MaxLengthAttribute>(bill.GetType(), "ExemptionCode1", true, attr => attr.MaxLength == 10);
				AssertHasCustomAttribute<ResourceStringDataAttribute>(bill.GetType(), "ExemptionCode1", true, attr => attr.Caption == "Exemption Code 1");
				AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaBill), nameof(bill.ExemptionCode1), true, attribute => attribute.ListDataSourceMember == (nameof(bill.Lookups) + "." + nameof(AsycudaBillLookups.ExemptionCodeList)));
			});
		}

		public void TestExemptionCode2Attribute()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<MaxLengthAttribute>(bill.GetType(), "ExemptionCode2", true, attr => attr.MaxLength == 10);
				AssertHasCustomAttribute<ResourceStringDataAttribute>(bill.GetType(), "ExemptionCode2", true, attr => attr.Caption == "Exemption Code 2");
				AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaBill), nameof(bill.ExemptionCode2), true, attribute => attribute.ListDataSourceMember == (nameof(bill.Lookups) + "." + nameof(AsycudaBillLookups.ExemptionCodeList)));
			});
		}

		public void TestSCDRateFixForETrade()
		{
			PrepareTestDataForRefSys();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals("VFD * 0.20", bill.SCDRateFixForETrade);
		}
		
		public void TestCaptionSeparated()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(bill.GetType(), "Separated", true, attr => attr.Caption == "Separated");
		}

		public void TestABL_OA_NotifyParty_Caption()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertHasCustomAttribute<ResourceStringDataAttribute>(bill.GetType(), "ABL_OA_NotifyParty", true, attr => attr.Caption == "Market Place");
		}

		public void TestBusinessObjectWithRelatedEventsOnBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var tax1 = bill1.AsycudaTaxes.AddNew();
			var tax2 = bill1.AsycudaTaxes.AddNew();

			var bill2 = header.Bills.AddNew();
			var tax3 = bill2.AsycudaTaxes.AddNew();

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newBill1 = newFactory.Load<AsycudaBill>(bill1.PK);

			Assert("Tax1 should be in the list of related objects", newBill1.BusinessObjectsWithRelatedEvents.Any(x => x.PK == tax1.PK));
			Assert("Tax2 should be in the list of related objects", newBill1.BusinessObjectsWithRelatedEvents.Any(x => x.PK == tax2.PK));
			Assert("Tax3 should not be in the list of related objects", !newBill1.BusinessObjectsWithRelatedEvents.Any(x => x.PK == tax3.PK));
		}

		public void TestCalculateCustomsValueFromGoodsValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_MessageType = JobMessageTypeList.Codes.Export;
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;

				CurrencyTestHelper helper = new CurrencyTestHelper(Factory);
				helper.SetExchangeRate(helper.EURCurrency, 7.7448m, ZDateTime.Today);
				helper.SetExchangeRate(helper.USDCurrency, 6.7344m, ZDateTime.Today);
				helper.SetExchangeRate(helper.TRYCurrency, 1m, ZDateTime.Today);
				PrepareTestDataForRefSys();

				header.Branch.Company.GC_IsReciprocal = true;
				Factory.Save();
				var bill = header.Bills.AddNew();
				bill.ABL_Incoterm = "CIF";
				bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

				CombineAssertions("Calc Customs Value", () =>
				{
					bill.ABL_TransportValue = ZDecimal.Zero;
					bill.ABL_RX_NKTransportValueCurrency = ZString.Empty;
					bill.ABL_GoodsValue = 40;
					AssertEquals("CIF | 40 + 3", 43m, bill.ABL_CustomsValue);

					bill.ABL_TransportValue = 5;
					bill.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
					AssertEquals("CIF | 40 + 0", 40m, bill.ABL_CustomsValue);

					bill.ABL_Incoterm = "FOB";
					bill.ABL_TransportValue = ZDecimal.Zero;
					bill.ABL_RX_NKTransportValueCurrency = ZString.Empty;
					AssertEquals("FOB | 40 + 3", 43m, bill.ABL_CustomsValue);

					bill.ABL_TransportValue = 5m;
					bill.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
					AssertEquals("FOB | 40 + 5", 45m, bill.ABL_CustomsValue);

					bill.ABL_Incoterm = "XXX";
					bill.ABL_GoodsValue = 40;
					bill.ABL_TransportValue = ZDecimal.Zero;
					bill.ABL_RX_NKTransportValueCurrency = ZString.Empty;
					AssertEquals("XXX | Not Calc", 40m, bill.ABL_CustomsValue);
				});
			}
		}

		public void TestPrecedentFreightToDisplay()
		{
			PrepareTestDataForRefSys();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals("PrecedentFreightCost", 3m, bill.PrecedentFreightCost);

			bill.ABL_Incoterm = "CIF";
			bill.ABL_TransportValue = ZDecimal.Zero;
			bill.ABL_RX_NKTransportValueCurrency = ZString.Empty;
			AssertEquals(3m, bill.PrecedentFreightToDisplay);

			bill.ABL_Incoterm = "FOB";
			bill.ABL_TransportValue = ZDecimal.Zero;
			bill.ABL_RX_NKTransportValueCurrency = ZString.Empty;
			AssertEquals(3m, bill.PrecedentFreightToDisplay);

			bill.ABL_Incoterm = "AAA";
			bill.ABL_TransportValue = ZDecimal.Zero;
			bill.ABL_RX_NKTransportValueCurrency = ZString.Empty;
			AssertEquals(0m, bill.PrecedentFreightToDisplay);

			bill.ABL_Incoterm = "CIF";
			bill.ABL_TransportValue = 22m;
			bill.ABL_RX_NKTransportValueCurrency = ZString.Empty;
			AssertEquals(0m, bill.PrecedentFreightToDisplay);
		}

		public void TestPrecedentFreightCost()
		{
			PrepareTestDataForRefSys();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals("PrecedentFreightCost", 3m, bill.PrecedentFreightCost);
		}

		void PrepareTestDataForRefSys()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetExistingRefSysConfigType("TRSCDRATE", "TRSCDRATE DESCRIPTION", "TRSCDRATE LONG DESCRIPTION");
			helper.CreateOrUpdateExistingRefSysConfig("TRSCDRATE", "VFD * 0.20", new ZDateTime(2023, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateOrGetExistingRefSysConfigType("TREPREFRCO", "TR E-Trade Precedent Freight Cost Value", "There is a Precedent Freight Cost Value for TR Customs E-Trade. The limit currency code is EUR");
			helper.CreateOrUpdateExistingRefSysConfig("TREPREFRCO", 3, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();
		}

		void PrepareReferenceTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var taxorfee = helper.CreateTaxOrFee("89", 119, Core.Constants.CountryCodes.Turkey, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "E-Trade Stamp Tax");
			taxorfee.ZZF_Value = 119;
			taxorfee.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			var tariffTypeHSN = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, "HSN");
			var tariffTypeETR = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, "ETR");
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Turkey, "SCD");
			var rateType2 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Turkey, "ETR");
			var rateCode10 = helper.CreateCusRateCode(Factory, "10", rateType.PK);
			var rateCode = helper.CreateCusRateCode(Factory, "10", rateType2.PK);
			var tradeGroupCountry2 = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "EU", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var tradeGroupCountry3 = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "All Countries", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

			Factory.Save();

			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypeHSN.PK, "1000", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypeHSN.PK, "2000", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var tariff5 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypeHSN.PK, "5000", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var rateForTariff1 = helper.CreateRefCusRate(tariff1.PK, rateCode10.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "VFD * 0.20", null, "20%", Core.Constants.CountryCodes.Turkey);
			var rateFortariff4 = helper.CreateRefCusRate(tariff4.PK, rateCode10.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "VFD * 0", null, "0%", Core.Constants.CountryCodes.Turkey);
			var rate5 = helper.CreateRefCusRate(tariff5.PK, rateCode10.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "VFD * 0.30", null, "30%", Core.Constants.CountryCodes.Turkey);
			var rate6 = helper.CreateRefCusRate(tariff5.PK, rateCode10.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "VFD * 0.50", null, "50%", Core.Constants.CountryCodes.Turkey);

			CurrencyTestHelper curHelper = new CurrencyTestHelper(Factory);
			curHelper.SetExchangeRate(curHelper.EURCurrency, 7.5m, ZDateTime.Today);
			curHelper.SetExchangeRate(curHelper.USDCurrency, 5m, ZDateTime.Today);
			curHelper.SetExchangeRate(curHelper.TRYCurrency, 1m, ZDateTime.Today);

			helper.AddCountry(tradeGroupCountry2, Core.Constants.CountryCodes.Germany);
			helper.AddCountry(tradeGroupCountry3, Core.Constants.CountryCodes.Germany);

			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypeETR.PK, "HK18", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypeETR.PK, "DOC", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var rate2 = helper.CreateRefCusRate(tariff2.PK, rateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "VFD * 0.18", null, "18");
			rate2.ZZ2_RateFormulaDerivedFrom = "18";
			var rate3 = helper.CreateRefCusRate(tariff3.PK, rateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			rate3.ZZ2_RateFormulaDerivedFrom = "0";

			helper.CreateCusApplicability(rate2.PK, tradeGroupCountry2, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusApplicability(rate3.PK, tradeGroupCountry3, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusApplicability(rate5, tradeGroupCountry3, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), "AC1");
			helper.CreateCusApplicability(rate6, tradeGroupCountry3, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), "AC2");

			helper.CreateCusApplicability(rateForTariff1.PK, tradeGroupCountry3, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusApplicability(rateFortariff4.PK, tradeGroupCountry3, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			var bill = header.Bills.AddNew();
			return bill;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}
	}
}
