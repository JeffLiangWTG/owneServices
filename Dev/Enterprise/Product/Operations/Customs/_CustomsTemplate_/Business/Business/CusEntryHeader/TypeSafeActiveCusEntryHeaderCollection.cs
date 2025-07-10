namespace Enterprise.Customs._CustomsTemplate_.Business
{
	partial class ActiveCusEntryHeaderCollection : Customs.Business.ActiveCusEntryHeaderCollection
	{
		public new CusEntryHeader this[int index] => (CusEntryHeader)base[index];

		public new CusEntryHeader AddNew() => (CusEntryHeader)base.AddNew();
	}
}
