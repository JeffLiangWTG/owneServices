using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

abstract class EntryCreationStrategyTest : EU.Business.Declaration.Testing.EntryCreationStrategyTest
{
	public void TestPreviousDocumentKeys()
	{
		var dec = GetJobDeclarationForTest();
		var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		var inv1 = dec.Invoices.AddNew();
		var invLine = inv1.JobComInvoiceLines.AddNew();
		invLine.JI_CEI = cei.PK;
		var mergeStrategy = dec.CreateEntryCreationStrategy();
		var previousDocumentKeys = mergeStrategy.GetPreviousDocumentKeys();
		AssertArrayEqualsByElements(GetExpectedPreviousDocumentKeys(), previousDocumentKeys);
	}

	protected abstract string[] GetExpectedPreviousDocumentKeys();

	protected void TestPreviousDocumentsAffectingMergingBy(string fieldName, object valueForDoc1, object valueForDoc2)
	{
		var (dec, invLines) = PrepareTestJobData(Factory, 2);
		var strategy = dec.CreateEntryCreationStrategy();
		var line0 = invLines[0];
		var line1 = invLines[1];

		var propertyInfo = typeof(PreviousDocument).GetProperty(fieldName);

		var doc1 = line0.PreviousDocuments.AddNew();
		propertyInfo.SetValue(doc1, valueForDoc1);

		var doc2 = line1.PreviousDocuments.AddNew();
		propertyInfo.SetValue(doc2, valueForDoc2);

		CombineAssertions(() =>
		{
			AssertNotEquals($"Keys are not equal for different {fieldName}", strategy.GetKeyForLine(line0), strategy.GetKeyForLine(line1));
			propertyInfo.SetValue(doc2, valueForDoc1);
			AssertEquals($"Keys are equal for identical {fieldName}", strategy.GetKeyForLine(line0), strategy.GetKeyForLine(line1));
		});
	}

