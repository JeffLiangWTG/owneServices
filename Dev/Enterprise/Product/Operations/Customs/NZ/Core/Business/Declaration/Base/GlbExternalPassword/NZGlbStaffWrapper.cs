using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.NZ;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZGlbStaffWrapper : GlbStaffWrapper, INZGlbStaffWrapper
	{
		protected NZGlbStaffWrapper(GlbStaff staff)
			: base(staff)
		{
		}

		public static NZGlbStaffWrapper Get(GlbStaff staff)
		{
			return staff == null ? null : staff.Factory.GetCachedValue(staff.PK.ToString(), () => new NZGlbStaffWrapper(staff));
		}

		#region NZBPassword

		public GlbExternalPassword_NZ NZBPassword
		{
			get
			{
				if (nzbPassword == null)
				{
					nzbPassword = GetGlbExternalPasswordOrCreateNew<GlbExternalPassword_NZ>(PasswordTypesList.Codes.NZB, GlbCompany.CurrentCompany.PK);
					RegisterEditableChildObject(nzbPassword);
				}

				return nzbPassword;
			}
		}
		GlbExternalPassword_NZ nzbPassword;

		#endregion

		IGlbExternalPassword INZGlbStaffWrapper.NZBPassword => NZBPassword;
	}
}
