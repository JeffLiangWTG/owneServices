using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DefaultSetterForInvoiceHeader))]
	sealed class DefaultSetterForInvoiceHeaderTest : TestCaseWithFactory
	{
		public void TestExcludeColumns()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();

			var declaration = Factory.New<JobDeclaration>();
			var sourceSupplierAddress = declaration.SupplierDocumentaryAddress;
			sourceSupplierAddress.E2_AddressType = "UNK";
			sourceSupplierAddress.E2_AddressOverride = true;
			var sourceSupplierLocalAddress = sourceSupplierAddress.LocalAddress as TWJobDocAddress;
			sourceSupplierLocalAddress.E2_AddressType = "UNK";

			var sourceImporterAddress = declaration.ImporterDocumentaryAddress;
			sourceImporterAddress.E2_AddressType = "UNK";
			sourceImporterAddress.E2_AddressOverride = true;
			var sourceImporterLocalAddress = sourceImporterAddress.LocalAddress as TWJobDocAddress;
			sourceImporterLocalAddress.E2_AddressType = "UNK";

			var invoices = declaration.Invoices;
			var invoice = invoices.AddNew();
			CombineAssertions(() =>
			{
				var targetSupplierAddress = invoice.SupplierDocumentaryAddress;
				var targetSupplierLocalAddress = targetSupplierAddress.LocalAddress;
				var targetBuyerAddress = invoice.BuyerDocumentaryAddress;
				var targetBuyerLocalAddress = targetBuyerAddress.LocalAddress;

				AssertEquals("Supplier E2_AddressType", AutoDocAddressTypes.Codes.SupplierDocumentaryAddress, targetSupplierAddress.E2_AddressType);
				AssertEquals("Supplier Local E2_AddressType", AutoDocAddressTypes.Codes.SupplierTranslatedDocumentaryAddress, targetSupplierLocalAddress.E2_AddressType);

				AssertEquals("Buyer E2_AddressType", AutoDocAddressTypes.Codes.BuyerDocumentaryAddress, targetBuyerAddress.E2_AddressType);
				AssertEquals("Buyer Local E2_AddressType", AutoDocAddressTypes.Codes.BuyerTranslatedDocumentaryAddress, targetBuyerLocalAddress.E2_AddressType);
			});
		}

		public void TestDefaultsIncoTermFromJE_ShipmentIncoTermForFirstInvoiceLine()
		{
			var incoterms = new List<string>();
			incoterms.AddRange(IncoTerms.Incoterms2000);
			incoterms.AddRange(IncoTerms.Incoterms2010);
			incoterms.AddRange(IncoTerms.Incoterms2020);
			foreach (var incoterm in incoterms)
			{
				AssertDefaultsIncoTermFromJE_ShipmentIncoTermForFirstInvoiceLine(incoterm);
			}
		}

		void AssertDefaultsIncoTermFromJE_ShipmentIncoTermForFirstInvoiceLine(ZString incoterm)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ShipmentIncoTerm = incoterm;
			var invoices = declaration.Invoices;
			var invoice = invoices.AddNew();
			if (incoterm == IncoTerms.FreeCarrierSeller || incoterm == IncoTerms.FreeCarrierBuyer)
			{
				AssertEquals("IncoTerm should be", IncoTerms.FreeCarrier, invoice.JZ_IncoTerm);
			}
			else
			{
				AssertEquals("IncoTerm should be", incoterm, invoice.JZ_IncoTerm);
			}
		}

		public void TestDefaultsIncoTermFromJE_ShipmentIncoTermForSecondInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ShipmentIncoTerm = IncoTerms.FreeOnBoard;
			var invoices = declaration.Invoices;
			var invoice = invoices.AddNew();
			AssertEquals("IncoTerm should be", IncoTerms.FreeOnBoard, invoice.JZ_IncoTerm);
			invoice.JZ_IncoTerm = IncoTerms.ExWorks;

			invoice = invoices.AddNew();
			AssertEquals("IncoTerm should be", IncoTerms.FreeOnBoard, invoice.JZ_IncoTerm);
		}

		public void TestDefaultNoOfPacksForInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacks = 10;
			var invoices = declaration.Invoices;
			var invoice = invoices.AddNew();
			AssertEquals("JZ_NoOfPacks should be 10", 10m, invoice.JZ_NoOfPacks);
			invoice.JZ_NoOfPacks = 5;
			invoice = invoices.AddNew();
			AssertEquals("JZ_NoOfPacks should be 5", 5m, invoice.JZ_NoOfPacks);
			invoice.JZ_NoOfPacks = 2;
			invoice = invoices.AddNew();
			AssertEquals("JZ_NoOfPacks should be 3", 3m, invoice.JZ_NoOfPacks);
			invoice.JZ_NoOfPacks = 6;
			invoice = invoices.AddNew();
			AssertEquals("JZ_NoOfPacks should be 0", 0m, invoice.JZ_NoOfPacks);
		}

		public void TestDefaultSupplierDocAddressForInvoice()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();

			var declaration = Factory.New<JobDeclaration>();
			var declarationSupplier = declaration.SupplierDocumentaryAddress;
			declarationSupplier.OrganisationPK = header.PK;
			declarationSupplier.E2_OA_Address = header.Addresses[1].PK;
			declarationSupplier.E2_Contact = "Test Contact";
			var invoices = declaration.Invoices;
			var invoice = invoices.AddNew();
			CombineAssertions(() =>
			{
				var supplierDocumentaryAddress = invoice.SupplierDocumentaryAddress;
				AssertEquals("OrganisationPK should be defaulted", header.PK, supplierDocumentaryAddress.OrganisationPK);
				AssertEquals("E2_OA_Address should be defaulted", header.Addresses[1].PK, supplierDocumentaryAddress.E2_OA_Address);
				AssertEquals("E2_Contact should be defaulted", "Test Contact", supplierDocumentaryAddress.E2_Contact);
			});

			declarationSupplier.E2_AddressOverride = true;
			var sourceLocalAddress = declarationSupplier.LocalAddress as TWJobDocAddress;
			sourceLocalAddress.E2_CompanyName = "Local Company Name";
			sourceLocalAddress.E2_Address1 = "Local Address 1";
			sourceLocalAddress.E2_Address2 = "Local Address 2";
			sourceLocalAddress.E2_AdditionalAddressInformation = "Local Additional Address";
			sourceLocalAddress.E2_RN_NKCountryCode = "CA";
			sourceLocalAddress.E2_City = "Local City";
			sourceLocalAddress.E2_Postcode = "123";
			sourceLocalAddress.E2_State = "BC";

			declarationSupplier.E2_CompanyName = "Company Name";
			declarationSupplier.E2_Address1 = "Address 1";
			declarationSupplier.E2_Address2 = "Address 2";
			declarationSupplier.E2_AdditionalAddressInformation = "Additional Address";
			declarationSupplier.E2_RN_NKCountryCode = "AU";
			declarationSupplier.E2_City = "City";
			declarationSupplier.E2_Postcode = "789";
			declarationSupplier.E2_State = "NSW";

			declarationSupplier.E2_Contact = "Contact";
			declarationSupplier.E2_Email = "Email";
			declarationSupplier.E2_Phone = "Phone";
			declarationSupplier.E2_Fax = "Fax";
			declarationSupplier.IDCodeType = "PAS";
			declarationSupplier.IDCode = "234";
			declarationSupplier.AEOCode = "345";
			declarationSupplier.CBPCodeType = "CBF";
			declarationSupplier.CBPCode = "456";
			declarationSupplier.TPCCode = "567";

			invoice = invoices.AddNew();
			CombineAssertions(() =>
			{
				var supplierDocumentaryAddress = invoice.SupplierDocumentaryAddress;
				var localAddress = supplierDocumentaryAddress.LocalAddress;
				AssertEquals("Local Company Name", localAddress.E2_CompanyName);
				AssertEquals("Local Address 1", localAddress.E2_Address1);
				AssertEquals("Local Address 2", localAddress.E2_Address2);
				AssertEquals("Local Additional Address", localAddress.E2_AdditionalAddressInformation);
				AssertEquals("CA", localAddress.E2_RN_NKCountryCode);
				AssertEquals("Local City", localAddress.E2_City);
				AssertEquals("123", localAddress.E2_Postcode);
				AssertEquals("BC", localAddress.E2_State);

				AssertEquals("Company Name", supplierDocumentaryAddress.E2_CompanyName);
				AssertEquals("Address 1", supplierDocumentaryAddress.E2_Address1);
				AssertEquals("Address 2", supplierDocumentaryAddress.E2_Address2);
				AssertEquals("Additional Address", supplierDocumentaryAddress.E2_AdditionalAddressInformation);
				AssertEquals("AU", supplierDocumentaryAddress.E2_RN_NKCountryCode);
				AssertEquals("City", supplierDocumentaryAddress.E2_City);
				AssertEquals("789", supplierDocumentaryAddress.E2_Postcode);
				AssertEquals("NSW", supplierDocumentaryAddress.E2_State);

				AssertEquals("Contact", supplierDocumentaryAddress.E2_Contact);
				AssertEquals("Email", supplierDocumentaryAddress.E2_Email);
				AssertEquals("Phone", supplierDocumentaryAddress.E2_Phone);
				AssertEquals("Fax", supplierDocumentaryAddress.E2_Fax);
				AssertEquals("PAS", supplierDocumentaryAddress.IDCodeType);
				AssertEquals("234", supplierDocumentaryAddress.IDCode);
				AssertEquals("345", supplierDocumentaryAddress.AEOCode);
				AssertEquals("CBF", supplierDocumentaryAddress.CBPCodeType);
				AssertEquals("456", supplierDocumentaryAddress.CBPCode);
				AssertEquals("567", supplierDocumentaryAddress.TPCCode);
			});
		}

		[ExpectNoExceptions]
		public void TestDefaultDocumentaryAddressFromSource_WhenSourceDocAddressLocalAddressIsNull()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacks = 10;
			var invoices = declaration.Invoices;
			var invoice = invoices.AddNew();
			var sourceDocAddress = Factory.New<TWJobDocAddress>();
			sourceDocAddress.E2_AddressOverride = true;
			var targetDocAddress = declaration.ImporterDocumentaryAddress;
			targetDocAddress.OrganisationPK = header.PK;

			var defaultSetterForInvoiceHeaderObj = new DefaultSetterForInvoiceHeader(invoice, declaration);
			var methodInfo = typeof(DefaultSetterForInvoiceHeader).GetMethod("DefaultDocumentaryAddressFromSource", BindingFlags.NonPublic | BindingFlags.Instance);
			methodInfo.Invoke(defaultSetterForInvoiceHeaderObj, new object[] { sourceDocAddress, targetDocAddress });
		}

		[ExpectNoExceptions]
		public void TestDefaultDocumentaryAddressFromSource_WhenTargetDocAddressLocalAddressIsNull()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacks = 10;
			var invoices = declaration.Invoices;
			var invoice = invoices.AddNew();
			var sourceDocAddress = Factory.New<TWJobDocAddress>();
			sourceDocAddress.E2_AddressOverride = true;
			var targetDocAddress = Factory.New<TWJobDocAddress>();

			var defaultSetterForInvoiceHeaderObj = new DefaultSetterForInvoiceHeader(invoice, declaration);
			var methodInfo = typeof(DefaultSetterForInvoiceHeader).GetMethod("DefaultDocumentaryAddressFromSource", BindingFlags.NonPublic | BindingFlags.Instance);
			methodInfo.Invoke(defaultSetterForInvoiceHeaderObj, new object[] { sourceDocAddress, targetDocAddress });
		}

		public void TestDefaultBuyerDocAddressForInvoice()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();

			var declaration = Factory.New<JobDeclaration>();
			var declarationImporter = declaration.ImporterDocumentaryAddress;
			declarationImporter.OrganisationPK = header.PK;
			declarationImporter.E2_OA_Address = header.Addresses[1].PK;
			declarationImporter.E2_Contact = "Test Contact";
			var invoices = declaration.Invoices;
			var invoice = invoices.AddNew();
			CombineAssertions(() =>
			{
				var buyerDocumentaryAddress = invoice.BuyerDocumentaryAddress;
				AssertEquals("OrganisationPK should be defaulted", header.PK, buyerDocumentaryAddress.OrganisationPK);
				AssertEquals("E2_OA_Address should be defaulted", header.Addresses[1].PK, buyerDocumentaryAddress.E2_OA_Address);
				AssertEquals("E2_Contact should be defaulted", "Test Contact", buyerDocumentaryAddress.E2_Contact);
			});

			declarationImporter.E2_AddressOverride = true;
			var sourceLocalAddress = declarationImporter.LocalAddress as TWJobDocAddress;
			sourceLocalAddress.E2_CompanyName = "Local Company Name";
			sourceLocalAddress.E2_Address1 = "Local Address 1";
			sourceLocalAddress.E2_Address2 = "Local Address 2";
			sourceLocalAddress.E2_AdditionalAddressInformation = "Local Additional Address";
			sourceLocalAddress.E2_RN_NKCountryCode = "CA";
			sourceLocalAddress.E2_City = "Local City";
			sourceLocalAddress.E2_Postcode = "123";
			sourceLocalAddress.E2_State = "BC";

			declarationImporter.E2_CompanyName = "Company Name";
			declarationImporter.E2_Address1 = "Address 1";
			declarationImporter.E2_Address2 = "Address 2";
			declarationImporter.E2_AdditionalAddressInformation = "Additional Address";
			declarationImporter.E2_RN_NKCountryCode = "AU";
			declarationImporter.E2_City = "City";
			declarationImporter.E2_Postcode = "789";
			declarationImporter.E2_State = "NSW";

			declarationImporter.E2_Contact = "Contact";
			declarationImporter.E2_Email = "Email";
			declarationImporter.E2_Phone = "Phone";
			declarationImporter.E2_Fax = "Fax";
			declarationImporter.IDCodeType = "PAS";
			declarationImporter.IDCode = "234";
			declarationImporter.AEOCode = "345";
			declarationImporter.CBPCodeType = "CBF";
			declarationImporter.CBPCode = "456";
			declarationImporter.TPCCode = "567";

			invoice = invoices.AddNew();
			CombineAssertions(() =>
			{
				var buyerDocumentaryAddress = invoice.BuyerDocumentaryAddress;
				var localAddress = buyerDocumentaryAddress.LocalAddress;
				AssertEquals("Local Company Name", localAddress.E2_CompanyName);
				AssertEquals("Local Address 1", localAddress.E2_Address1);
				AssertEquals("Local Address 2", localAddress.E2_Address2);
				AssertEquals("Local Additional Address", localAddress.E2_AdditionalAddressInformation);
				AssertEquals("CA", localAddress.E2_RN_NKCountryCode);
				AssertEquals("Local City", localAddress.E2_City);
				AssertEquals("123", localAddress.E2_Postcode);
				AssertEquals("BC", localAddress.E2_State);

				AssertEquals("Company Name", buyerDocumentaryAddress.E2_CompanyName);
				AssertEquals("Address 1", buyerDocumentaryAddress.E2_Address1);
				AssertEquals("Address 2", buyerDocumentaryAddress.E2_Address2);
				AssertEquals("Additional Address", buyerDocumentaryAddress.E2_AdditionalAddressInformation);
				AssertEquals("AU", buyerDocumentaryAddress.E2_RN_NKCountryCode);
				AssertEquals("City", buyerDocumentaryAddress.E2_City);
				AssertEquals("789", buyerDocumentaryAddress.E2_Postcode);
				AssertEquals("NSW", buyerDocumentaryAddress.E2_State);

				AssertEquals("Contact", buyerDocumentaryAddress.E2_Contact);
				AssertEquals("Email", buyerDocumentaryAddress.E2_Email);
				AssertEquals("Phone", buyerDocumentaryAddress.E2_Phone);
				AssertEquals("Fax", buyerDocumentaryAddress.E2_Fax);
				AssertEquals("PAS", buyerDocumentaryAddress.IDCodeType);
				AssertEquals("234", buyerDocumentaryAddress.IDCode);
				AssertEquals("345", buyerDocumentaryAddress.AEOCode);
				AssertEquals("CBF", buyerDocumentaryAddress.CBPCodeType);
				AssertEquals("456", buyerDocumentaryAddress.CBPCode);
				AssertEquals("567", buyerDocumentaryAddress.TPCCode);
			});
		}

		public void TestDefaultRelatedIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RelatedIndicator = RelationshipIndicatorList.Codes.RelationshipIndicator138;
			var invoice2 = declaration.Invoices.AddNew();
			AssertEquals(RelationshipIndicatorList.Codes.RelationshipIndicator138, invoice2.JZ_RelatedIndicator);

			invoice2.JZ_RelatedIndicator = RelationshipIndicatorList.Codes.RelationshipIndicatorNoEffect;
			var invoice3 = declaration.Invoices.AddNew();
			AssertEquals(RelationshipIndicatorList.Codes.RelationshipIndicatorNoEffect, invoice3.JZ_RelatedIndicator);
		}

		public void TestDefaultJZ_Description()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GoodsDescription = "Test Goods Description";
			var invoices = declaration.Invoices;
			var invoice = invoices.AddNew();
			AssertEquals("JZ_Description should be ", "Test Goods Description", invoice.JZ_Description);
		}
	}
}
