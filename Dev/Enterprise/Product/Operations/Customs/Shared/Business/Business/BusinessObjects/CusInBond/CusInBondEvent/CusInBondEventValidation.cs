namespace Enterprise.Customs.Business
{
	public class CusInBondEventValidation : AutoCusInBondEventValidation
	{
		public CusInBondEventValidation(AutoCusInBondEvent parent)
			: base(parent)
		{
		}

		public new CusInBondEvent Parent => (CusInBondEvent)base.Parent;
	}
}
