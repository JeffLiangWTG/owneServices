using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public sealed class ExportTransportDetailsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; } = CreateLayout();

	static PanelLayout CreateLayout()
	{
		var builder = new ExportTransportDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var nlBag = NLTransportDetailsControlBag.Instance;
		builder.AddControlBag(nlBag);

		builder.AddColumn();
		builder.Add(commonBag.OverrideValuesCheckBox, ControlWidthClass.Long);
		builder.Add(commonBag.MasterBillTextBox, ControlWidthClass.Medium);
		builder.Add(commonBag.OceanBillTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.VesselCodeFindBox, ControlWidthClass.Auto);
		builder.Add(nlBag.FlightAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.VoyageAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.PortOfLoadingUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.PortOfDischargeUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportInlandSeparatorUserControl, ControlWidthClass.Auto);

		builder.Add(commonBag.TransportInlandModeAndTypeOfIdUserControl, ControlWidthClass.Auto);
		builder.Add(nlBag.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportInlandRoadUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportInlandIDAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportInlandSeaUserControl, ControlWidthClass.Auto);

		return builder.Build();
	}
}
