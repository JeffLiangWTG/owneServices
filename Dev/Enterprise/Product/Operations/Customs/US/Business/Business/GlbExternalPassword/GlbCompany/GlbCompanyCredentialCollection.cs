using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class GlbCompanyCredentialCollection : DependentBusinessObjectCollection<GlbCompanyCredential, GlbCompany>, MasterFiles.Integration.Customs.US.IGlbExternalPasswordCollection_US
	{
		public GlbCompanyCredentialCollection(GlbCompany company)
			: base(company, new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.EBD).AddToFilter(GlbExternalPasswordSchema.GP_GS, SQLComparisonOperator.Equal, null))
		{
		}
	}
}
