namespace Enterprise.Customs.SG.V4.Business
{
	public class CusEntryLineValidation : Customs.Business.CusEntryLineValidation
	{
		public CusEntryLineValidation(CusEntryLine parent)
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

		protected JobDeclaration Declaration
		{
			get { return (JobDeclaration)EntryLine.Declaration; }
		}
	}
}
