using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class ImportCustomsDeclarationNonCondensedDocumentWrapper : ImportCustomsDeclarationDocumentWrapper
	{
		public ImportCustomsDeclarationNonCondensedDocumentWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factory)
			: base(entryHeader, entryHeader != null ? new ImportNonCondensedDeclarationMessageSendingObject(entryHeader) : null, factory)
		{
		}

		public new static ImportCustomsDeclarationNonCondensedDocumentWrapper New(BusinessObject bizO, BusinessObjectFactory factoryToWrap)
		{
			return bizO is CusEntryHeader entryHeader ? new ImportCustomsDeclarationNonCondensedDocumentWrapper(entryHeader, factoryToWrap) : null;
		}
	}
}
