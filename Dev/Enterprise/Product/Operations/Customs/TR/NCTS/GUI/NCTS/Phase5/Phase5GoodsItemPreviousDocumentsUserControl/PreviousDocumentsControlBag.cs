using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public class PreviousDocumentsControlBag : ControlBag
	{
		public PreviousDocumentsControlBag()
		{
			AmountCalcDropEdit = RegisterControl(nameof(PreviousDocumentsUserControl.AmountCalcDropEdit));
			CountryCodeFindBox = RegisterControl(nameof(PreviousDocumentsUserControl.CountryCodeFindBox));
			PrevDocsTypeDropEdit = RegisterControl(nameof(PreviousDocumentsUserControl.PrevDocsTypeDropEdit));
			PaymentTypeDropEdit = RegisterControl(nameof(PreviousDocumentsUserControl.PaymentTypeDropEdit));
			NatureOfBussinessDropEdit = RegisterControl(nameof(PreviousDocumentsUserControl.NatureOfBussinessDropEdit));
		}

		public static PreviousDocumentsControlBag Instance => instance ?? (instance = new PreviousDocumentsControlBag());

		public ControlReference AmountCalcDropEdit { get; }
		public ControlReference CountryCodeFindBox { get; }
		public ControlReference PrevDocsTypeDropEdit { get; }
		public ControlReference PaymentTypeDropEdit { get; }
		public ControlReference NatureOfBussinessDropEdit { get; }

		protected override Control CreateTemplate() => new PreviousDocumentsUserControl();

		[ThreadStatic]
		static PreviousDocumentsControlBag instance;
	}
}
