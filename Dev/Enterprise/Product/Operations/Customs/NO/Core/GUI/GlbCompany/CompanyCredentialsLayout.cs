using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

sealed class CompanyCredentialsLayout : IPanelLayoutProvider
{
	PanelLayout Layout { get; } = CreateLayout();
	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<GlbCompanyWrapper>();
		var noBag = CompanyCredentialsControlBag.Instance;
		builder.AddControlBag(noBag);

		builder.AddColumn();
		builder.Add(noBag.CompanyCredentialsDetailsUserControl, ControlWidthClass.Auto);

		return builder.Build();
	}
}
