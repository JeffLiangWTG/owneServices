using Enterprise.MasterFiles.Integration.Customs.NO;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

public sealed class GlbCompanyWrapperProvider : MasterFiles.GUI.GlbCompanyWrapperProvider, INOGlbCompanyWrapperProvider
{
	public override IPanelLayoutProvider GetNewCompanyCredentialsLayout() => new CompanyCredentialsLayout();
}
