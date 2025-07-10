namespace Enterprise.Customs.US.Business
{
	public class RegoNumberValidation : Customs.Business.CusCodeDataValidation
	{
		public RegoNumberValidation(RegoNumber regoNumber)
			: base(regoNumber)
		{
		}

		public new RegoNumber Parent
		{
			get { return (RegoNumber)base.Parent; }
		}
	}
}
