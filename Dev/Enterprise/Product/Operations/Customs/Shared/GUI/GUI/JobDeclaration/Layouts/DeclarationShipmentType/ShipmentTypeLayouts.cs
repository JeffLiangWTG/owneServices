using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class ShipmentTypeLayouts : IPanelLayoutProvider
	{
		PanelLayout ShipmentType { get; }

		PanelLayout IPanelLayoutProvider.Layout => ShipmentType;

		public ShipmentTypeLayouts()
		{
			ShipmentType = CreateShipmentTypeLayout();
		}

		PanelLayout CreateShipmentTypeLayout()
		{
			var builder = new ShipmentTypeLayoutBuilder<BaseJobDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.MessageSubTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
