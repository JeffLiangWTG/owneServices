using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using MessageSender = Enterprise.Customs.US.LVS.Business.MessageSender;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.GUI
{
	public class EDIMenu : ZMenuItem
	{
		public EDIMenu()
		{
			this.Text = Res.GetString("88CDAE98-F050-4E0F-B762-2C1AF53DF46B", "&Messaging");
			SetupMenuItems();
		}

		public CusUSLVClearance Header { get; set; }

		void SetupMenuItems()
		{
			AddMenuItemIfNotNull(AddMessagingMenuItem);
			AddMenuItemIfNotNull(ReplaceMessagingMenuItem);
			AddMenuItemIfNotNull(UpdateMessagingMenuItem);
			AddMenuItemIfNotNull(DeleteMessagingMenuItem);
		}

		void AddMenuItemIfNotNull(ZMenuItem menuItem)
		{
			if (menuItem != null)
			{
				MenuItems.Add(menuItem);
			}
		}

		protected virtual ZMenuItem AddMessagingMenuItem => new ZMenuItem(ResString.GetMultilingualString("CB2D3515-6991-4198-81F2-BB3C39A959C4", "Send Original Messages"), SendOriginalMessages_Click);
		protected virtual ZMenuItem ReplaceMessagingMenuItem => new ZMenuItem(ResString.GetMultilingualString("541298F7-420D-4F9E-8DF3-06D8B3FBE1FF", "Send Replacement Messages"), SendReplacementMessages_Click);
		protected virtual ZMenuItem UpdateMessagingMenuItem => new ZMenuItem(ResString.GetMultilingualString("6E6BDB8A-0638-448C-93B6-4201221C4B9E", "Send Update Messages"), SendUpdateMessages_Click);
		protected virtual ZMenuItem DeleteMessagingMenuItem => new ZMenuItem(ResString.GetMultilingualString("DA93372F-CE16-4B5C-97D5-8460017F129F", "Send Deletion Messages"), SendDeletionMessages_Click);

		protected void SendOriginalMessages_Click(object sender, EventArgs e)
		{
			SendMessages(UpdateActionCode.Add);
		}

		protected void SendReplacementMessages_Click(object sender, EventArgs e)
		{
			SendMessages(UpdateActionCode.Replace);
		}

		protected void SendUpdateMessages_Click(object sender, EventArgs e)
		{
			SendMessages(UpdateActionCode.Update);
		}

		protected void SendDeletionMessages_Click(object sender, EventArgs e)
		{
			SendMessages(UpdateActionCode.Delete);
		}

		void SendMessages(UpdateActionCode updateAction)
		{
			var shouldSendMessages = true;

			if (!Header.IsInDatabase || Header.HasChanges)
			{
				if (Globals.Message.Show(Res.GetString("37a867f6-ac9b-4d35-aab1-84479bc84f74", "The data has not yet been saved. Do you want to save and proceed?"), Res.GetString("f60602c1-35e5-4d60-a565-ac719eb6c451", "Save Data"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					shouldSendMessages = MainForm.FireSaveButton() == ContinueWithSave.Yes;
				}
				else
				{
					shouldSendMessages = false;
				}
			}

			if (shouldSendMessages)
			{
				if (!Env.Security.USLVClearanceImportMessaging.IsAllowed)
				{
					Env.Security.USLVClearanceImportMessaging.ShowError();
				}
				else if (Header.IsSendCustomsMessageMutexLocked)
				{
					var mutexLockByInfo = Header.GetSendCustomsMessageMutexLockByInfo();
					if (!string.IsNullOrEmpty(mutexLockByInfo))
					{
						Globals.Message.ShowWarning(Res.GetString("8c3eba0f-8e19-4d10-9266-4d311848868f", "{0} is sending messages for this Low Value Entries job, please try again later.", mutexLockByInfo));
					}
				}
				else
				{
					PrepareCusUSLVConsignmentsForUpdateAction(updateAction);
					var clearanceWrapper = GetMessageWrapper();

					if (clearanceWrapper.CusUSLVConsignmentsToSend.Count == 0)
					{
						var message = updateAction == UpdateActionCode.Add ? "Original" : updateAction == UpdateActionCode.Delete ? "Deletion" : updateAction == UpdateActionCode.Replace ? "Replacement" : "Update";
						Globals.Message.Show(ResString.GetMultilingualString("A1DF39A4-3D68-4854-BBC7-7C5E4403AB2F", "There is no house bill eligible for sending {0} Messages.", message), "Unable to complete your request", MessageBoxButtons.OK, DialogResult.OK);
					}
					else
					{
						using (var directMessageSubmitForm = new MessagesSubmitForm(clearanceWrapper, updateAction))
						{
							var messageSender = new MessageSender(clearanceWrapper.Clearance);
							messageSender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(
							() =>
							{
								if (updateAction != UpdateActionCode.Add)
								{
									MainForm.FireSaveButton();
								}
							});

							messageSender.OnPrepare += new MessageSender.PrepareEventHandler(() => (ZDialogResult)ZFormModaliser.ShowDialogWithoutDispose(directMessageSubmitForm));
							messageSender.SendMessage();
						}
					}
				}
			}
		}

		protected virtual void PrepareCusUSLVConsignmentsForUpdateAction(UpdateActionCode updateAction)
		{
			Header.PrepareCusUSLVConsignmentsForUpdateAction(updateAction);
		}

		protected virtual CusUSLVClearanceMessageWrapper GetMessageWrapper() => new CusUSLVClearanceMessageWrapper(Header);

		ZForm MainForm
		{
			get { return (ZForm)GetMainMenu().GetForm(); }
		}
	}
}
