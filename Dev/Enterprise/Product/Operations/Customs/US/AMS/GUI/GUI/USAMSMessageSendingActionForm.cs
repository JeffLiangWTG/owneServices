using System;
using System.Windows.Forms;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.GUI
{
	public partial class USAMSMessageSendingActionForm : ZChildForm
	{
		public USAMSMessageSendingActionForm()
		{
			UpdateMB_Date(ActionCode.Creating);
		}

		public USAMSMessageSendingActionForm(MessageSendingAction sendingAction)
			: base(sendingAction)
		{
			this.header = sendingAction.Header;
			BillsAndMessageContentSplitContainer.Panel2MinSize = 150;
			MovementsAndBillsSplitContainer.Panel2MinSize = 200;

			var isPTT = ActionCodeTool.IsPermitToTransferAction(sendingAction.ActionCode);
			var isInBondAction = ActionCodeTool.IsInBondType(sendingAction.ActionCode);
			var isVesselDepartureAction = sendingAction.ActionCode == ActionCode.VesselDeparture;
			MovementsAndBillsSplitContainer.Panel1Collapsed = !isPTT && !isInBondAction && !isVesselDepartureAction;
			BillsAndMessageContentSplitContainer.Panel1Collapsed = isVesselDepartureAction;

			formCaption = "";
			switch (sendingAction.ActionCode)
			{
				case ActionCode.Creating:
					BillsGrid.RemoveFromAvailableColumns(MessageSendingObject.Schema.MB_Send, MessageSendingObject.Schema.MB_AmendmentCode, MessageSendingObject.Schema.MB_BillActionCode, MessageSendingObject.Schema.MB_Date, MessageSendingObject.Schema.MB_PortOfUnlading, MessageSendingObject.Schema.MB_PortOfUnladingOverride);
					formCaption = Res.GetString("40618F2D-5660-41FA-A6BD-79521B084283", "Manifest Original");
					break;
				case ActionCode.AmendingAdd:
				case ActionCode.AmendingUpdate:
				case ActionCode.AmendingDelete:
					BillsGrid.RemoveFromAvailableColumns(MessageSendingObject.Schema.MB_Date, MessageSendingObject.Schema.MB_PortOfUnlading);
					formCaption = Res.GetString("9D9DC270-AF87-44C5-92C1-DF9646068D2A", "Manifest Amendments");
					break;
				case ActionCode.PermitToTransfer:
					BillsGrid.RemoveFromAvailableColumns(MessageSendingObject.Schema.MB_Send, MessageSendingObject.Schema.MB_AmendmentCode, MessageSendingObject.Schema.MB_BillActionCode, MessageSendingObject.Schema.MB_Date, MessageSendingObject.Schema.MB_PortOfUnlading, MessageSendingObject.Schema.MB_PortOfUnladingOverride);
					formCaption = Res.GetString("D4FDF85C-1208-42D4-90CF-571D4E805F58", "Permit To Transfer");
					break;
				case ActionCode.CancelPermitToTransfer:
					BillsGrid.RemoveFromAvailableColumns(MessageSendingObject.Schema.MB_AmendmentCode, MessageSendingObject.Schema.MB_Date, MessageSendingObject.Schema.MB_PortOfUnlading, MessageSendingObject.Schema.MB_PortOfUnladingOverride);
					formCaption = Res.GetString("E0F809B9-8AA2-4F8B-B340-D892839FA39F", "Cancel Permit To Transfer by Bill");
					break;
				case ActionCode.ChangeEstDateOfArrival:
				case ActionCode.VesselArrival:
					BillsGrid.RemoveFromAvailableColumns(MessageSendingObject.Schema.MB_AmendmentCode, MessageSendingObject.Schema.MB_IssuerCode, MessageSendingObject.Schema.MB_BillOfLadingSequenceNumber, MessageSendingObject.Schema.MB_BillActionCode, MessageSendingObject.Schema.MB_CustomsStatus, MessageSendingObject.Schema.MB_CustomsStatusDescription, MessageSendingObject.Schema.MB_MessageStatus, MessageSendingObject.Schema.MB_MessageStatusDescription, MessageSendingObject.Schema.MB_PortOfUnladingOverride);
					if (sendingAction.ActionCode == ActionCode.ChangeEstDateOfArrival)
					{
						formCaption = Res.GetString("3882E4C1-23A5-438D-9018-BC0532A42776", "New Estimated Date Of Arrival");
					}
					else
					{
						formCaption = Res.GetString("98F0BB2F-9090-4BAC-B4F0-5AD97C8A7A84", "Vessel Arrival");
					}
					break;
				case ActionCode.VesselDeparture:
					BillsGrid.RemoveFromAvailableColumns(MessageSendingObject.Schema.MB_AmendmentCode, MessageSendingObject.Schema.MB_Date, MessageSendingObject.Schema.MB_PortOfUnlading, MessageSendingObject.Schema.MB_PortOfUnladingOverride);
					formCaption = Res.GetString("BF8D391A-3C16-4D9D-885A-E48A6AD73E3A", "Vessel Departure");
					break;
			}

			if (isInBondAction)
			{
				if (sendingAction.ActionCode == ActionCode.SubsequentInBondAmendment)
				{
					BillsGrid.RemoveFromAvailableColumns(MessageSendingObject.Schema.MB_Send);
				}
				else if (ActionCodeTool.IsInBondArrivalExportationTOL(sendingAction.ActionCode))
				{
					BillsGrid.RemoveFromAvailableColumns(MessageSendingObject.Schema.MB_Send, MessageSendingObject.Schema.MB_AmendmentCode);
				}
				else
				{
					BillsGrid.RemoveFromAvailableColumns(MessageSendingObject.Schema.MB_Send, MessageSendingObject.Schema.MB_AmendmentCode, MessageSendingObject.Schema.MB_BillActionCode);
				}
				formCaption = GetInBondCaption(sendingAction.ActionCode);
			}
			else
			{
				BillsGrid.RemoveFromAvailableColumns(MessageSendingObject.Schema.MB_RelatedDetails);
			}

			if (ActionCodeTool.IsVesselEvent(sendingAction.ActionCode))
			{
				UpdateMB_Date(sendingAction.ActionCode);
			}
			else
			{
				MovementsGrid.RemoveFromAvailableColumns(MessageSendingMovement.Schema.MM_Date);
			}

			if (sendingAction.ActionCode != ActionCode.VesselDeparture)
			{
				MovementsGrid.RemoveFromAvailableColumns(MessageSendingMovement.Schema.MM_ForeignDeparturePort);
			}

			if (!ActionCodeTool.IsPermitToTransferAction(sendingAction.ActionCode))
			{
				MovementsGrid.RemoveFromAvailableColumns(MessageSendingMovement.Schema.MM_PTTFiler);
			}

			MovementsGrid.GridId = MovementsGrid.GridId + sendingAction.ActionCode.ToString();
			BillsGrid.GridId = BillsGrid.GridId + sendingAction.ActionCode.ToString();
			BillsGrid.ContextMenu.MenuItems.Add("-");
			BillsGrid.ContextMenu.MenuItems.Add("Tick 'Send' for Selected", TickSendForSelected_Clicked);
			BillsGrid.ContextMenu.MenuItems.Add("Untick 'Send' for Selected", UntickSendForSelected_Clicked);
		}

		void UpdateMB_Date(ActionCode actionCode)
		{
			var columnStyleInfo = BillsGrid.GetColumnStyle(MessageSendingObject.Schema.MB_Date) as ZArchitecture.ZDateEditColumnStyleInfo;

			if (actionCode == ActionCode.VesselDeparture)
			{
				columnStyleInfo = MovementsGrid.GetColumnStyle(MessageSendingMovement.Schema.MM_Date) as ZArchitecture.ZDateEditColumnStyleInfo;
			}

			if (columnStyleInfo != null)
			{
				if (actionCode == ActionCode.VesselArrival)
				{
					columnStyleInfo.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("6A5A79D4-C3D4-4184-8E96-4D404BF5048E", "Arrival Date");
				}
				else
				{
					columnStyleInfo.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("1e25e6d0-aec0-47b9-a4db-3af0e9de6520", "Date");
				}
			}
		}

		string GetInBondCaption(ActionCode actionCode)
		{
			string caption;
			switch (actionCode)
			{
				case ActionCode.InBondArrival:
					caption = "In-Bond Arrival";
					break;
				case ActionCode.SubsequentInBondAmendment:
					caption = "Subsequent In-Bond Amendment";
					break;
				case ActionCode.InBondExportation:
					caption = "In-Bond Exportation";
					break;
				case ActionCode.InBondDiversion:
					caption = "In-Bond Diversion";
					break;
				case ActionCode.InBondTransferOfLiability:
					caption = "In-Bond Transfer of Liability";
					break;
				default:
					caption = "Subsequent In-Bond Original";
					break;
			}

			return caption;
		}

		public CusInBondHeader Header
		{
			get { return header; }
		}
		readonly CusInBondHeader header;

		public new MessageSendingAction BusinessEntity
		{
			get { return (MessageSendingAction)base.BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb => Res.GetString("1dadf2c1-7f79-45c4-b3a7-940ef08fc314", "Send");

		public override string FormCaption
		{
			get { return formCaption; }
		}
		readonly string formCaption;

		bool NoBillWasFlaggedToSend
		{
			get
			{
				var result = true;
				foreach (MessageSendingObject obj in BusinessEntity.MessageSendingObjects)
				{
					if (obj.MB_Send)
					{
						result = false;
						break;
					}
				}
				return result;
			}
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			if (NoBillWasFlaggedToSend)
			{
				Globals.Message.ShowError(NoBillWasFlaggedToSendMessage, MessageSender.AMSReportingCaption);
			}
			else
			{
				this.DialogResult = DialogResult.Cancel;
				Customs.Business.MessageSendingNotificationCollection notifications = null;

				using (var progressForm = new ProgressForm())
				{
					progressForm.ShowCancelButton = false;
					progressForm.ShowProgressBar = false;
					progressForm.CaptionResourceString = new ResourceStringData("845d2f4b-d7ce-4165-8dc7-ee32e662e863", "Validating data");
					progressForm.ShowModalTo(this);

					notifications = MessageSender.GetNotifications(BusinessEntity);
				}

				if (notifications.ContainsError())
				{
					Globals.Message.ShowError(notifications.NotificationsAsString(), MessageSender.AMSReportingCaption);
				}
				else if (!notifications.ContainsWarning() || Globals.Message.Show(notifications.NotificationsAsString(), MessageSender.AMSReportingCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					this.DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		public static string NoBillWasFlaggedToSendMessage
		{
			get
			{
				return ResString.GetMultilingualString("USAMSMessageSendingActionForm|7ACE91F6-A462-4ED7-9816-9A360DA13FE7",
					"No entities have been selected for AMS reporting");
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}

		void TickSendForSelected_Clicked(object sender, EventArgs e)
		{
			SetSendForSelected(true);
		}

		void UntickSendForSelected_Clicked(object sender, EventArgs e)
		{
			SetSendForSelected(false);
		}

		void SetSendForSelected(bool ticked)
		{
			foreach (MessageSendingObject obj in BillsGrid.SelectedElements)
			{
				obj.MB_Send = ticked;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var messageAction = BusinessEntity;
				if (messageAction != null)
				{
					messageAction.ResetValidationModesAndUnRegisterBillAsEditableChildObject();
					messageAction.Movements.ReleaseAllInBondNumberMutex();
				}
			}
			base.Dispose(disposing);
		}
	}
}
