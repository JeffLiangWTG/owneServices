using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public class DocPackingLineTransitNoteHelper : TransitLogTableHelper<DocPackingLine, TransitLogColumnIDs.DocPackingLineColumn>
	{
		protected override ZString GetHeader(TransitLogColumnIDs.DocPackingLineColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.DocPackingLineColumn.RefType:
					return Res.GetString("c5b6de0c-b4f1-4f02-91b8-177e29dd724a", "Ref Type");
				case TransitLogColumnIDs.DocPackingLineColumn.RefCode:
					return Res.GetString("d216dbd0-3c7f-4f4b-b583-372782c1e213", "Ref Code");
				case TransitLogColumnIDs.DocPackingLineColumn.Quantity:
					return Padding + Res.GetString("4bc69da3-58ce-4c87-aed5-ced7f9a0244c", "Quantity");
				case TransitLogColumnIDs.DocPackingLineColumn.Weight:
					return Res.GetString("89976868-94f8-4531-8085-d841f56c0c24", "Weight");
				case TransitLogColumnIDs.DocPackingLineColumn.Description:
					return Res.GetString("b99de994-f68a-4639-b601-115be458f35a", "Description");
				case TransitLogColumnIDs.DocPackingLineColumn.PNTS:
					return Res.GetString("867d8c32-5952-4871-a8a2-aecd3fb953e2", "PNTS");
				case TransitLogColumnIDs.DocPackingLineColumn.AccompanyingDocumentType:
					return Res.GetString("22b7d9d8-08c7-428f-9b5a-2beaa1389a6c", "Doc Type");
				case TransitLogColumnIDs.DocPackingLineColumn.AccompanyingDocumentRef:
					return Res.GetString("f9705072-e596-4f07-9807-0198ef677cbb", "Doc Ref");
				case TransitLogColumnIDs.DocPackingLineColumn.TemporaryStorageDeclaration:
					return Res.GetString("c83f8a98-ecb6-441b-be22-6dbbc4ee00fe", "TSD");
				default:
					return ZString.Empty;
			}
		}

		protected override ZString GetValue(DocPackingLine docPackingLine, TransitLogColumnIDs.DocPackingLineColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.DocPackingLineColumn.RefType:
					return docPackingLine?.RefType?.Code ?? ZString.Empty;
				case TransitLogColumnIDs.DocPackingLineColumn.RefCode:
					return docPackingLine?.RefCode ?? ZString.Empty;
				case TransitLogColumnIDs.DocPackingLineColumn.Quantity:
					return docPackingLine?.AmountQuantity is null ? ZString.Empty : Padding + docPackingLine.AmountQuantity.ToString();
				case TransitLogColumnIDs.DocPackingLineColumn.Weight:
					return docPackingLine?.AmountWeight is null ? ZString.Empty : docPackingLine.AmountWeight.ToString("F3") + " KG";
				case TransitLogColumnIDs.DocPackingLineColumn.Description:
					return docPackingLine?.Description ?? ZString.Empty;
				case TransitLogColumnIDs.DocPackingLineColumn.PNTS:
					return docPackingLine?.TemporaryStorageDeclaration ?? ZString.Empty;
				case TransitLogColumnIDs.DocPackingLineColumn.AccompanyingDocumentType:
					return docPackingLine?.AccompanyDocumentType ?? ZString.Empty;
				case TransitLogColumnIDs.DocPackingLineColumn.AccompanyingDocumentRef:
					return docPackingLine?.AccompanyDocumentRef ?? ZString.Empty;
				case TransitLogColumnIDs.DocPackingLineColumn.TemporaryStorageDeclaration:
					return docPackingLine?.TemporaryStorageDeclaration ?? ZString.Empty;
				default:
					return ZString.Empty;
			}
		}

		const string Padding = "    ";

		public override int MinimumColumnWidth { get; set; } = 6;
	}
}
