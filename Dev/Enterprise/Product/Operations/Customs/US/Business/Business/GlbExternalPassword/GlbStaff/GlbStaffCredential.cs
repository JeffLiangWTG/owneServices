using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public sealed class GlbStaffCredential : GlbExternalPassword
	{
		public GlbStaffCredential(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new GlbStaffCredentialLookups Lookups => (GlbStaffCredentialLookups)base.Lookups;
		protected override MasterFiles.Business.GlbExternalPasswordLookups GetNewLookups()
		{
			return new GlbStaffCredentialLookups(this);
		}

		public new GlbStaffCredentialValidation Validation => (GlbStaffCredentialValidation)GetNewValidation();
		protected override MasterFiles.Business.GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbStaffCredentialValidation(this);
		}
	}
}
