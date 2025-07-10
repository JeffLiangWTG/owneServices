using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public sealed class GlbCompanyCredential : GlbExternalPassword
	{
		public GlbCompanyCredential(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new GlbCompanyCredentialLookups Lookups => (GlbCompanyCredentialLookups)base.Lookups;
		protected override MasterFiles.Business.GlbExternalPasswordLookups GetNewLookups()
		{
			return new GlbCompanyCredentialLookups(this);
		}

		public new GlbCompanyCredentialValidation Validation => (GlbCompanyCredentialValidation)GetNewValidation();
		protected override MasterFiles.Business.GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbCompanyCredentialValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_GS = ZGuid.Empty;
		}
	}
}
