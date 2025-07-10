using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanySignatureCredentialCollection : DependentBusinessObjectCollection<GlbCompanySignatureCredential, GlbCompany>
	{
		public GlbCompanySignatureCredentialCollection(GlbCompany master) : base(master, new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.TRU))
		{
		}

		protected override bool AllowNewCore => IsEditAllowed && IsLoaded && Count < 1;

		public bool IsEditAllowed => IsEditAllowedForCompany(Master);

		public static bool IsEditAllowedForCompany(GlbCompany company) =>
				(company?.Country?.Code.ToString() ?? string.Empty) == Core.Constants.CountryCodes.Turkey
				&& AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeatures;
	}
}
