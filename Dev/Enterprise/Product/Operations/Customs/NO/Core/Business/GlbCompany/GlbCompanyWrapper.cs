using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.NO;

namespace Enterprise.Customs.NO.Business;

public sealed class GlbCompanyWrapper : MasterFiles.Business.GlbCompanyWrapper, INOGlbCompanyWrapper
{
	public GlbCompanyWrapper(GlbCompany company) : base(company)
	{
	}

	#region GlbExternalPassword

	[ChildEditable]
	public GlbExternalPassword_NOD Credential
	{
		get
		{
			if (credential == null)
			{
				credential = GetGlbExternalPasswordOrCreateNew<GlbExternalPassword_NOD>(PasswordTypesList.Codes.NOD);
				RegisterEditableChildObject(credential);
			}

			return credential;
		}
	}
	GlbExternalPassword_NOD credential;

	#endregion

	IGlbExternalPasswordWithCertificate INOGlbCompanyWrapper.Credential => Credential;

	public override bool IsValidWrapper => true;
}
