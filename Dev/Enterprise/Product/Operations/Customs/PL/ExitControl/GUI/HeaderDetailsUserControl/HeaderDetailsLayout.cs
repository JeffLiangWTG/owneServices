using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.ExitControl.GUI;

public sealed class HeaderDetailsLayout : IPanelLayoutProvider
{
	public HeaderDetailsLayout()
	{
		Layout = CreateHeaderDetailsLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	PanelLayout CreateHeaderDetailsLayout()
	{
		var builder = new EU.ExitControl.GUI.HeaderDetailsLayoutBuilder<Business.CusExitHeader>();
		var commonBag = builder.CommonBag;
		var plBag = HeaderDetailsControlBag.Instance;
		builder.AddControlBag(plBag);

		builder.AddColumn();
		builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ExporterOrgAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.CarrierAddressWithContactControl, ControlWidthClass.Auto);

		builder.Add(plBag.BrokerCodeFindBox, ControlWidthClass.Auto);
		builder.Add(plBag.CertificateDropEdit, ControlWidthClass.Auto);
		builder.Add(plBag.TrainingCheckBox, ControlWidthClass.Long);
		builder.Add(plBag.StoringFlagCheckBox, ControlWidthClass.Long);

		var visiablity = false;
		builder.SetVisibility(plBag.BrokerCodeFindBox, isVisible => visiablity);
		builder.SetVisibility(plBag.CertificateDropEdit, isVisible => visiablity);
		builder.SetVisibility(plBag.TrainingCheckBox, isVisible => visiablity);

		return builder.Build();
	}
}
