using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.UY;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class GlbCompanyWrapper : MasterFiles.Business.GlbCompanyWrapper, IUYGlbCompanyWrapper
	{
		protected GlbCompanyWrapper(GlbCompany company)
			: base(company)
		{
		}

		#region GlbExternalPassword

		[ChildEditable]
		public GlbCompanyCredential GlbExternalPassword
		{
			get
			{
				if (glbExternalPassword == null)
				{
					glbExternalPassword = GetGlbExternalPasswordOrCreateNew<GlbCompanyCredential>(PasswordTypesList.Codes.UTB);
					RegisterEditableChildObject(glbExternalPassword);
				}

				return glbExternalPassword;
			}
		}
		GlbCompanyCredential glbExternalPassword;

		#endregion

		IGlbExternalPassword IUYGlbCompanyWrapper.GlbExternalPassword => GlbExternalPassword;

		public override bool IsValidWrapper => true;
	}
}
