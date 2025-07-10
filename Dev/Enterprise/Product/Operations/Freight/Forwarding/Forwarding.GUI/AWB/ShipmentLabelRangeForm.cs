using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class ShipmentLabelRangeForm : ZChildForm
	{
		public ShipmentLabelRangeForm(ShipmentAWBActions shipmentAWBActions)
			: base(shipmentAWBActions)
		{
			InitializeComponent();

			Init();
		}

		ShipmentAWBActions AWBActions
		{
			get { return (ShipmentAWBActions)BusinessEntity; }
		}

		void Init()
		{
			okButton.Click += (s, e) =>
					{
						if (AWBActions.HasErrors)
						{
							ShowErrorsDialog();
						}
						else
						{
							AWBActions.PrintAWBBarcodeLabel();
							AWBActions.SaveSettings();
							DialogResult = DialogResult.OK;
						}
					};

			cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;
		}
	}
}
