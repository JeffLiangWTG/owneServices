namespace Enterprise.Customs.SG.V4.Business
{
	public class GlbExternalPasswordValidation_SGA : GlbExternalPasswordValidation_SGv4
	{
		public GlbExternalPasswordValidation_SGA(GlbExternalPassword_SGA parent)
			: base(parent)
		{
		}

		protected override void CheckGP_UserID()
		{
		}
	}
}
