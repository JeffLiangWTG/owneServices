using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI
{
	public class TRBillControlBag : ControlBag
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "There is no way to change static backing field of this property.")]
		public static TRBillControlBag Instance { get; } = new TRBillControlBag();

		TRBillControlBag()
		{
			RoRoCheckBox = RegisterControl(nameof(TRBillCountrySpecificUserControl.RoRoCheckBox));
			PaymentTypeDropEdit = RegisterControl(nameof(TRBillCountrySpecificUserControl.PaymentTypeDropEdit));
			TransshipmentTypeDropEdit = RegisterControl(nameof(TRBillCountrySpecificUserControl.TransshipmentTypeDropEdit));
			BillStampDutyValueCalcEdit = RegisterControl(nameof(TRBillCountrySpecificUserControl.BillStampDutyValueCalcEdit));
			AirBillStampDutyABSValueCalcEdit = RegisterControl(nameof(TRBillCountrySpecificUserControl.AirBillStampDutyABSValueCalcEdit));
		}

		protected override Control CreateTemplate() => new TRBillCountrySpecificUserControl();

		public ControlReference RoRoCheckBox { get; }
		public ControlReference PaymentTypeDropEdit { get; }
		public ControlReference TransshipmentTypeDropEdit { get; }
		public ControlReference BillStampDutyValueCalcEdit { get; }
		public ControlReference AirBillStampDutyABSValueCalcEdit { get; }
	}
}
