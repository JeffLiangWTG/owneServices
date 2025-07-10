using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public class USExportManifestBillControlBag : ControlBag
	{
		public static USExportManifestBillControlBag Instance => usExportManifestBillControlBag.Value;

		USExportManifestBillControlBag()
		{
			BoardedQuantityCalcEdit = RegisterControl(nameof(USExportManifestBillUserControl.BoardedQuantityCalcEdit));
			BoardedWeightCalcDropEdit = RegisterControl(nameof(USExportManifestBillUserControl.BoardedWeightCalcDropEdit));
			PriorTransportationModeDropEdit = RegisterControl(nameof(USExportManifestBillUserControl.PriorTransportationModeDropEdit));
			FinalDestinationPortUserControl = RegisterControl(nameof(USExportManifestBillUserControl.FinalDestinationPortUserControl));
			ArrivalPortUserControl = RegisterControl(nameof(USExportManifestBillUserControl.ArrivalPortUserControl));
			DeparturePortUserControl = RegisterControl(nameof(USExportManifestBillUserControl.DeparturePortUserControl));
			LadingPortUserControl = RegisterControl(nameof(USExportManifestBillUserControl.LadingPortUserControl));
			UnladingPortUserControl = RegisterControl(nameof(USExportManifestBillUserControl.UnladingPortUserControl));
			OriginPortUserControl = RegisterControl(nameof(USExportManifestBillUserControl.OriginPortUserControl));
			SpecialCargoCodesDropEdit = RegisterControl(nameof(USExportManifestBillUserControl.SpecialCargoCodesDropEdit));
			PlaceOfReceiptTextBox = RegisterControl(nameof(USExportManifestBillUserControl.PlaceOfReceiptTextBox));
			AESExemptionCodeTextBox = RegisterControl(nameof(USExportManifestBillUserControl.AESExemptionCodeTextBox));
			AESITNNumbersUserControl = RegisterControl(nameof(USExportManifestBillUserControl.AESITNNumbersUserControl));
			InBondNumbersUserControl = RegisterControl(nameof(USExportManifestBillUserControl.InBondNumbersUserControl));
		}

		public ControlReference BoardedQuantityCalcEdit { get; }
		public ControlReference BoardedWeightCalcDropEdit { get; }
		public ControlReference PriorTransportationModeDropEdit { get; }

		public ControlReference FinalDestinationPortUserControl { get; }
		public ControlReference ArrivalPortUserControl { get; }
		public ControlReference DeparturePortUserControl { get; }
		public ControlReference LadingPortUserControl { get; }
		public ControlReference UnladingPortUserControl { get; }
		public ControlReference OriginPortUserControl { get; }
		public ControlReference SpecialCargoCodesDropEdit { get; }
		public ControlReference PlaceOfReceiptTextBox { get; }
		public ControlReference AESExemptionCodeTextBox { get; }
		public ControlReference AESITNNumbersUserControl {  get; }
		public ControlReference InBondNumbersUserControl { get; }

		protected override Control CreateTemplate() => new USExportManifestBillUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<USExportManifestBillControlBag> usExportManifestBillControlBag = new Lazy<USExportManifestBillControlBag>(() => new USExportManifestBillControlBag());
	}
}
