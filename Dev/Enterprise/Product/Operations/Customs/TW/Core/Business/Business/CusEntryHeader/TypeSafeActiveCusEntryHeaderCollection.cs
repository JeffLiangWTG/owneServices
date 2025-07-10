namespace Enterprise.Customs.TW.Business
{
	partial class ActiveCusEntryHeaderCollection : Customs.Business.ActiveCusEntryHeaderCollection
	{
		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		public new CusEntryHeader this[int index]
		{
			get { return (CusEntryHeader)base[index]; }
		}

		public new CusEntryHeader AddNew()
		{
			return (CusEntryHeader)base.AddNew();
		}
	}
}
