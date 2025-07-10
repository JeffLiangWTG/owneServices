using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class ShipmentTypeLayout : IPanelLayoutProvider
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
		var euBag = ShipmentTypeControlBag.Instance;
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
		builder.Add(euBag.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);

		var dropEditSizeBehaviour = new ZDropEditSizeBehaviour(preBoundMaxLength: 3);
		builder.AddControlBehaviour(commonBag.MessageTypeDropEdit, dropEditSizeBehaviour);
		builder.AddControlBehaviour(euBag.EntryStyleDropEdit, dropEditSizeBehaviour);
		builder.AddControlBehaviour(commonBag.TransportModeDropEdit, dropEditSizeBehaviour);
		builder.AddControlBehaviour(euBag.BorderTransportMeansDropEdit, dropEditSizeBehaviour);
		builder.AddControlBehaviour(commonBag.ContainerModeDropEdit, dropEditSizeBehaviour);
		builder.AddControlBehaviour(euBag.CTStatusIDDropEdit, dropEditSizeBehaviour);
		builder.AddControlBehaviour(euBag.SpecificCircumstanceDropEdit, dropEditSizeBehaviour);
		builder.AddControlBehaviour(commonBag.ApplicationCodeDropEdit, dropEditSizeBehaviour);

		return builder.Build();
	}
}
