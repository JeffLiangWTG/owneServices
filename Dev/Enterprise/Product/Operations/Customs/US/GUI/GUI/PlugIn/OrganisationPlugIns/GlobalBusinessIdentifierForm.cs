using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class GlobalBusinessIdentifierForm : ZChildForm
	{
		public GlobalBusinessIdentifierForm(GlobalBusinessIdentifierData messageData)
			: base(messageData)
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return "Add/Update/Delete Global Business Identifier"; }
		}

		public new GlobalBusinessIdentifierData BusinessEntity
		{
			get { return (GlobalBusinessIdentifierData)base.BusinessEntity; }
		}

		void SendGBIAddButton_Click(object sender, EventArgs e)
		{
			SendGBIButton_Click(GlobalBusinessIdentifierMessageType.Original);
		}

		void SendGBIUpdateButton_Click(object sender, EventArgs e)
		{
			SendGBIButton_Click(GlobalBusinessIdentifierMessageType.Update);
		}

		void SendGBIDeleteButton_Click(object sender, EventArgs e)
		{
			SendGBIButton_Click(GlobalBusinessIdentifierMessageType.Delete);
		}

		void SendGBIButton_Click(GlobalBusinessIdentifierMessageType messageType)
		{
			if (BusinessEntity.SubmissionStatus.IsEmpty && (messageType == GlobalBusinessIdentifierMessageType.Update || messageType == GlobalBusinessIdentifierMessageType.Delete))
			{
				if (Globals.Message.Show("A GBI Add message has not yet been sent for this Organization/Address. Click OK to continue or Cancel to abort.", "Continue?", MessageBoxButtons.OKCancel, DialogResult.OK) != DialogResult.OK)
				{
					messageType = GlobalBusinessIdentifierMessageType.None;
				}
			}

			if (messageType != GlobalBusinessIdentifierMessageType.None)
			{
				BusinessEntity.RunPreSaveValidation();
				MessageType = messageType;

				if (BusinessEntity.HasErrors)
				{
					MessageType = GlobalBusinessIdentifierMessageType.None;
					Globals.Message.Show("Please fix the following error to continue." + System.Environment.NewLine + BusinessEntity.GetErrors().ToUniqueMessageListString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else if (BusinessEntity.HasMessageErrors)
				{
					IEnumerable<INotification> collector = new ZNotificationCollector(BusinessEntity, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();

					if (GetConfirmationWithThisWarning(collector.ToUniqueMessageListString()) == DialogResult.No)
					{
						MessageType = GlobalBusinessIdentifierMessageType.None;
					}
				}
			}

			if (MessageType != GlobalBusinessIdentifierMessageType.None)
			{
				Close();
			}
		}

		DialogResult GetConfirmationWithThisWarning(string warningMessage)
		{
			return Globals.Message.Show("There are message errors.\r\n\r\n" + warningMessage + "\r\n\r\nAre you sure you wish to continue?", "Message Errors", MessageBoxButtons.YesNo, DialogResult.No);
		}

		void CancelButton2_Click(object sender, EventArgs e)
		{
			MessageType = GlobalBusinessIdentifierMessageType.None;
			Close();
		}

		public GlobalBusinessIdentifierMessageType MessageType;
	}
}
