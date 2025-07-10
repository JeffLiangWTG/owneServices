using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class CompanyCredentialsLayout : IPanelLayoutProvider
	{
		public CompanyCredentialsLayout()
		{
			Layout = CreateCompanyCredentialsLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout CreateCompanyCredentialsLayout()
		{
			var builder = new CompanyCredentialsLayoutBuilder<Business.GlbCompanyWrapper>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.ICS2CredentialUserControl, ControlWidthClass.LongControl);

			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;
	}
}
