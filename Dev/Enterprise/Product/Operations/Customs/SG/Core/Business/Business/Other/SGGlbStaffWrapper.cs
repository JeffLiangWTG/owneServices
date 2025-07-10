using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.SG;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGGlbStaffWrapper : GlbStaffWrapper, ISGGlbStaffWrapper
	{
		protected SGGlbStaffWrapper(GlbStaff staff)
			: base(staff)
		{
		}

		public static SGGlbStaffWrapper Get(GlbStaff staff)
		{
			return staff == null ? null : staff.Factory.GetCachedValue(staff.PK.ToString(), () => new SGGlbStaffWrapper(staff));
		}

		#region Tradenetv4Password

		public GlbExternalPassword_SGv4 Tradenetv4Password
		{
			get
			{
				if (tradenetv4Password == null)
				{
					tradenetv4Password = GetGlbExternalPasswordOrCreateNew<GlbExternalPassword_SGv4>(PasswordTypesList.Codes.SG4, GlbCompany.CurrentCompany.PK);
					RegisterEditableChildObject(tradenetv4Password);
				}

				return tradenetv4Password;
			}
		}
		GlbExternalPassword_SGv4 tradenetv4Password;

		#endregion

		#region SGNationalTradePlatform

		public GlbExternalPassword_SGNTP SGNationalTradePlatformPassword
		{
			get
			{
				if (sgNationalTradePlatformPassword == null)
				{
					sgNationalTradePlatformPassword = GetGlbExternalPasswordOrCreateNew<GlbExternalPassword_SGNTP>(PasswordTypesList.Codes.NTP, GlbCompany.CurrentCompany.PK);
					RegisterEditableChildObject(sgNationalTradePlatformPassword);
				}

				return sgNationalTradePlatformPassword;
			}
		}
		GlbExternalPassword_SGNTP sgNationalTradePlatformPassword;

		#endregion

		#region ACCESSPassword

		public GlbExternalPassword_SGA AccessPassword
		{
			get
			{
				if (accessPassword == null)
				{
					accessPassword = GetGlbExternalPasswordOrCreateNew<GlbExternalPassword_SGA>(PasswordTypesList.Codes.SGA, GlbCompany.CurrentCompany.PK);
					RegisterEditableChildObject(accessPassword);
				}

				return accessPassword;
			}
		}

		GlbExternalPassword_SGA accessPassword;

		#endregion

		IGlbExternalPassword ISGGlbStaffWrapper.Tradenetv4Password => Tradenetv4Password;

		IGlbExternalPassword ISGGlbStaffWrapper.SGNationalTradePlatformPassword => SGNationalTradePlatformPassword;

		IGlbExternalPassword ISGGlbStaffWrapper.AccessPassword => AccessPassword;
	}
}
