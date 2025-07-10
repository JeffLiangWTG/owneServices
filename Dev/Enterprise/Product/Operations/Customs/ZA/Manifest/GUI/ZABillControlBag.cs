using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Manifest.GUI
{
	public class ZABillControlBag : ControlBag
	{
		public static ZABillControlBag Instance => billControlBag.Value;

		ZABillControlBag()
		{
			CargoReleaseStatusOtherDescriptionTextBox = RegisterControl(nameof(ZABillSpecificUserControl.CargoReleaseStatusOtherDescriptionTextBox));
			CargoReleaseStatusDropEdit = RegisterControl(nameof(ZABillSpecificUserControl.CargoReleaseStatusDropEdit));
			CaseNumberBillsGroupBox = RegisterControl(nameof(ZABillSpecificUserControl.CaseNumberBillsGroupBox));
		}

		public ControlReference CargoReleaseStatusOtherDescriptionTextBox { get; }
		public ControlReference CargoReleaseStatusDropEdit { get; }
		public ControlReference CaseNumberBillsGroupBox { get; }

		protected override Control CreateTemplate() => new ZABillSpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<ZABillControlBag> billControlBag = new Lazy<ZABillControlBag>(() => new ZABillControlBag());
	}
}
