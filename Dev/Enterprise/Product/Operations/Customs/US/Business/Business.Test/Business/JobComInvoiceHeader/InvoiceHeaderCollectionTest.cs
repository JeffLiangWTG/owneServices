using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	sealed class InvoiceHeaderCollectionTest : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
	{
		public void TestAddInvoiceChangesInBondRelatedRecords()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = ZBool.True;
			declaration.US_EnableAII = ZBool.False;
			var invoice = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Tariff = "1010101010";
			invoiceLine.JI_JZ = invoice.PK;
			var message = Factory.New<EDIMessage>();
			invoice.Messages.Add(message);
			var inBondRelatedRecords = declaration.InBondRelatedRecords;
			AssertEquals(0, inBondRelatedRecords.Count);
			declaration.Invoices.Add(invoice);
			AssertEquals(1, inBondRelatedRecords.Count);
			AssertNotNull(inBondRelatedRecords.GetElementWrapping(invoice));
		}

		public void TestHasPriorNoticeTariffs()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, declaration.Invoices.RequiresPriorNoticeReporting);
			invoice.JobComInvoiceLines.AddNew().JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			AssertEquals(true, declaration.Invoices.RequiresPriorNoticeReporting);
		}

		public void TestDefaultCurrencyForRecon()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			JobComInvoiceHeader invoice = reconDec.Invoices.AddNew();
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, invoice.JZ_RX_NKInvoice_Currency);
		}

		public void TestHasFDATariffsToBeDeclared()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, declaration.Invoices.HasFDATariffsToBeDeclared);
			JobComInvoiceLine invLine3 = invoice.JobComInvoiceLines.AddNew();
			invLine3.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invLine3.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, declaration.Invoices.HasFDATariffsToBeDeclared);
			invLine3.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, declaration.Invoices.HasFDATariffsToBeDeclared);
			invLine3.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			invLine3.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, declaration.Invoices.HasFDATariffsToBeDeclared);
			invLine3.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, declaration.Invoices.HasFDATariffsToBeDeclared);
			invLine3.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			invLine3.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, declaration.Invoices.HasFDATariffsToBeDeclared);
			invLine3.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, declaration.Invoices.HasFDATariffsToBeDeclared);
			invLine3.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invLine3.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, declaration.Invoices.HasFDATariffsToBeDeclared);
			invLine3.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, declaration.Invoices.HasFDATariffsToBeDeclared);
		}

		public void TestHasOGATariffsToBeDeclared()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, declaration.Invoices.HasOGATariffsToBeDeclared);
			JobComInvoiceLine invLine3 = invoice.JobComInvoiceLines.AddNew();
			invLine3.JI_Tariff = USCTariff.FCCApplicable;
			invLine3.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, declaration.Invoices.HasOGATariffsToBeDeclared);
			invLine3.US_FCCIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, declaration.Invoices.HasOGATariffsToBeDeclared);
			invLine3.JI_Tariff = USCTariff.FCCMayBeApplicable;
			invLine3.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, declaration.Invoices.HasOGATariffsToBeDeclared);
			invLine3.US_FCCIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, declaration.Invoices.HasOGATariffsToBeDeclared);
			invLine3.JI_Tariff = USCTariff.DOTIsApplicable;
			invLine3.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, declaration.Invoices.HasOGATariffsToBeDeclared);
			invLine3.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, declaration.Invoices.HasOGATariffsToBeDeclared);
		}

		public void TestReconIssues()
		{
			var declarationSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			var declarationImporter = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_OH_Importer = declarationImporter.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "USCHI";
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(ReconIssues.None, declaration.Invoices.ReconIssues.FirstOrDefault());
			var link1 = declarationImporter.SupplierLinks.AddNew(declarationSupplier);
			link1.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			var addInfo = link1.GetAddInfo();
			addInfo.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			var invoice2 = declaration.Invoices.AddNew();
			AssertEquals(ReconIssues.CL, declaration.Invoices.ReconIssues.FirstOrDefault());
			var invoiceSupplier = Factory.NewWithValidTestData<OrgHeader>();
			var invoiceImporter = Factory.NewWithValidTestData<OrgHeader>();
			var link2 = invoiceImporter.SupplierLinks.AddNew(invoiceSupplier);
			link2.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			var addInfo2 = link2.GetAddInfo();
			addInfo2.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			Factory.Save();
			invoice2.JZ_OH_Supplier = invoiceSupplier.PK;
			invoice2.JZ_OH_Buyer = invoiceImporter.PK;
			AssertEquals(ReconIssues.CL, declaration.Invoices.ReconIssues.ToArray()[0]);
			AssertEquals(ReconIssues.VL, declaration.Invoices.ReconIssues.ToArray()[1]);
		}

		public void TestUpdateExchangeRateOnExportDateChanged()
		{
			ZDateTime today = ZDateTime.Today;
			RefCurrency aUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			SetExchangeRate(today.AddDays(-10), today.AddDays(-8), 0.70m, aUDCurrency);
			SetExchangeRate(today.AddDays(-7), today.AddDays(-5), 0.69m, aUDCurrency);
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_ExportDate = today.AddDays(-9);
			JobComInvoiceHeader invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			AssertEquals("PreCondition:Exchange rate", 0.70m, invoice.JZ_InvoiceCurrLandedCostExRate);
			testDec.JE_ExportDate = today.AddDays(-6);
			AssertEquals("Exchange rate updated", 0.69m, invoice.JZ_InvoiceCurrLandedCostExRate);
		}

		public void TestIShouldUpdateScreeningStatus()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("ShouldUpdateScreeningStatus should be false", !((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			Assert("ShouldUpdateScreeningStatus", ((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus);
			invoice = declaration.Invoices.AddNew();
			((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus = false;
			declaration.Invoices.Delete(invoice);
			Assert("ShouldUpdateScreeningStatus", ((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus);
			var org = Factory.New<OrgHeader>();
			invoice = declaration.Invoices.AddNew();
			((IShouldUpdateScreeningStatus)declaration.Invoices).ShouldUpdateScreeningStatus = false;
			((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus = false;
			invoice.JZ_OH_Consignee = org.PK;
			Assert("ShouldUpdateScreeningStatus", ((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus);
			((IShouldUpdateScreeningStatus)invoice).ShouldUpdateScreeningStatus = false;
			((IShouldUpdateScreeningStatus)declaration.Invoices).ShouldUpdateScreeningStatus = false;
			((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus = false;
			invoice.US_USPPI.ZO_OH_Organisation = org.PK;
			Assert("ShouldUpdateScreeningStatus", ((IShouldUpdateScreeningStatus)invoice).ShouldUpdateScreeningStatus);
			Assert("ShouldUpdateScreeningStatus", ((IShouldUpdateScreeningStatus)declaration.Invoices).ShouldUpdateScreeningStatus);
			Assert("ShouldUpdateScreeningStatus", ((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus);
			((IShouldUpdateScreeningStatus)invoice).ShouldUpdateScreeningStatus = false;
			((IShouldUpdateScreeningStatus)declaration.Invoices).ShouldUpdateScreeningStatus = false;
			((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus = false;
			invoice.US_ExportUltimateConsignee.ZO_OH_Organisation = org.PK;
			Assert("ShouldUpdateScreeningStatus", ((IShouldUpdateScreeningStatus)invoice).ShouldUpdateScreeningStatus);
			Assert("ShouldUpdateScreeningStatus", ((IShouldUpdateScreeningStatus)declaration.Invoices).ShouldUpdateScreeningStatus);
			Assert("ShouldUpdateScreeningStatus", ((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus);
		}
	}
}
