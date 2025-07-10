using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public class ACEBillControlBag : ControlBag
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "There is no way to change static backing field of this property.")]
		public static ACEBillControlBag Instance { get; } = new ACEBillControlBag();

		ACEBillControlBag()
		{
			FDAIndicatorCheckBox = RegisterControl(nameof(ACEManifestBillSpecificUserControl.FDAIndicatorCheckBox));
			BillStatusTextBox = RegisterControl(nameof(ACEManifestBillSpecificUserControl.BillStatusTextBox));
			BillStatusDescriptionTextBox = RegisterControl(nameof(ACEManifestBillSpecificUserControl.BillStatusDescriptionTextBox));
			GoodsValueConvertToLocalCurrencyControl = RegisterControl(nameof(ACEManifestBillSpecificUserControl.GoodsValueConvertToLocalCurrencyControl));
			EntryNumberTypeDropEdit = RegisterControl(nameof(ACEManifestBillSpecificUserControl.EntryNumberTypeDropEdit));
			EntryNumberTextBox = RegisterControl(nameof(ACEManifestBillSpecificUserControl.EntryNumberTextBox));
			GoodsOriginCodeFindBox = RegisterControl(nameof(ACEManifestBillSpecificUserControl.GoodsOriginCodeFindBox));
			TariffCodeFindBox = RegisterControl(nameof(ACEManifestBillSpecificUserControl.TariffCodeFindBox));
		}

		protected override Control CreateTemplate() => new ACEManifestBillSpecificUserControl();

		public ControlReference FDAIndicatorCheckBox { get; }
		public ControlReference BillStatusTextBox { get; }
		public ControlReference BillStatusDescriptionTextBox { get; }
		public ControlReference GoodsValueConvertToLocalCurrencyControl { get; }
		public ControlReference EntryNumberTypeDropEdit { get; }
		public ControlReference EntryNumberTextBox { get; }
		public ControlReference GoodsOriginCodeFindBox { get; }
		public ControlReference TariffCodeFindBox { get; }
	}
}
