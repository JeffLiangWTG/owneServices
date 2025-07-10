using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class PhoneDiallerUserControl : ZUserControl
	{
		public PhoneDiallerUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				SetupDropButton();
			}
		}

		#region CurrentDataItem

		new ZString CurrentDataItem
		{
			get { return base.CurrentDataItem != null ? (ZString)base.CurrentDataItem : ZString.Empty; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			RefreshDialingProtocolsContextMenu();
		}

		#endregion

		#region CallButton

		void CallButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem.IsEmpty)
			{
				ShowNoPhoneEnteredMessage();
			}
			else
			{
				var defaultDialingProtocol = SystemDataRegistry.Instance.PhoneDialingUriProtocols.Value.Default;
				if (defaultDialingProtocol != null)
				{
					Dial(CurrentDataItem, defaultDialingProtocol.Code);
				}
				else
				{
					ShowNoDialingUriProtocolMessage();
				}
			}
		}

		#endregion

		#region DropButton

		void SetupDropButton()
		{
			DropButton.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
		}

		void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			if (DialingProtocolsContextMenuItems.Count == 0)
			{
				if (CurrentDataItem.IsEmpty)
				{
					ShowNoPhoneEnteredMessage();
				}
				else
				{
					ShowNoDialingUriProtocolMessage();
				}
			}
		}

		#endregion

		#region DialingProtocolsContextMenu

		protected ToolStripItemCollection DialingProtocolsContextMenuItems
		{
			get { return DropButton.Items; }
		}

		void RefreshDialingProtocolsContextMenu()
		{
			DialingProtocolsContextMenuItems.Clear();

			if (!CurrentDataItem.IsEmpty)
			{
				foreach (var dialingProtocol in SystemDataRegistry.Instance.PhoneDialingUriProtocols.Value.GetEnabled())
				{
					var dialingProtocolMenuItem = new ZToolStripMenuItem(
							Res.GetString("008c5ac8-ffc4-4be5-a40b-6432bf9bb4c6", "Call using {0}", dialingProtocol.Description),
							(sender, e) =>
							{
								if (!CurrentDataItem.IsEmpty)
								{
									Dial(CurrentDataItem, dialingProtocol.Code);
								}
							}
						);
					DialingProtocolsContextMenuItems.Add(dialingProtocolMenuItem);
				}
			}
		}

		#endregion

		#region Dial

		void Dial(ZString number, string protocol)
		{
			var diallingEventArgs = new DiallingEventArgs();
			if (Dialling != null)
			{
				Dialling(this, diallingEventArgs);
			}

			if (diallingEventArgs.CreateRelatedCommunication)
			{
				var communicationController = GetNewCommunicationController();
				((ICommunicationController)communicationController).CreateNewWithParentFormBizObjDefaults = true;
				var communication = (OrgSalesCall)((ZControllerInternals)communicationController).GetNewBusinessEntityInLocalFactory();

				var diallingRelatedCommunicationContactDecidingArgs = new DiallingRelatedCommunicationContactDecidingArgs();
				if (DiallingRelatedCommunicationContactDeciding != null)
				{
					DiallingRelatedCommunicationContactDeciding(this, diallingRelatedCommunicationContactDecidingArgs);
				}
				communication.OQ_OC = diallingRelatedCommunicationContactDecidingArgs.ContactPk;
				communicationController.ShowFormForNewEntity(communication);
			}

			PhoneDialler.Dial(number, protocol);
		}
		public event EventHandler<DiallingEventArgs> Dialling;
		public event EventHandler<DiallingRelatedCommunicationContactDecidingArgs> DiallingRelatedCommunicationContactDeciding;

		#endregion

		#region Messages

		void ShowNoDialingUriProtocolMessage()
		{
			Globals.Message.ShowError(NoDialingUriProtocolMessage, CannotMakeCallCaption);
		}

		void ShowNoPhoneEnteredMessage()
		{
			Globals.Message.ShowInformation(ResString.GetMultilingualString("82f9e978-eae6-4492-96d8-b5b8e26df463", "No phone number entered."), CannotMakeCallCaption);
		}

		public static string NoDialingUriProtocolMessage
		{
			get { return ResString.GetMultilingualString("ee2185e5-7cc0-4702-a2ab-c676b673b726", "Phone Dialing URI Protocol must be set before phone calls can be established. You can modify this in the System Registry under {0}.", SystemDataRegistry.Instance.PhoneDialingUriProtocols.Category + "/" + SystemDataRegistry.Instance.PhoneDialingUriProtocols.Caption); }
		}

		public static string CannotMakeCallCaption
		{
			get { return ResString.GetMultilingualString("90c23088-169a-47f4-8915-99964e6b6257", "Can not make call"); }
		}

		#endregion

		#region Implementation

		#region CommunicationController

		public ZController GetNewCommunicationController()
		{
			var result = ZControllerFactory.Create(ControllerIDs.Communication);

#if DEBUG
			LastCommunicationControllerForTesting = result;
#endif

			return result;
		}

#if DEBUG
		public ZController LastCommunicationControllerForTesting;
#endif

		#endregion

		#region PhoneDialler

		PhoneDialler PhoneDialler
		{
			get { return phoneDialler ?? (phoneDialler = GetNewPhoneDialler()); }
		}
		PhoneDialler phoneDialler;

		protected virtual PhoneDialler GetNewPhoneDialler()
		{
#if DEBUG
			if (PhoneDiallerOverrideForTest != null)
			{
				return PhoneDiallerOverrideForTest;
			}
#endif
			return new PhoneDialler();
		}

#if DEBUG
		public PhoneDialler PhoneDiallerOverrideForTest;
#endif

		#endregion

		#region Classes

		public class DiallingEventArgs : EventArgs
		{
			public DiallingEventArgs()
			{
				CreateRelatedCommunication = false;
			}

			public bool CreateRelatedCommunication;
		}

		public class DiallingRelatedCommunicationContactDecidingArgs : EventArgs
		{
			public DiallingRelatedCommunicationContactDecidingArgs()
			{
				ContactPk = ZGuid.Empty;
			}

			public ZGuid ContactPk;
		}

		#endregion

		#endregion
	}
}
