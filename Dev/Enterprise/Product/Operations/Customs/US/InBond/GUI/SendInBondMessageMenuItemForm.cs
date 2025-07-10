using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.GUI
{
	public partial class SendInBondMessageMenuItemForm : ZChildForm
	{
		public SendInBondMessageMenuItemForm(InBondMenuItemMessageData inBondMenuItemMessageData)
			: base(inBondMenuItemMessageData)
		{
		}

		public new InBondMenuItemMessageData BusinessEntity
		{
			get { return (InBondMenuItemMessageData)base.BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.InitializeComponent();

			if (BusinessEntity.IsArrival)
			{
				this.CaptionResourceString = Res.GetData("267e02f7-9716-4bb5-913a-0499c6c8e4d2", "Send In-Bond Arrival Messages");
				this.SendExportationMessagesTabPage.TabVisible = false;
				this.MessageSendingObjectsGrid.RemoveFromAvailableColumns("EntryType", "PedimentoNumber");
			}
			else if (BusinessEntity.IsExport)
			{
				this.CaptionResourceString = Res.GetData("2d7c2a3c-557b-4ecd-88b9-4e738ec6235b", "Send In-Bond Exportation Messages");
				this.SendArrivalMessagesTabPage.TabVisible = false;
				this.MessageSendingObjectsGrid.RemoveFromAvailableColumns("PedimentoNumber");
			}
			else if (BusinessEntity.IsPedimento)
			{
				this.CaptionResourceString = Res.GetData("8f9aa90c-1170-4976-a867-43b66c0e20d3", "Allocate Pedimento Number");
				this.TopSplitContainer.Panel2Collapsed = true;
				this.MessageSendingObjectsGrid.RemoveFromAvailableColumns("EntryType");
				this.SendButton.Text = "&OK";
			}
			else
			{
				this.CaptionResourceString = Res.GetData("c115470a-aa20-497c-bc62-b3ac40c1f2eb", "Bulk Print 7512 Departure Document");
				this.TopSplitContainer.Panel2Collapsed = true;
				this.MessageSendingObjectsGrid.RemoveFromAvailableColumns("PedimentoNumber");
				this.SendButton.Text = "&Next";
			}
		}

		public override string FormVerb => string.Empty;

		void SendButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.IsArrival || BusinessEntity.IsExport)
			{
				SendMessageClicked();
			}
			else if (BusinessEntity.IsPedimento)
			{
				AllocatePedimentoNumberClicked();
			}
			else
			{
				BulkPrintDocumentClicked();
			}
		}

		void SendMessageClicked()
		{
			var isCancelled = false;
			var inBondMenuItemMessageData = BusinessEntity;
			var validation = Customs.Business.MessageSendingValidation.New(inBondMenuItemMessageData, new Customs.Business.CustomsNotificationCollector(inBondMenuItemMessageData, true, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName));
			var notifications = validation.CheckBusinessObjectLevelValidation();

			if (notifications.ContainsError())
			{
				isCancelled = true;
				Globals.Message.ShowError(notifications.NotificationsAsString());
			}
			else if (notifications.ContainsWarning())
			{
				isCancelled = Globals.Message.Show(notifications.NotificationsAsString(), "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No;
			}

			if (!isCancelled)
			{
				try
				{
					var movementHeaderNeedToBeSent = inBondMenuItemMessageData.CreateOrUpdateMovementHeader();
					inBondMenuItemMessageData.Factory.Save();
					var (invalidOperationText, result) = inBondMenuItemMessageData.SendMessages(movementHeaderNeedToBeSent);
					if (!string.IsNullOrEmpty(invalidOperationText))
					{
						Globals.Message.ShowError(invalidOperationText);
					}
					Globals.Message.ShowInformation(result);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError("An error occurred when sending messages, please try again later.");
					ZExceptionReporting.HandleSaveException(ex);
				}

				Close();
			}
		}

		void AllocatePedimentoNumberClicked()
		{
			var isCancelled = false;
			var inBondMenuItemMessageData = BusinessEntity;
			inBondMenuItemMessageData.RunPreSaveValidation();
			var notifications = inBondMenuItemMessageData.NotificationsIncludingChildren;

			var errors = notifications.GetErrors();
			if (errors.Any())
			{
				isCancelled = true;
				Globals.Message.ShowError(errors.ToUniqueMessageListString());
			}

			if (!isCancelled)
			{
				try
				{
					inBondMenuItemMessageData.AllocatePredimentoNumber();
					inBondMenuItemMessageData.Factory.Save();
					Globals.Message.ShowInformation("Allocate Pedimento Number Successfully.");
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError("An error occurred when allocating Pedimento Number, please try again later.");
					ZExceptionReporting.HandleSaveException(ex);
				}

				Close();
			}
		}

		void BulkPrintDocumentClicked()
		{
			var movementHeaders = BusinessEntity.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().Select(x => x.MovementHeader).Where(w => w != null);
			if (USInBond7512DataHelper.Print7512DepartureBulk(movementHeaders) != DeliveryInstructionDestination.UserCancelled)
			{
				Close();
			}
		}

		void CancelAndCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
