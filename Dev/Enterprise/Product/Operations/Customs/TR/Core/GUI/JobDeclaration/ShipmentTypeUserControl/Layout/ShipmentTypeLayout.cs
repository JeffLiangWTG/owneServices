using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public sealed class ShipmentTypeLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public ShipmentTypeLayout()
		{
			Layout = CreateShipmentTypeLayout();
		}

		static PanelLayout CreateShipmentTypeLayout()
		{
			var builder = new ShipmentTypeLayoutBuilder<Business.Declaration.JobDeclaration>();
			var commonBag = builder.CommonBag;
			var euBag = EU.GUI.ShipmentTypeControlBag.Instance;
			builder.AddControlBag(euBag);
			var trBag = ShipmentTypeControlBag.Instance;
			builder.AddControlBag(trBag);

			builder.AddColumn();
			builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.EntryStyleDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.Long);
			builder.Add(trBag.EntrySubStyleDropEdit, ControlWidthClass.Long);
			builder.Add(trBag.EntryDateForDutyDateEdit, ControlWidthClass.Long);
			builder.Add(trBag.BankCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
			builder.Add(trBag.DutyPaymentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(trBag.InspectionClerkTextBox, ControlWidthClass.Long);
			builder.Add(trBag.GoodsAtCustomsAreaCheckBox, ControlWidthClass.Long);
			builder.Add(trBag.OverTimePaymentCompletedCheckBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
