using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public sealed class TWBillControlBag : ControlBag
	{
		public static TWBillControlBag Instance => billControlBag.Value;

		TWBillControlBag()
		{
			SequenceNumberCalcEdit = RegisterControl(nameof(TWBillCountrySpecificUserControl.SequenceNumberCalcEdit));
			RemarksLongTextControl = RegisterControl(nameof(TWBillCountrySpecificUserControl.RemarksLongTextControl));
			ProcedureDropEdit = RegisterControl(nameof(TWBillCountrySpecificUserControl.ProcedureDropEdit));
			ManifestQtyCalcDropEdit = RegisterControl(nameof(TWBillCountrySpecificUserControl.ManifestQtyCalcDropEdit));
			GrossWeightCalcDropEdit = RegisterControl(nameof(TWBillCountrySpecificUserControl.GrossWeightCalcDropEdit));
			GoodsValueConvertToLocalCurrencyControl = RegisterControl(nameof(TWBillCountrySpecificUserControl.GoodsValueConvertToLocalCurrencyControl));
			PortOfLoadingCodeFindBox = RegisterControl(nameof(TWBillCountrySpecificUserControl.PortOfLoadingCodeFindBox));
			PortOfDischargeCodeFindBox = RegisterControl(nameof(TWBillCountrySpecificUserControl.PortOfDischargeCodeFindBox));
		}

		public ControlReference SequenceNumberCalcEdit { get; }

		public ControlReference RemarksLongTextControl { get; }

		public ControlReference ProcedureDropEdit { get; }

		public ControlReference ManifestQtyCalcDropEdit { get; }

		public ControlReference GrossWeightCalcDropEdit { get; }

		public ControlReference GoodsValueConvertToLocalCurrencyControl { get; }

		public ControlReference PortOfLoadingCodeFindBox { get; }

		public ControlReference PortOfDischargeCodeFindBox { get; }

		protected override Control CreateTemplate() => new TWBillCountrySpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<TWBillControlBag> billControlBag = new Lazy<TWBillControlBag>(() => new TWBillControlBag());
	}
}
