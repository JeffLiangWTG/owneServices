using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobComInvoiceHeaderDeepCloneStrategyTest : Customs.Business.Testing.JobComInvoiceHeaderDeepCloneStrategyTest
	{
		[ExpectNoExceptions]
		public void TestCloneInvoiceLineStmNotes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_DeclarationGoodsDescription = "XXX";
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines[0].JI_DeclarationGoodsDescription, NUnit.Framework.Is.EqualTo("XXX").Using(CustomComparers.TypeComparison));
			var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines[0].JI_DeclarationGoodsDescription, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCloneInvoiceHeaderTW_MarksAndNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var aLength514 = new ZString('A', 514);
			invoice.TW_MarksAndNumbers = aLength514;
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec.Invoices[0].TW_MarksAndNumbers, NUnit.Framework.Is.EqualTo(aLength514));
			var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec2.Invoices[0].TW_MarksAndNumbers, NUnit.Framework.Is.EqualTo(aLength514));
		}

		[ExpectNoExceptions]
		public new void TestCloneJobDocAddresses()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "OrgTest1";
			var mainAddress = header.MainAddress;
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";

			var invoice = Factory.New<JobComInvoiceHeader>();
			var docAddress = invoice.DocAddresses.AddNew();
			docAddress.E2_AddressType = "SUD";
			docAddress.E2_Contact = "contact";
			docAddress.E2_OA_Address = mainAddress.PK;
			var clonedInvoice = (JobComInvoiceHeader)new JobComInvoiceHeaderDeepCloneStrategy(invoice, Customs.Business.CloneType.TemplateCopy).Clone();
			NUnit.Framework.Assert.That(clonedInvoice.DocAddresses.Count, NUnit.Framework.Is.EqualTo(1), "Copy DocAddresses");
			var clonedDocAddress = clonedInvoice.DocAddresses[0];
			NUnit.Framework.Assert.That(clonedDocAddress.E2_AddressType, NUnit.Framework.Is.EqualTo("SUD").Using(CustomComparers.TypeComparison), "Copied DocAddress E2_AddressType should be the same");
			NUnit.Framework.Assert.That(clonedDocAddress.E2_Contact, NUnit.Framework.Is.EqualTo("contact").Using(CustomComparers.TypeComparison), "Copied DocAddress E2_Contact should be the same");
			NUnit.Framework.Assert.That(clonedDocAddress.E2_OA_Address, NUnit.Framework.Is.EqualTo(mainAddress.PK), "Copied DocAddress E2_OA_Address should be the same");

			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.Add(invoice);
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			var clonedInvoiceFromDec = clonedDec.Invoices[0];
			NUnit.Framework.Assert.That(clonedInvoiceFromDec.DocAddresses.Count, NUnit.Framework.Is.EqualTo(1), "Copy DocAddresses");
			var clonedDocAddressFromDecInvoice = clonedInvoiceFromDec.DocAddresses[0];
			NUnit.Framework.Assert.That(clonedDocAddressFromDecInvoice.E2_AddressType, NUnit.Framework.Is.EqualTo("SUD").Using(CustomComparers.TypeComparison), "Copied DocAddress E2_AddressType should be the same");
			NUnit.Framework.Assert.That(clonedDocAddressFromDecInvoice.E2_Contact, NUnit.Framework.Is.EqualTo("contact").Using(CustomComparers.TypeComparison), "Copied DocAddress E2_Contact should be the same");
			NUnit.Framework.Assert.That(clonedDocAddressFromDecInvoice.E2_OA_Address, NUnit.Framework.Is.EqualTo(mainAddress.PK), "Copied DocAddress E2_OA_Address should be the same");
		}

		[ExpectNoExceptions]
		public new void TestCloneJobDocAddressesWithOverride()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "OrgTest1";
			var mainAddress = header.MainAddress;
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";

			var invoice = Factory.New<JobComInvoiceHeader>();
			var docAddress = invoice.DocAddresses.AddNew();
			docAddress.E2_AddressType = "SUD";
			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address1 = "Test override address1";
			docAddress.E2_Address2 = "Test override address2";
			var clonedInvoice = (JobComInvoiceHeader)new JobComInvoiceHeaderDeepCloneStrategy(invoice, Customs.Business.CloneType.TemplateCopy).Clone();
			NUnit.Framework.Assert.That(clonedInvoice.DocAddresses.Count, NUnit.Framework.Is.EqualTo(1), "Copy DocAddresses");
			var clonedDocAddress = clonedInvoice.DocAddresses[0];
			NUnit.Framework.Assert.That(clonedDocAddress.E2_AddressType, NUnit.Framework.Is.EqualTo("SUD").Using(CustomComparers.TypeComparison), "Copied DocAddress E2_AddressType should be the same");
			NUnit.Framework.Assert.That(clonedDocAddress.E2_Address1, NUnit.Framework.Is.EqualTo("Test override address1").Using(CustomComparers.TypeComparison), "Copied DocAddress E2_Address1 should be the same");
			NUnit.Framework.Assert.That(clonedDocAddress.E2_Address2, NUnit.Framework.Is.EqualTo("Test override address2").Using(CustomComparers.TypeComparison), "Copied DocAddress E2_Address2 should be the same");

			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.Add(invoice);
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			var clonedInvoiceFromDec = clonedDec.Invoices[0];
			NUnit.Framework.Assert.That(clonedInvoiceFromDec.DocAddresses.Count, NUnit.Framework.Is.EqualTo(1), "Copy DocAddresses");
			var clonedDocAddressFromDecInvoice = clonedInvoiceFromDec.DocAddresses[0];
			NUnit.Framework.Assert.That(clonedDocAddressFromDecInvoice.E2_AddressType, NUnit.Framework.Is.EqualTo("SUD").Using(CustomComparers.TypeComparison), "Copied DocAddress E2_AddressType should be the same");
			NUnit.Framework.Assert.That(clonedDocAddressFromDecInvoice.E2_Address1, NUnit.Framework.Is.EqualTo("Test override address1").Using(CustomComparers.TypeComparison), "Copied DocAddress E2_Address1 should be the same");
			NUnit.Framework.Assert.That(clonedDocAddressFromDecInvoice.E2_Address2, NUnit.Framework.Is.EqualTo("Test override address2").Using(CustomComparers.TypeComparison), "Copied DocAddress E2_Address2 should be the same");
		}

		[ExpectNoExceptions]
		public void TestCloneCusPackingList()
		{
			var expectedDesc = "Goods Description";
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_Description = expectedDesc;
			invoice.CreateCusPackingList(Factory);
			var clonedInvoice = (JobComInvoiceHeader)new JobComInvoiceHeaderDeepCloneStrategy(invoice, Customs.Business.CloneType.TemplateCopy).Clone();
			var cusPackingList2 = clonedInvoice.LoadCusPackingList(clonedInvoice.Factory);
			NUnit.Framework.Assert.That(cusPackingList2.CUL_Description, NUnit.Framework.Is.EqualTo(expectedDesc).Using(CustomComparers.TypeComparison), "Copied CusPackingList Description should be the same");
		}
	}
}
