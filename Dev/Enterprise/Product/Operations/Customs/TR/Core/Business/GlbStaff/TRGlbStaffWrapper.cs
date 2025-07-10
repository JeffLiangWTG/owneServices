using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.TR;

namespace Enterprise.Customs.TR.Business
{
	public class TRGlbStaffWrapper : GlbStaffWrapper, ITRGlbStaffWrapper
	{
		protected TRGlbStaffWrapper(GlbStaff staff)
			: base(staff)
		{
		}

		public static TRGlbStaffWrapper Get(GlbStaff staff)
		{
			return staff == null ? null : staff.Factory.GetCachedValue(staff.PK.ToString(), () => new TRGlbStaffWrapper(staff));
		}

		#region TRBPassword

		public GlbExternalPassword_TR TRBPassword
		{
			get
			{
				if (trbPassword == null)
				{
					trbPassword = GetGlbExternalPasswordOrCreateNew<GlbExternalPassword_TR>(PasswordTypesList.Codes.TRK, GlbCompany.CurrentCompany.PK);
					RegisterEditableChildObject(trbPassword);
				}

				return trbPassword;
			}
		}
		GlbExternalPassword_TR trbPassword;

		#endregion

		IGlbExternalPassword ITRGlbStaffWrapper.TRBPassword => TRBPassword;
	}
}
