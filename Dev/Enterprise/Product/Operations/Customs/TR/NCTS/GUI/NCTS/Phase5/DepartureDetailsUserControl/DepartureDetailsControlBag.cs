using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public class DepartureDetailsControlBag : ControlBag
	{
		DepartureDetailsControlBag()
		{
			StampDutyStatusDropEdit = RegisterControl(nameof(DepartureDetailsUserControl.StampDutyStatusDropEdit));
			StampDutyCalcEdit = RegisterControl(nameof(DepartureDetailsUserControl.StampDutyCalcEdit));
			RegistrationDateEdit = RegisterControl(nameof(DepartureDetailsUserControl.RegistrationDateEdit));
			GoodsShippingLocationAndGIKUserControl = RegisterControl(nameof(DepartureDetailsUserControl.GoodsShippingLocationAndGIKUserControl));
		}

		public static DepartureDetailsControlBag Instance => instance ?? (instance = new DepartureDetailsControlBag());

		public ControlReference StampDutyStatusDropEdit { get; }
		public ControlReference StampDutyCalcEdit { get; }
		public ControlReference RegistrationDateEdit { get; }
		public ControlReference GoodsShippingLocationAndGIKUserControl { get; }

		protected override Control CreateTemplate() => new DepartureDetailsUserControl();

		[ThreadStatic]
		static DepartureDetailsControlBag instance;
	}
}
