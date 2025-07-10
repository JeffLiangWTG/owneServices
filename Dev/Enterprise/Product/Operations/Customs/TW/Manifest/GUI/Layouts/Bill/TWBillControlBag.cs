using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public sealed class TWBillControlBag : ControlBag
	{
		public static TWBillControlBag Instance => billControlBag.Value;

		TWBillControlBag()
		{
			PortOfLoadingCodeFindBox = RegisterControl(nameof(TWBillCountrySpecificUserControl.PortOfLoadingCodeFindBox));
			GoodsDescriptionLongTextControl = RegisterControl(nameof(TWBillCountrySpecificUserControl.GoodsDescriptionLongTextControl));
			BagNumberDropEdit = RegisterControl(nameof(TWBillCountrySpecificUserControl.BagNumberDropEdit));
			ManifestQtyCalcDropEdit = RegisterControl(nameof(TWBillCountrySpecificUserControl.ManifestQtyCalcDropEdit));
			SplitQuantityCalcDropEdit = RegisterControl(nameof(TWBillCountrySpecificUserControl.SplitQuantityCalcDropEdit));
			MarksAndNumbersLongTextControl = RegisterControl(nameof(TWBillCountrySpecificUserControl.MarksAndNumbersLongTextControl));
			TariffFindBox = RegisterControl(nameof(TWBillCountrySpecificUserControl.TariffFindBox));
			DGUNNOCodeFindBox = RegisterControl(nameof(TWBillCountrySpecificUserControl.DGUNNOCodeFindBox));
			IsEscortRequiredCheckBox = RegisterControl(nameof(TWBillCountrySpecificUserControl.IsEscortRequiredCheckBox));
		}

		public ControlReference PortOfLoadingCodeFindBox { get; }

		public ControlReference GoodsDescriptionLongTextControl { get; }

		public ControlReference BagNumberDropEdit { get; }

		public ControlReference ManifestQtyCalcDropEdit { get; }

		public ControlReference SplitQuantityCalcDropEdit { get; }

		public ControlReference MarksAndNumbersLongTextControl { get; }

		public ControlReference TariffFindBox { get; }

		public ControlReference DGUNNOCodeFindBox { get; }

		public ControlReference IsEscortRequiredCheckBox { get; }

		protected override Control CreateTemplate() => new TWBillCountrySpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<TWBillControlBag> billControlBag = new Lazy<TWBillControlBag>(() => new TWBillControlBag());
	}
}
