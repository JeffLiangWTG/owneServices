namespace Enterprise.Customs.Business
{
	public class CusGuaranteeHeaderValidation : SharedCusPermitHeaderValidation
	{
		public CusGuaranteeHeaderValidation(BaseCusGuaranteeHeader parent)
			: base(parent)
		{
		}

		public new BaseCusGuaranteeHeader Parent => (BaseCusGuaranteeHeader)base.Parent;

		#region New Checks

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateMainAccessCode();
			ValidateMainAccessPersonName();
		}

		public void ValidateMainAccessCode()
		{
			ValidateCalculatedProperty(Parent.MainAccessCodeInfo);
		}

		public void ValidateMainAccessPersonName()
		{
			ValidateCalculatedProperty(Parent.MainAccessPersonNameInfo);
		}

		protected virtual void CheckMainAccessCode()
		{
		}

		protected virtual void CheckMainAccessPersonName()
		{
		}

		#endregion
	}
}
