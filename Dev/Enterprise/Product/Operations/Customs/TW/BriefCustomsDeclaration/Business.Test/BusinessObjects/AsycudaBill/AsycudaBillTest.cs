using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestABL_ManifestQty()
		{
			var targetInfo = bill.ABL_ManifestQtyInfo;
			AssertEquals("MaxLength", 8, targetInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
			AssertSetToZeroIfNegativeForInt(targetInfo);
		}

		void AssertSetToZeroIfNegativeForInt(ZPropertyInfo targetInfo)
		{
			targetInfo.Value = new ZInt(-1);
			AssertEquals("Should be 0 if is negative", ZInt.Zero, targetInfo.Value);
		}

		void AssertSetToZeroIfNegativeForDecimal(ZPropertyInfo targetInfo)
		{
			targetInfo.Value = new ZDecimal(-1);
			AssertEquals("Should be 0m if is negative", ZDecimal.Zero, targetInfo.Value);
		}

		public void TestIsImport()
		{
			var billForTesting = bill;
			var header = billForTesting.Header;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			Assert("Should be false", !billForTesting.IsImport);

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			Assert("Should be true", billForTesting.IsImport);
		}

		public void TestIsExport()
		{
			var billForTesting = bill;
			var header = billForTesting.Header;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			Assert("Should be true", billForTesting.IsExport);

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			Assert("Should be false", !billForTesting.IsExport);
		}

		public void TestABL_GrossWeight()
		{
			var targetInfo = bill.ABL_GrossWeightInfo;
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(targetInfo, "Weight", "Gross Weight");
			AssertSetToZeroIfNegativeForDecimal(targetInfo);
		}

		public void TestABL_OtherDeductions()
		{
			var targetInfo = bill.ABL_OtherDeductionsInfo;
			BusinessObjectCaptionTestHelper.AssertCaptions(targetInfo, "Deductions");
			AssertEquals(2, targetInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		}

		public void TestABL_RX_NKOtherDeductionsCurrency()
		{
			var targetInfo = bill.ABL_RX_NKOtherDeductionsCurrencyInfo;
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(targetInfo, "Deductions Currency", "Curr.");
			AssertEquals(true, targetInfo.ReadOnly);
		}

		public void TestABL_FreightValue()
		{
			var targetInfo = bill.ABL_FreightValueInfo;
			BusinessObjectCaptionTestHelper.AssertCaptions(targetInfo, "Freight");
			AssertEquals(2, targetInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		}

		public void TestABL_RX_NKFreightValueCurrency()
		{
			var targetInfo = bill.ABL_RX_NKFreightValueCurrencyInfo;
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(targetInfo, "Freight Currency", "Curr.");
			AssertEquals(true, targetInfo.ReadOnly);
		}

		public void TestABL_InsuranceValue()
		{
			AssertEquals(2, bill.ABL_InsuranceValueInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		}

		public void TestABL_RX_NKInsuranceValueCurrency()
		{
			AssertEquals(true, bill.ABL_RX_NKInsuranceValueCurrencyInfo.ReadOnly);
		}

		public void TestABL_OtherValue()
		{
			var targetInfo = bill.ABL_OtherValueInfo;
			BusinessObjectCaptionTestHelper.AssertCaptions(targetInfo, "Additions");
			AssertEquals(2, targetInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		}

		public void TestABL_RX_NKOtherValueCurrency()
		{
			var targetInfo = bill.ABL_RX_NKOtherValueCurrencyInfo;
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(targetInfo, "Additions Currency", "Curr.");
			AssertEquals(true, targetInfo.ReadOnly);
		}

		public void TestABL_CustomsValue()
		{
			var targetInfo = bill.ABL_CustomsValueInfo;
			AssertEquals("DecimalPlaces", 2, targetInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
			BusinessObjectCaptionTestHelper.AssertCaptions(targetInfo, "Customs Value");
			AssertSetToZeroIfNegativeForDecimal(targetInfo);
			AssertEquals(true, targetInfo.ReadOnly);
		}

		public void TestABL_CustomsValue_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(bill.ABL_CustomsValueInfo, AsycudaBill.ABL_CustomsValueIMPCaptionKey);
			AssertEquals("Caption (Import)", "Customs Value", captionResourceString.Caption);

			captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(bill.ABL_CustomsValueInfo, AsycudaBill.ABL_CustomsValueEXPCaptionKey);
			AssertEquals("Caption (Export)", "FOB", captionResourceString.Caption);
		}

		public void TestMultipleKeysToUse()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			var bill = header.Bills.AddNew();
			AssertEquals("IMP", AsycudaBill.ABL_CustomsValueIMPCaptionKey, ((ISupportMultipleResourceStringData)bill).MultipleKeysToUse.Single());

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			AssertEquals("EXP", AsycudaBill.ABL_CustomsValueEXPCaptionKey, ((ISupportMultipleResourceStringData)bill).MultipleKeysToUse.Single());
		}

		public void TestABL_RX_NKCustomsValueCurrency_ReadOnly()
		{
			AssertEquals(true, bill.ABL_RX_NKCustomsValueCurrencyInfo.ReadOnly);
		}

		[TestDate(2023, 08, 04)]
		public void TestExchangeRate()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30.13m, new ZDateTime(2023, 08, 04), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30.64m, new ZDateTime(2023, 08, 04), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = "IMP";
			var billObj = header.Bills.AddNew();
			header.DeclarationDate = new ZDateTime(2023, 08, 04);

			billObj.ABL_RX_NKGoodsValueCurrency = ZString.Empty;
			AssertEquals(0m, billObj.ExchangeRate);

			billObj.ABL_RX_NKGoodsValueCurrency = header.Branch.Company.Country.RN_RX_NKLocalCurrency;
			AssertEquals(1m, billObj.ExchangeRate);

			billObj.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(30.64m, billObj.ExchangeRate);

			header.AMA_Nature = "EXP";
			header.DeclarationDate = new ZDateTime(2023, 08, 04);
			AssertEquals(30.13m, billObj.ExchangeRate);
		}

		public void TestSetValuesOnABL_RX_NKGoodsValueCurrencyChanged()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			CombineAssertions(() =>
			{
				AssertEquals("ABL_RX_NKOtherDeductionsCurrency", Core.Constants.CurrencyCodes.UnitedStates, bill.ABL_RX_NKOtherDeductionsCurrency);
				AssertEquals("ABL_RX_NKFreightValueCurrency", Core.Constants.CurrencyCodes.UnitedStates, bill.ABL_RX_NKFreightValueCurrency);
				AssertEquals("ABL_RX_NKInsuranceValueCurrency", Core.Constants.CurrencyCodes.UnitedStates, bill.ABL_RX_NKInsuranceValueCurrency);
				AssertEquals("ABL_RX_NKOtherValueCurrency", Core.Constants.CurrencyCodes.UnitedStates, bill.ABL_RX_NKOtherValueCurrency);
			});

			bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			CombineAssertions(() =>
			{
				AssertEquals("ABL_RX_NKOtherDeductionsCurrency", Core.Constants.CurrencyCodes.EuropeanUnion, bill.ABL_RX_NKOtherDeductionsCurrency);
				AssertEquals("ABL_RX_NKFreightValueCurrency", Core.Constants.CurrencyCodes.EuropeanUnion, bill.ABL_RX_NKFreightValueCurrency);
				AssertEquals("ABL_RX_NKInsuranceValueCurrency", Core.Constants.CurrencyCodes.EuropeanUnion, bill.ABL_RX_NKInsuranceValueCurrency);
				AssertEquals("ABL_RX_NKOtherValueCurrency", Core.Constants.CurrencyCodes.EuropeanUnion, bill.ABL_RX_NKOtherValueCurrency);
			});
		}

		#region test ICurrencyConverterDataProvider

		public void TestICurrencyConverterDataProvider_DateOfValuation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			header.DeclarationDate = ZDateTime.BrettsBirthday;
			var testObj = (ICurrencyConverterDataProvider)bill;
			AssertEquals(ZDateTime.BrettsBirthday, testObj.DateOfValuation);

			header.DeclarationDate = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), testObj.DateOfValuation);
		}

		public void TestICurrencyConverterDataProvider_RateType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.Header.AMA_Nature = "IMP";
			var testObj = (ICurrencyConverterDataProvider)bill;
			AssertEquals(ExchangeRateType.Customs, testObj.RateType);

			bill.Header.AMA_Nature = "EXP";
			AssertEquals(ExchangeRateType.CustomsSecondary, testObj.RateType);
		}

		public void TestICurrencyConverterDataProvider_MaximumDaysToFallback()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var testObj = (ICurrencyConverterDataProvider)bill;

			AssertEquals(7, testObj.MaximumDaysToFallback);
		}

		public void TestICurrencyConverterDataProvider_Company()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var testObj = (ICurrencyConverterDataProvider)bill;

			AssertEquals(header.Branch.Company.PK, testObj.Company.PK);
		}

		public void TestICurrencyConverterDataProvider_LocalCurrencyCode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var testObj = (ICurrencyConverterDataProvider)bill;

			AssertEquals(header.Branch.Company.Country.RN_RX_NKLocalCurrency, testObj.LocalCurrencyCodeOverride);
		}

		public void TestICurrencyConverterDataProvider_IsReciprocalOverride()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var testObj = (ICurrencyConverterDataProvider)bill;

			AssertNull(testObj.IsReciprocalOverride);
		}

		#endregion

		public void TestABL_GoodsValue()
		{
			var targetInfo = bill.ABL_GoodsValueInfo;
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(targetInfo, "Total Invoice Amount", "Total Inv. Amt");
			AssertEquals("DecimalPlaces", 2, targetInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
			AssertSetToZeroIfNegativeForDecimal(targetInfo);
		}

		[ExpectNoExceptions]
		public void TestABL_RX_NKGoodsValueCurrency()
		{
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(bill.ABL_RX_NKGoodsValueCurrencyInfo, "Total Invoice Amount Currency", "Currency", "Curr.", string.Empty);
		}

		public void TestABL_GoodsLocationAttribute()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.ABL_GoodsLocationInfo);
			AssertEquals("ABL_GoodsLocation Caption", "Goods Location", resourceStringData.Caption);
		}

		public void TestABL_BillNumber()
		{
			var info = bill.ABL_BillNumberInfo;
			AssertEquals("ABL_BillNumber MaxLength", 35, info.MaxLength);

			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			CombineAssertions(() =>
			{
				AssertEquals("ABL_BillNumber Caption", "Bill Number", resourceStringData.Caption);
				AssertEquals("ABL_BillNumber ShortCaption", "Bill No.", resourceStringData.ShortCaption);
			});
		}

		public void TestABL_CarrierReference()
		{
			var info = bill.ABL_CarrierReferenceInfo;
			AssertEquals("ABL_CarrierReference MaxLength", 4, info.MaxLength);

			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals("ABL_CarrierReference Caption", "Shipping Order", resourceStringData.Caption);
		}

		public void TestRemarks()
		{
			var info = bill.RemarksInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			CombineAssertions(() =>
			{
				AssertEquals("Remarks MaxLength", 256, info.MaxLength);
				AssertEquals("Remarks Caption", "Remarks", resourceStringData.Caption);
			});
		}

		public void TestABL_Procedure()
		{
			var info = bill.ABL_ProcedureInfo;
			AssertEquals("ABL_Procedure MaxLength", 1, info.MaxLength);

			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			CombineAssertions(() =>
			{
				AssertEquals("ABL_Procedure Caption", "Examination Mode", resourceStringData.Caption);
				AssertEquals("ABL_Procedure ShortCaption", "Exam. Mode", resourceStringData.ShortCaption);
			});
		}

		public void TestABL_Incoterm()
		{
			var info = bill.ABL_IncotermInfo;

			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			CombineAssertions(() =>
			{
				AssertEquals("ABL_Incoterm Caption", "Unit Price Term", resourceStringData.Caption);
				AssertEquals("ABL_Incoterm ShortCaption", "U/P. Term", resourceStringData.ShortCaption);
			});
		}

		public void TestABL_E_ARV()
		{
			var info = bill.ABL_E_ARVInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals("ABL_E_ARV Caption", "ETA", resourceStringData.Caption);
		}

		[ExpectNoExceptions]
		public void TestABL_RL_NKPortOfLoading()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(bill.ABL_RL_NKPortOfLoadingInfo, "Port of Loading", "Place at which the goods (consignments) are loaded on to the active means of transport.");
		}

		[ExpectNoExceptions]
		public void TestABL_RL_NKPortOfDischarge()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(bill.ABL_RL_NKPortOfDischargeInfo, "Port of Discharge", "Place at which the goods (consignment) are unloaded from the active means of transport having been used for their carriage.");
		}

		[ExpectNoExceptions]
		public void TestABL_LocationInformation()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(bill.ABL_LocationInformationInfo, "Final Destination", "The destination to which goods are to be delivered, if different from the buyer address and the consignee address.");
		}

		public void TestValidationType()
		{
			var bizObj = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaBillValidationForRegularBill>(bizObj.Validation);

			bizObj.ABL_BolType = "BOL";
			AssertType<AsycudaBillValidationForMasterChild>(bizObj.Validation);
		}

		public void TestHeaderType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<AsycudaManifestHeader>(bill.Header);
		}

		[TestDate(2022, 4, 1)]
		public void TestSetDefaultValues()
		{
			AssertEquals(new ZDateTime(2022, 4, 1), bill.ABL_E_ARV);
		}

		public void TestLookups()
		{
			AssertType<AsycudaBillLookups>(bill.Lookups);
		}

		public void TestShipperRegNoTypes()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertContainsExactElementsInExactOrder(new ZString[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID }, bill.ShipperRegNoTypes());
		}

		public void TestConsigneeRegNoTypes()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertContainsExactElementsInExactOrder(new ZString[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID }, bill.ConsigneeRegNoTypes());
		}

		public void TestSetABL_RX_NKGoodsValueCurrency()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var packedItem1 = bill.PackedItems.AddNew();
			var packedItem2 = bill.PackedItems.AddNew();

			bill.ABL_RX_NKGoodsValueCurrency = "TWD";
			CombineAssertions(() =>
			{
				AssertEquals("item1 goods value currency", "TWD", packedItem1.API_RX_NKGoodsValueCurrency);
				AssertEquals("item2 goods value currency", "TWD", packedItem2.API_RX_NKGoodsValueCurrency);
			});

			bill.ABL_RX_NKGoodsValueCurrency = "USD";
			CombineAssertions(() =>
			{
				AssertEquals("item1 goods value currency", "USD", packedItem1.API_RX_NKGoodsValueCurrency);
				AssertEquals("item2 goods value currency", "USD", packedItem2.API_RX_NKGoodsValueCurrency);
			});
		}

		public void TestSetShipperLocalAddressDefaultValues()
		{
			var header = CreateOrganizationForJobDocAddress();
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_OA_Shipper = header.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("綠晃科技股份有限公司", bill.ABL_ShipperLocalName);
				AssertEquals("臺北加工出口區園東街6號", bill.ABL_ShipperLocalStreet1);
				AssertEquals("5樓之1", bill.ABL_ShipperLocalStreet2);
				AssertEquals("臺北巿", bill.ABL_ShipperLocalCity);
				AssertEquals("TPE", bill.ABL_ShipperLocalState);
			});

			bill.ABL_OA_Shipper = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("綠晃科技股份有限公司", bill.ABL_ShipperLocalName);
				AssertEquals("臺北加工出口區園東街6號", bill.ABL_ShipperLocalStreet1);
				AssertEquals("5樓之1", bill.ABL_ShipperLocalStreet2);
				AssertEquals("臺北巿", bill.ABL_ShipperLocalCity);
				AssertEquals("TPE", bill.ABL_ShipperLocalState);
			});

			bill.ABL_OA_Shipper = header.Addresses.Cast<OrgAddress>().FirstOrDefault(c => c.OA_CompanyNameOverride == "HAPPY CO., LTD.1").PK;
			CombineAssertions(() =>
			{
				AssertNullOrEmpty(bill.ABL_ShipperLocalName);
				AssertNullOrEmpty(bill.ABL_ShipperLocalStreet1);
				AssertNullOrEmpty(bill.ABL_ShipperLocalStreet2);
				AssertNullOrEmpty(bill.ABL_ShipperLocalCity);
				AssertNullOrEmpty(bill.ABL_ShipperLocalState);
			});
		}

		public void TestABL_OA_ShipperBindingList()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var descriptor = bill.ABL_OA_ShipperInfo.PropertyDescriptor;
			var listAttribute = descriptor.Attributes[typeof(ListAttribute)] as ListAttribute;
			AssertEquals("Lookups.Consignors", listAttribute.ListDataSourceMember);
		}

		public void TestABL_OA_ConsigneeBindingList()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var descriptor = bill.ABL_OA_ConsigneeInfo.PropertyDescriptor;
			var listAttribute = descriptor.Attributes[typeof(ListAttribute)] as ListAttribute;
			AssertEquals("Lookups.Consignees", listAttribute.ListDataSourceMember);
		}

		public void TestCalculateBillDuties()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var packedItem1 = bill.PackedItems.AddNew();
			AddTax(packedItem1, ChargeTypeOtherList.Codes.AT, 100m);
			AddTax(packedItem1, ChargeTypeOtherList.Codes.CT, 200m);
			AddTax(packedItem1, ChargeTypeOtherList.Codes.SS, 300m);
			AddTax(packedItem1, ChargeTypeOtherList.Codes.TT, 400m);
			AddTax(packedItem1, ChargeTypeOtherList.Codes.HWS, 500m);
			packedItem1.AsycudaTaxes.Find(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.VAT).DeleteAll();

			var packedItem2 = bill.PackedItems.AddNew();
			AddTax(packedItem2, ChargeTypeOtherList.Codes.DTA, 600m);
			AddTax(packedItem2, ChargeTypeOtherList.Codes.DTS, 700m);
			packedItem2.AsycudaTaxes.Find(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.VAT).DeleteAll();

			var packedItem3 = bill.PackedItems.AddNew();
			AddTax(packedItem3, ChargeTypeOtherList.Codes.VAT, 800m);
			AddTax(packedItem3, ChargeTypeOtherList.Codes.TPF, 900m);

			bill.CalculateBillDuties();
			var billTaxes = bill.AsycudaTaxes;
			CombineAssertions(() =>
			{
				AssertEquals(7, billTaxes.Count);
				AssertEquals("AET_ChargeType = B31 And AET_ChargeAmount = 500", 1, billTaxes.Find(tax => tax.AET_ChargeType == "B31" && tax.AET_ChargeAmount == 500m).Count());
				AssertEquals("AET_ChargeType = B10 And AET_ChargeAmount = 200", 1, billTaxes.Find(tax => tax.AET_ChargeType == "B10" && tax.AET_ChargeAmount == 200m).Count());
				AssertEquals("AET_ChargeType = B60 And AET_ChargeAmount = 300", 1, billTaxes.Find(tax => tax.AET_ChargeType == "B60" && tax.AET_ChargeAmount == 300m).Count());
				AssertEquals("AET_ChargeType = B32 And AET_ChargeAmount = 500", 1, billTaxes.Find(tax => tax.AET_ChargeType == "B32" && tax.AET_ChargeAmount == 500m).Count());
				AssertEquals("AET_ChargeType = A10 And AET_ChargeAmount = 700", 1, billTaxes.Find(tax => tax.AET_ChargeType == "A10" && tax.AET_ChargeAmount == 700m).Count());
				AssertEquals("AET_ChargeType = B40 And AET_ChargeAmount = 800", 1, billTaxes.Find(tax => tax.AET_ChargeType == "B40" && tax.AET_ChargeAmount == 800m).Count());
				AssertEquals("AET_ChargeType = B51 And AET_ChargeAmount = 900", 1, billTaxes.Find(tax => tax.AET_ChargeType == "B51" && tax.AET_ChargeAmount == 900m).Count());
			});

			AddTax(packedItem2, ChargeTypeOtherList.Codes.DTA, 200m);
			bill.CalculateBillDuties();
			AssertEquals("AET_ChargeType = A10 And AET_ChargeAmount = 800", 1, billTaxes.Find(tax => tax.AET_ChargeType == "A10" && tax.AET_ChargeAmount == 800m).Count());

			void AddTax(AsycudaPackedItem packedItem, ZString type, ZDecimal amount)
			{
				var tax = packedItem.AsycudaTaxes.AddNew();
				tax.AET_ChargeType = type;
				tax.AET_ChargeAmount = amount;
			}
		}

		public void TestPackedItemsType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<AsycudaBillPackedItemCollection>(bill.PackedItems);
		}

		public void TestCalculateCustomsValueWhenRelatedFieldsChanged()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bills = manifestHeader.Bills;
			var bill = bills.AddNew();
			bill.ABL_Incoterm = BriefCustomsDeclarationIncotermList.Codes.CostAndInsurance;
			bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.Taiwan;
			bill.ABL_InsuranceValue = 10m;
			bill.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.Taiwan;
			bill.ABL_FreightValue = 30m;
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.Taiwan;
			bill.ABL_OtherDeductions = 40m;
			bill.ABL_OtherValue = 50m;

			var packedItem1 = bill.PackedItems.AddNew();
			packedItem1.API_UnitPrice = 1000m;
			packedItem1.API_CustomsQty = 1m;

			var packedItem2 = bill.PackedItems.AddNew();
			packedItem2.API_UnitPrice = 2000m;
			packedItem2.API_CustomsQty = 1m;
			CombineAssertions(() =>
			{
				AssertEquals(3000m, bill.ABL_GoodsValue);
				AssertEquals(3040m, bill.ABL_CustomsValue);
				AssertEquals(1013m, packedItem1.API_CustomsValue);
				AssertEquals(2027m, packedItem2.API_CustomsValue);
			});

			bill.ABL_FreightValue = 31m;
			CombineAssertions(() =>
			{
				AssertEquals(3000m, bill.ABL_GoodsValue);
				AssertEquals(3041m, bill.ABL_CustomsValue);
				AssertEquals(1014m, packedItem1.API_CustomsValue);
				AssertEquals(2027m, packedItem2.API_CustomsValue);
			});

			bill.ABL_Incoterm = BriefCustomsDeclarationIncotermList.Codes.FreeOnBoard;
			CombineAssertions(() =>
			{
				AssertEquals(3000m, bill.ABL_GoodsValue);
				AssertEquals(3051m, bill.ABL_CustomsValue);
				AssertEquals(1017m, packedItem1.API_CustomsValue);
				AssertEquals(2034m, packedItem2.API_CustomsValue);
			});

			bill.ABL_InsuranceValue = 11m;
			CombineAssertions(() =>
			{
				AssertEquals(3000m, bill.ABL_GoodsValue);
				AssertEquals(3052m, bill.ABL_CustomsValue);
				AssertEquals(1017m, packedItem1.API_CustomsValue);
				AssertEquals(2035m, packedItem2.API_CustomsValue);
			});

			bill.ABL_OtherValue = 51m;
			CombineAssertions(() =>
			{
				AssertEquals(3000m, bill.ABL_GoodsValue);
				AssertEquals(3053m, bill.ABL_CustomsValue);
				AssertEquals(1018m, packedItem1.API_CustomsValue);
				AssertEquals(2035m, packedItem2.API_CustomsValue);
			});

			bill.ABL_OtherDeductions = 41m;
			CombineAssertions(() =>
			{
				AssertEquals(3000m, bill.ABL_GoodsValue);
				AssertEquals(3052m, bill.ABL_CustomsValue);
				AssertEquals(1017m, packedItem1.API_CustomsValue);
				AssertEquals(2035m, packedItem2.API_CustomsValue);
			});

			packedItem1.API_GoodsValue = 100m;
			CombineAssertions(() =>
			{
				AssertEquals(2100m, bill.ABL_GoodsValue);
				AssertEquals(2152m, bill.ABL_CustomsValue);
				AssertEquals(564m, packedItem1.API_CustomsValue);
				AssertEquals(1588m, packedItem2.API_CustomsValue);
			});

			packedItem1.API_UnitPrice = 2000m;
			CombineAssertions(() =>
			{
				AssertEquals(2100m, bill.ABL_GoodsValue);
				AssertEquals(2152m, bill.ABL_CustomsValue);
				AssertEquals(1076m, packedItem1.API_CustomsValue);
				AssertEquals(1076m, packedItem2.API_CustomsValue);
			});

			packedItem1.API_CustomsQty = 2m;
			CombineAssertions(() =>
			{
				AssertEquals(2100m, bill.ABL_GoodsValue);
				AssertEquals(2152m, bill.ABL_CustomsValue);
				AssertEquals(2100m, packedItem1.API_CustomsValue);
				AssertEquals(52m, packedItem2.API_CustomsValue);
			});

			bill.ABL_RX_NKGoodsValueCurrency = "USD";
			CombineAssertions(() =>
			{
				AssertEquals(2100m, bill.ABL_GoodsValue);
				AssertEquals(2991m, bill.ABL_CustomsValue);
				AssertEquals(2920m, packedItem1.API_CustomsValue);
				AssertEquals(71m, packedItem2.API_CustomsValue);
			});
		}

		[TestDate(2023, 09, 22)]
		public void TestReconcilePackedItemCustomsValue()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 31.95m, new ZDateTime(2023, 09, 22), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_Nature = "IMP";
			manifestHeader.AMA_TransportMode = "AIR";
			var bills = manifestHeader.Bills;
			var bill = bills.AddNew();
			bill.Header.BagNumber = "22";
			bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.ABL_OtherValue = 28m;
			bill.ABL_OtherDeductions = 10m;
			bill.ABL_InsuranceValue = 12m;
			bill.ABL_FreightValue = 25m;
			manifestHeader.DeclarationDate = new ZDateTime(2023, 09, 22);

			var packedItem1 = bill.PackedItems.AddNew();
			packedItem1.API_UnitPrice = 101.111111m;
			packedItem1.API_CustomsQty = 1m;

			var packedItem2 = bill.PackedItems.AddNew();
			packedItem2.API_UnitPrice = 107.228600m;
			packedItem2.API_CustomsQty = 1m;

			var packedItem3 = bill.PackedItems.AddNew();
			packedItem3.API_UnitPrice = 102.000001m;
			packedItem3.API_CustomsQty = 1m;

			bill.CalculateCustomsValue();
			CombineAssertions(() =>
			{
				AssertEquals(310.34m, bill.ABL_GoodsValue);
				AssertEquals(11673m, bill.ABL_CustomsValue);
				AssertEquals(3803m, packedItem1.API_CustomsValue);
				AssertEquals("Before reconciling value is 4033", 4034m, packedItem2.API_CustomsValue);
				AssertEquals(3836m, packedItem3.API_CustomsValue);
			});
		}

		public void TestCalculateCustomsValue_Incoterm()
		{
			CombineAssertions(() =>
			{
				AssertCalculateCustomsValue_Incoterm(BriefCustomsDeclarationIncotermList.Codes.CostInsuranceAndFreight, "EXP", 960m, "1000-10-20+30-40");
				AssertCalculateCustomsValue_Incoterm(BriefCustomsDeclarationIncotermList.Codes.CostAndFreight, "EXP", 970m, "1000-20+30-40");
				AssertCalculateCustomsValue_Incoterm(BriefCustomsDeclarationIncotermList.Codes.CostAndInsurance, "EXP", 980m, "1000-10+30-40");
				AssertCalculateCustomsValue_Incoterm(BriefCustomsDeclarationIncotermList.Codes.ExWorks, "EXP", 990m, "1000+30-40");

				AssertCalculateCustomsValue_Incoterm(BriefCustomsDeclarationIncotermList.Codes.CostInsuranceAndFreight, "IMP", 990m, "1000+30-40");
				AssertCalculateCustomsValue_Incoterm(BriefCustomsDeclarationIncotermList.Codes.CostAndFreight, "IMP", 1000m, "1000+10+30-40");
				AssertCalculateCustomsValue_Incoterm(BriefCustomsDeclarationIncotermList.Codes.CostAndInsurance, "IMP", 1010m, "1000+20+30-40");
				AssertCalculateCustomsValue_Incoterm(BriefCustomsDeclarationIncotermList.Codes.ExWorks, "IMP", 1020m, "1000+10+20+30-40");
			});
		}

		void AssertCalculateCustomsValue_Incoterm(ZString incoterm, ZString nature, ZDecimal expectCustomsValue, string assertMessage)
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bills = manifestHeader.Bills;
			var bill1 = bills.AddNew();
			manifestHeader.AMA_Nature = nature;
			bill1.ABL_Incoterm = incoterm;
			bill1.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.Taiwan;
			bill1.ABL_InsuranceValue = 10m;
			bill1.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.Taiwan;
			bill1.ABL_FreightValue = 20m;
			bill1.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.Taiwan;
			bill1.ABL_OtherValue = 30m;
			bill1.ABL_RX_NKOtherValueCurrency = Core.Constants.CurrencyCodes.Taiwan;
			bill1.ABL_OtherDeductions = 40m;
			bill1.ABL_RX_NKOtherDeductionsCurrency = Core.Constants.CurrencyCodes.Taiwan;

			var packedItem1 = bill1.PackedItems.AddNew();
			packedItem1.API_UnitPrice = 1000m;
			packedItem1.API_CustomsQty = 1m;
			bill1.CalculateCustomsValue();
			AssertEquals($"{assertMessage}, ABL_Incoterm: {incoterm}, AMA_Nature: {nature}", expectCustomsValue, bill1.ABL_CustomsValue);
		}

		[TestDate(2023, 08, 04)]
		public void TestCalculateCustomsValue_Currency()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 0.33m, new ZDateTime(2023, 08, 04), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 0.32m, new ZDateTime(2023, 08, 04), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_Nature = "IMP";
			var bills = manifestHeader.Bills;
			var bill1 = bills.AddNew();
			manifestHeader.DeclarationDate = new ZDateTime(2023, 08, 04);

			bill1.ABL_Incoterm = BriefCustomsDeclarationIncotermList.Codes.CostAndInsurance;
			bill1.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill1.ABL_InsuranceValue = 10m;
			bill1.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill1.ABL_FreightValue = 30m;
			bill1.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			bill1.ABL_OtherValue = 4m;
			bill1.ABL_RX_NKOtherValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill1.ABL_OtherDeductions = 5m;
			bill1.ABL_RX_NKOtherDeductionsCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var packedItem1 = bill1.PackedItems.AddNew();
			packedItem1.API_UnitPrice = 1000m;
			packedItem1.API_CustomsQty = 1m;

			var packedItem2 = bill1.PackedItems.AddNew();
			packedItem2.API_UnitPrice = 2000m;
			packedItem2.API_CustomsQty = 1m;
			bill1.CalculateCustomsValue();
			CombineAssertions(() =>
			{
				AssertEquals(3000m, bill1.ABL_GoodsValue);
				AssertEquals("(3000 + 30  + 4  - 5) * 0.32 TWD", 969m, bill1.ABL_CustomsValue);
				AssertEquals("1000 * 0.32 * (3000 + 30  + 4  - 5) / 3000 TWD", 323m, packedItem1.API_CustomsValue);
				AssertEquals("2000 * 0.32 * (3000 + 30  + 4  - 5) / 3000 TWD", 646m, packedItem2.API_CustomsValue);
			});
		}

		public void TestShipperBondedIDType()
		{
			var header = CreateOrganizationForJobDocAddress();
			var bill = (AsycudaBill)GetNewBusinessObject();
			bill.ABL_OA_Shipper = header.MainAddress.PK;
			BusinessObjectCaptionTestHelper.AssertCaptions(bill.ShipperBondedIDTypeInfo, "Bonded ID");
			AssertEquals(OrgCusCode.TaiwanCodeTypes.EPZ, bill.ShipperBondedIDType);
		}

		public void TestShipperBondedID()
		{
			var header = CreateOrganizationForJobDocAddress();
			var bill = (AsycudaBill)GetNewBusinessObject();
			bill.ABL_OA_Shipper = header.MainAddress.PK;
			AssertEquals("123456", bill.ShipperBondedID);
		}

		public void TestCalculateItemTaxes()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTPFRate(Factory);
			var bill = (AsycudaBill)GetNewBusinessObject();
			var packItem = bill.PackedItems.AddNew();
			packItem.API_CustomsValue = 250000m;
			bill.ABL_CustomsValue = 2000m;
			bill.CalculateItemTaxes();
			AssertContainsExactElementsInAnyOrder(["VAT", "TPF"] , packItem.AsycudaTaxes.Where(c => c.AET_RateOverrideReasonCode.IsEmpty).Select(c => c.AET_ChargeType));

			bill.ABL_CustomsValue = 1999m;
			bill.CalculateItemTaxes();
			AssertEquals(0, packItem.AsycudaTaxes.Count);
		}

		public void TestRequiresCalculateTPF()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTPFRate(Factory);
			var bill = (AsycudaBill)GetNewBusinessObject();
			var packItem = bill.PackedItems.AddNew();
			packItem.API_CustomsValue = 250000m;
			AssertEquals(true, bill.RequiresCalculateTPF);

			packItem.API_CustomsValue = 249999m;
			AssertEquals(false, bill.RequiresCalculateTPF);
		}

		public void TestTpfRate()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTPFRate(Factory);
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertEquals(0.0004m, bill.TpfRate);
		}

		public void TestVatRate()
		{
			AsycudaPackedItemTaxHelperForTest.CreateVATRate(Factory);
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertEquals(0.05m, bill.VatRate);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}

		AsycudaBill bill => (AsycudaBill)GetNewBusinessObject();

		OrgHeader CreateOrganizationForJobDocAddress()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "OrgTest1";
			header.OH_IsConsignee = true;

			var mainAddress = header.MainAddress;
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = Enterprise.Core.SharedConstants.Languages.English;
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";
			mainAddress.OA_City = "APPLE CITY";
			mainAddress.OA_State = "TPE";
			mainAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "123456", Core.Constants.CountryCodes.Taiwan);
			mainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "456789", Core.Constants.CountryCodes.Australia);

			var zhTWtranslatedAddress1 = mainAddress.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Enterprise.Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "綠晃科技股份有限公司";
			zhTWtranslatedAddress1.OTA_Address1 = "臺北加工出口區園東街6號";
			zhTWtranslatedAddress1.OTA_Address2 = "5樓之1";
			zhTWtranslatedAddress1.OTA_City = "臺北巿";
			zhTWtranslatedAddress1.OTA_State = "TPE";

			var addresse = header.Addresses.AddNew();
			addresse.OA_CompanyNameOverride = "HAPPY CO., LTD.1";
			addresse.OA_Language = Core.SharedConstants.Languages.English;
			addresse.OA_Address1 = "004 HAPPY RD";
			addresse.OA_Address2 = "ORANGE DISTRICT1";
			addresse.OA_City = "APPLE CITY1";
			addresse.OA_State = "TPE";
			Factory.Save();
			return header;
		}
	}
}
