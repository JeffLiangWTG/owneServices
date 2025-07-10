using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentNumberEntryForm : ZChildForm
	{
		#region Construction

#if DEBUG
		public
#endif
 ShipmentNumberEntryForm(ShipmentNumberEntry entry)
			: base(entry)
		{
		}

		public static bool ShowForm(ForwardingShipment shipment)
		{
			ShipmentNumberEntry entry = new ShipmentNumberEntry(shipment);
			ZFormModaliser.ShowDialogAndDispose(new ShipmentNumberEntryForm(entry));
			return entry.Accepted;
		}

		ShipmentNumberEntry Entry
		{
			get { return (ShipmentNumberEntry)BusinessEntity; }
		}

		#endregion

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
				Globals.Message.ShowError(Res.GetString("a27144a5-fe43-490d-aa34-eec958838b0a", "There are errors on this form that need to be fixed first"), Res.GetString("60688c9e-f0c2-4d3f-be58-783a81db73ea", "Error"));
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
