using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.UY.Manifest.GUI
{
	public class GlbCompanyWrapperProvider : MasterFiles.GUI.GlbCompanyWrapperProvider, MasterFiles.Integration.Customs.UY.IUYGlbCompanyWrapperProvider
	{
		public override IPanelLayoutProvider GetNewCompanyCredentialsLayout() => new CompanyCredentialsLayout();
	}
}
