using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public partial class TransferHeaderItemSelectionDialog : ASYCUDA.GUI.AsycudaItemSelectionDialog
	{
		[Obsolete("This constructor is just for the designer")]
		public TransferHeaderItemSelectionDialog() : base()
		{
			InitializeComponent();
		}

		public TransferHeaderItemSelectionDialog(TransferHeaderMessageChooser messageChooser, string itemsType)
			: base(messageChooser, itemsType)
		{
			InitializeComponent();
			AddColumnsForArrivalMessage();
			InitializeColumns();
			HideSelectAllButton();
		}

		void InitializeColumns()
		{
			var descriptionColumn = ItemsGrid.GetColumnStyle("Description");
			ItemsGrid.ColumnStyles.Remove(descriptionColumn);

			var flightNoColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			flightNoColumn.ColumnName = nameof(TransferHeaderMessageChooserItem.VoyageFlightNo);
			flightNoColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			flightNoColumn.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("0C7297B6-91A3-4426-909D-90B2AF5CD112", "Flight No.");

			var etaColumn = new ZArchitecture.ZDateEditColumnStyleInfo();
			etaColumn.ColumnName = nameof(TransferHeaderMessageChooserItem.ETAAtDischargePort);
			etaColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			etaColumn.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("32DFF88C-31F8-4200-B2E1-CF39BB9D998A", "ETA");

			var referenceColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			referenceColumn.ColumnName = nameof(TransferHeaderMessageChooserItem.ArrivalReference);
			referenceColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			referenceColumn.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("BB8F8EDA-09BC-408C-8F3F-2596EFC43876", "Reference");

			var destinationPortColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			destinationPortColumn.ColumnName = nameof(TransferHeaderMessageChooserItem.DestinationPort);
			destinationPortColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			destinationPortColumn.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("EA8E35D0-0B8B-44F5-BB40-235D54553B45", "Destination Port");

			var transferTypeColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			transferTypeColumn.ColumnName = nameof(TransferHeaderMessageChooserItem.TransferType);
			transferTypeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			transferTypeColumn.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("96CA671C-9454-4E69-8B3D-84D0788C36F3", "Transfer Type");

			var inBondCarrierColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			inBondCarrierColumn.ColumnName = nameof(TransferHeaderMessageChooserItem.InBondCarrier);
			inBondCarrierColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			inBondCarrierColumn.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("4820F98C-53AA-4916-BA13-823D2F665848", "In-Bond Carrier");

			var inBondCarrierIDColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			inBondCarrierIDColumn.ColumnName = nameof(TransferHeaderMessageChooserItem.InBondCarrierID);
			inBondCarrierIDColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			inBondCarrierIDColumn.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("026408B6-3E75-4A36-B77E-E12DF77D7AF4", "In-Bond Carrier ID");

			var onwardCarrierColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			onwardCarrierColumn.ColumnName = nameof(TransferHeaderMessageChooserItem.OnwardCarrier);
			onwardCarrierColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			onwardCarrierColumn.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("809C6A1E-D030-4F55-8694-B31F1C36A833", "Onward Carrier");

			var bondedPremisesColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			bondedPremisesColumn.ColumnName = nameof(TransferHeaderMessageChooserItem.BondedPremises);
			bondedPremisesColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			bondedPremisesColumn.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("304BF025-C709-4EC4-A9FC-578B974B6D32", "Bonded Premises");

			var bondedPremisesIDColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			bondedPremisesIDColumn.ColumnName = nameof(TransferHeaderMessageChooserItem.BondedPremisesID);
			bondedPremisesIDColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			bondedPremisesIDColumn.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("DA485F29-E0E1-4025-85F5-3D4F431C239A", "Bonded Premises ID");

			ItemsGrid.ColumnStyles.Add(flightNoColumn);
			ItemsGrid.ColumnStyles.Add(etaColumn);
			ItemsGrid.ColumnStyles.Add(referenceColumn);
			ItemsGrid.ColumnStyles.Add(destinationPortColumn);
			ItemsGrid.ColumnStyles.Add(transferTypeColumn);
			ItemsGrid.ColumnStyles.Add(inBondCarrierColumn);
			ItemsGrid.ColumnStyles.Add(inBondCarrierIDColumn);
			ItemsGrid.ColumnStyles.Add(onwardCarrierColumn);
			ItemsGrid.ColumnStyles.Add(bondedPremisesColumn);
			ItemsGrid.ColumnStyles.Add(bondedPremisesIDColumn);
		}

		void AddColumnsForArrivalMessage()
		{
			if (BusinessEntity.IsArrivalMessage)
			{
				var statusCodeColumn = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
				statusCodeColumn.ColumnName = nameof(TransferHeaderMessageChooserItem.StatusCode);
				statusCodeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
				statusCodeColumn.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("3A2A71B6-6278-42AE-A27F-E790A10E8A4B", "Status Code");

				ItemsGrid.ColumnStyles.Add(statusCodeColumn);
			}
		}

		void HideSelectAllButton()
		{
			DeselectAllButton.Visible = false;
			SelectAllButton.Visible = false;
		}

		protected override bool IsValidToSend()
		{
			var result = base.IsValidToSend();

			if (result)
			{
				BusinessEntity.RunPreSaveValidation();
				if (BusinessEntity.HasErrors)
				{
					result = false;
					ShowErrorsDialog();
				}
				else
				{
					var warnings = BusinessEntity.NotificationsIncludingChildren.GetNotifications(CargoWise.EntityFramework.NotificationType.Warning).Select(c => c.Message).Distinct();

					if (warnings.Any(x => x.Contains(TransferHeaderMessageChooserItemValidation.TransferHasNoValidBills)))
					{
						Globals.Message.ShowWarning(TransferHeaderMessageChooserItemValidation.TransferHasNoValidBills);
						result = false;
					}
					else if (warnings.Any(x => x.Contains(TransferHeaderMessageChooserItemValidation.TransferAlreadyArrivedWarning)))
					{
						var dialogResult = Globals.Message.Show(TransferHeaderMessageChooserItemValidation.TransferAlreadyArrivedWarning, Res.GetString("519381DB-DC50-4454-92BE-385811CD5578", "Warning"), MessageBoxButtons.YesNo, DialogResult.Yes);
						result = dialogResult == DialogResult.Yes;
					}
				}
			}
			return result;
		}

		public new TransferHeaderMessageChooser BusinessEntity => (TransferHeaderMessageChooser)base.BusinessEntity;
	}
}