	public void TestPreviousDocumentCreatesTwoEntryLines_Invoices()
	{
		var dec = GetJobDeclarationForTest();
		var inv1 = dec.Invoices.AddNew();
		inv1.JobComInvoiceLines.AddNew();

		var inv2 = dec.Invoices.AddNew();
		inv2.JobComInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Same keys", 1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			var inv1PrevDoc = inv1.PreviousDocuments.AddNew();
			inv1PrevDoc.CSI_Code = "111";
			inv1PrevDoc.CSI_ReferenceNumber = "111";
			var inv2PrevDoc = inv2.PreviousDocuments.AddNew();
			inv2PrevDoc.CSI_Code = "111";
			inv2PrevDoc.CSI_ReferenceNumber = "111";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Same data", 1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			inv2PrevDoc.CSI_Code = "222";
			inv2PrevDoc.CSI_ReferenceNumber = "222";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Different previous document", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestInvoiceLinesMergeByTRD()
	{
		var declaration = GetJobDeclarationForTest();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
		var strategy = declaration.CreateEntryCreationStrategy();

		CombineAssertions(() =>
		{
			invoiceLine1.JI_Description = "Description1";
			invoiceLine2.JI_Description = "Description1";
			AssertEquals("When the JI_NDescription is empty, and the same JI_Description values will be applied as key", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

			invoiceLine2.JI_Description = "Description2";
			AssertNotEquals("When the JI_NDescription is empty, and different JI_Description values will be applied as key", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

			invoiceLine1.JI_NDescription = "N1";
			invoiceLine2.JI_NDescription = "N1";
			AssertEquals("When the JI_NDescription is not empty, and the same JI_NDescription will be applied as key instead of JI_Description", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

			invoiceLine2.JI_NDescription = "N2";
			AssertNotEquals("When the JI_NDescription is not empty, and different JI_NDescription will be applied as key instead of JI_Description", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

			invoiceLine1.JI_Description = "Description1";
			invoiceLine2.JI_Description = "Description1";
			invoiceLine1.JI_NDescription = ZString.Empty;
			invoiceLine2.JI_NDescription = "N1";
			AssertNotEquals("When one of the JI_NDescription is empty and the other is not, while all the JI_Description are same, the lines should have different keys", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));
		});
	}

	public void TestPreviousDocumentCreatesTwoEntryLines_InvoiceLines()
	{
		var dec = GetJobDeclarationForTest();
		var inv1 = dec.Invoices.AddNew();
		var inv1Line = inv1.JobComInvoiceLines.AddNew();

		var inv2 = dec.Invoices.AddNew();
		var inv2Line = inv2.JobComInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Same keys", 1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			var inv1LinePrevDoc = inv1Line.PreviousDocuments.AddNew();
			inv1LinePrevDoc.CSI_Code = "111";
			inv1LinePrevDoc.CSI_ReferenceNumber = "111";

			var inv2LinePrevDoc = inv2Line.PreviousDocuments.AddNew();
			inv2LinePrevDoc.CSI_Code = "111";
			inv2LinePrevDoc.CSI_ReferenceNumber = "111";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Same data on both previous documents", 1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			inv2LinePrevDoc.CSI_Code = "222";
			inv2LinePrevDoc.CSI_ReferenceNumber = "222";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Different data on previous documents", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestPreviousDocumentCreatesTwoEntryLines_InvoiceLinesAndInvoices()
	{
		var dec = GetJobDeclarationForTest();
		var inv1 = dec.Invoices.AddNew();
		var inv1Line = inv1.JobComInvoiceLines.AddNew();

		var inv2 = dec.Invoices.AddNew();
		var inv2Line = inv2.JobComInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Same keys", 1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			var inv1PrevDoc = inv1.PreviousDocuments.AddNew();
			inv1PrevDoc.CSI_Code = "111";
			inv1PrevDoc.CSI_ReferenceNumber = "111";
			var inv2PrevDoc = inv2.PreviousDocuments.AddNew();
			inv2PrevDoc.CSI_Code = "111";
			inv2PrevDoc.CSI_ReferenceNumber = "111";

			var inv1LinePrevDoc = inv1Line.PreviousDocuments.AddNew();
			inv1LinePrevDoc.CSI_Code = "111";
			inv1LinePrevDoc.CSI_ReferenceNumber = "111";
			var inv2LinePrevDoc = inv2Line.PreviousDocuments.AddNew();
			inv2LinePrevDoc.CSI_Code = "111";
			inv2LinePrevDoc.CSI_ReferenceNumber = "111";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Same data", 1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			inv2LinePrevDoc.CSI_Code = "222";
			inv2LinePrevDoc.CSI_ReferenceNumber = "222";
			inv2PrevDoc.CSI_Code = "222";
			inv2PrevDoc.CSI_ReferenceNumber = "222";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("different Data", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			inv2PrevDoc.CSI_Code = "333";
			inv2PrevDoc.CSI_ReferenceNumber = "333";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("invoice level previous document", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			inv1LinePrevDoc.CSI_Code = "444";
			inv1LinePrevDoc.CSI_ReferenceNumber = "444";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("invoice line previous document", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestProcessInvoiceHeaderForLineMergeKey_Seller()
	{
		var dec = GetJobDeclarationForTest();
		var inv1 = dec.Invoices.AddNew();
		var inv1Line = inv1.JobComInvoiceLines.AddNew();

		var inv2 = dec.Invoices.AddNew();
		var inv2Line = inv2.JobComInvoiceLines.AddNew();

		var organisation1 = Factory.New<OrgHeader>();
		var org1Address1 = organisation1.Addresses.AddNew();
		var org1Address2 = organisation1.Addresses.AddNew();

		var organisation2 = Factory.New<OrgHeader>();
		var org2Address = organisation2.Addresses.AddNew();

		CombineAssertions(() =>
		{
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("same keys", 1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			inv1.SellerOrgPK = organisation1.PK;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("1 OrgHeader is empty 2nd is not", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			inv2.SellerOrgPK = organisation1.PK;
			inv1.JZ_OA_SellerAddress = org1Address1.PK;
			inv2.JZ_OA_SellerAddress = org1Address2.PK;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("same OrgHeader but different OrgAddress", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			inv1.SellerOrgPK = organisation1.PK;
			inv2.SellerOrgPK = organisation2.PK;
			inv1.JZ_OA_SellerAddress = org1Address1.PK;
			inv2.JZ_OA_SellerAddress = org2Address.PK;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("different OrgHeader and OrgAddress", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			inv1.SellerOrgPK = organisation1.PK;
			inv2.SellerOrgPK = organisation1.PK;
			inv1.JZ_OA_SellerAddress = org1Address1.PK;
			inv2.JZ_OA_SellerAddress = org1Address1.PK;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("same OrgHeader and OrgAddress", 1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestProcessInvoiceHeaderForLineMergeKey_Buyer()
	{
		var dec = GetJobDeclarationForTest();
		var inv1 = dec.Invoices.AddNew();
		var inv1Line = inv1.JobComInvoiceLines.AddNew();

		var inv2 = dec.Invoices.AddNew();
		var inv2Line = inv2.JobComInvoiceLines.AddNew();

		var organisation1 = Factory.New<OrgHeader>();
		var org1Address1 = organisation1.Addresses.AddNew();
		var org1Address2 = organisation1.Addresses.AddNew();

		var organisation2 = Factory.New<OrgHeader>();
		var org2Address = organisation2.Addresses.AddNew();

		CombineAssertions(() =>
		{
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("same keys", 1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			inv1.BuyerOrgPK = organisation1.PK;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("1 OrgHeader is empty 2nd is not", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			inv2.BuyerOrgPK = organisation1.PK;
			inv1.JZ_OA_BuyerAddress = org1Address1.PK;
			inv2.JZ_OA_BuyerAddress = org1Address2.PK;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("same OrgHeader but different OrgAddress", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			inv1.BuyerOrgPK = organisation1.PK;
			inv2.BuyerOrgPK = organisation2.PK;
			inv1.JZ_OA_BuyerAddress = org1Address1.PK;
			inv2.JZ_OA_BuyerAddress = org2Address.PK;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("different OrgHeader and OrgAddress", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			inv1.BuyerOrgPK = organisation1.PK;
			inv2.BuyerOrgPK = organisation1.PK;
			inv1.JZ_OA_BuyerAddress = org1Address1.PK;
			inv2.JZ_OA_BuyerAddress = org1Address1.PK;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("same OrgHeader and OrgAddress", 1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	protected void TestSupportingDocumentsAffectingMergingBy(string fieldName, object valueForDoc1, object valueForDoc2)
	{
		var (dec, invLines) = PrepareTestJobData(Factory, 2);
		var strategy = dec.CreateEntryCreationStrategy();
		var line0 = invLines[0];
		var line1 = invLines[1];

		var propertyInfo = typeof(SupportingDocument).GetProperty(fieldName);

		var doc1 = line0.SupportingDocuments.AddNew();
		propertyInfo.SetValue(doc1, valueForDoc1);

		var doc2 = line1.SupportingDocuments.AddNew();
		propertyInfo.SetValue(doc2, valueForDoc2);

		CombineAssertions(() =>
		{
			AssertNotEquals($"Keys are not equal for different {fieldName}", strategy.GetKeyForLine(line0), strategy.GetKeyForLine(line1));
			propertyInfo.SetValue(doc2, valueForDoc1);
			AssertEquals($"Keys are equal for identical {fieldName}", strategy.GetKeyForLine(line0), strategy.GetKeyForLine(line1));
		});
	}

	protected abstract string GetTestedMessageType { get; }

	protected (JobDeclaration, JobComInvoiceLine[]) PrepareTestJobData(BusinessObjectFactory factory, int countOfInvoiceLines)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = GetTestedMessageType;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		var invoice = declaration.Invoices.AddNew();
		for (int i = 0; i < countOfInvoiceLines; i++)
		{
			invoice.InvoiceLines.AddNew();
		}
		return (declaration, invoice.InvoiceLines.OfType<JobComInvoiceLine>().ToArray());
	}
}
