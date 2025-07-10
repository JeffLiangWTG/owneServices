using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ACEDrawbackMessageSendingForm : ZChildForm
	{
		public ACEDrawbackMessageSendingForm()
		{
			InitializeComponent();
		}

		public ACEDrawbackMessageSendingForm(ACEDrawbackAcknowledgeAndSign sign)
			: base(sign)
		{
			this.sign = sign;
			this.sign.US_SendMessage = true;
			InitializeComponent();
		}
		readonly ACEDrawbackAcknowledgeAndSign sign;

		void OKButton_Click(object sender, EventArgs e)
		{
			if (!sign.US_SendMessage)
			{
				Globals.Message.ShowInformation(YouHaveNotSelectedAnythingToSendMessagesFor);
			}
			else
			{
				sign.RunPreSaveValidation();

				if (sign.HasNotifications(CargoWise.EntityFramework.NotificationType.MessageError) &&
						!Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed)
				{
					Globals.Message.ShowInformation(Enterprise.Customs.Business.MessageSendingValidation.MessageErrorsExistWithNoSecurityRight);
				}
				else if (!sign.HasNotifications() || Globals.Message.Show(ThereIsANotification, "Send messages", MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK)
				{
					sign.ShouldSendMessage = true;
					Close();
				}
			}
		}
		public const string YouHaveNotSelectedAnythingToSendMessagesFor = "There is nothing to send a message for";
		public const string ThereIsANotification = "There is a notification. Are you sure you wish to continue?";

		void CancelButton_Click(object sender, EventArgs e)
		{
			sign.ShouldSendMessage = false;
			Close();
		}
	}
}
