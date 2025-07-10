namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ReadOnlySupportingDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocumentCollection
	{
		public ReadOnlySupportingDocumentCollection(CusEntryLine entryLine) : base(entryLine)
		{
		}

		protected new CusEntryLine EntryLine => (CusEntryLine)base.EntryLine;

		protected override void AddSupportingDocument(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument supportingDocument)
		{
			Add(new ReadOnlySupportingDocument((SupportingDocument)supportingDocument));
		}

		public new ReadOnlySupportingDocument this[int index] => (ReadOnlySupportingDocument)Elements[index];

		public new ReadOnlySupportingDocument AddNew() => (ReadOnlySupportingDocument)base.AddNew();
	}
}
