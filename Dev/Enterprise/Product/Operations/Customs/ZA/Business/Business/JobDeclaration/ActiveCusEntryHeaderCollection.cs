namespace Enterprise.Customs.ZA.Business
{
	public class ActiveCusEntryHeaderCollection : Customs.Business.ActiveCusEntryHeaderCollection
	{
		public ActiveCusEntryHeaderCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new CusEntryHeader this[int index]
		{
			get { return (CusEntryHeader)base[index]; }
		}

		public new CusEntryHeader AddNew()
		{
			return (CusEntryHeader)base.AddNew();
		}

		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}
	}
}
