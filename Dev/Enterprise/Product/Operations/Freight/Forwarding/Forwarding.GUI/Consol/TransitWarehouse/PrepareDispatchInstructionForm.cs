using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Freight.Forwarding.Business.TransitWarehouseInstructionHelper;
using EventRefParams = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class PrepareDispatchInstructionForm : ZChildForm, INotifications
	{
		public PrepareDispatchInstructionForm(ConsolPrepareForDispatchInstruction consolForPrepareDispatchInstruction)
			: base(consolForPrepareDispatchInstruction)
		{
			InitializeComponent();
			this.consolForPrepareDispatchInstruction = consolForPrepareDispatchInstruction;
		}

		readonly ConsolPrepareForDispatchInstruction consolForPrepareDispatchInstruction;

		public static void ShowDialog(ForwardingConsol consol, Direction direction)
		{
			var consolForPrepareDispatch = new ConsolPrepareForDispatchInstruction(consol, direction);
			ZFormModaliser.ShowDialogAndDispose(new PrepareDispatchInstructionForm(consolForPrepareDispatch));
		}

		public override string FormVerb => string.Empty;

		public override string FormCaption => Res.GetString("91324fd6-8552-b4aa-42c7-bae4481ceca6", "Deliver Prepare Dispatch");

		void CancelButton_OnClick(object sender, EventArgs e)
		{
			Close();
		}

		void Deliverbutton_OnClick(object sender, EventArgs e)
		{
			var selectedShipments = consolForPrepareDispatchInstruction.GetAllSelectedShipments().ToArray();
			if (selectedShipments.Length == 0)
			{
				Globals.Message.ShowError(Res.GetString("21f33fdc-1a56-46a9-433e-a4d5b717e231", "No Shipment(s) have been selected."));
				return;
			}

			var invalidShipments = selectedShipments
				.Where(shipment => !shipment.IsValidToSend)
				.ToArray();

			var alreadyDeliveredShipments = selectedShipments
				.Where(shipment => shipment.HasPreviouslySentPrepareDispatchInstruction)
				.ToArray();

			if (invalidShipments.Length == selectedShipments.Length)
			{
				Globals.Message.ShowError(GetInvalidShipmentsMessage(invalidShipments, GetCFSWarehouseOfConsol(consolForPrepareDispatchInstruction)));
				return;
			}

			var confirmationMessage = GetMessageForInvalidAndSentShipments(invalidShipments, alreadyDeliveredShipments);

			if (string.IsNullOrEmpty(confirmationMessage))
			{
				RequestMessageSending();
				Close();
				return;
			}

			if (Globals.Message.Show(confirmationMessage,
					Res.GetString("eef3c849-e57d-6fae-420f-765c37d6880f", "Send Prepare Dispatch Instruction"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question) == DialogResult.Yes)
			{
				RequestMessageSending();
				Close();
			}
		}

		void RequestMessageSending()
		{
			var consol = consolForPrepareDispatchInstruction.Consol;
			var direction = consolForPrepareDispatchInstruction.Direction;

			var selectedShipments = consolForPrepareDispatchInstruction
				.GetAllSelectedShipments()
				.Where(shipment => shipment.IsValidToSend)
				.Select(selectedShipment => selectedShipment.Shipment)
				.ToArray();

			if (selectedShipments.Length <= TransitWarehouseInstructionGUIHelper.MaximumNumberOfShipmentsCanBeProcessedInGUI || !FreightDataRegistry.Instance.EnableSendingForwardingConsolToTWHAsynchronously.Value)
			{
				using (new ZWaitCursorChanger(this))
				{
					Func<BusinessObjectFactory, ManualDataExport> func = factory => GetManualDataExportForSendingSelectedShipmentsForPrepareDispatch(factory, consol);
					new TransitWarehouseInstructionHelper(consol, this, func, selectedShipments).SendTransitWarehouseInstruction(direction, ServiceRequest.PrepareDispatch);
				}
			}
			else
			{
				if (consol.HasChanges)
				{
					Globals.Message.ShowError(Res.GetString("83f1fada-6890-4b74-90e0-f59ad3b932bc", "Please save your changes before sending the Transit Warehouse Instruction."));
					return;
				}

				var eventTime = ZDateTimeOffset.Now;
				var shipmentParams = new Dictionary<string, string>();
				shipmentParams[EventRefParams.DeclarationID] = consol.JK_UniqueConsignRef;

				foreach (var shipment in selectedShipments)
				{
					shipment.Logs.AddNew(AutoEvents.JobShipmentSelected, eventTime, shipmentParams.ToArray());
				}

				var parameters = new Dictionary<string, string>();
				parameters[EventRefParams.Direction] = direction.ToString();
				parameters[EventRefParams.Service] = nameof(ServiceRequest.PrepareDispatch);

				consol.Logs.AddNew(AutoEvents.MessageSendingRequest, eventTime, parameters.ToArray());
				consol.Factory.Save();
				Globals.Message.Show(Res.GetString("db99b595-06d4-4ed6-b842-b505771ce438", "Message Sending Request is in Progress. User may need to check to confirm UXML sending completion status after a short period."));
			}
		}

		ManualDataExport GetManualDataExportForSendingSelectedShipmentsForPrepareDispatch(BusinessObjectFactory factory, ForwardingConsol consol)
		{
			return new ManualDataExport(factory,
				new[] { consol },
				UniversalDataType.UniversalShipment,
				null, null, null,
				manager => ObjectFactory.Get<IConsolPrepareForDispatchDataObjectWriter>(nameof(IConsolPrepareForDispatchDataObjectWriter), manager, consolForPrepareDispatchInstruction));
		}

		string GetMessageForInvalidAndSentShipments(ShipmentPrepareForDispatchInstruction[] invalidShipments, ShipmentPrepareForDispatchInstruction[] alreadyDeliveredShipments)
		{
			var invalidShipmentsMessage = GetInvalidShipmentsMessage(invalidShipments, GetCFSWarehouseOfConsol(consolForPrepareDispatchInstruction));
			var alreadySentMessage = GetShipmentsAlreadyDeliveredMessage(alreadyDeliveredShipments);

			var messageBuilder = new StringBuilder();

			if (!string.IsNullOrEmpty(invalidShipmentsMessage))
			{
				messageBuilder.AppendLine(invalidShipmentsMessage);
				messageBuilder.AppendLine();
			}

			if (!string.IsNullOrEmpty(alreadySentMessage))
			{
				messageBuilder.AppendLine(alreadySentMessage);
				messageBuilder.AppendLine();
			}

			if (messageBuilder.Length > 0)
			{
				messageBuilder.AppendLine(Res.GetString("9f700f96-278b-e193-4a04-2d4f785b52ca", "Would you like to continue?"));
			}

			return messageBuilder.ToString();
		}

		string GetInvalidShipmentsMessage(ShipmentPrepareForDispatchInstruction[] invalidShipments, ZString cfsWarehouseCode)
		{
			if (invalidShipments.Length == 0)
			{
				return string.Empty;
			}

			return Res.GetString("0042e6c7-b9a2-24a6-4ce5-84bedf861ba6", @"The Prepare Dispatch instruction cannot be sent for the following shipment(s):
{0}
as shipments must have the RCV - Received status at {1} facility in order to be dispatched.", GetFormattedStringOfShipmentsWithNewLines(invalidShipments), cfsWarehouseCode);
		}

		string GetShipmentsAlreadyDeliveredMessage(ShipmentPrepareForDispatchInstruction[] alreadyDeliveredShipments)
		{
			if (alreadyDeliveredShipments.Length == 0)
			{
				return string.Empty;
			}

			return Res.GetString("412d45c7-4796-2fa8-4a8e-bca54fb280f2", @"The Prepare Dispatch Instruction has already been sent for the selected shipment(s):
{0}", GetFormattedStringOfShipmentsWithNewLines(alreadyDeliveredShipments));
		}

		string GetFormattedStringOfShipmentsWithNewLines(ShipmentPrepareForDispatchInstruction[] shipments)
		{
			return string.Join(System.Environment.NewLine, shipments.Select(shipment => shipment.ShipmentID));
		}

		ZString GetCFSWarehouseOfConsol(ConsolPrepareForDispatchInstruction consolForPrepareDispatch)
		{
			return TransitWarehouseInstructionHelper
				.GetTransitWarehouseAddress(consolForPrepareDispatch.Consol, consolForPrepareDispatch.Direction)?.Header?.OH_Code ?? ZString.Empty;
		}

		void SelectAllButton_OnClick(object sender, EventArgs e)
		{
			consolForPrepareDispatchInstruction
				.ShipmentsForSelection.OfType<ShipmentPrepareForDispatchInstruction>()
				.ForEach(shipment => shipment.SelectedForDelivery = true);
		}

		void SelectNoneButton_OnClick(object sender, EventArgs e)
		{
			consolForPrepareDispatchInstruction
				.ShipmentsForSelection.OfType<ShipmentPrepareForDispatchInstruction>()
				.ForEach(shipment => shipment.SelectedForDelivery = false);
		}

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}
	}
}
