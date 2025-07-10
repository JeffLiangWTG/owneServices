using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public class GlbCompanyWrapperProvider : MasterFiles.GUI.GlbCompanyWrapperProvider, MasterFiles.Integration.Customs.US.IUSGlbCompanyWrapperProvider
	{
		public override IPanelLayoutProvider GetNewCompanyCredentialsLayout() => new CompanyCredentialsLayout();
	}
}
