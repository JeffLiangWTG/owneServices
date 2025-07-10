using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class ExportNonCondensedDeclarationDocumentWrapper : ExportCustomsDeclarationDocumentWrapper
	{
		public ExportNonCondensedDeclarationDocumentWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factory) : base(entryHeader, factory)
		{
		}

		public new static ExportNonCondensedDeclarationDocumentWrapper New(BusinessObject bizO, BusinessObjectFactory factoryToWrap)
		{
			return bizO is CusEntryHeader entryHeader ? new ExportNonCondensedDeclarationDocumentWrapper(entryHeader, factoryToWrap) : null;
		}

		protected override IN5203Declaration GetMessageSendingObject(CusEntryHeader entryHeader)
		{
			return new ExportNonCondensedDeclarationMessageSendingObject(entryHeader);
		}
	}
}
