using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class GlbExternalPassword : GlbExternalPasswordBase_TW
	{
		public GlbExternalPassword(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.TVA;
			GP_GC = GlbCompany.CurrentCompany.PK;
			CalculateGP_CertificatePassphraseStatus();
		}

		public new GlbExternalPasswordLookups Lookups => (GlbExternalPasswordLookups)base.Lookups;
		protected override MasterFiles.Business.GlbExternalPasswordLookups GetNewLookups()
		{
			return new GlbExternalPasswordLookups(this);
		}

		public new GlbExternalPasswordValidation Validation => (GlbExternalPasswordValidation)GetNewValidation();
		protected override MasterFiles.Business.GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbExternalPasswordValidation(this);
		}

		protected override string InterchangeTypeForSending => EDIInterchangeTypeList.Codes.Configuration;
	}
}
