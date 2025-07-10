using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class GlbExternalPassword_SGv4 : GlbExternalPassword_SG
	{
		public GlbExternalPassword_SGv4(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.SG4;
		}

		public new GlbExternalPasswordValidation_SGv4 Validation => (GlbExternalPasswordValidation_SGv4)base.Validation;
		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbExternalPasswordValidation_SGv4(this);
		}

		public new GlbExternalPasswordLookups_SGv4 Lookups => (GlbExternalPasswordLookups_SGv4)base.Lookups;
		protected override GlbExternalPasswordLookups GetNewLookups()
		{
			return new GlbExternalPasswordLookups_SGv4(this);
		}
	}
}
