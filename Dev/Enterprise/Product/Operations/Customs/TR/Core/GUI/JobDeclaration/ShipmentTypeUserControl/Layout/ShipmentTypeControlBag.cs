using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public sealed class ShipmentTypeControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ShipmentTypeUserControl();

		public static ShipmentTypeControlBag Instance => instance ?? (instance = new ShipmentTypeControlBag());

		[ThreadStatic]
		static ShipmentTypeControlBag instance;

		ShipmentTypeControlBag()
		{
			EntrySubStyleDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.EntrySubStyleDropEdit));
			EntryDateForDutyDateEdit = RegisterControl(nameof(ShipmentTypeUserControl.EntryDateForDutyDateEdit));
			BankCodeFindBox = RegisterControl(nameof(ShipmentTypeUserControl.BankCodeFindBox));
			DutyPaymentTypeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.DutyPaymentTypeDropEdit));
			InspectionClerkTextBox = RegisterControl(nameof(ShipmentTypeUserControl.InspectionClerkTextBox));
			GoodsAtCustomsAreaCheckBox = RegisterControl(nameof(ShipmentTypeUserControl.GoodsAtCustomsAreaCheckBox));
			OverTimePaymentCompletedCheckBox = RegisterControl(nameof(ShipmentTypeUserControl.OverTimePaymentCompletedCheckBox));
		}

		public ControlReference EntrySubStyleDropEdit;
		public ControlReference EntryDateForDutyDateEdit;
		public ControlReference BankCodeFindBox;
		public ControlReference DutyPaymentTypeDropEdit;
		public ControlReference InspectionClerkTextBox;
		public ControlReference GoodsAtCustomsAreaCheckBox;
		public ControlReference OverTimePaymentCompletedCheckBox;
	}
}
