namespace Enterprise.Customs.SG.V4.Business
{
	public class CusEntryLineLookups : Customs.Business.CusEntryLineLookups
	{
		public CusEntryLineLookups(CusEntryLine parent)
			: base(parent)
		{
		}

		public CusEntryLine EntryLine
		{
			get { return Parent; }
		}

		protected new CusEntryLine Parent
		{
			get { return (CusEntryLine)base.Parent; }
		}
	}
}
