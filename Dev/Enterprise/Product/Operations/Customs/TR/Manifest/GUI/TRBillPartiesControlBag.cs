using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI
{
	class TRBillPartiesControlBag : ControlBag
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "There is no way to change static backing field of this property.")]
		public static TRBillPartiesControlBag Instance { get; } = new TRBillPartiesControlBag();

		TRBillPartiesControlBag()
		{
			ToOrderCheckBox = RegisterControl(nameof(TRBillPartiesCountrySpecificUserControl.ToOrderCheckBox));
			NotOwnedCheckBox = RegisterControl(nameof(TRBillPartiesCountrySpecificUserControl.NotOwnedCheckBox));
		}

		protected override Control CreateTemplate() => new TRBillPartiesCountrySpecificUserControl();

		public ControlReference ToOrderCheckBox { get; }

		public ControlReference NotOwnedCheckBox { get; }
	}
}
