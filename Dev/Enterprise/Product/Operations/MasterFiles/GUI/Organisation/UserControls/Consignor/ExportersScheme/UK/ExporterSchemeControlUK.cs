using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.GUI
{
	public class ExporterSchemeControlUK : AddressLevelExporterSchemeControl
	{
		public ExporterSchemeControlUK() : base()
		{
		}

		protected override ResourceStringData GroupBoxCaption => Res.GetData("ExporterSchemeControlUK|244a0f29-7384-4039-bac9-f1c32f87bc80", "The details entered here do not affect the calculation of known status in the Shipment Inspection field.");
	}
}
