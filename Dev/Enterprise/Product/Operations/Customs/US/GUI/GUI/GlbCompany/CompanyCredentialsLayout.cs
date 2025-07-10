using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public class CompanyCredentialsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<Business.GlbCompanyWrapper>();
			var uSBag = CompanyCredentialsControlBag.Instance;
			var commonBag = builder.CommonBag;
			builder.AddControlBag(uSBag);

			builder.AddColumn();
			builder.Add(commonBag.ICS2CredentialUserControl, ControlWidthClass.Auto);
			builder.Add(uSBag.CredentialGroupBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
