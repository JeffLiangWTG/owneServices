using System.Linq;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ImportNonCondensedDeclarationGoodsShipmentTest : NX5105Declaration_GoodsShipmentAbstractTest<ImportNonCondensedDeclarationGoodsShipment>
	{
		protected override IGoodsShipment GetGoodsShipment(CusEntryHeader entryHeader, SupportingDocumentCollection supportingDocuments, IStorageDocsBaseCollection[] allEDocs, bool includeControllingMessageInformation)
		{
			return new ImportNonCondensedDeclarationGoodsShipment(entryHeader, supportingDocuments, allEDocs);
		}

		[ExpectNoExceptions]
		public override void TestGoodsShipment_GovernmentAgencyGoodsItems()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.First(), NUnit.Framework.Is.TypeOf<ImportNonCondensedDeclarationGovernmentAgencyGoodsItem>());
		}
	}
}
