using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class PGACorrectionSendingForm : ZChildForm
	{
		public PGACorrectionSendingForm()
		{
			InitializeComponent();
		}

		public PGACorrectionSendingForm(PGACorrectionMessageSendingAction action)
			: base(action)
		{
			this.action = action;
			InitializeComponent();
			ChangePanelVisibility();
		}
		readonly PGACorrectionMessageSendingAction action;

		void OKButton_Click(object sender, EventArgs e)
		{
			if (!action.US_SendMessage)
			{
				Globals.Message.ShowInformation(YouHaveNotSelectedAnythingToSendMessagesFor);
			}
			else
			{
				action.RunPreSaveValidation();

				if (action.HasNotifications(CargoWise.EntityFramework.NotificationType.MessageError) &&
						!Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed)
				{
					Globals.Message.ShowInformation(Enterprise.Customs.Business.MessageSendingValidation.MessageErrorsExistWithNoSecurityRight);
				}
				else if (!action.HasNotifications() || Globals.Message.Show(ThereIsANotification, "Send messages", MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK)
				{
					action.ShouldSendMessage = true;
					Close();
				}
			}
		}
		public const string YouHaveNotSelectedAnythingToSendMessagesFor = "There is nothing to send a message for";
		public const string ThereIsANotification = "There is a notification. Are you sure you wish to continue?";

		void CancelButton_Click(object sender, EventArgs e)
		{
			action.ShouldSendMessage = false;
			Close();
		}

		void ChangePanelVisibility()
		{
			var entryLinesWithPGAToSendCollection = new List<IPGAGovernmentAgenciesCommon>();
			var entryHeader = action.Entry;
			if (entryHeader != null)
			{
				foreach (var entryLine in entryHeader.GetEntryLinesWithPGAToSend())
				{
					var lineToSend = new PGAGovernmentAgenciesWithPGACorrection(entryLine, (IPGAGovernmentAgenciesCommon)entryLine);
					entryLinesWithPGAToSendCollection.Add(lineToSend);

					foreach (var secondaryLine in entryLine.SecondaryTariffLines)
					{
						var secondLineToSend = new PGAGovernmentAgenciesWithPGACorrection(entryLine, secondaryLine);
						entryLinesWithPGAToSendCollection.Add(secondLineToSend);
					}
				}
			}
		}
	}
}
