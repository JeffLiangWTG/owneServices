namespace Enterprise.Customs.Business
{
	public class BaseCusPermitHeaderLookups : SharedCusPermitHeaderLookups
	{
		public BaseCusPermitHeaderLookups(BaseCusPermitHeader parent)
			: base(parent)
		{
		}

		public new BaseCusPermitHeader Parent => (BaseCusPermitHeader)base.Parent;
	}
}
