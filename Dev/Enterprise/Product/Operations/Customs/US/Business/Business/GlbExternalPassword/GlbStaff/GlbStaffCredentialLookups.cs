namespace Enterprise.Customs.US.Business
{
	public class GlbStaffCredentialLookups : GlbExternalPasswordLookups
	{
		public GlbStaffCredentialLookups(GlbStaffCredential parent)
			: base(parent)
		{
		}

		protected new GlbStaffCredential Parent => (GlbStaffCredential)base.Parent;
	}
}
