using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class GlbStaffCredentialCollection : DependentBusinessObjectCollection<GlbStaffCredential, GlbStaff>, MasterFiles.Integration.Customs.US.IGlbExternalPasswordCollection_US
	{
		public GlbStaffCredentialCollection(GlbStaff staff)
			: base(staff, new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.EBD))
		{
		}
	}
}
