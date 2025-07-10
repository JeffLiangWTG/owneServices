using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class GlbExternalPasswordCollection : DependentBusinessObjectCollection<GlbExternalPassword, GlbStaff>, MasterFiles.Integration.Customs.TW.IGlbExternalPasswordCollection_TW
	{
		public GlbExternalPasswordCollection(GlbStaff staff)
			: base(staff, GetExternalPasswordFilter(GlbCompany.CurrentCompany.PK))
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (Count > 0)
			{
				var previousLine = this[Count - 1];
				if (previousLine.CertificateStatus == GlbExternalPasswordWithCertificate.CertificateLoaded)
				{
					var currentLine = (GlbExternalPassword)child;
					currentLine.CurrentDecryptedCertificatePassphrase = previousLine.CurrentDecryptedCertificatePassphrase;
					currentLine.GP_Certificate = previousLine.GP_Certificate;
				}
			}
		}

		static ZQuery GetExternalPasswordFilter(ZGuid companyPk)
		{
			var passwordTypesQuery = new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.TVA);
			passwordTypesQuery.AddToFilter(JoinCondition.Or, GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.UVC);

			var result = new ZQuery(GlbExternalPasswordSchema.GP_GC, companyPk);
			result.AddToFilter(passwordTypesQuery);
			return result;
		}
	}
}
