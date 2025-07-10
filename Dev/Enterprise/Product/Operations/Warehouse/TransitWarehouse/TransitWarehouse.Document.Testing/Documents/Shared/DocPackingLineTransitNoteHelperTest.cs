using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class DocPackingLineTransitNoteHelperTest : TransitLogTableHelperTest<DocPackingLine, DocPackingLineColumn>
	{
		public override void TestGetColumns()
		{
			var tableHelper = GetTableHelper();
			var (docPackingLine, _) = CreateTestBO(3);

			var column = tableHelper.GetColumn(new DocPackingLine[] { docPackingLine }, DocPackingLineColumn.Description);

			AssertEquals("Description", column.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "Desc-003" }, column.Values);
		}

		protected override (DocPackingLine BusinessObject, string expectedDisplayId) CreateTestBO(int id)
		{
			var docPackingline = new DocPackingLine(ZGuid.NewZGuid())
			{
				AmountQuantity = 1 + id,
				AmountWeight = 0.5 + id,
				Description = "Desc-" + string.Format("{0:000}", id),
				AccompanyDocumentRef = "CEN" + id.ToString(),
				AccompanyDocumentType = "T1",
				TemporaryStorageDeclaration = "TSD" + id.ToString()
			};

			return (docPackingline, "Desc-" + string.Format("{0:000}", id));
		}

		protected override DocPackingLineColumn GetDefaultColumn() => DocPackingLineColumn.Description;

		protected override TransitLogTableHelper<DocPackingLine, DocPackingLineColumn> GetTableHelper() => new DocPackingLineTransitNoteHelper();

		protected override int ExpectedMinimumColumnWidth { get; set; } = 6;
	}
}
