using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DefaultSetterForInvoiceHeaderTest : TestCaseWithFactory
	{
		public void TestSetDefaultForDrawback()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_DeclarationReference = "DEC REF";
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("Invoice Number", "DEC REF", invoice.JZ_InvoiceNumber);
			AssertEquals("INCO Terms", "FOB", invoice.JZ_IncoTerm);
			AssertEquals("Invoice Number", "USD", invoice.JZ_RX_NKInvoice_Currency);
		}

		public void TestSetDefaultForExport()
		{
			var importer = Factory.New<OrgHeader>();
			importer.MainAddress.OA_Address1 = "IMP ADDRESS 1";
			importer.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			importer.OH_RL_NKClosestPort = "CA2CW";
			var impContact = importer.Contacts.AddNew();
			impContact.OC_ContactName = "IMP CONTACT";
			impContact.OC_Phone = "+61 (2) 9025 1100";
			var impDocument = impContact.Documents.AddNew();
			impDocument.OD_DocumentGroup = ContactType.All.ToString();
			impDocument.OD_DefaultContact = true;
			var supplier = Factory.New<OrgHeader>();
			supplier.MainAddress.OA_Address1 = "SUP ADDRESS 1";
			supplier.OH_RL_NKClosestPort = "US2CW";
			var supContact = supplier.Contacts.AddNew();
			supContact.OC_ContactName = "SUP CONTACT";
			supContact.OC_Phone = "+61 (2) 9025 5642";
			var supDocument = supContact.Documents.AddNew();
			supDocument.OD_DocumentGroup = ContactType.All.ToString();
			supDocument.OD_DefaultContact = true;
			var consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_Address1 = "CON ADDRESS 1";
			consignee.OH_RL_NKClosestPort = "US3CW";
			var conContact = consignee.Contacts.AddNew();
			conContact.OC_ContactName = "CON CONTACT";
			conContact.OC_Phone = "+61 (2) 9025 5643";
			var conDocument = conContact.Documents.AddNew();
			conDocument.OD_DocumentGroup = ContactType.All.ToString();
			conDocument.OD_DefaultContact = true;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Consignee = consignee.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(supplier.PK, invoice.US_USPPI.ZO_OH_Organisation);
			AssertEquals(supplier.MainAddress.PK, invoice.US_USPPI.ZO_OA_Address);
			AssertEquals("SUP CONTACT", invoice.US_USPPI.ZO_Contact);
			AssertEquals("290255642", invoice.US_USPPI.ZO_Phone);
			AssertEquals(importer.PK, invoice.US_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals(importer.MainAddress.PK, invoice.US_ExportUltimateConsignee.ZO_OA_Address);
			AssertEquals("IMP CONTACT", invoice.US_ExportUltimateConsignee.ZO_Contact);
			AssertEquals("61290251100", invoice.US_ExportUltimateConsignee.ZO_Phone);
			AssertEquals(consignee.PK, invoice.US_IntermediateConsignee.ZO_OH_Organisation);
			AssertEquals(consignee.MainAddress.PK, invoice.US_IntermediateConsignee.ZO_OA_Address);
			AssertEquals("CON CONTACT", invoice.US_IntermediateConsignee.ZO_Contact);
			AssertEquals("290255643", invoice.US_IntermediateConsignee.ZO_Phone);
			AssertEquals(supplier.PK, invoice.SupplierPickupAddress.Organisation.PK);
			AssertEquals(supplier.MainAddress.PK, invoice.SupplierPickupAddress.E2_OA_Address);
			declaration.JE_OH_Supplier = importer.PK;
			var invoice2 = declaration.Invoices.AddNew();
			AssertEquals(ZGuid.Empty, invoice2.SupplierPickupAddress.E2_OA_Address);
		}

		public void TestSetDefaultForImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			var uSD = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = uSD.RX_Code;
			CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("", invoice.JZ_RX_NKInvoice_Currency);
			invoice.Delete();
			CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			invoice = declaration.Invoices.AddNew();
			AssertEquals(uSD.RX_Code, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("AU", invoice.US_UC_NKCountryOfExport);
			AssertEquals("Transactions Related", "", invoice.US_TransactionsRelated);
			AssertEquals("First Sale", ZString.Empty, invoice.US_FirstSale);
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = "CATOR";
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var link1 = supplier.BuyerLinks.AddNew(importer);
			link1.OL_RelatedParty = "Y";
			link1.OL_RN_NKImporterCountry = "CA";
			var link2 = supplier.BuyerLinks.AddNew(importer);
			link2.OL_RelatedParty = "N";
			link2.OL_RN_NKImporterCountry = "US";
			link2.GetAddInfo().ZO_FirstSale = YesNoDefaultList.Codes.Yes;
			Factory.Save();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.Invoices.DeleteAll();
			var invoice2 = declaration.Invoices.AddNew();
			AssertEquals("Transactions Related - Use US Country", "N", invoice2.US_TransactionsRelated);
			AssertEquals("First Sale", YesNoDefaultList.Codes.Yes, invoice2.US_FirstSale);
			declaration.JE_RL_NKFinalDestination = "";
			importer.OH_RL_NKClosestPort = "MXXXX";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.DeleteAll();
			var invoice3 = declaration.Invoices.AddNew();
			AssertEquals("Transactions Related - Use importer's country. Link will be matched using Invoice>Branch.Company.GC_RN_NKCountryCode", "N", invoice3.US_TransactionsRelated);
			AssertEquals("First Sale", YesNoDefaultList.Codes.Yes, invoice3.US_FirstSale);
		}

		public void TestDefaultUS_UC_NKCountryOfExport()
		{
			USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("", declaration.US_UC_NKCountryOfExport);
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("", invoice.US_UC_NKCountryOfExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("AU", declaration.US_UC_NKCountryOfExport);
			invoice = declaration.Invoices.AddNew();
			AssertEquals("AU", invoice.US_UC_NKCountryOfExport);
		}
	}
}
