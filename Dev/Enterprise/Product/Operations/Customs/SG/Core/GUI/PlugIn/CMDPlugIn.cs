using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Customs.SG.V4.GUI.CMDMessaging;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.SG.V4.GUI
{
	public abstract class CMDPlugIn : ZPlugIn
	{
		public CMDPlugIn(IBusiness hostEntity)
			: base(hostEntity)
		{
			HookEvents();
			SetEnabled();
		}

		public override string Name
		{
			get { return "CMD"; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();
			}

			base.Dispose(disposing);
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		protected virtual void SetEnabled()
		{
			Enabled = TransportMode == Constants.TransportModes.Air ||
								TransportMode == Constants.TransportModes.AirSea ||
								TransportMode == Constants.TransportModes.SeaAir;
		}

		protected override sealed IBusiness GetBusinessEntityForPlugIn()
		{
			return GetCMDWrapperBizO();
		}

		protected new CMDWrapperBase BusinessEntity
		{
			get { return (CMDWrapperBase)base.BusinessEntity; }
		}

		protected new BusinessObject HostBusinessEntity
		{
			get { return (BusinessObject)base.HostBusinessEntity; }
		}

		#region CMD Menu Item

		protected override MenuItem GetNewTopLevelMenu()
		{
			MenuItem result = new ZMenuItem(MenuItemText.MainMenu);

			result.MenuItems.Add(MenuItemText.SendCMD, SendCMDMenuItem_Click);
			result.MenuItems.Add(MenuItemText.DeleteCMD, DeleteCMDMenuItem_Click);
			result.MenuItems.Add(MenuItemText.SendCMDToSpecificGHA, SendCMDToSpecificGHAMenuItem_Click);

			return result;
		}

		void SendCMDMenuItem_Click(object sender, EventArgs e)
		{
			HandleMenuItemClick(BusinessEntity.RunPreSendValidation,
				delegate(CMDNotificationBuffer notifications, CancelEventArgs args)
				{
					BusinessEntity.SendMessage(notifications);
				});
		}

		void SendCMDToSpecificGHAMenuItem_Click(object sender, EventArgs e)
		{
			HandleMenuItemClick(BusinessEntity.RunPreSendValidation, SendCMDToSpecificGHA);
		}

		void DeleteCMDMenuItem_Click(object sender, EventArgs e)
		{
			HandleMenuItemClick(BusinessEntity.RunPreSendValidation,
				delegate(CMDNotificationBuffer notifications, CancelEventArgs args)
				{
					BusinessEntity.DeleteExistingCMDMessages(notifications);
				});
		}

		void HandleMenuItemClick(ValidationMethodEventHandler validationMethod, MessageSendingMethodEventHandler messageSendingMethod)
		{
			if (HostBusinessEntity.IsInDatabase && !HostBusinessEntity.HasChanges)
			{
				CMDNotificationBuffer notifications = new CMDNotificationBuffer(HostBusinessEntity);
				validationMethod.Invoke(notifications);
				if (!notifications.HasErrors)
				{
					CancelEventArgs eventArgs = new CancelEventArgs();
					messageSendingMethod.Invoke(notifications, eventArgs);

					if (!eventArgs.Cancel)
					{
						Factory.Save();
						Globals.Message.ShowInformation(notifications.GetInfoMessages(), "CMD Messages Submitted");
					}
				}
				else
				{
					Globals.Message.ShowError(notifications.GetErrorMessages(), "Please fix the following error(s)");
				}
			}
			else
			{
				Globals.Message.ShowError("You must save before you can send CMD messages");
			}
		}

		void SendCMDToSpecificGHA(CMDNotificationBuffer notifications, CancelEventArgs args)
		{
			GHACapture bizO = new GHACapture(BusinessEntity);
			if (ZFormModaliser.ShowDialogAndDispose(new GHACaptureForm(bizO)) == DialogResult.OK)
			{
				BusinessEntity.SendMessage(bizO.GHA, notifications);
			}
			else
			{
				args.Cancel = true;
			}
		}

		delegate void ValidationMethodEventHandler(CMDNotificationBuffer notifications);
		delegate void MessageSendingMethodEventHandler(CMDNotificationBuffer notifications, CancelEventArgs eventArgs);

		#endregion

		#region MenuItemText

		static class MenuItemText
		{
			public const string MainMenu = "CMD";

			public const string SendCMD = "Send CMD";
			public const string DeleteCMD = "Delete CMD";
			public const string SendCMDToSpecificGHA = "Send CMD to Specific GHA";
		}

		#endregion

		#region Abstract

		protected internal abstract ZString TransportMode { get; }

		protected internal abstract void HookEvents();

		protected internal abstract void UnhookEvents();

		protected abstract CMDWrapperBase GetCMDWrapperBizO();

		#endregion
	}
}
