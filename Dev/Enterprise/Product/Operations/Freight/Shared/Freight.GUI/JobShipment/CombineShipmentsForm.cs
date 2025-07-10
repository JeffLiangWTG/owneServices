using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class CombineShipmentsForm : ZChildForm
	{
		public CombineShipmentsForm(CombineShipmentsHelper helper)
			: base(helper)
		{
			InitializeComponent();
			Helper = helper;
		}

		public readonly CombineShipmentsHelper Helper;

		#region Buttons

		void CombineButton_Click(object sender, System.EventArgs e)
		{
			if (ShipmentsGrid.SelectedElements.Length > 0)
			{
				CommonShipment[] selectedShipments = ShipmentsGrid.GetSelectedElements<CommonShipment>();
				if (selectedShipments != null)
				{
					string message = Helper.CombineSelectedShipments(selectedShipments);
					if (string.IsNullOrEmpty(message))
					{
						this.Close();
					}
					else
					{
						string caption = Res.GetString("fbcacf0a-dbed-4883-b7ac-43c8b8d21508", "Can't combine");
						Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("c2ee0766-7fdb-4762-a95a-482c73a95f93", "Please select shipments to combine."), Res.GetString("cc2cb6ac-fa00-4f17-b8f7-82e24cb51329", "Combine Shipments"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		#endregion
	}
}
