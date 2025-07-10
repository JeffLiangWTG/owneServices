using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

sealed class ShipmentTypeLayouts : IPanelLayoutProvider
{
	public PanelLayout Layout => ShipmentType;

	public ShipmentTypeLayouts()
	{
		ShipmentType = CreateShipmentTypeLayout();
	}

	static PanelLayout CreateShipmentTypeLayout()
	{
		var builder = new ShipmentTypeLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;

		var noBag = ShipmentTypeControlBag.Instance;
		builder.AddControlBag(noBag);

		builder.AddColumn();
		builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.MessageSubTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
		builder.Add(noBag.CustomsTransportModeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ContainerModeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);

		builder.SetVisibility(commonBag.ContainerModeDropEdit, h => !h.IsFixedInstallation && !h.IsNonTransportDeclarationType, h => h.JE_TransportModeInfo);

		return builder.Build();
	}

	PanelLayout ShipmentType { get; }
}
