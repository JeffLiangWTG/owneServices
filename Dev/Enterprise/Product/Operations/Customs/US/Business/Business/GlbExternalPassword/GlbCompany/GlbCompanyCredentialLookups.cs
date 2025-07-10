namespace Enterprise.Customs.US.Business
{
	public class GlbCompanyCredentialLookups : GlbExternalPasswordLookups
	{
		public GlbCompanyCredentialLookups(GlbCompanyCredential parent)
			: base(parent)
		{
		}

		protected new GlbCompanyCredential Parent => (GlbCompanyCredential)base.Parent;
	}
}
