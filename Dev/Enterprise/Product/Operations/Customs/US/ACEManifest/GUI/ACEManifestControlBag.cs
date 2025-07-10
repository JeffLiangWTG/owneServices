using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public class ACEManifestControlBag : ControlBag
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "There is no way to change static backing field of this property.")]
		public static ACEManifestControlBag Instance { get; } = new ACEManifestControlBag();

		ACEManifestControlBag()
		{
			EstDateAtFirstArrivalDateEdit = RegisterControl(nameof(ACEManifestCountrySpecificUserControl.EstDateAtFirstArrivalDateEdit));
			BillStatusTextBox = RegisterControl(nameof(ACEManifestCountrySpecificUserControl.BillStatusTextBox));
			BillStatusDescriptionTextBox = RegisterControl(nameof(ACEManifestCountrySpecificUserControl.BillStatusDescriptionTextBox));
			FIRMSTextBox = RegisterControl(nameof(ACEManifestCountrySpecificUserControl.FIRMSTextBox));
			ExpressCourierCheckBox = RegisterControl(nameof(ACEManifestCountrySpecificUserControl.ExpressCourierCheckBox));
		}

		protected override Control CreateTemplate() => new ACEManifestCountrySpecificUserControl();

		public ControlReference EstDateAtFirstArrivalDateEdit { get; }
		public ControlReference BillStatusTextBox { get; }
		public ControlReference BillStatusDescriptionTextBox { get; }
		public ControlReference FIRMSTextBox { get; }
		public ControlReference ExpressCourierCheckBox {  get; }
	}
}
