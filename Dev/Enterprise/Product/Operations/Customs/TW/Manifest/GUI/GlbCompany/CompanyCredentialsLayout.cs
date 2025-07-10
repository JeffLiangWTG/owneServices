using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public class CompanyCredentialsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<TWGlbCompanyWrapper>();
			var tWBag = CompanyCredentialsControlBag.Instance;
			var commonBag = builder.CommonBag;
			builder.AddControlBag(tWBag);

			builder.AddColumn();
			builder.Add(tWBag.LicensingCertificateUserControl, ControlWidthClass.LongControl);
			builder.Add(tWBag.ForwarderCertificateUserControl, ControlWidthClass.LongControl);
			builder.Add(commonBag.ICS2CredentialUserControl, ControlWidthClass.LongControl);

			return builder.Build();
		}
	}
}
