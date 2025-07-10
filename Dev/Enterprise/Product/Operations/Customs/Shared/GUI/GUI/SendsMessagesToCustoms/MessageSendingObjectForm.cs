using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class MessageSendingObjectForm : ZChildForm
	{
		public MessageSendingObjectForm()
		{
#if DEBUG
			if (this.IsDesignMode())
			{
				BindingSource.DataSourceType = typeof(Business.Testing.BaseMessageSendingObjectForTest);
			}
			else
			{
				ChangeFormBorderStyleForRendering();
			}
#endif
		}

		public MessageSendingObjectForm(BaseMessageSendingObjectParent messageParent)
			: base(messageParent)
		{
			BindingSource.DataSourceType = messageParent.GetType();
			ChangeFormBorderStyleForRendering();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		public override string FormHeading => Res.GetString("8F3FAEC3-EA48-4650-B952-418449501943", "Sending Messages");

		protected virtual ResourceStringData MessageSendingObjectsGroupBoxCaption => Res.GetData("22FCD9E4-51E1-43D7-BDA4-EBE460884B25", "Messages to be sent");

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Implementation

		protected virtual void SendButton_Click(object sender, EventArgs e)
		{
			if (CheckIsOKToSend())
			{
				this.DialogResult = DialogResult.OK;
				SendButton_ClickCore();
				Close();
			}
		}

		protected virtual void SendButton_ClickCore()
		{
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			CancelButton_ClickCore();
			Close();
		}

		protected virtual void CancelButton_ClickCore()
		{
		}

		public new BaseMessageSendingObjectParent BusinessEntity => (BaseMessageSendingObjectParent)base.BusinessEntity;

		protected virtual bool CheckIsOKToSend()
		{
			var result = false;
			if ((!BusinessEntity?.AllowEmptyDeclaration ?? true) && (!BusinessEntity?.HasAnyObjectToSend ?? true))
			{
				Globals.Message.ShowError(NothingSelectedMessage);
			}
			else
			{
				var notifications = RunPreSendValidation();
				if (notifications.ContainsError())
				{
					Globals.Message.ShowError(notifications.ErrorNotificationsAsString());
				}
				else if (!notifications.ContainsWarning() || ContinueToSendWithWarnings(notifications.NotificationsAsString()))
				{
					result = IsSupervisorApproved();
				}
			}

			return result;
		}

		protected virtual bool ContinueToSendWithWarnings(string warnings)
		{
			return Globals.Message.Show(warnings, Res.GetString("38C96720-3BED-4196-AFA0-E992A870C0A8", "Continue to Send"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		protected virtual bool IsSupervisorApproved()
		{
			var result = true;
			var businessEntity = BusinessEntity;
			if (businessEntity != null && IsSupportSupervisorApprove(businessEntity))
			{
				var businessObject = businessEntity.TopLevelBusinessObject as EnterpriseBusinessObject;
				if (businessObject != null)
				{
					var supervisorOverrides = new SupervisorOverrides(businessObject, SupervisorOverridesContext.SendingMessages);
					result = SupervisorOverridesHelper.IsSupervisorApproved(supervisorOverrides, businessObject.Logs);
				}
			}
			return result;
		}

		protected virtual bool IsSupportSupervisorApprove(BaseMessageSendingObjectParent businessEntity) => businessEntity.SecurityCheckpointToSendWithMessageError.IsAllowed;

		protected virtual string NothingSelectedMessage => Res.GetString("17A65515-6892-4B9E-907C-ED01CEC6476A", "There's nothing selected to be sent to Customs");

		protected virtual MessageSendingNotificationCollection RunPreSendValidation()
		{
			var sendingValidation = MessageSendingValidation.New(BusinessEntity, null);
			return sendingValidation.CheckBusinessObjectLevelValidation();
		}

		protected virtual ZButton GetEffectiveSendButton() => SendButton;

		#endregion
	}
}
