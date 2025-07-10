using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ElectronicBOLUserControl : ZUserControl
	{
		public ElectronicBOLUserControl()
		{
			InitializeComponent();
			ManageConsigneeAndToOrderVisibility();
		}

		protected ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)DataSource; }
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			if (Shipment != null)
			{
				Shipment.JS_ElectronicBillOfLadingStatusInfo.ValueChanged -= JS_ElectronicBillOfLadingStatusInfo_ValueChanged;
			}

			base.OnAfterFirstBinding(e);

			if (Shipment != null)
			{
				Shipment.JS_ElectronicBillOfLadingStatusInfo.ValueChanged += JS_ElectronicBillOfLadingStatusInfo_ValueChanged;
				JS_ElectronicBillOfLadingStatusInfo_ValueChanged(null, null);

				using (Shipment.SuspendSettingHasChanges())
				{
					Shipment.DefaultElectronicBillOfLadingHouseBill();
				}
			}
		}

		void JS_ElectronicBillOfLadingStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			SetPublishButton();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Shipment != null)
				{
					Shipment.JS_ElectronicBillOfLadingStatusInfo.ValueChanged -= JS_ElectronicBillOfLadingStatusInfo_ValueChanged;
				}

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		bool IsAmendmentRequestStatus => Shipment != null && Shipment.JS_ElectronicBillOfLadingStatus == FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;

		void ButtonPublish_Click(object sender, EventArgs e)
		{
			var publishElectronicBillOfLadingCaption = Res.GetString("5f272c60-4166-43fe-8246-5bd1bf279871", "Publish Electronic Bill Of Lading");

			if (Shipment.HasChanges || !Shipment.IsInDatabase)
			{
				Globals.Message.Show(
					Res.GetString("1483f9d5-fb11-4ac7-a952-dc7b8c15ddc7", "Please save all changes before publishing."),
					publishElectronicBillOfLadingCaption,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			if (!Shipment.CheckElectronicBOLMinimumRequirements())
			{
				Globals.Message.Show(
					Res.GetString("81b64991-e939-494d-a03a-1b60c71a6462", "There are validation errors on this form due to missing mandatory information. Please correct these errors before publishing."),
					publishElectronicBillOfLadingCaption,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			if (Shipment.JS_ElectronicBillOfLadingStatus.IsEmpty || Shipment.JS_ElectronicBillOfLadingStatus == FreightConstants.BillOfLadingBillStatus.Codes.PublishingRejected)
			{
				LaunchHouseBillForm();
				return;
			}

			if (IsAmendmentRequestStatus)
			{
				var amendmentRequestCaption =
					Res.GetString("ElectronicBOLUserControl|648d082d-651d-4889-a682-6a06317a7f8f", "Accept/Deny Amendment Request");
				var result = Globals.Message.Show(
					Res.GetString("ElectronicBOLUserControl|e554bd73-bf18-458f-8322-26ae92c0e06f", "Please Accept or Deny the Amendment Request"), amendmentRequestCaption,
					ZMessageBoxButtons.YesNoCancel, ZMessageBoxIcon.Information,
					Res.GetString("ea5954fa-de78-4d54-941d-7c40d0dbc759", "Accept"),
					Res.GetString("9ef1a969-9cca-45b0-80da-e572ed504c66", "Deny")
				);

				if (result == ZDialogResult.Yes)
				{
					LaunchHouseBillForm();
				}
				else if (result == ZDialogResult.No)
				{
					var denyAmendmentRequestCaption = Res.GetString("ElectronicBOLUserControl|9c668015-e921-4036-90d9-0e7daab3df19", "Deny Amendment Request");
					ZString reasonString = Globals.Message.QueryDefaultValue(string.Empty,
						Res.GetString("ElectronicBOLUserControl|1b04e61c-56ea-436b-88ed-8cded3053f42", "Please input a reason for your Amendment Request Rejection:"), denyAmendmentRequestCaption, 10, true,
						Res.GetString("bba912e5-28c2-4420-801b-dc46c1d5365f", "Send"));
					if (!reasonString.IsEmpty)
					{
						var latestAmendmentRequestedEventLog = Shipment.GetLatestAmendmentRequestedEHBLStatusEventLog();
						if (latestAmendmentRequestedEventLog == null)
						{
							Globals.Message.Show(Res.GetString("5970516d-5799-4fdf-8a01-760d37d32d09", "You have not received Amendment Requested. No Accept/Deny action is required."));
						}
						else
						{
							var helper = new EHBLUniversalEventSenderHelper()
							{
								DirectXTClientID = ElectronicBOLConstants.DirectXTClientID,
								MessageBroker = EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface
							};
							var parameters = new List<KeyValuePair<string, string>>();
							parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.BillStatusUpdatedTypes.AmendmentDenied));
							parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, ElectronicBOLConstants.EHBLEventDepartments.Carrier));
							parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, latestAmendmentRequestedEventLog.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber)));
							parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.RequestNumber, latestAmendmentRequestedEventLog.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.RequestNumber)));
							var errorMessage = helper.SendUniversalEvent(Shipment, AutoEvents.BillStatusUpdatedCode, parameters.ToArray(), reasonString);
							if (errorMessage.IsEmpty)
							{
								Shipment.RevertToPreviousEHBLStatus();
							}
							Globals.Message.Show(errorMessage.IsEmpty ? Res.GetString("bbc45e66-eae8-4d86-9bb5-f4870bb6bf54", "Sent successfully.") : errorMessage);
						}
					}
				}
			}
		}

		void BillOfLadingBillTypeDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			ManageConsigneeAndToOrderVisibility();
		}

		void ManageConsigneeAndToOrderVisibility()
		{
			var electronicBillOfLadingType = this.BillOfLadingBillTypeDropEdit.Text;

			if (electronicBillOfLadingType == Constants.BillOfLadingBillType.Codes.ToOrder)
			{
				ToOrderDocAddressControl.Visible = true;
				ConsigneeDocAddressControl.Visible = false;
				ConsigneeTextBox.Visible = false;
				ConsigneeTextBoxLabel.Visible = false;
			}
			else
			{
				ToOrderDocAddressControl.Visible = false;

				if (electronicBillOfLadingType == Constants.BillOfLadingBillType.Codes.Straight)
				{
					ConsigneeTextBox.Visible = false;
					ConsigneeTextBoxLabel.Visible = false;
					ConsigneeDocAddressControl.Visible = true;
				}
				else
				{
					ConsigneeTextBox.Visible = true;
					ConsigneeTextBoxLabel.Visible = true;
					ConsigneeDocAddressControl.Visible = false;
				}
			}
		}

		void SetPublishButton()
		{
			if (Shipment == null)
			{
				return;
			}

			if (IsAmendmentRequestStatus)
			{
				this.PublishButton.Text = Res.GetString("ea0cb5ff-99c1-4467-90f7-5cb3ab6d485f", "Accept/Deny Amendment Request");
			}
			else
			{
				this.PublishButton.Text = Res.GetString("b08dd8fc-8154-4139-8bca-685cc2afa5fb", "Publish");
			}

			this.PublishButton.Enabled = !new[] { FreightConstants.BillOfLadingBillStatus.Codes.SentForPublication
				, FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillPublished
				, FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred
				, FreightConstants.BillOfLadingBillStatus.Codes.Surrendered
				, FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper }.Contains(Shipment.JS_ElectronicBillOfLadingStatus.ToString());
		}

		void ViewEditBillButton_Click(object sender, EventArgs e)
		{
			LaunchHouseBillForm();
		}

		void LaunchHouseBillForm()
		{
			if (Shipment != null)
			{
				var provider = ObjectFactory.Get<IVisualizableDocumentCommandProvider>();
				var command = provider.GetCommand(Shipment, BillOfLadingtMenu, ModuleIDs.JobShipment);
				if (command == null)
				{
					Globals.Message.Show(Res.GetString("ElectronicBOLUserControl|939d1f5a-ca87-4642-bbe5-8b11ef1ec98b", "Bill of Lading applicable to this shipment could not be found."));
				}
				else
				{
					Shipment.IsEditingElectronicBOL = true;
					command.Execute();
					Shipment.IsEditingElectronicBOL = false;

					Shipment.CheckElectronicBOLMinimumRequirements();
					JS_ElectronicBillOfLadingStatusInfo_ValueChanged(null, null);
				}
			}
		}

		StmMenuItemBase BillOfLadingtMenu
		{
			get
			{
				if (billOfLadingtMenu == null)
				{
					billOfLadingtMenu = Shipment?.Factory.Load<StmMenuItemBase>(ShipmentSystemFormMenuItems.BillOfLadingPK);
				}

				return billOfLadingtMenu;
			}
		}

		StmMenuItemBase billOfLadingtMenu;
	}
}
