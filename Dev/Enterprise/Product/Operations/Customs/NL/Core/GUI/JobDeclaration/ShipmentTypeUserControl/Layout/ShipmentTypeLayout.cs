using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public sealed class ShipmentTypeLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public ShipmentTypeLayout()
	{
		Layout = CreateShipmentTypeLayout();
	}

	static PanelLayout CreateShipmentTypeLayout()
	{
		var builder = new ShipmentTypeLayoutBuilder();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.ShipmentTypeControlBag.Instance;
		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.EntryStyleDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.IsHighValueOvrdCheckBox, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.BorderTransportMeansDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ContainerModeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.CTStatusIDDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.SecurityDropEdit, ControlWidthClass.Long);

		return builder.Build();
	}
}
