using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineViewCollection))]
	sealed class JobComInvoiceLineViewCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
	{
		public void TestNoRowNotInTableExceptionThrownWhenAddingLineForDeletedInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.Delete();

			AssertNoExceptionThrown(() => invoice.JobComInvoiceLines.AddNew());
		}

		public void TestTotalNonSecondaryInvoiceLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();

			AssertEquals(1, invoice.JobComInvoiceLines.TotalNonSecondaryInvoiceLines);
		}

		public void TestUpdateFirstAIILinesChildEditableStatus()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_IsInvoiceByRequest = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_IsLineGrouping = true;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AIILine aiiLine = invoiceLine.FirstAIILine;

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			AIILine aiiLine2 = invoiceLine2.FirstAIILine;

			AssertEquals(aiiLine, invoiceLine.FirstAIILine);
			AssertEquals(false, invoiceLine.IsRegisteredEditableChildObject(aiiLine));
			AssertEquals(aiiLine2, invoiceLine2.FirstAIILine);
			AssertEquals(true, invoiceLine2.IsRegisteredEditableChildObject(aiiLine2));
		}

		public void TestHasPriorNoticeTariffs()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FCCApplicable;
			AssertEquals(false, declaration.Invoices.RequiresPriorNoticeReporting);

			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			AssertEquals(true, declaration.Invoices.RequiresPriorNoticeReporting);
		}

		public void TestHasFDATariffsToBeDeclared()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FCCApplicable;
			AssertEquals(false, declaration.Invoices.HasFDATariffsToBeDeclared);

			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewDONOTSUBMITTariff;
			AssertEquals(false, declaration.Invoices.HasFDATariffsToBeDeclared);

			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, declaration.Invoices.HasFDATariffsToBeDeclared);

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, declaration.Invoices.HasFDATariffsToBeDeclared);

			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, declaration.Invoices.HasFDATariffsToBeDeclared);

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, declaration.Invoices.HasFDATariffsToBeDeclared);

			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, declaration.Invoices.HasFDATariffsToBeDeclared);

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, declaration.Invoices.HasFDATariffsToBeDeclared);

			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, declaration.Invoices.HasFDATariffsToBeDeclared);

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
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

		public void TestHasNonContainerisedInvoiceLines()
		{
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusContainer container = declaration.CusContainers.AddNew();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals("HasNonContainerisedInvoiceLines", false, invoice.JobComInvoiceLines.HasNonContainerisedInvoiceLines);
			AssertEquals("HasNonContainerisedInvoiceLines", false, declaration.Invoices.HasNonContainerisedInvoiceLines);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("HasNonContainerisedInvoiceLines", true, invoice.JobComInvoiceLines.HasNonContainerisedInvoiceLines);
			AssertEquals("HasNonContainerisedInvoiceLines", true, declaration.Invoices.HasNonContainerisedInvoiceLines);

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			AssertEquals("HasNonContainerisedInvoiceLines", false, invoice.JobComInvoiceLines.HasNonContainerisedInvoiceLines);
			AssertEquals("HasNonContainerisedInvoiceLines", false, declaration.Invoices.HasNonContainerisedInvoiceLines);

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;
			AssertEquals("HasNonContainerisedInvoiceLines", true, invoice.JobComInvoiceLines.HasNonContainerisedInvoiceLines);
			AssertEquals("HasNonContainerisedInvoiceLines", true, declaration.Invoices.HasNonContainerisedInvoiceLines);
		}

		public void TestTotalInnerPackQty()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_ManifestQty = 50;
			AssertEquals(50m, invoice.JobComInvoiceLines.TotalPackQty);

			invoiceLine2.US_ManifestQty = 20;
			invoiceLine3.US_ManifestQty = 10;
			AssertEquals(80m, invoice.JobComInvoiceLines.TotalPackQty);

			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			AssertEquals(60m, invoice.JobComInvoiceLines.TotalPackQty);
		}

		public void TestTypedIndexer()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			InvoiceLineCompleteCollection lineCollection = new InvoiceLineCompleteCollection(declaration);
			JobComInvoiceLineViewCollection collection = new JobComInvoiceLineViewCollection(header, lineCollection);
			JobComInvoiceLine invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		[ExpectNoExceptions]
		public void TestUpdateProductDetailsRaisesNoException()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TESTPRODUCT";
			product.OP_Desc = "TESTPRODUCT";

			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "9101000010";

			var childPivot1 = pivot1.Children.AddNew();
			childPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			childPivot1.CI_TariffNum = "6211320025";

			OrgHeader supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_Code = "SUPPLIER1234";
			supplier1.OH_FullName = "SUPPLIER 1";

			product.RelatedOrganisations.AddSupplier(supplier1);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = supplier1.PK;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_PartNo = product.OP_PartNum;

			Factory.Save();

			AssertNoExceptionThrown(() => invoice.JZ_OH_Supplier = ZGuid.Invalid);
		}

		protected override JobComInvoiceLineViewCollection GetCollectionToTest() => Invoice.JobComInvoiceLines;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_JZ = Invoice.PK;
			if (Invoice.JobComInvoiceLines.Contains(invoiceLine))
			{
				Invoice.JobComInvoiceLines.Remove(invoiceLine);
			}
			return invoiceLine;
		}

		JobComInvoiceHeader Invoice
		{
			get
			{
				if (invoice == null)
				{
					var declartion = Factory.New<JobDeclaration>();
					invoice = declartion.Invoices.AddNew();
				}
				return invoice;
			}
		}
		JobComInvoiceHeader invoice;
	}
}
