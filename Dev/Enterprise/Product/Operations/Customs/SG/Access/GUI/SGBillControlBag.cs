using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class SGBillControlBag : ControlBag
	{
		public static SGBillControlBag Instance => billControlBag.Value;

		SGBillControlBag()
		{
			CycleNumberDropEditWithFixedWidth = RegisterControl(nameof(SGBillSpecificUserControl.CycleNumberDropEditWithFixedWidth));
			CycleDateDateEdit = RegisterControl(nameof(SGBillSpecificUserControl.CycleDateDateEdit));
			SGPayeeIndicatorDropEdit = RegisterControl(nameof(SGBillSpecificUserControl.SGPayeeIndicatorDropEdit));
			SGPartyStatusDropEdit = RegisterControl(nameof(SGBillSpecificUserControl.SGPartyStatusDropEdit));
			SGPartyIDTextBox = RegisterControl(nameof(SGBillSpecificUserControl.SGPartyIDTextBox));
			SGGstAmountCalcEdit = RegisterControl(nameof(SGBillSpecificUserControl.SGGstAmountCalcEdit));
			SGDutyAmountCalcEdit = RegisterControl(nameof(SGBillSpecificUserControl.SGDutyAmountCalcEdit));
			MessageStatusDescriptionTextBox = RegisterControl(nameof(SGBillSpecificUserControl.MessageStatusDescriptionTextBox));
			GSTNReferenceNoTextBox = RegisterControl(nameof(SGBillSpecificUserControl.GSTNReferenceNoTextBox));
		}

		public ControlReference CycleNumberDropEditWithFixedWidth { get; }
		public ControlReference CycleDateDateEdit { get; }
		public ControlReference SGPayeeIndicatorDropEdit { get; }
		public ControlReference SGPartyStatusDropEdit { get; }
		public ControlReference SGPartyIDTextBox { get; }
		public ControlReference SGGstAmountCalcEdit { get; }
		public ControlReference SGDutyAmountCalcEdit { get; }
		public ControlReference MessageStatusDescriptionTextBox { get; }
		public ControlReference GSTNReferenceNoTextBox { get; }

		protected override Control CreateTemplate() => new SGBillSpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<SGBillControlBag> billControlBag = new Lazy<SGBillControlBag>(() => new SGBillControlBag());
	}
}
