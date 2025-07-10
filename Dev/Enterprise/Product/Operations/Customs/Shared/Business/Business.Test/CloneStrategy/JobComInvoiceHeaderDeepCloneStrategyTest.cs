using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public class JobComInvoiceHeaderDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCloneInvoice()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_MasterBill = "MB123";
			Bill bill = testDec.PrimaryMasterBill;
			BaseJobComInvoiceGroupHeader group = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;
			invoice.JZ_InvoiceNumber = "~~~111~~~";
			invoice.Charges.AddNew();
			invoice.GroupCharges.AddNew();
			invoice.GroupCharges.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			BaseJobDeclaration clonedDec = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(testDec, CloneType.TemplateCopy).Clone();
			BaseJobComInvoiceGroupHeader clonedGroup = clonedDec.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceHeader clonedInvoice = clonedDec.Invoices[0];
			AssertEquals("Copied JZ_JE", clonedDec.PK, clonedInvoice.JZ_JE);
			AssertEquals("Not copied bills", 0, clonedDec.Bills.Count);
			AssertEquals("Copied JZ_JZ_GroupInvoiceFK", clonedGroup.PK, clonedInvoice.JZ_JZ_GroupInvoiceFK);
			AssertEquals("No HasChanges", false, clonedInvoice.HasChanges);
			AssertEquals("one charge", 1, clonedInvoice.Charges.Count);
			AssertEquals("Apportioned charges not cloned", 0, clonedInvoice.GroupCharges.Count);
			AssertEquals("Three invoice lines", 3, clonedInvoice.JobComInvoiceLines.Count);
			AssertEquals("invoicenumber is copied", invoice.JZ_InvoiceNumber, clonedInvoice.JZ_InvoiceNumber);
			AssertEquals("ClonedDec has the cloned invoice", true, clonedDec.Invoices.Contains(clonedInvoice));

			clonedDec = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(testDec, CloneType.CountryToCountryCopy).Clone();
			clonedGroup = clonedDec.JobComInvoiceGroupHeaders[0];

			clonedInvoice = clonedDec.Invoices[0];
			AssertEquals("Copied JZ_JE", clonedDec.PK, clonedInvoice.JZ_JE);
			AssertEquals("Not copied JZ_CU_RelatedHouseBill, but mapped to a new cloned bill", clonedDec.PrimaryMasterBill.PK, clonedInvoice.JZ_CU_RelatedHouseBill);
			AssertEquals("Copied JZ_JZ_GroupInvoiceFK", clonedGroup.PK, clonedInvoice.JZ_JZ_GroupInvoiceFK);
			AssertEquals("No HasChanges", false, clonedInvoice.HasChanges);
			AssertEquals("one charge", 1, clonedInvoice.Charges.Count);
			AssertEquals("Apportioned charges not cloned", 0, clonedInvoice.GroupCharges.Count);
			AssertEquals("Three invoice lines", 3, clonedInvoice.JobComInvoiceLines.Count);
			AssertEquals("invoicenumber is copied", invoice.JZ_InvoiceNumber, clonedInvoice.JZ_InvoiceNumber);
			AssertEquals("ClonedDec has the cloned invoice", true, clonedDec.Invoices.Contains(clonedInvoice));
		}

		public void TestCloneInvoiceLine_CI_ParentID()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			BaseJobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_OrderNumber = "2";

			BaseJobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_OrderNumber = "3";

			BaseJobComInvoiceLine invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_OrderNumber = "4";

			BaseJobComInvoiceLine invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_OrderNumber = "5";

			BaseJobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_OrderNumber = "1";

			invoiceLine3.JI_ParentID = invoiceLine1.PK;
			invoiceLine5.JI_ParentID = invoiceLine1.PK;

			BaseJobDeclaration clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals(1, clonedDeclaration.Invoices.Count);
			AssertEquals(5, clonedDeclaration.InvoiceLines.Count);

			ZQuery orderNumberQuery = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, "1");
			BaseJobComInvoiceLine[] invoiceLines = (BaseJobComInvoiceLine[])clonedDeclaration.Invoices[0].JobComInvoiceLines.Find(orderNumberQuery);
			AssertEquals(true, invoiceLines[0].JI_ParentID.IsEmpty);
			ZGuid invoice1Pk = invoiceLines[0].PK;

			orderNumberQuery = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, "2");
			invoiceLines = (BaseJobComInvoiceLine[])clonedDeclaration.Invoices[0].JobComInvoiceLines.Find(orderNumberQuery);
			AssertEquals(true, invoiceLines[0].JI_ParentID.IsEmpty);

			orderNumberQuery = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, "3");
			invoiceLines = (BaseJobComInvoiceLine[])clonedDeclaration.Invoices[0].JobComInvoiceLines.Find(orderNumberQuery);
			AssertEquals(invoice1Pk, invoiceLines[0].JI_ParentID);

			orderNumberQuery = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, "4");
			invoiceLines = (BaseJobComInvoiceLine[])clonedDeclaration.Invoices[0].JobComInvoiceLines.Find(orderNumberQuery);
			AssertEquals(true, invoiceLines[0].JI_ParentID.IsEmpty);

			orderNumberQuery = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, "5");
			invoiceLines = (BaseJobComInvoiceLine[])clonedDeclaration.Invoices[0].JobComInvoiceLines.Find(orderNumberQuery);
			AssertEquals(invoice1Pk, invoiceLines[0].JI_ParentID);
		}

		public void TestCloneInvoiceLine_JI_LineNO_ShouldKeepUnchanged()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			BaseJobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_OrderNumber = "3";

			BaseJobComInvoiceLine invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_OrderNumber = "5";

			BaseJobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_OrderNumber = "1";

			BaseJobComInvoiceLine invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_OrderNumber = "4";

			BaseJobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_OrderNumber = "2";

			invoice.JobComInvoiceLines.Sort("JI_OrderNumber");

			BaseJobDeclaration clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals(1, clonedDeclaration.Invoices.Count);
			AssertEquals(5, clonedDeclaration.InvoiceLines.Count);

			ZQuery orderNumberQuery = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, "3");
			BaseJobComInvoiceLine[] invoiceLines = (BaseJobComInvoiceLine[])clonedDeclaration.Invoices[0].JobComInvoiceLines.Find(orderNumberQuery);
			AssertEquals((short)1, invoiceLines[0].JI_LineNo);

			orderNumberQuery = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, "5");
			invoiceLines = (BaseJobComInvoiceLine[])clonedDeclaration.Invoices[0].JobComInvoiceLines.Find(orderNumberQuery);
			AssertEquals((short)2, invoiceLines[0].JI_LineNo);

			orderNumberQuery = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, "1");
			invoiceLines = (BaseJobComInvoiceLine[])clonedDeclaration.Invoices[0].JobComInvoiceLines.Find(orderNumberQuery);
			AssertEquals((short)3, invoiceLines[0].JI_LineNo);

			orderNumberQuery = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, "4");
			invoiceLines = (BaseJobComInvoiceLine[])clonedDeclaration.Invoices[0].JobComInvoiceLines.Find(orderNumberQuery);
			AssertEquals((short)4, invoiceLines[0].JI_LineNo);

			orderNumberQuery = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, "2");
			invoiceLines = (BaseJobComInvoiceLine[])clonedDeclaration.Invoices[0].JobComInvoiceLines.Find(orderNumberQuery);
			AssertEquals((short)5, invoiceLines[0].JI_LineNo);
		}

		public void TestCloneJobDocAddresses()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "OrgTest1";
			var mainAddress = header.MainAddress;
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";

			var invoice1 = Factory.New<BaseJobComInvoiceHeader>();
			var docAddress1 = invoice1.DocAddresses.AddNew();
			docAddress1.E2_AddressType = "SUD";
			docAddress1.E2_Contact = "contact";
			docAddress1.E2_OA_Address = mainAddress.PK;
			var clonedInvoice1 = (BaseJobComInvoiceHeader)new JobComInvoiceHeaderDeepCopyStrategy(invoice1, CloneType.TemplateCopy).Clone();
			AssertEquals("Copy DocAddresses", 0, clonedInvoice1.DocAddresses.Count);

			var invoice2 = Factory.New<JobComInvoiceHeaderWithSupportedAddressTypesForTesting>();
			var docAddress2 = invoice2.DocAddresses.AddNew();
			docAddress2.E2_AddressType = "SUD";
			docAddress2.E2_Contact = "contact";
			docAddress2.E2_OA_Address = mainAddress.PK;

			var clonedInvoice2 = (JobComInvoiceHeaderWithSupportedAddressTypesForTesting)new JobComInvoiceHeaderDeepCopyStrategy(invoice2, CloneType.TemplateCopy).Clone();
			AssertEquals("Copy DocAddresses", 1, clonedInvoice2.DocAddresses.Count);
			var clonedDocAddress = clonedInvoice2.DocAddresses[0];
			AssertEquals("Copied DocAddress E2_AddressType should be the same", "SUD", clonedDocAddress.E2_AddressType);
			AssertEquals("Copied DocAddress E2_Contact should be the same", "contact", clonedDocAddress.E2_Contact);
			AssertEquals("Copied DocAddress E2_OA_Address should be the same", mainAddress.PK, clonedDocAddress.E2_OA_Address);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.Invoices.Add(invoice2);
			var clonedDec = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			var clonedInvoiceFromDec = clonedDec.Invoices[0];
			AssertEquals("Copy DocAddresses", 1, clonedInvoiceFromDec.DocAddresses.Count);
			var clonedDocAddressFromDecInvoice = clonedInvoiceFromDec.DocAddresses[0];
			AssertEquals("Copied DocAddress E2_AddressType should be the same", "SUD", clonedDocAddressFromDecInvoice.E2_AddressType);
			AssertEquals("Copied DocAddress E2_Contact should be the same", "contact", clonedDocAddressFromDecInvoice.E2_Contact);
			AssertEquals("Copied DocAddress E2_OA_Address should be the same", mainAddress.PK, clonedDocAddressFromDecInvoice.E2_OA_Address);
		}

		public void TestCloneJobDocAddressesWithOverride()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "OrgTest1";
			var mainAddress = header.MainAddress;
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";

			var invoice1 = Factory.New<BaseJobComInvoiceHeader>();
			var docAddress1 = invoice1.DocAddresses.AddNew();
			docAddress1.E2_AddressType = "SUD";
			docAddress1.E2_AddressOverride = true;
			docAddress1.E2_Address1 = "Test override address1";
			docAddress1.E2_Address2 = "Test override address2";
			var clonedInvoice1 = (BaseJobComInvoiceHeader)new JobComInvoiceHeaderDeepCopyStrategy(invoice1, CloneType.TemplateCopy).Clone();
			AssertEquals("Copy DocAddresses", 0, clonedInvoice1.DocAddresses.Count);

			var invoice2 = Factory.New<JobComInvoiceHeaderWithSupportedAddressTypesForTesting>();
			var docAddress2 = invoice2.DocAddresses.AddNew();
			docAddress2.E2_AddressType = "SUD";
			docAddress2.E2_AddressOverride = true;
			docAddress2.E2_Address1 = "Test override address1";
			docAddress2.E2_Address2 = "Test override address2";

			var clonedInvoice2 = (JobComInvoiceHeaderWithSupportedAddressTypesForTesting)new JobComInvoiceHeaderDeepCopyStrategy(invoice2, CloneType.TemplateCopy).Clone();
			AssertEquals("Copy DocAddresses", 1, clonedInvoice2.DocAddresses.Count);
			var clonedDocAddress = clonedInvoice2.DocAddresses[0];
			AssertEquals("Copied DocAddress E2_AddressType should be the same", "SUD", clonedDocAddress.E2_AddressType);
			AssertEquals("Copied DocAddress E2_Address1 should be the same", "Test override address1", clonedDocAddress.E2_Address1);
			AssertEquals("Copied DocAddress E2_Address2 should be the same", "Test override address2", clonedDocAddress.E2_Address2);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.Invoices.Add(invoice2);
			var clonedDec = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			var clonedInvoiceFromDec = clonedDec.Invoices[0];
			AssertEquals("Copy DocAddresses", 1, clonedInvoiceFromDec.DocAddresses.Count);
			var clonedDocAddressFromDecInvoice = clonedInvoiceFromDec.DocAddresses[0];
			AssertEquals("Copied DocAddress E2_AddressType should be the same", "SUD", clonedDocAddressFromDecInvoice.E2_AddressType);
			AssertEquals("Copied DocAddress E2_Address1 should be the same", "Test override address1", clonedDocAddressFromDecInvoice.E2_Address1);
			AssertEquals("Copied DocAddress E2_Address2 should be the same", "Test override address2", clonedDocAddressFromDecInvoice.E2_Address2);
		}

		public void TestCloneInvoice_SuspendValidationAndSettingHasChanges()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "~~~111~~~";
			invoice.JobComInvoiceLines.AddNew();

			var clonedInvoice = new JobComInvoiceHeaderDeepCopyStrategy(invoice, CloneType.TemplateCopy).Clone();
			Assert("Validation should be suspended", !clonedInvoice.HasNotifications());
			Assert("Setting HasChanges should be suspended", !clonedInvoice.HasChanges);
		}

		class JobComInvoiceHeaderWithSupportedAddressTypesForTesting : BaseJobComInvoiceHeader
		{
			public JobComInvoiceHeaderWithSupportedAddressTypesForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override DocAddressType[] SupportedAddressTypesCore()
			{
				return new DocAddressType[] { DocAddressType.SupplierDocumentaryAddress };
			}
		}
	}
}
