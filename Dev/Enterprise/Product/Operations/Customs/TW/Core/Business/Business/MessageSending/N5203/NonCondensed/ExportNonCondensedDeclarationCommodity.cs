using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class ExportNonCondensedDeclarationCommodity : Commodity
	{
		public ExportNonCondensedDeclarationCommodity(CusEntryLine entryLine, JobComInvoiceLine invoiceLine) : base(entryLine, invoiceLine)
		{
		}

		public override IEnumerable<IAdditionalDocument> AdditionalDocuments => GetAdditionalDocuments(FirstInvoiceLine);

		public override ZString Description => FirstInvoiceLine.JI_DeclarationGoodsDescription;

		public override IEnumerable<IClassification> Classifications => FirstInvoiceLine.GetClassifications((hazMatCode, idTypeCode) => new ClassificationWrapper(hazMatCode, idTypeCode), new List<ZString>());

		protected override IInvoiceLine InvoiceLineCore => new ExportNonCondensedDeclarationInvoiceLine(EntryLine, FirstInvoiceLine);

		public override IEnumerable<ZString> VehicleIDs => FirstInvoiceLine.GetVehicleIDs();
	}
}
