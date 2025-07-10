using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.UY.Manifest.GUI
{
	public class CompanyCredentialsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<Business.GlbCompanyWrapper>();
			var uYBag = CompanyCredentialsControlBag.Instance;
			var commonBag = builder.CommonBag;
			builder.AddControlBag(uYBag);

			builder.AddColumn();
			builder.Add(commonBag.ICS2CredentialUserControl, ControlWidthClass.LongControl);
			builder.Add(uYBag.CompanyCredentialsDetailsUserControl, ControlWidthClass.LongControl);

			return builder.Build();
		}
	}
}
