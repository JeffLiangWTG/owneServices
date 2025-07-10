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
	public partial class OrgAddressMessageDataForm : ZChildForm
	{
		public OrgAddressMessageDataForm()
		{
		}

		public OrgAddressMessageDataForm(OrgAddressMessageData addressMsgData)
			: base(addressMsgData)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			this.ImporterADDMessagePanel.Visible = false;
			this.ImporterConsigneeCreateUpdateMessagePanel.Visible = true;
			this.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("OrgAddressMessageDataForm|18120bcb-34f9-49f9-9baa-9ef16ffe90fd", "Importer/Consignee File (CBPF-5106) Add/Update");
			this.AutoScroll = true;
		}

		public new OrgAddressMessageData BusinessEntity
		{
			get { return (OrgAddressMessageData)base.BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public bool IsOKToSendMessage;
		internal void SendButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();

			IsOKToSendMessage = true;

			if (BusinessEntity.HasMessageErrors)
			{
				IEnumerable<INotification> collector = new ZNotificationCollector(BusinessEntity, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();

				if (GetConfirmationWithThisWarning(collector.ToUniqueMessageListString()) == DialogResult.No)
				{
					IsOKToSendMessage = false;
				}
			}

			if (IsOKToSendMessage)
			{
				Close();
			}
		}

		internal void CancelButton_Click(object sender, EventArgs e)
		{
			IsOKToSendMessage = false;
			Close();
		}

		DialogResult GetConfirmationWithThisWarning(string warningMessage)
		{
			return Globals.Message.Show("There are message errors.\r\n\r\n" + warningMessage + "\r\nAre you sure you wish to continue?", "Message Errors", MessageBoxButtons.YesNo, DialogResult.No);
		}

		void ViewStatementButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new StatDescriptionForm("5106"), ParentForm as ZForm);
		}
	}
}
