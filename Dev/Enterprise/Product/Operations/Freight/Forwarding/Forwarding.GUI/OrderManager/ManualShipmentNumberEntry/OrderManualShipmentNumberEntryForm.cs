using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class OrderManualShipmentNumberEntryForm : ZChildForm
	{
		public OrderManualShipmentNumberEntryForm(ShipmentNumberEntry businessObject)
			: base(businessObject)
		{
			InitializeComponent();
		}

		ShipmentNumberEntry Entry
		{
			get { return (ShipmentNumberEntry)BusinessEntity; }
		}

		#region OkButton_Click

		void OkButton_Click(object sender, System.EventArgs e)
		{
			HandleOkButtonClick();
		}

		void HandleOkButtonClick()
		{
			BusinessEntity.RunPreSaveValidation();

			if (Entry.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("d7d1719f-bfda-46fa-a714-4ca58b9d8b31", "There are errors on this form that need to be fixed first"), Res.GetString("8b801efc-287c-47ee-882f-fb3f3e4d9ab9", "Error"));
			}
			else
			{
				Entry.Accept();
				Close();
			}
		}

		#endregion

		#region CancelButton_Click

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
