using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class ShipmentsUserControl : ZUserControl
	{
		public ShipmentsUserControl()
		{
			InitializeComponent();
			ShipmentsGrid.ContextMenu.MenuItems.Add(0, new ZMenuItem(ResString.GetMultilingualString("a6228ef0-3ebc-4d1a-943d-13116f97a630", "Transfer Shipments From Other Trips"), OnTransferShipments_Click));
		}

		void OnTransferShipments_Click(object sender, EventArgs eventArgs)
		{
			var trip = CurrentDataItem as Trip;
			if (trip != null && SaveData(trip))
			{
				ZFormModaliser.ShowDialogAndDispose(new MultiSelectModuleForm(trip));
			}
		}

		bool SaveData(BusinessObject parent)
		{
			bool result = true;
			if (parent.HasChanges)
			{
				if (Globals.Message.Show(ResString.GetMultilingualString("40C10C5D-1532-40F6-8006-C321A32D9FEC", "The data has not yet been saved. Do you want to save and proceed?"),
						ResString.GetMultilingualString("D4B81755-418C-4220-8DCD-02C80148D109", "Save Data"), MessageBoxButtons.YesNo,
						DialogResult.No) == DialogResult.Yes)
				{
					var parentForm = this.ParentForm as ZForm;
					result = parentForm != null && parentForm.FireSaveButton() == ContinueWithSave.Yes;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}
	}
}
