using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using EntryCreationStrategy = Enterprise.Customs.TR.Business.Declaration.EntryCreationStrategy;
using LineMerger = Enterprise.Customs.TR.Business.Declaration.LineMerger;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class EntryCreationStrategyTest : EU.Business.Declaration.Testing.EntryCreationStrategyTest
	{
		public void TestMergeKeyForLine()
		{
			invoiceLine.JI_ZZF_NKTaxType = "KD8";
			invoiceLine.JI_NDescription = "Test Descripton for TR";
			invoiceLine.JI_CustomsFifthQuantity = 4;
			invoiceLine.JI_CustomsFifthUnitQty = "KGM";
			invoiceLine.ZG_ReturnToOrigin = true;
			invoiceLine.ZG_SecondaryTreatedProduct = false;
			invoiceLine.ZG_InwardProcessingLicenseLineNumber = "45";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;

			var key = entryCreationStrategy.GetKeyForLine(invoiceLine);
			var actualKeys = key.Keys;
			var expectedKeys = new IZType[]
			{
				new ZString("KD8"),
				new ZString("KGM"),
				new ZBool(true),
				new ZBool(false),
				new ZString("45"),
				new ZString("Test Descripton for TR")
			};

			CombineAssertions("MergeKeys for Line TariffAndDescription", () =>
			{
				Assert(key.Contains(invoiceLine.JI_ZZF_NKTaxType));
				Assert(key.Contains(invoiceLine.JI_NDescription));
				Assert(key.Contains(invoiceLine.ZG_ReturnToOrigin));
				Assert(key.Contains(invoiceLine.ZG_SecondaryTreatedProduct));
				Assert(key.Contains(invoiceLine.ZG_InwardProcessingLicenseLineNumber));
				Assert(key.Contains(invoiceLine.JI_CustomsFifthUnitQty));

				AssertArrayEqualsByElements(expectedKeys, actualKeys.Skip(actualKeys.Count - expectedKeys.Length).ToArray());
			});

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			key = entryCreationStrategy.GetKeyForLine(invoiceLine);

			CombineAssertions("MergeKeys for Line Tariff", () =>
			{
				Assert(key.Contains(invoiceLine.JI_ZZF_NKTaxType));
				Assert(!key.Contains(invoiceLine.JI_NDescription));
				Assert(key.Contains(invoiceLine.ZG_ReturnToOrigin));
				Assert(key.Contains(invoiceLine.ZG_SecondaryTreatedProduct));
				Assert(key.Contains(invoiceLine.ZG_InwardProcessingLicenseLineNumber));
				Assert(key.Contains(invoiceLine.JI_CustomsFifthUnitQty));

				AssertArrayEqualsByElements(expectedKeys, actualKeys.Skip(actualKeys.Count - expectedKeys.Length).ToArray());
			});
		}

		public void TestInvoiceLinesMerge()
		{
			var declaration = GetJobDeclarationForTest();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var supportingDocuments1 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocuments1.CSI_Code = "0100";
			supportingDocuments1.CSI_Status = "V";
			supportingDocuments1.CSI_DateOfIssue = new ZDateTime(2021, 2, 24);
			supportingDocuments1.CSI_ReferenceNumber = "544554";

			var supportingDocuments2 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocuments2.CSI_Code = "0200";
			supportingDocuments2.CSI_Status = "Y";
			supportingDocuments2.CSI_DateOfIssue = new ZDateTime(2021, 2, 25);
			supportingDocuments2.CSI_ReferenceNumber = "666666";

			var supportingDocuments3 = invoiceLine2.SupportingDocuments.AddNew();
			supportingDocuments3.CSI_Code = "0100";
			supportingDocuments3.CSI_Status = "V";
			supportingDocuments3.CSI_DateOfIssue = new ZDateTime(2021, 2, 24);
			supportingDocuments3.CSI_ReferenceNumber = "544554";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var strategy = declaration.CreateEntryCreationStrategy();

			CombineAssertions(() =>
			{
				invoiceLine1.JI_NDescription = "N1";
				invoiceLine2.JI_NDescription = "N1";
				AssertEquals("When the JI_NDescription same keys should be same", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

				invoiceLine2.JI_NDescription = "N2";
				AssertEquals("When the JI_NDescription different keys should be same merge by Tariff", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
				invoiceLine2.JI_NDescription = "N2";
				AssertNotEquals("When the JI_NDescription different keys should be different merge by TariffAndDescription", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				invoiceLine2.JI_NDescription = "N1";
				invoiceLine1.JI_ZZF_NKTaxType = "KD10";
				invoiceLine2.JI_ZZF_NKTaxType = "KD10";
				AssertEquals("When the JI_ZZF_NKTaxType keys should be same", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

				invoiceLine2.JI_ZZF_NKTaxType = "KD20";
				AssertNotEquals("When the JI_ZZF_NKTaxType different keys should be different", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

				invoiceLine2.JI_ZZF_NKTaxType = "KD10";
				invoiceLine1.JI_CustomsFifthQuantity = 4;
				invoiceLine1.JI_CustomsFifthUnitQty = "KGM";
				invoiceLine2.JI_CustomsFifthQuantity = 5;
				invoiceLine2.JI_CustomsFifthUnitQty = "KGM";
				AssertEquals("When the JI_CustomsFifthQuantity keys should be same ", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

				invoiceLine2.JI_CustomsFifthUnitQty = "KGX";
				AssertNotEquals("When the JI_CustomsFifthQuantity different keys should be different", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

				invoiceLine2.JI_CustomsFifthUnitQty = "KGM";
				invoiceLine1.ZG_ReturnToOrigin = true;
				invoiceLine2.ZG_ReturnToOrigin = true;
				AssertEquals("When the ZG_ReturnToOrigin keys should be same ", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

				invoiceLine2.ZG_ReturnToOrigin = false;
				AssertNotEquals("When the ZG_ReturnToOrigin different keys should be different", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

				invoiceLine2.ZG_ReturnToOrigin = true;
				invoiceLine1.ZG_SecondaryTreatedProduct = true;
				invoiceLine2.ZG_SecondaryTreatedProduct = true;
				AssertEquals("When the ZG_SecondaryTreatedProduct keys should be same ", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

				invoiceLine2.ZG_SecondaryTreatedProduct = false;
				AssertNotEquals("When the ZG_SecondaryTreatedProduct different keys should be different", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

				invoiceLine2.ZG_SecondaryTreatedProduct = true;
				invoiceLine1.ZG_InwardProcessingLicenseLineNumber = "45";
				invoiceLine2.ZG_InwardProcessingLicenseLineNumber = "45";
				AssertEquals("When the ZG_InwardProcessingLicenseLineNumber keys should be same ", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

				invoiceLine2.ZG_InwardProcessingLicenseLineNumber = "44";
				AssertNotEquals("When the ZG_InwardProcessingLicenseLineNumber different keys should be different", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));
			});
		}

		public void TestMergeTariffAndDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			var invLine1 = invoice.JobComInvoiceLines.AddNew();
			var invLine2 = invoice.JobComInvoiceLines.AddNew();

			invLine1.JI_Tariff = "001";
			invLine1.JI_Description = "English Description";
			invLine1.JI_NDescription = "Turkish Description";

			invLine2.JI_Tariff = "001";
			invLine2.JI_Description = "English Description";
			invLine2.JI_NDescription = "Turkish Description";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CombineAssertions(() =>
			{
				AssertEquals("1 Expected CusEntryHeader after merge", 1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("1 Expected CusEntryLines after merge", 1, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);

				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				invLine2.JI_NDescription = "Turkish Description 2";

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

				AssertEquals("1 Expected CusEntryHeader after merge", 1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("1 Expected CusEntryLines after merge", 1, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);

				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

				AssertEquals("1 Expected CusEntryHeader after merge", 1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("2 Expected CusEntryLines after merge", 2, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);

				var invLine3 = invoice.JobComInvoiceLines.AddNew();
				var invLine4 = invoice.JobComInvoiceLines.AddNew();

				invLine3.JI_Tariff = "002";
				invLine3.JI_Description = "English Description";
				invLine3.JI_NDescription = "Turkish Description 3";

				invLine4.JI_Tariff = "002";
				invLine4.JI_Description = "English Description";
				invLine4.JI_NDescription = "Turkish Description 4";

				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

				AssertEquals("1 Expected CusEntryHeader after merge", 1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("4 Expected CusEntryLines after merge", 4, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);

				invLine4.JI_NDescription = "Turkish Description 3";

				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

				AssertEquals("1 Expected CusEntryHeader after merge", 1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("3 Expected CusEntryLines after merge", 3, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);

				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

				AssertEquals("1 Expected CusEntryHeader after merge", 1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("2 Expected CusEntryLines after merge", 2, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);
			});
		}

		public void TestInvoiceLineWithSupportingDocs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.ZG_ReturnToOrigin = false;
			var supportingDocuments1 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocuments1.CSI_Code = "0100";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.ZG_ReturnToOrigin = false;
			var supportingDocuments2 = invoiceLine2.SupportingDocuments.AddNew();
			supportingDocuments2.CSI_Code = "0100";

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.ZG_ReturnToOrigin = true;
			var supportingDocuments3 = invoiceLine3.SupportingDocuments.AddNew();
			supportingDocuments2.CSI_Code = "0100";

			var lineMerger = new LineMerger(declaration);
			var entryCreationStrategy = new EntryCreationStrategy(declaration);

			CombineAssertions(() =>
			{
				lineMerger.DoMerge();
				AssertEquals("Invoice Line Count", 3, declaration.InvoiceLines.Count);
				AssertEquals("Entry Line Count", 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);

				var supportingDocumentsNew = invoiceLine1.SupportingDocuments.AddNew();
				supportingDocumentsNew.CSI_Code = "0200";

				lineMerger.DoMerge();
				AssertEquals("Invoice Line Count", 3, declaration.InvoiceLines.Count);
				AssertEquals("Entry Line Count", 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
			});
		}

		public void TestMergeCombineDescriptionForTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			var invLine1 = invoice.JobComInvoiceLines.AddNew();
			var invLine2 = invoice.JobComInvoiceLines.AddNew();

			invLine1.JI_Tariff = "001";
			invLine1.JI_Description = "English Description";
			invLine1.JI_NDescription = "Turkish Description";

			invLine2.JI_Tariff = "001";
			invLine2.JI_Description = "English Description";
			invLine2.JI_NDescription = "Turkish Description 2";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CombineAssertions(() =>
			{
				AssertEquals("1 Expected CusEntryHeader after merge", 1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("1 Expected CusEntryLines after merge", 1, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);
				AssertEquals("Turkish Description, Turkish Description 2", invLine1.CusEntryLine.CL_Description);

				var invLine3 = invoice.JobComInvoiceLines.AddNew();
				var invLine4 = invoice.JobComInvoiceLines.AddNew();
				invLine3.JI_Tariff = "002";
				invLine3.JI_Description = "English Description 3";
				invLine3.JI_NDescription = "Turkish Description 3";

				invLine4.JI_Tariff = "002";
				invLine4.JI_Description = "English Description 3";
				invLine4.JI_NDescription = "Turkish Description 4";

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

				AssertEquals("1 Expected CusEntryHeader after merge", 1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("3 Expected CusEntryLines after merge", 2, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);
				AssertEquals("Turkish Description 3, Turkish Description 4", invLine3.CusEntryLine.CL_Description);

				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("1 Expected CusEntryHeader after merge", 1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("4 Expected CusEntryLines after merge", 4, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);
				AssertEquals("Turkish Description", invLine1.CusEntryLine.CL_Description);
				AssertEquals("Turkish Description 2", invLine2.CusEntryLine.CL_Description);
				AssertEquals("Turkish Description 3", invLine3.CusEntryLine.CL_Description);
				AssertEquals("Turkish Description 4", invLine4.CusEntryLine.CL_Description);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			entryCreationStrategy = new EntryCreationStrategy(declaration);
		}
		EntryCreationStrategy entryCreationStrategy;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		JobDeclaration declaration;
	}
}
