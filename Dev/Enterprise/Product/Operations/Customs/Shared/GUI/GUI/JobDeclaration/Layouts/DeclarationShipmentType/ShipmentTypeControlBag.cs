using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class ShipmentTypeControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ShipmentTypeUserControl();

		public static ShipmentTypeControlBag Instance => instance ?? (instance = new ShipmentTypeControlBag());

		[ThreadStatic]
		static ShipmentTypeControlBag instance;

		ShipmentTypeControlBag()
		{
			ApplicationCodeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.ApplicationCodeDropEdit));
			ServiceLevelCodeFindBox = RegisterControl(nameof(ShipmentTypeUserControl.ServiceLevelCodeFindBox));
			MessageSubTypeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.MessageSubTypeDropEdit));
			MessageTypeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.MessageTypeDropEdit));
			CustomsProfileDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.CustomsProfileDropEdit));
			TransportModeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.TransportModeDropEdit));
			ContainerModeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.ContainerModeDropEdit));
			InlandModeOfTransportDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.InlandModeOfTransportDropEdit));
			DeclarantTypeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.DeclarantTypeDropEdit));
		}

		public ControlReference ApplicationCodeDropEdit;
		public ControlReference ServiceLevelCodeFindBox;
		public ControlReference MessageSubTypeDropEdit;
		public ControlReference MessageTypeDropEdit;
		public ControlReference CustomsProfileDropEdit;
		public ControlReference TransportModeDropEdit;
		public ControlReference ContainerModeDropEdit;
		public ControlReference InlandModeOfTransportDropEdit;
		public ControlReference DeclarantTypeDropEdit;
	}
}
