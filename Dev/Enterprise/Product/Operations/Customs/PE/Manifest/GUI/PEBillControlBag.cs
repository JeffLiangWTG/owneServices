using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PE.Manifest.GUI
{
	public sealed class PEBillControlBag : ControlBag
	{
		public static PEBillControlBag Instance => billControlBag.Value;

		PEBillControlBag()
		{
			BillIssueDateEdit = RegisterControl(nameof(PEBillCountrySpecificUserControl.BillIssueDateEdit));
			CargoNatureDropEdit = RegisterControl(nameof(PEBillCountrySpecificUserControl.CargoNatureDropEdit));
			CargoConditionDropEdit = RegisterControl(nameof(PEBillCountrySpecificUserControl.CargoConditionDropEdit));
		}

		public ControlReference BillIssueDateEdit { get; }

		public ControlReference CargoNatureDropEdit { get; }

		public ControlReference CargoConditionDropEdit { get; }

		protected override Control CreateTemplate() => new PEBillCountrySpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<PEBillControlBag> billControlBag = new Lazy<PEBillControlBag>(() => new PEBillControlBag());
	}
}
