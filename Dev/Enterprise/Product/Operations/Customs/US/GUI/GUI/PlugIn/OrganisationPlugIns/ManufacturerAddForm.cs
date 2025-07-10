using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public enum ManufacturerAddFormResult
	{
		Cancel,
		SendAddMessage,
		SendQueryMessage,
		SendUpdateMessage
	}

	public partial class ManufacturerAddForm : ZChildForm
	{
		public ManufacturerAddForm(ManufacturerAddMessageData addMessageData)
			: base(addMessageData)
		{
			addMessageData.US_MIDInfo.ValueChanged += delegate
			{
				SendUpdateButton.Enabled = !addMessageData.US_MID.IsEmpty;
				SendQueryButton.Enabled = !addMessageData.US_MID.IsEmpty;
			};
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public override string FormCaption
		{
			get { return "Add/Update/Query Manufacturer Name & Address"; }
		}

		public ManufacturerAddMessageData AddMessageData
		{
			get { return (ManufacturerAddMessageData)base.BusinessEntity; }
		}

		public ManufacturerAddFormResult Result
		{
			get;
			internal set;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		bool CanSend()
		{
			var messageData = AddMessageData;
			if (messageData.HasErrors)
			{
				Globals.Message.Show(
					"Please fix the following error to continue." + System.Environment.NewLine + messageData.GetErrors().ToUniqueMessageListString(),
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			else if (messageData.HasMessageErrors || messageData.HasWarnings)
			{
				var warningsAndMessageErrors =
					messageData.GetMessageErrors().Concat(
					messageData.GetWarnings());
				return GetConfirmationWithThisWarning(warningsAndMessageErrors.ToUniqueMessageListString()) == DialogResult.Yes;
			}
			else
			{
				return true;
			}
		}

		void SendAddButton_Click(object sender, EventArgs e)
		{
			AddMessageData.RunPreSaveValidation();
			if (CanSend())
			{
				Close(ManufacturerAddFormResult.SendAddMessage);
			}
		}

		void SendUpdateButton_Click(object sender, EventArgs e)
		{
			AddMessageData.RunPreSaveValidation();
			if (CanSend())
			{
				Close(ManufacturerAddFormResult.SendUpdateMessage);
			}
		}

		void SendQueryButton_Click(object sender, EventArgs e)
		{
			AddMessageData.RunPreSaveValidation();
			if (CanSend())
			{
				Close(ManufacturerAddFormResult.SendQueryMessage);
			}
		}

		void CancelButton2_Click(object sender, EventArgs e)
		{
			Close(ManufacturerAddFormResult.Cancel);
		}

		void Close(ManufacturerAddFormResult result)
		{
			Result = result;
			Close();
		}

		DialogResult GetConfirmationWithThisWarning(string warningMessage)
		{
			return Globals.Message.Show("There are message errors.\r\n\r\n" + warningMessage + "\r\nAre you sure you wish to continue?", "Message Errors", MessageBoxButtons.YesNo, DialogResult.No);
		}
	}
}
