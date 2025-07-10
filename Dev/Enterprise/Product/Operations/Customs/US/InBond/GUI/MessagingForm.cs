using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.GUI
{
	public partial class MessagingForm : ZChildForm, IDataGridLayoutIdentifierRoot
	{
		public MessagingForm(InBondMessageSendingHeaderObject sendingHeader)
			: base(sendingHeader)
		{
			Text = GetFormHeading(sendingHeader.MessageType);
			isAMSHBREffective = US.Business.ZZCustomsFunctionality.IsAMSHBREffective;
			using (MovementHeadersMessagesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				MovementHeadersMessagesGrid.SetAllAvailability(false);
				foreach (KeyValuePair<string, bool> pair in GetAvailableColumnDetails(sendingHeader.MessageType))
				{
					MovementHeadersMessagesGrid.SetAvailability(true, pair.Key);
					MovementHeadersMessagesGrid.SetColumnVisible(pair.Value, pair.Key);
				}
			}
			this.MovementHeadersMessagesGrid.GridId = MovementHeadersMessagesGrid.GridId + sendingHeader.MessageType;
			if (sendingHeader.IsWarehouseType)
			{
				TopSplitContainer.Panel2Collapsed = true;
			}

			MessageOptionsTabPage.TabVisible = sendingHeader.MessageType == InBondMessageType.DiversionRequest;

			if (isAMSHBREffective && sendingHeader.Header.IsSea)
			{
				MovementHeadersMessagesGrid.SetColumnCaption(InBondMessageSendingObject.Schema.US_MasterBillNumber, "Master Bill Number");
				MovementHeadersMessagesGrid.SetColumnCaption(InBondMessageSendingObject.Schema.US_MasterBillIssuer, "Master Bill Issuer");
			}
		}

		readonly bool isAMSHBREffective;

		public new InBondMessageSendingHeaderObject BusinessEntity
		{
			get { return (InBondMessageSendingHeaderObject)base.BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return Text; }
		}

		string GetFormHeading(InBondMessageType messageType)
		{
			switch (messageType)
			{
				case InBondMessageType.DepartureAdd:
				case InBondMessageType.AirInBondAdd:
					return ResString.GetMultilingualString("USInBond|MessagingForm|F202AC2F-2246-42e5-996E-760F48B5195F", "In-Bond Departure Add Messaging");
				case InBondMessageType.DepartureDelete:
				case InBondMessageType.AirInBondDelete:
					return ResString.GetMultilingualString("USInBond|MessagingForm|22EE4CB9-E4C1-4d05-BE5A-C3A5FE802ED2", "In-Bond Departure Delete Messaging");
				case InBondMessageType.DepartureBillDelete:
				case InBondMessageType.AirBillDelete:
					return ResString.GetMultilingualString("USInBond|MessagingForm|9522908F-F1FD-4425-BC90-80638695CC9A", "Bill of Lading Delete Messaging");
				case InBondMessageType.DepartureAmend:
				case InBondMessageType.AirInBondAmend:
					return ResString.GetMultilingualString("USInBond|MessagingForm|37C75643-4DA5-4718-8821-84D4A2398A95", "In-Bond Departure Amendment (Delete/Re-Add) Messaging");
				case InBondMessageType.InBondLevelArrival:
				case InBondMessageType.AirEntireInBondArrival:
					return ResString.GetMultilingualString("USInBond|MessagingForm|B3EEFED4-9153-4024-A61D-1B2BE5CCD630", "In-Bond Arrival Messaging");
				case InBondMessageType.BillOfLadingLevelArrival:
					return ResString.GetMultilingualString("USInBond|MessagingForm|4d448f54-2dd6-4006-afbf-68fc9a65f755", "Bill of Lading Arrival Messaging");
				case InBondMessageType.ContainerLevelArrival:
					return ResString.GetMultilingualString("USInBond|MessagingForm|8ecc2570-b579-439d-898f-25da3e83c884", "Container Arrival Messaging");
				case InBondMessageType.InBondLevelExportation:
				case InBondMessageType.AirEntireInBondExportation:
					return ResString.GetMultilingualString("USInBond|MessagingForm|5359D1FA-D575-43eb-8E42-A6938415CA34", "In-Bond Exportation Messaging");
				case InBondMessageType.BillOfLadingLevelExportation:
					return ResString.GetMultilingualString("USInBond|MessagingForm|5cf1eda9-721f-4a3b-aeb0-211575b3fec8", "Bill of Lading Exportation Messaging");
				case InBondMessageType.ContainerLevelExportation:
					return ResString.GetMultilingualString("USInBond|MessagingForm|df27758f-dfde-4590-bba7-39d7be34d4a4", "Container Exportation Messaging");
				case InBondMessageType.InBondLevelTransferOfLiability:
					return ResString.GetMultilingualString("USInBond|MessagingForm|9F058182-8406-494e-937C-D4434815AEA8", "In-Bond Transfer Of Liability Messaging");
				case InBondMessageType.BondedWarehouseUpdate:
					return ResString.GetMultilingualString("USInBond|MessagingForm|8A58995F-49A3-435B-8DC6-26E00560EC72", "Update Inventory");
				case InBondMessageType.BondedWarehouseCancel:
					return ResString.GetMultilingualString("USInBond|MessagingForm|BC0EE0EB-0F02-406C-B68A-B5C8E6E12651", "Cancel Inventory Stock Release");
				case InBondMessageType.DiversionRequest:
					return ResString.GetMultilingualString("USInBond|MessagingForm|E0072907-DA7A-43DF-B9A5-26907888DCFE", "Diversion Request");
				default:
					throw new NotSupportedException();
			}
		}

		IEnumerable<KeyValuePair<string, bool>> GetAvailableColumnDetails(InBondMessageType messageType)
		{
			Dictionary<string, bool> result = new Dictionary<string, bool>();
			result.Add(InBondMessageSendingObject.Schema.US_ShouldSend, true);
			switch (messageType)
			{
				case InBondMessageType.DepartureAdd:
				case InBondMessageType.DepartureDelete:
				case InBondMessageType.DepartureAmend:
				case InBondMessageType.BondedWarehouseUpdate:
				case InBondMessageType.BondedWarehouseCancel:
					result.Add(InBondMessageSendingObject.Schema.US_InBondNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_InBondEntryType, true);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierID, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierSCAC, false);
					result.Add(InBondMessageSendingObject.Schema.US_DestinationPortDCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_ForeignDestPortKCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_MonetaryValue, false);
					result.Add(InBondMessageSendingObject.Schema.US_BTAIndicator, false);
					if (messageType == InBondMessageType.BondedWarehouseCancel || messageType == InBondMessageType.BondedWarehouseUpdate)
					{
						result.Add(InBondMessageSendingObject.Schema.US_WarehouseAddressDetail, BusinessEntity.IsWarehouseType);
					}
					break;
				case InBondMessageType.InBondLevelArrival:
					result.Add(InBondMessageSendingObject.Schema.US_InBondNumber, messageType == InBondMessageType.InBondLevelArrival);
					result.Add(InBondMessageSendingObject.Schema.US_ActionCode, true);
					result.Add(InBondMessageSendingObject.Schema.US_ActionDescription, true);
					result.Add(InBondMessageSendingObject.Schema.US_ArrivalDate, true);
					result.Add(InBondMessageSendingObject.Schema.US_ArrivalPort, true);
					result.Add(InBondMessageSendingObject.Schema.US_ArrivalFirmsCode, true);
					result.Add(InBondMessageSendingObject.Schema.US_InBondEntryType, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierID, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierSCAC, false);
					result.Add(InBondMessageSendingObject.Schema.US_DestinationPortDCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_ForeignDestPortKCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_MonetaryValue, false);
					result.Add(InBondMessageSendingObject.Schema.US_BTAIndicator, false);
					break;
				case InBondMessageType.DepartureBillDelete:
				case InBondMessageType.AirBillDelete:
					result.Add(InBondMessageSendingObject.Schema.US_InBondNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_ActionCode, true);
					result.Add(InBondMessageSendingObject.Schema.US_ActionDescription, true);
					result.Add(InBondMessageSendingObject.Schema.US_MasterBillNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_MasterBillIssuer, true);
					if (isAMSHBREffective && BusinessEntity.Header.IsSea)
					{
						result.Add(InBondMessageSendingObject.Schema.US_HouseBillNumber, true);
						result.Add(InBondMessageSendingObject.Schema.US_HouseBillIssuer, true);
					}
					result.Add(InBondMessageSendingObject.Schema.US_InBondEntryType, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierID, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierSCAC, false);
					result.Add(InBondMessageSendingObject.Schema.US_DestinationPortDCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_ForeignDestPortKCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_MonetaryValue, false);
					result.Add(InBondMessageSendingObject.Schema.US_BTAIndicator, false);
					break;
				case InBondMessageType.BillOfLadingLevelArrival:
					result.Add(InBondMessageSendingObject.Schema.US_InBondNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_ActionCode, true);
					result.Add(InBondMessageSendingObject.Schema.US_ActionDescription, true);
					result.Add(InBondMessageSendingObject.Schema.US_MasterBillNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_MasterBillIssuer, true);
					if (isAMSHBREffective && BusinessEntity.Header.IsSea)
					{
						result.Add(InBondMessageSendingObject.Schema.US_HouseBillNumber, true);
						result.Add(InBondMessageSendingObject.Schema.US_HouseBillIssuer, true);
					}
					result.Add(InBondMessageSendingObject.Schema.US_ArrivalDate, true);
					result.Add(InBondMessageSendingObject.Schema.US_ArrivalPort, true);
					result.Add(InBondMessageSendingObject.Schema.US_ArrivalFirmsCode, true);
					result.Add(InBondMessageSendingObject.Schema.US_InBondEntryType, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierID, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierSCAC, false);
					result.Add(InBondMessageSendingObject.Schema.US_DestinationPortDCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_ForeignDestPortKCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_MonetaryValue, false);
					result.Add(InBondMessageSendingObject.Schema.US_BTAIndicator, false);
					break;
				case InBondMessageType.ContainerLevelArrival:
					result.Add(InBondMessageSendingObject.Schema.US_InBondNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_ActionCode, true);
					result.Add(InBondMessageSendingObject.Schema.US_ActionDescription, true);
					result.Add(InBondMessageSendingObject.Schema.US_MasterBillNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_MasterBillIssuer, true);
					result.Add(InBondMessageSendingObject.Schema.US_ContainerNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_ArrivalDate, true);
					result.Add(InBondMessageSendingObject.Schema.US_ArrivalPort, true);
					result.Add(InBondMessageSendingObject.Schema.US_ArrivalFirmsCode, true);
					result.Add(InBondMessageSendingObject.Schema.US_InBondEntryType, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierID, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierSCAC, false);
					result.Add(InBondMessageSendingObject.Schema.US_DestinationPortDCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_ForeignDestPortKCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_MonetaryValue, false);
					result.Add(InBondMessageSendingObject.Schema.US_BTAIndicator, false);
					break;
				case InBondMessageType.InBondLevelExportation:
					result.Add(InBondMessageSendingObject.Schema.US_InBondNumber, messageType == InBondMessageType.InBondLevelExportation);
					result.Add(InBondMessageSendingObject.Schema.US_ActionCode, true);
					result.Add(InBondMessageSendingObject.Schema.US_ActionDescription, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportDate, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportPort, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportConveyance, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportTransportMode, true);
					result.Add(InBondMessageSendingObject.Schema.US_InBondEntryType, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierID, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierSCAC, false);
					result.Add(InBondMessageSendingObject.Schema.US_DestinationPortDCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_ForeignDestPortKCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_MonetaryValue, false);
					result.Add(InBondMessageSendingObject.Schema.US_BTAIndicator, false);
					break;
				case InBondMessageType.BillOfLadingLevelExportation:
					result.Add(InBondMessageSendingObject.Schema.US_InBondNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_ActionCode, true);
					result.Add(InBondMessageSendingObject.Schema.US_ActionDescription, true);
					result.Add(InBondMessageSendingObject.Schema.US_MasterBillNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_MasterBillIssuer, true);
					if (isAMSHBREffective && BusinessEntity.Header.IsSea)
					{
						result.Add(InBondMessageSendingObject.Schema.US_HouseBillNumber, true);
						result.Add(InBondMessageSendingObject.Schema.US_HouseBillIssuer, true);
					}
					result.Add(InBondMessageSendingObject.Schema.US_ExportDate, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportPort, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportConveyance, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportTransportMode, true);
					result.Add(InBondMessageSendingObject.Schema.US_InBondEntryType, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierID, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierSCAC, false);
					result.Add(InBondMessageSendingObject.Schema.US_DestinationPortDCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_ForeignDestPortKCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_MonetaryValue, false);
					result.Add(InBondMessageSendingObject.Schema.US_BTAIndicator, false);
					break;
				case InBondMessageType.ContainerLevelExportation:
					result.Add(InBondMessageSendingObject.Schema.US_InBondNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_ActionCode, true);
					result.Add(InBondMessageSendingObject.Schema.US_ActionDescription, true);
					result.Add(InBondMessageSendingObject.Schema.US_MasterBillNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_MasterBillIssuer, true);
					result.Add(InBondMessageSendingObject.Schema.US_ContainerNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportDate, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportPort, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportConveyance, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportTransportMode, true);
					result.Add(InBondMessageSendingObject.Schema.US_InBondEntryType, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierID, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierSCAC, false);
					result.Add(InBondMessageSendingObject.Schema.US_DestinationPortDCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_ForeignDestPortKCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_MonetaryValue, false);
					result.Add(InBondMessageSendingObject.Schema.US_BTAIndicator, false);
					break;
				case InBondMessageType.InBondLevelTransferOfLiability:
					result.Add(InBondMessageSendingObject.Schema.US_InBondNumber, messageType == InBondMessageType.InBondLevelTransferOfLiability);
					result.Add(InBondMessageSendingObject.Schema.US_TOLDate, true);
					result.Add(InBondMessageSendingObject.Schema.US_TOLCarrierID, true);
					result.Add(InBondMessageSendingObject.Schema.US_TOLCarrierCode, true);
					result.Add(InBondMessageSendingObject.Schema.US_TOLCityName, true);
					result.Add(InBondMessageSendingObject.Schema.US_TOLStateCode, true);
					result.Add(InBondMessageSendingObject.Schema.US_InBondEntryType, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierSCAC, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierID, false);
					result.Add(InBondMessageSendingObject.Schema.US_DestinationPortDCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_ForeignDestPortKCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_MonetaryValue, false);
					result.Add(InBondMessageSendingObject.Schema.US_BTAIndicator, false);
					break;
				case InBondMessageType.AirInBondAdd:
				case InBondMessageType.AirInBondDelete:
				case InBondMessageType.AirInBondAmend:
					result.Add(InBondMessageSendingObject.Schema.US_InBondNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_InBondEntryType, true);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierID, false);
					result.Add(InBondMessageSendingObject.Schema.US_InBondCarrierSCAC, false);
					result.Add(InBondMessageSendingObject.Schema.US_DestinationPortDCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_ForeignDestPortKCode, false);
					result.Add(InBondMessageSendingObject.Schema.US_MonetaryValue, false);
					break;
				case InBondMessageType.AirEntireInBondArrival:
					result.Add(InBondMessageSendingObject.Schema.US_InBondNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_ArrivalDate, true);
					result.Add(InBondMessageSendingObject.Schema.US_ArrivalPort, true);
					result.Add(InBondMessageSendingObject.Schema.US_ArrivalFirmsCode, true);
					result.Add(InBondMessageSendingObject.Schema.US_DestinationPortDCode, false);
					break;
				case InBondMessageType.AirEntireInBondExportation:
					result.Add(InBondMessageSendingObject.Schema.US_InBondNumber, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportDate, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportPort, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportConveyance, true);
					result.Add(InBondMessageSendingObject.Schema.US_ExportTransportMode, true);
					result.Add(InBondMessageSendingObject.Schema.US_DestinationPortDCode, false);
					break;
				case InBondMessageType.DiversionRequest:
					result.Add(InBondMessageSendingObject.Schema.US_InBondNumber, true);
					break;
			}
			return result;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var bizEntity = BusinessEntity;
			if (bizEntity != null)
			{
				bizEntity.Header.ValidationModes = savedValidationModes;
			}
			base.SetDataBinding(dataSource, dataMember);
			bizEntity = BusinessEntity;
			if (bizEntity != null)
			{
				savedValidationModes = bizEntity.Header.ValidationModes;
				bizEntity.Header.RecalculateValidationModesOnHeader(BusinessEntity.MessageType);
			}
		}
		ValidationModes savedValidationModes;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.InitializeComponent();
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.MessageType == InBondMessageType.DiversionRequest)
			{
				if (BusinessEntity.HasNotificationsOnObjectsMarkedForSending())
				{
					if (Globals.Message.Show("There is a notification. Are you sure you wish to continue?", "Send messages", MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.Cancel)
					{
						return;
					}
				}
			}

			if (BusinessEntity.SendingObjects.HasAtLeastOneMarkedForSending)
			{
				if (BusinessEntity.SendData())
				{
					Close();
				}
			}
			else
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("USInBond|MessagingForm|264A85BA-AC41-4397-889A-433821EB51DF", "No Movement has marked for sending"));
			}
		}

		void CancelAndCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && BusinessEntity != null)
			{
				BusinessEntity.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}
			base.Dispose(disposing);
		}

		#region IDataGridLayoutIdentifierRoot Members

		string IDataGridLayoutIdentifierRoot.ID
		{
			get { return BusinessEntity.MessageType.ToString(); }
		}

		#endregion
	}
}
