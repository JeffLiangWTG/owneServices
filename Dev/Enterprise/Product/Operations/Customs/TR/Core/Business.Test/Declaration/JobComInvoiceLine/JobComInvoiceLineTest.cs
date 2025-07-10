using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	sealed class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public void TestDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			Factory.Save();

			CombineAssertions("Default values", () =>
			{
				AssertEquals("ZG_UsedGoodsCode Default Value", UsedGoodsCodeList.Codes.K1, invoiceLine.ZG_UsedGoodsCode);
				AssertEquals("ZG_PriceType Default Value", PriceTypeList.Codes._01, invoiceLine.ZG_PriceType);
				AssertEquals("JI_ValuationCode Default Value", "11", invoiceLine.JI_ValuationCode);
			});
		}

		public override void TestMakeCustomsQuantityReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Customs unit qty default to KGM and Qty field should be editable", false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

				InvoiceLine.JI_CustomsUnitQty = ZString.Empty;
				AssertEquals("Customs unit qty empty Qty field should be readonly", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
			});
		}

		public void TestJI_StatisticalValueUSD()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
				var exchangeRate = currency.ExchangeRates.AddNew();
				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRate.RE_StartDate = ZDateTime.Now.AddDays(-2);
				exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
				exchangeRate.RE_SellRate = 17.265300000m;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

				currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.EuropeanUnion);
				exchangeRate = currency.ExchangeRates.AddNew();
				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRate.RE_StartDate = ZDateTime.Now.AddDays(-2);
				exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
				exchangeRate.RE_SellRate = 18.037900000m;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var invoice = dec.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				invoice.JZ_InvoiceAmount = 1000m;
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1000m;
				invoiceLine.ZG_StatisticalValueManualOverride = true;

				var insuranceCharge1 = invoiceLine.Charges.AddNew();
				insuranceCharge1.J7_ChargeType = "OFT";
				insuranceCharge1.J7_RX_NKCurrency = "EUR";
				insuranceCharge1.J7_Amount = 100m;
				insuranceCharge1.J7_IsStatisticalValueApplicable = true;

				var insuranceCharge2 = invoiceLine.Charges.AddNew();
				insuranceCharge2.J7_ChargeType = "ONS";
				insuranceCharge2.J7_RX_NKCurrency = "EUR";
				insuranceCharge2.J7_Amount = 50M;
				insuranceCharge2.J7_IsStatisticalValueApplicable = true;

				AssertEquals("In TRY", 20743.59m, invoiceLine.JI_Calc_StatisticalValue);
				AssertEquals("In USD", 1201.46m, invoiceLine.JI_StatisticalValueUSD);
			}
		}

		#region Captions
		public void TestCaptions()
		{
			CombineAssertions("Below assertions are for the captions with attributes!", () =>
			{
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_PrimaryPreference), false, attr => attr.Caption == "[36] Preference Code" && attr.ShortCaption == "[36] Preference Code");
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_CustomsSecondQuantity), false, attr => attr.Caption == "[41] Supporting Qty" && attr.ShortCaption == "[41] Supporting Qty");
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_InwardProcessingLicenseLineNumber), false, attr => attr.Caption == "Inward Processing Line No" && attr.ShortCaption == "Inward Pro. Line No");
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_ReturningGoodsReasonCode), false, attr => attr.Caption == "Return Goods Reason" && attr.ShortCaption == "Return Goods Reason");
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_ReturningGoodsReasonDetail), false, attr => attr.Caption == "Details" && attr.ShortCaption == "Details");
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_RW_NKBorderTradeStateCode), false, attr => attr.Caption == "Border Trade City" && attr.ShortCaption == "Border Trade City");
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_PreviousEntryNumber), false, attr => attr.Caption == "Prev.Declaration No" && attr.ShortCaption == "Prev.Dec.No");
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_PreviousEntryLineNumber), false, attr => attr.Caption == "Prev.Dec.Line No" && attr.ShortCaption == "Line No");
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_PriceType), false, attr => attr.Caption == "Price Type" && attr.ShortCaption == "Price Type");
			});

			CombineAssertions("Below assertions are for the captions with propertyInfos!", () =>
			{
				AssertCaption(InvoiceLine.JI_ValuationCodeInfo, "[24] Tran. Nature", "The Nature of the transaction");
				AssertCaption(InvoiceLine.JI_CustomsFifthQuantityInfo, "Calc. Qty 3", "Calculation Qty 3");
			});
		}

		public void TestCaptionOfTRProperties()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			CombineAssertions("TR Properties Caption", () =>
			{
				AssertCaption(invoiceLine.JI_ZZF_NKTaxTypeInfo, "VAT Code", "VAT Code");
				AssertCaption(invoiceLine.JI_CustomsThirdQuantityInfo, "Calc. Qty 1", "Calculation Qty 1");
				AssertCaption(invoiceLine.JI_CustomsFourthQuantityInfo, "Calc. Qty 2", "Calculation Qty 2");
			});
		}

		void AssertCaption(ZPropertyInfo info, string shortCaption, string caption)
		{
			var propertyData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals($"{info.Name} Caption", caption, propertyData.Caption);
			AssertEquals($"{info.Name} ShortCaption", shortCaption, propertyData.ShortCaption);
		}
		#endregion

		#region MaxLengths
		public void TestMaxLengths()
		{
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_PreviousEntryNumber), false, attr => attr.MaxLength == 20);
		}

		public void TestJI_TariffMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			AssertEquals(16, invoiceLine.JI_TariffInfo.MaxLength);
		}

		public void TestJI_PrimaryPreference()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("JI_PrimaryPreference MaxLength Should be 10", 10, invoiceLine.JI_PrimaryPreferenceInfo.MaxLength);
		}
		#endregion

		public void TestJI_CurrencyUSD()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals("Always USD", RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).PK, invoiceLine.JI_CurrencyUSD);
		}

		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be TR", Core.Constants.CountryCodes.Turkey, InvoiceLine.CustomsCountryCode);
		}

		public void TestIsPreviousEntryAvailable()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			AssertEquals("CusProcedure is null", false, invoiceLine.IsPreviousEntryAvailable);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure("TR", "", "22", "11", "", "des", "IMP");
			var procedureExp = helper.CreateRefCusProcedure("TR", "", "22", "11", "", "des", "EXP");
			invoiceLine.JI_Procedure = "2211";

			procedure.ZZ6_OutOfInwardProcessing = "Y";
			AssertEquals("When IsOutOfInwardProcessing returns true", true, invoiceLine.IsPreviousEntryAvailable);
			procedure.ZZ6_OutOfInwardProcessing = "N";
			AssertEquals("When IsOutOfInwardProcessing returns false", false, invoiceLine.IsPreviousEntryAvailable);

			procedure.ZZ6_IntoTemporaryExport = "Y";
			AssertEquals("When IsIntoTemporaryExport returns true", true, invoiceLine.IsPreviousEntryAvailable);
			procedure.ZZ6_IntoTemporaryExport = "N";
			AssertEquals("When IsIntoTemporaryExport returns false", false, invoiceLine.IsPreviousEntryAvailable);

			procedure.ZZ6_OutOfTemporaryExport = "Y";
			AssertEquals("When IsOutOfTemporaryExport returns true", true, invoiceLine.IsPreviousEntryAvailable);
			procedure.ZZ6_OutOfTemporaryExport = "N";
			AssertEquals("When IsOutOfTemporaryExport returns false", false, invoiceLine.IsPreviousEntryAvailable);

			procedure.ZZ6_IntoTemporaryImport = "Y";
			AssertEquals("When IsIntoTemporaryImport returns true", true, invoiceLine.IsPreviousEntryAvailable);
			procedure.ZZ6_IntoTemporaryImport = "N";
			AssertEquals("When IsIntoTemporaryImport returns false", false, invoiceLine.IsPreviousEntryAvailable);

			procedure.ZZ6_OutOfTemporaryImport = "Y";
			AssertEquals("When IsOutOfTemporaryImport returns true", true, invoiceLine.IsPreviousEntryAvailable);
			procedure.ZZ6_OutOfTemporaryImport = "N";
			AssertEquals("When IsOutOfTemporaryImport returns false", false, invoiceLine.IsPreviousEntryAvailable);

			procedure.ZZ6_IntoWarehouse = "Y";
			AssertEquals("When IsIntoWarehouse returns true", true, invoiceLine.IsPreviousEntryAvailable);
			procedure.ZZ6_IntoWarehouse = "N";
			AssertEquals("When IsIntoWarehouse returns false", false, invoiceLine.IsPreviousEntryAvailable);

			procedure.ZZ6_OutOfWarehouse = "Y";
			AssertEquals("When IsOutOfWarehouse returns true", true, invoiceLine.IsPreviousEntryAvailable);
			procedure.ZZ6_OutOfWarehouse = "N";
			AssertEquals("When IsOutOfWarehouse returns false", false, invoiceLine.IsPreviousEntryAvailable);

			procedure.ZZ6_OutOfInwardProcessing = "Y";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("When Import", true, invoiceLine.IsPreviousEntryAvailable);

			procedureExp.ZZ6_OutOfInwardProcessing = "Y";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("When Export", true, invoiceLine.IsPreviousEntryAvailable);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("When neither Import nor Export", false, invoiceLine.IsPreviousEntryAvailable);
		}

		public void TestIsNotImportOrExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals(false, invoiceLine.IsNotImportOrExport);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals(true, invoiceLine.IsNotImportOrExport);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals(false, invoiceLine.IsNotImportOrExport);
		}

		public void TestJI_OA_ManufacturerAddressEffectiveValueToReturn()
		{
			var dec = Factory.New<JobDeclaration>();
			Assert(dec.JE_OA_ManufacturerAddress.IsEmpty);
			Assert(dec.JE_OH_Manufacturer.IsEmpty);

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var manufactureraddress = manufacturer.Addresses.AddNew();

			dec.JE_OA_ManufacturerAddress = manufactureraddress.PK;
			AssertEquals(manufacturer.PK, dec.JE_OH_Manufacturer);

			dec.JE_OA_ManufacturerAddress = ZGuid.Empty;
			Assert(dec.JE_OH_Manufacturer.IsEmpty);

			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			Assert(invoiceLine.JI_OA_ManufacturerAddress.IsEmpty);
		}

		public void TestJI_OA_ManufacturerAddress()
		{
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			OrgAddress address1 = manufacturer.MainAddress;
			address1.Address1 = "testAddress";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			declaration.JE_OA_ManufacturerAddress = address1.PK;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(address1.PK, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("testAddress", invoiceLine.JI_OA_ManufacturerAddress_ZAddress.AddressFull);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.Turkey, partDetails.CustomsCountryCode);
				AssertEquals(typeof(MasterFiles.OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestVehicles()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var vehicle1 = Factory.New<CusVehicle>();
			var vehicle2 = Factory.New<CusVehicle>();
			vehicle1.CVH_ParentID = invoiceLine.PK;
			vehicle2.CVH_ParentID = invoiceLine.PK;

			AssertEquals(2, invoiceLine.Vehicles.Count);
			AssertEquals(true, invoiceLine.Vehicles.Contains(vehicle1));
			AssertEquals(true, invoiceLine.Vehicles.Contains(vehicle2));
		}

		public void TestJI_TariffDefaultingCustomsSecondaryUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "88881998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, Core.Constants.Weight.LongTons);
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.CustomsUOM3Type, "CX3");
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.CustomsUOM4Type, "CX4");
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.CustomsUOM5Type, "CX5");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;

			AssertEquals(Universal.Constants.TariffTypes.HarmonizedSystem, invoiceLine.UniversalTariffType);
			AssertEquals("KGM", invoiceLine.CustomsUQ);
			AssertEquals("TL", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("KGM", invoiceLine.JI_CustomsUnitQty);

			AssertEquals("CX3", invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals("CX4", invoiceLine.JI_CustomsFourthUnitQty);
			AssertEquals("CX5", invoiceLine.JI_CustomsFifthUnitQty);
		}

		public void TestJI_NDescriptionAndJI_Description()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "88881998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Test Description");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;

			AssertEquals(ZString.Empty, invoiceLine.JI_Description);
			AssertEquals("Test Description", invoiceLine.JI_NDescription);

			invoiceLine.JI_Tariff = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;

			AssertEquals(ZString.Empty, invoiceLine.JI_Description);
			AssertEquals("Test Description", invoiceLine.JI_NDescription);
		}

		public void TestTariffFormatter()
		{
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			AssertType<TariffFormatter>(invoiceLine.TariffFormatterExposed);
		}

		public void TestAddInfoJobComInvoiceLineFieldsValue()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("K1", invoiceLine.ZG_UsedGoodsCode);
				AssertEquals(false, invoiceLine.ZG_SecondaryTreatedProduct);
				AssertEquals(false, invoiceLine.ZG_ReturnToOrigin);
			});

			invoiceLine.ZG_UsedGoodsCode = "K2";
			invoiceLine.ZG_SecondaryTreatedProduct = true;
			invoiceLine.ZG_ReturnToOrigin = true;
			invoiceLine.ZG_ReturningGoodsReasonCode = "01";
			invoiceLine.ZG_ReturningGoodsReasonDetail = "Returning Goods Reason Detail Text";
			invoiceLine.ZG_ExportUnionPackCode = "A0001";
			invoiceLine.ZG_ExportUnionThreadCode = "A0002";
			invoiceLine.ZG_ExportUnionProductionYear = 2021;
			invoiceLine.ZG_ExportUnionEcological = true;
			invoiceLine.ZG_ExportUnionDeferredInstallment = "Export Union Deferred Installment Text";
			invoiceLine.ZG_RW_NKBorderTradeStateCode = "34";
			invoiceLine.ZG_ExportUnionAdditionalTariffCode = "123456789012";
			invoiceLine.ZG_ExcessStock = ZBool.True;
			invoiceLine.ZG_EntryExitPurposeCode = "01";
			invoiceLine.ZG_EntryExitPurposeDetail = "Entry Exit Purpose Detail Text";
			invoiceLine.ZG_CommercialPaymentCode = "1";
			invoiceLine.ZG_CommercialPaymentAmount = 100;
			invoiceLine.ZG_CommercialPaymentNumber = "12345678901234567890";
			Factory.Save();

			var newInvoiceLine = NewFactory().Load<JobComInvoiceLine>(invoiceLine.PK);

			CombineAssertions(() =>
			{
				AssertEquals("K2", newInvoiceLine.ZG_UsedGoodsCode);
				AssertEquals(true, newInvoiceLine.ZG_SecondaryTreatedProduct);
				AssertEquals(true, newInvoiceLine.ZG_ReturnToOrigin);
				AssertEquals("01", newInvoiceLine.ZG_ReturningGoodsReasonCode);
				AssertEquals("Returning Goods Reason Detail Text", newInvoiceLine.ZG_ReturningGoodsReasonDetail);
				AssertEquals("A0001", newInvoiceLine.ZG_ExportUnionPackCode);
				AssertEquals("A0002", newInvoiceLine.ZG_ExportUnionThreadCode);
				AssertEquals(new ZShort(2021), newInvoiceLine.ZG_ExportUnionProductionYear);
				AssertEquals(true, newInvoiceLine.ZG_ExportUnionEcological);
				AssertEquals("Export Union Deferred Installment Text", newInvoiceLine.ZG_ExportUnionDeferredInstallment);
				AssertEquals("Border Trade City Code", "34", invoiceLine.ZG_RW_NKBorderTradeStateCode);
				AssertEquals("Additional Code", "123456789012", invoiceLine.ZG_ExportUnionAdditionalTariffCode);
				AssertEquals("Excess Stock", ZBool.True, invoiceLine.ZG_ExcessStock);
				AssertEquals("01", newInvoiceLine.ZG_EntryExitPurposeCode);
				AssertEquals("Entry Exit Purpose Detail Text", newInvoiceLine.ZG_EntryExitPurposeDetail);
				AssertEquals("1", newInvoiceLine.ZG_CommercialPaymentCode);
				AssertEquals(100m, newInvoiceLine.ZG_CommercialPaymentAmount);
				AssertEquals("12345678901234567890", newInvoiceLine.ZG_CommercialPaymentNumber);
			});
		}

		public void TestSupplementaryAdditionalCodeTest()
		{
			SetupTariffAndRate();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "88881998";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Turkey;
			invoiceLine.JI_PrimaryPreference = "STD";

			var supplementaryCodeLoader = new BaseSupplementaryCode.Loader(Factory);
			var code = supplementaryCodeLoader.LoadOrCreate<SupplementaryCode, JobComInvoiceLine>(invoiceLine, 1);
			var list = code.Lookups.CY_CodeList;
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode("additionalcode"));
			AssertEquals("Additional Code 1 Descriptions", list.GetDescriptionFromCode("additionalcode"));
		}

		public override void TestChargeTypeList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			ICommonInvoice commonInvoice = invoiceLine;

			CombineAssertions(() =>
			{
				AssertSame("cached", commonInvoice.ChargeTypeList, commonInvoice.ChargeTypeList);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Import", "COM, DEM, INT, LBC, LCC, LDC, LEC, LOT, LPC, LRU, LSC, LTC, OBS, OFT, ONS, OTH, ROY, SUR, TFC", commonInvoice.ChargeTypeList.CodesAsString);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Export", "COM, DEM, INT, LBC, LCC, LDC, LEC, LOT, LPC, LRU, LSC, LTC, OBS, OFT, ONS, OTH, ROY, SUR, TFC", commonInvoice.ChargeTypeList.CodesAsString);
			});
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CustomsUnitQty = "";
			AssertEquals(true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals(false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public override void TestJI_FormattedTariff()
		{
			CombineAssertions(() =>
			{
				InvoiceLine.JI_Tariff = "1234567890";
				AssertEquals("No formatting", "1234.56.78.90", InvoiceLine.JI_FormattedTariff);
				InvoiceLine.JI_FormattedTariff = "9876.54.32.10";
				AssertEquals("Already Formatted", "9876.54.32.10", InvoiceLine.JI_FormattedTariff);
			});
		}

		public void TestJI_ZZF_NKTaxType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ZZF_NKTaxType = "MUAF";
			AssertEquals("MUAF", invoiceLine.JI_ZZF_NKTaxType);
		}

		public void TestSetDefaultTaxOrFeeCode()
		{
			var (tariff, expectedTaxType) = SetupTariffAndFeeDetails();
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			AssertEquals(invoiceLine.UseUniversalTariff ? expectedTaxType : ZString.Empty, invoiceLine.JI_ZZF_NKTaxType);
		}

		public void TestJI_ValuationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			AssertEquals("11", invoiceLine.JI_ValuationCode);
		}

		public void TestGetAllActiveCusSupportingInfoTypes()
		{
			CombineAssertions(() =>
			{
				AssertEquals(typeof(AdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)InvoiceLine).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
				AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)InvoiceLine).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
			});
		}

		public void TestAdditionalInfos()
		{
			AssertType<AdditionalInfoCollection>(InvoiceLine.AdditionalInfos);
		}

		public void TestTaxes()
		{
			AssertType<JobComInvoiceLineTaxCollection>(InvoiceLine.Taxes);
		}

		public void TestIsEntryExitPurposeCodeVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = string.Empty;

			CombineAssertions("Is Entry Exit Purpose Field Procedure Codes", () =>
			{
				AssertEquals("Empty", false, invoiceLine.IsEntryExitPurposeCodeVisibility);
				invoiceLine.JI_Procedure = "XXXX";
				AssertEquals("XXXX", false, invoiceLine.IsEntryExitPurposeCodeVisibility);
				invoiceLine.JI_Procedure = "21";
				AssertEquals("21", false, invoiceLine.IsEntryExitPurposeCodeVisibility);
				invoiceLine.JI_Procedure = "2100";
				AssertEquals("2100", true, invoiceLine.IsEntryExitPurposeCodeVisibility);
				invoiceLine.JI_Procedure = "3151";
				AssertEquals("3151", true, invoiceLine.IsEntryExitPurposeCodeVisibility);
				invoiceLine.JI_Procedure = "5100";
				AssertEquals("5100", true, invoiceLine.IsEntryExitPurposeCodeVisibility);
				invoiceLine.JI_Procedure = "5171";
				AssertEquals("5171", true, invoiceLine.IsEntryExitPurposeCodeVisibility);
				invoiceLine.JI_Procedure = "6121";
				AssertEquals("6121", true, invoiceLine.IsEntryExitPurposeCodeVisibility);
				invoiceLine.JI_Procedure = "6321";
				AssertEquals("6321", true, invoiceLine.IsEntryExitPurposeCodeVisibility);
				invoiceLine.JI_Procedure = "6771";
				AssertEquals("6771", true, invoiceLine.IsEntryExitPurposeCodeVisibility);
			});

			CombineAssertions("Empty ZG_EntryExitPurposeCode and ZG_EntryExitPurposeDetail", () =>
			{
				invoiceLine.JI_Procedure = "2100";
				invoiceLine.ZG_EntryExitPurposeCode = "5";
				invoiceLine.ZG_EntryExitPurposeDetail = "XXXXXXXXXXXX";
				AssertEquals("ZG_EntryExitPurposeCode should not be empty", "5", invoiceLine.ZG_EntryExitPurposeCode);
				AssertEquals("ZG_EntryExitPurposeDetail should not be empty", "XXXXXXXXXXXX", invoiceLine.ZG_EntryExitPurposeDetail);

				invoiceLine.JI_Procedure = "5300";
				AssertEquals("ZG_EntryExitPurposeCode should be empty", ZString.Empty, invoiceLine.ZG_EntryExitPurposeCode);
				AssertEquals("ZG_EntryExitPurposeDetail should be empty", ZString.Empty, invoiceLine.ZG_EntryExitPurposeDetail);
			});
		}

		void TestDecimalPlaces(params (string PropertyName, int DecimalPlaces)[] properties)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			foreach (var property in properties)
			{
				AssertHasCustomAttribute<DecimalPlacesAttribute>(invoiceLine.GetType(), property.PropertyName, false, attr => attr.DecimalPlaces == property.DecimalPlaces);
			}
		}

		public void TestQuantitiesDecimalPlaces()
		{
			TestDecimalPlaces(
				("JI_InvoiceQuantity", 3),
				("JI_CustomsQuantity", 3),
				("JI_CustomsSecondQuantity", 3),
				("JI_CustomsThirdQuantity", 4),
				("JI_CustomsFourthQuantity", 4),
				("JI_CustomsFifthQuantity", 4)
			);
		}

		public void TestIsExemptFromStampDuty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			AssertEquals("CusProcedure is null", false, invoiceLine.IsExemptFromStampDuty);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure("TR", "", "22", "11", "", "des", "IMP");
			var procedureExp = helper.CreateRefCusProcedure("TR", "", "22", "11", "", "des", "EXP");
			invoiceLine.JI_Procedure = "2211";

			CombineAssertions(() =>
			{
				procedure.ZZ6_IntoInwardProcessing = "Y";
				AssertEquals("When IsIntoInwardProcessing returns true", true, invoiceLine.IsExemptFromStampDuty);
				procedure.ZZ6_IntoInwardProcessing = "N";
				AssertEquals("When IsIntoInwardProcessing returns false", false, invoiceLine.IsExemptFromStampDuty);

				procedure.ZZ6_IntoOutwardProcessing = "Y";
				AssertEquals("When IsIntoOutwardProcessing returns true", true, invoiceLine.IsExemptFromStampDuty);
				procedure.ZZ6_IntoOutwardProcessing = "N";
				AssertEquals("When IsIntoOutwardProcessing returns false", false, invoiceLine.IsExemptFromStampDuty);

				procedure.ZZ6_IntoTemporaryExport = "Y";
				AssertEquals("When IsIntoTemporaryExport returns true", true, invoiceLine.IsExemptFromStampDuty);
				procedure.ZZ6_IntoTemporaryExport = "N";
				AssertEquals("When IsIntoTemporaryExport returns false", false, invoiceLine.IsExemptFromStampDuty);

				procedure.ZZ6_IntoTemporaryImport = "Y";
				AssertEquals("When IsIntoTemporaryImport returns true", true, invoiceLine.IsExemptFromStampDuty);
				procedure.ZZ6_IntoTemporaryImport = "N";
				AssertEquals("When IsIntoTemporaryImport returns false", false, invoiceLine.IsExemptFromStampDuty);

				procedure.ZZ6_IntoWarehouse = "Y";
				AssertEquals("When IsIntoWarehouse returns true", true, invoiceLine.IsExemptFromStampDuty);
				procedure.ZZ6_IntoWarehouse = "N";
				AssertEquals("When IsIntoWarehouse returns false", false, invoiceLine.IsExemptFromStampDuty);
			});
		}

		public void TestMaxLengthNDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeHSN = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, "HSN");
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);

			Factory.Save();

			string tariffDescription = new string('X', 524) + " " + "Test Desc";

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypeHSN.PK, "1000", startDate, endDate, tariffDescription);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("ZZ1_Description", 534, tariff.ZZ1_Description.Length);
				AssertNoExceptionThrown("Update with a length greater than the JI_NDescription maxLength", () => invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode);

				string expectedDescription = new string('X', 524);
				AssertEquals("JI_NDescription", expectedDescription, invoiceLine.JI_NDescription);
			});
		}

		public override void TestDefaultDataGroupingCode()
		{
			using (CustomsDataRegistry.Instance.CopyPreviousLineCustomsProcedureCode.SetTemporaryValue(Guid.Empty, Declaration.Branch.EntityPK.ToGuid(), Guid.Empty, false))
			{
				base.TestDefaultDataGroupingCode();
			}
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			return declaration;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			return invoiceLine;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<BaseJobDeclaration>();

			var importer = factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			var result = invoiceHeader.InvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();

			if (declaration.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine)
			{
				var link = (result.AdditionalEntryLineLinks.Count > 0 ? result.AdditionalEntryLineLinks[0] : null) ?? result.AdditionalEntryLineLinks.AddNew();
				link.BU_CL = entryLine.PK;
				link.BU_JI = result.PK;
			}

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var invoiceLine = (JobComInvoiceLine)base.GetNewBusinessObjectForSettingValueCallsRefreshBindingTest();
			invoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			return invoiceLine;
		}

		new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		void SetupTariffAndRate()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var currentCountry = Core.Constants.CountryCodes.Turkey;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "88881998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Test Description");
			Factory.Save();

			var tradeGroupStandard = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "STANDARD", date1, date4);
			helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Turkey, date1, date4);
			Factory.Save();

			var dutyRateType = helper.CreateNewOrGetExistingRateType(currentCountry, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "A00", dutyRateType.PK);
			Factory.Save();
			var preferenceSTD = helper.CreatePreferenceForCountry("STD", "Standard", currentCountry);
			Factory.Save();

			var testRate1 = helper.CreateRate(tariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			helper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "additionalcode", "ordernumber");
			Factory.Save();

			helper.CreateCusCodeType("ADDCD", "Additional Codes", "TR");
			Factory.Save();
			helper.CreateCusCodeList(currentCountry, "ADDCD", "additionalcode", "Additional Code 1 Descriptions", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
		}

		(Universal.TariffView tariff, ZString expectedTaxType) SetupTariffAndFeeDetails()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var countryCode = Core.Constants.CountryCodes.Turkey;
			helper.CreateTaxOrFee("KD1", 0.21, countryCode);
			var tariffType = helper.CreateTariffType(countryCode, "IMP");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(countryCode, tariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "KD1");
			Factory.Save();
			return (tariff, ZString.Empty);
		}

		protected override Type ExpectedTypeOfApportionedCharges => typeof(Common.JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(EU.Business.Declaration.InvoiceLineChargeCollection<InvoiceLineCharge>);

		sealed class JobComInvoiceLineForTest : JobComInvoiceLine
		{
			public JobComInvoiceLineForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Customs.Business.TariffFormatter TariffFormatterExposed => base.TariffFormatter;
		}
	}
}
