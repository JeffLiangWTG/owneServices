using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class GlbCompanyLicensingCredential : GlbCompanyCredential
	{
		public GlbCompanyLicensingCredential(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.NXM;
		}
	}
}
