using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobComInvoiceHeaderFunctionalTest : Customs.Business.Testing.BaseJobComInvoiceHeaderFunctionalTest
	{
		public void TestSetManufacturerAddress()
		{
			USCustomsDataRegistry.Instance.DefaultManufacturerFromSupplier.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = org1.PK;
			AssertEquals(declaration.JE_OA_ManufacturerAddress_ZAddress.OrgPK, org1.PK);
			var invoice1 = declaration.Invoices.AddNew();
			AssertEquals(invoice1.JZ_OA_SupplierAddress_ZAddress.OrgPK, org1.PK);
			AssertEquals(invoice1.JZ_OA_ManufacturerAddress_ZAddress.OrgPK, org1.PK);
			invoice1.JZ_OA_SupplierAddress_ZAddress.OrgPK = org3.PK;
			AssertEquals(invoice1.JZ_OA_ManufacturerAddress_ZAddress.OrgPK, org3.PK);
			declaration.JE_OA_ManufacturerAddress_ZAddress.OrgPK = org2.PK;
			invoice1.JZ_OA_SupplierAddress_ZAddress.OrgPK = org1.PK;
			AssertEquals(invoice1.JZ_OA_ManufacturerAddress_ZAddress.OrgPK, org2.PK);
		}

		public void TestWhenDateOfExportAndCurrencyAreSet()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			currency.ExchangeRates.DeleteAll();
			currency.SetUpExchangeRates(new ZDateTime(2012, 3, 1), 1.05m);
			currency.SetUpExchangeRates(new ZDateTime(2012, 3, 2), 1.06m);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = new ZDateTime(2012, 3, 1);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			AssertEquals("One foreign currency exists and dec's export date is used", new ZDateTime(2012, 3, 1), invoice.EffectiveValuationDate);
			AssertEquals("Ex-rate is set", 1.05m, invoice.JZ_InvoiceCurrExRate);
			Assert(!invoice.IsLatestRateDateDifferentToExportDateAndRatesExistOnExportDate);
			invoice.US_DateOfExport = new ZDateTime(2012, 3, 3);
			declaration.ResumeApportionment();
			AssertEquals("invoice has an overridden export date, but no rate exists on 3rd", new ZDateTime(2012, 3, 2), invoice.EffectiveValuationDate);
			AssertEquals("Ex-rate is set", 1.06m, invoice.JZ_InvoiceCurrExRate);
			Assert(!invoice.IsLatestRateDateDifferentToExportDateAndRatesExistOnExportDate);
			currency.SetUpExchangeRates(new ZDateTime(2012, 3, 3), 1.06m);
			Assert(invoice.IsLatestRateDateDifferentToExportDateAndRatesExistOnExportDate);
			declaration.RefreshExchangeRates();
			Assert(!invoice.IsLatestRateDateDifferentToExportDateAndRatesExistOnExportDate);
		}

		public void TestUS_ECCN_Effective()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			Declaration.US_ECCN = "DEC";
			AssertEquals("US_ECCN_Effective", Declaration.US_ECCN, invoice.US_ECCN);
			invoice.US_ECCN = "INV";
			AssertNotEquals("US_ECCN_Effective", Declaration.US_ECCN, invoice.US_ECCN);
			AssertEquals("US_ECCN_Effective", invoice.US_ECCN, invoice.US_ECCN);
			invoice.US_ECCN = "";
			AssertEquals("US_ECCN_Effective", Declaration.US_ECCN, invoice.US_ECCN);
		}

		public void TestUS_ExportCode_Effective()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			Declaration.US_ExportCode = ExportInformationCodeList.Codes.CR;
			AssertEquals("US_ExportCode_Effective", Declaration.US_ExportCode, invoice.US_ExportCode);
			invoice.US_ExportCode = ExportInformationCodeList.Codes.FS;
			AssertNotEquals("US_ExportCode_Effective", Declaration.US_ExportCode, invoice.US_ExportCode);
			AssertEquals("US_ExportCode_Effective", invoice.US_ExportCode, invoice.US_ExportCode);
			invoice.US_ExportCode = "";
			AssertEquals("US_ExportCode_Effective", Declaration.US_ExportCode, invoice.US_ExportCode);
		}

		public void TestUS_ForeignTradeZone_Effective()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			Declaration.US_ForeignTradeZone = "CAN";
			AssertEquals("US_ForeignTradeZone_Effective", Declaration.US_ForeignTradeZone, invoice.US_ForeignTradeZone);
			invoice.US_ForeignTradeZone = "MEX";
			AssertNotEquals("US_ForeignTradeZone_Effective", Declaration.US_ForeignTradeZone, invoice.US_ForeignTradeZone);
			AssertEquals("US_ForeignTradeZone_Effective", invoice.US_ForeignTradeZone, invoice.US_ForeignTradeZone);
			invoice.US_ForeignTradeZone = "";
			AssertEquals("US_ForeignTradeZone_Effective", Declaration.US_ForeignTradeZone, invoice.US_ForeignTradeZone);
		}

		public void TestUS_LicenseTypeRequiredSpaceCode()
		{
			var factory = new BusinessObjectFactory();
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C38, USAESLicenseCode.Codes.S00, USAESLicenseCode.Codes.VDO });

			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "IIVV11";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_OrderNumber = "IILL33";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_OrderNumber = "IILL22";
			invoiceLine2.US_LicenseType = USAESLicenseCode.Codes.C38;
			declaration.US_LicenseType = "";
			declaration.US_LicenseType = USAESLicenseCode.Codes.C33;
			AssertEquals(declaration.US_LicenseNo, LicenseExemptionTypeList.Codes.NLR);
			AssertEquals(invoice.US_LicenseType, USAESLicenseCode.Codes.C33);
			AssertEquals(invoice.US_LicenseNo, LicenseExemptionTypeList.Codes.NLR);
			AssertEquals(invoiceLine.US_LicenseType, USAESLicenseCode.Codes.C33);
			AssertEquals(invoiceLine.US_LicenseNo, LicenseExemptionTypeList.Codes.NLR);
			invoice.US_LicenseType = USAESLicenseCode.Codes.S00;
			AssertEquals("Header - LicenseType - S00", invoice.US_LicenseType, USAESLicenseCode.Codes.S00);
			AssertEquals("Header - LicenseNo - Space", invoice.US_LicenseNo, "");
			AssertEquals(invoiceLine.US_LicenseType, USAESLicenseCode.Codes.S00);
			AssertEquals(invoiceLine.US_LicenseNo, "");
			AssertEquals(invoiceLine2.US_LicenseType, USAESLicenseCode.Codes.C38);
			AssertEquals(invoiceLine2.US_LicenseNo, LicenseExemptionTypeList.Codes.TSR);
			invoiceLine2.US_LicenseType = USAESLicenseCode.Codes.VDO;
			AssertEquals(invoiceLine2.US_LicenseType, USAESLicenseCode.Codes.VDO);
			AssertEquals(invoiceLine2.US_LicenseNo, "");
			AssertEquals(declaration.US_LicenseNo, LicenseExemptionTypeList.Codes.NLR);
			AssertEquals(invoice.US_LicenseType, USAESLicenseCode.Codes.S00);
		}

		public void TestUS_HazardousCargo_Effective()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			AssertEquals("US_IsHazardousCargo", false, invoice.US_IsHazardousCargo);
			Declaration.US_HazardousCargo = YesNoDefaultList.Codes.Yes;
			AssertEquals("US_HazardousCargo_Effective", Declaration.US_HazardousCargo, invoice.US_HazardousCargo);
			AssertEquals("US_IsHazardousCargo", true, invoice.US_IsHazardousCargo);
			invoice.US_HazardousCargo = YesNoDefaultList.Codes.No;
			AssertNotEquals("US_HazardousCargo_Effective", Declaration.US_HazardousCargo, invoice.US_HazardousCargo);
			AssertEquals("US_HazardousCargo_Effective", invoice.US_HazardousCargo, invoice.US_HazardousCargo);
			AssertEquals("US_IsHazardousCargo", false, invoice.US_IsHazardousCargo);
			invoice.US_HazardousCargo = "";
			AssertEquals("US_HazardousCargo_Effective", Declaration.US_HazardousCargo, invoice.US_HazardousCargo);
			AssertEquals("US_IsHazardousCargo", true, invoice.US_IsHazardousCargo);
		}

		public void TestUS_ImportEntryNo_Effective()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			Declaration.US_InbondType = InbondTypeList.Codes.IEForeignTradeZoneWithdrawal;
			Declaration.US_ImportEntryNo = "DECENTRY";
			invoice.US_ImportEntryNo = "";
			AssertEquals("US_ImportEntryNo_Effective", Declaration.US_ImportEntryNo, invoice.US_ImportEntryNo);
			invoice.US_ImportEntryNo = "INVENTRY";
			AssertNotEquals("US_ImportEntryNo_Effective", Declaration.US_ImportEntryNo, invoice.US_ImportEntryNo);
			AssertEquals("US_ImportEntryNo_Effective", invoice.US_ImportEntryNo, invoice.US_ImportEntryNo);
			invoice.US_ImportEntryNo = "";
			AssertEquals("US_ImportEntryNo_Effective", Declaration.US_ImportEntryNo, invoice.US_ImportEntryNo);
		}

		public void TestUS_InbondType_Effective()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			Declaration.US_InbondType = InbondTypeList.Codes.IEForeignTradeZoneWithdrawal;
			AssertEquals("US_InbondType_Effective", Declaration.US_InbondType, invoice.US_InbondType);
			invoice.US_InbondType = InbondTypeList.Codes.TAndEWarehouseWithdrawal;
			AssertNotEquals("US_InbondType_Effective", Declaration.US_InbondType, invoice.US_InbondType);
			AssertEquals("US_InbondType_Effective", invoice.US_InbondType, invoice.US_InbondType);
			invoice.US_InbondType = "";
			AssertEquals("US_InbondType_Effective", Declaration.US_InbondType, invoice.US_InbondType);
		}

		public void TestUS_LicenseNo()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			Declaration.US_LicenseNo = "DEC123";
			AssertEquals("US_LicenseNo", Declaration.US_LicenseNo, invoice.US_LicenseNo);
			invoice.US_LicenseNo = "INV123";
			AssertNotEquals("US_LicenseNo", Declaration.US_LicenseNo, invoice.US_LicenseNo);
			AssertEquals("US_LicenseNo", invoice.US_LicenseNo, invoice.US_LicenseNo);
			invoice.US_LicenseNo = "";
			AssertEquals("US_LicenseNo", Declaration.US_LicenseNo, invoice.US_LicenseNo);
		}

		public void TestUS_LicenseType()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			Declaration.US_LicenseType = USAESLicenseCode.Codes.C32;
			AssertEquals("US_LicenseType", Declaration.US_LicenseType, invoice.US_LicenseType);
			invoice.US_LicenseType = USAESLicenseCode.Codes.C51;
			AssertNotEquals("US_LicenseType", Declaration.US_LicenseType, invoice.US_LicenseType);
			AssertEquals("US_LicenseType", invoice.US_LicenseType, invoice.US_LicenseType);
			invoice.US_LicenseType = "";
			AssertEquals("US_LicenseType", Declaration.US_LicenseType, invoice.US_LicenseType);
		}

		public void TestUS_RoutedTransaction_Effective()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			AssertEquals("US_RoutedTransaction", false, invoice.US_IsRoutedTransaction);
			Declaration.US_RoutedTransaction = YesNoDefaultList.Codes.No;
			AssertEquals("US_RoutedTransaction_Effective", Declaration.US_RoutedTransaction, invoice.US_RoutedTransaction);
			AssertEquals("US_RoutedTransaction", false, invoice.US_IsRoutedTransaction);
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			AssertNotEquals("US_RoutedTransaction_Effective", Declaration.US_RoutedTransaction, invoice.US_RoutedTransaction);
			AssertEquals("US_RoutedTransaction_Effective", invoice.US_RoutedTransaction, invoice.US_RoutedTransaction);
			AssertEquals("US_RoutedTransaction", true, invoice.US_IsRoutedTransaction);
			invoice.US_RoutedTransaction = "";
			AssertEquals("US_RoutedTransaction_Effective", Declaration.US_RoutedTransaction, invoice.US_RoutedTransaction);
			AssertEquals("US_RoutedTransaction", false, invoice.US_IsRoutedTransaction);
		}

		public void TestUS_StateOfOrigin_Effective()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			Declaration.US_StateOfOrigin = "WA";
			AssertEquals("US_StateOfOrigin_Effective", Declaration.US_StateOfOrigin, invoice.US_StateOfOrigin);
			invoice.US_StateOfOrigin = "NY";
			AssertNotEquals("US_StateOfOrigin_Effective", Declaration.US_StateOfOrigin, invoice.US_StateOfOrigin);
			AssertEquals("US_StateOfOrigin_Effective", invoice.US_StateOfOrigin, invoice.US_StateOfOrigin);
			invoice.US_StateOfOrigin = "";
			AssertEquals("US_StateOfOrigin_Effective", Declaration.US_StateOfOrigin, invoice.US_StateOfOrigin);
		}

		public void TestDefaultUS_StateOfOrigin()
		{
			var invoice = Declaration.Invoices.AddNew();
			var pickupAddress = invoice.SupplierPickupAddress;
			pickupAddress.E2_AddressOverride = true;
			pickupAddress.E2_RN_NKCountryCode = "US";
			pickupAddress.E2_State = "TN";
			AssertEquals("TN", invoice.US_StateOfOrigin);
		}

		public void TestUS_TransactionsRelated_Effective()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			AssertEquals("US_IsTransactionsRelated", false, invoice.US_IsTransactionsRelated);
			Declaration.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			AssertEquals("US_TransactionsRelated_Effective", Declaration.US_TransactionsRelated, invoice.US_TransactionsRelated);
			AssertEquals("US_IsTransactionsRelated", false, invoice.US_IsTransactionsRelated);
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			AssertNotEquals("US_TransactionsRelated_Effective", Declaration.US_TransactionsRelated, invoice.US_TransactionsRelated);
			AssertEquals("US_TransactionsRelated_Effective", invoice.US_TransactionsRelated, invoice.US_TransactionsRelated);
			AssertEquals("US_IsTransactionsRelated", true, invoice.US_IsTransactionsRelated);
			invoice.US_TransactionsRelated = "";
			AssertEquals("US_TransactionsRelated_Effective", Declaration.US_TransactionsRelated, invoice.US_TransactionsRelated);
			AssertEquals("US_IsTransactionsRelated", false, invoice.US_IsTransactionsRelated);
		}

		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => JobDeclaration.New(Factory);

		JobDeclaration Declaration => (JobDeclaration)declaration;
	}
}
