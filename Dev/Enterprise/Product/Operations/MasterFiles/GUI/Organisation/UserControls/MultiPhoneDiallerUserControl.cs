using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public abstract partial class MultiPhoneDiallerUserControl : ZUserControl
	{
		protected MultiPhoneDiallerUserControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				SetupDropButton();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			RefreshControls();
		}

		#region DataBinding

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			RefreshControls();
		}

		protected void RefreshControls()
		{
			RefreshCallButton();
			RefreshAlternativesContextMenu();
		}

		#endregion

		#region CallButton

		protected void RefreshCallButton()
		{
			DefaultDialInfo = GetDefaultDialInfo();

			CallButton.TextIgnoringInternalPadding =
				DefaultDialInfo != null ?
				Res.GetString("2469ec15-9d78-43fb-97ed-3fccd2d11c1a", "{0}{1}",
					DefaultDialInfo.Description,
					string.IsNullOrEmpty(DefaultDialInfo.Extension) ? string.Empty : "*")
				:
				Res.GetString("b1cd8753-6e75-42ca-a281-d10b9293b888", "Call");
		}

		protected abstract PhoneDialInfo GetDefaultDialInfo();

		void CallButton_Click(object sender, EventArgs e)
		{
			var defaultDialingProtocol = SystemDataRegistry.Instance.PhoneDialingUriProtocols.Value.Default;
			if (defaultDialingProtocol != null)
			{
				if (DefaultDialInfo != null)
				{
					Dial(DefaultDialInfo, defaultDialingProtocol.Code);
				}
				else
				{
					ShowNoDefaultPhoneContactDetailsMessage();
				}
			}
			else
			{
				ShowNoDialingUriProtocolMessage();
			}
		}

		PhoneDialInfo DefaultDialInfo;

		#endregion

		#region DropButton

		void SetupDropButton()
		{
			DropButton.ContextMenuStrip.Opening += DropButtonContextMenuStrip_Opening;
		}

		void DropButtonContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			if (AlternativesContextMenuItems.Count == 0)
			{
				if (!SystemDataRegistry.Instance.PhoneDialingUriProtocols.Value.GetEnabled().Any())
				{
					ShowNoDialingUriProtocolMessage();
				}
				else
				{
					ShowNoPhoneContactDetailsMessage();
				}
			}
		}

		#region AlternativesContextMenu

		protected ToolStripItemCollection AlternativesContextMenuItems
		{
			get { return DropButton.Items; }
		}

		protected abstract IEnumerable<PhoneDialInfo> AlternativePhoneDialInfoList { get; }

		protected virtual void RefreshAlternativesContextMenu()
		{
			AlternativesContextMenuItems.Clear();

			if (AlternativePhoneDialInfoList.Any())
			{
				var dialingProtocols = SystemDataRegistry.Instance.PhoneDialingUriProtocols.Value;
				var enabledDialingProtocols = dialingProtocols.GetEnabled();
				if (enabledDialingProtocols.Any())
				{
					var defaultDialingProtocol = dialingProtocols.Default;
					if (defaultDialingProtocol != null)
					{
						AddPhoneMenuItems(AlternativesContextMenuItems, defaultDialingProtocol);
					}

					foreach (var dialingProtocol in enabledDialingProtocols)
					{
						if (dialingProtocol != defaultDialingProtocol)
						{
							var dialingProtocolItem = new ZToolStripMenuItem(Res.GetString("9a65ab01-9745-491e-bdd5-45028c8ea432", "Call using {0}", dialingProtocol.Description));
							AddPhoneMenuItems(dialingProtocolItem.DropDownItems, dialingProtocol);
							AlternativesContextMenuItems.Add(dialingProtocolItem);
						}
					}
				}
			}
		}

		void AddPhoneMenuItems(ToolStripItemCollection parent, CodeDescriptionWithEnabledAndDefault dialingProtocol)
		{
			foreach (var dialInfo in OrderedList)
			{
				var shouldAdd = true;
				if (dialInfo.ContactItem != null && dialingProtocol.Code != SystemPhoneDiallingUriProtocols.Codes.Skype)
				{
					shouldAdd =
						dialInfo.ContactItem.OI_Description != PhoneContactItemDescriptionList.Codes.Skype &&
						dialInfo.ContactItem.OI_Description != PhoneContactItemDescriptionList.Codes.Skype2;
				}

				if (shouldAdd)
				{
					var text = string.IsNullOrEmpty(dialInfo.Extension)
									? ResString.GetMultilingualString("dbfe4fef-c057-4221-a094-f25f56562af0", "Call {0} ({1})", dialInfo.Description, dialInfo.Number)
									: ResString.GetMultilingualString("145bce3d-2c4f-4972-911d-6a8429439a0b", "Call {0} ({1}) {2}", dialInfo.Description, dialInfo.Number, dialInfo.Extension);

					var phoneMenuItem = new ZToolStripMenuItem(
							text,
							(sender, args) => Dial(dialInfo, dialingProtocol.Code)
						);
					parent.Add(phoneMenuItem);
				}
			}
		}

		protected virtual IEnumerable<PhoneDialInfo> OrderedList
		{
			get { return AlternativePhoneDialInfoList.OrderBy(item => item.Description).ThenBy(item => item.Number); }
		}

		#endregion

		#endregion

		#region Dial

		void Dial(PhoneDialInfo dialInfo, string protocol)
		{
			var diallingEventArgs = new PhoneDiallingEventArgs(dialInfo);
			if (Dialling != null)
			{
				Dialling(this, diallingEventArgs);
			}

			if (diallingEventArgs.CreateRelatedCommunication)
			{
				var communicationController = GetNewCommunicationController();
				((ICommunicationController)communicationController).CreateNewWithParentFormBizObjDefaults = true;
				var communication = (OrgSalesCall)((ZControllerInternals)communicationController).GetNewBusinessEntityInLocalFactory();
				PopulateRelatedCommunication(communication);

				communicationController.ShowFormForNewEntity(communication);
			}

			PhoneDialler.Dial(dialInfo.Number, protocol);
		}
		public event EventHandler<PhoneDiallingEventArgs> Dialling;

		protected virtual void PopulateRelatedCommunication(OrgSalesCall relatedCommunication)
		{
		}

		#endregion

		#region Messages

		void ShowNoDialingUriProtocolMessage()
		{
			Globals.Message.ShowError(PhoneDiallerUserControl.NoDialingUriProtocolMessage, PhoneDiallerUserControl.CannotMakeCallCaption);
		}

		void ShowNoDefaultPhoneContactDetailsMessage()
		{
			Globals.Message.ShowInformation(NoDefaultPhoneContactDialsCaption, PhoneDiallerUserControl.CannotMakeCallCaption);
		}

		void ShowNoPhoneContactDetailsMessage()
		{
			Globals.Message.ShowInformation(NoPhoneContactDetailsCaption, PhoneDiallerUserControl.CannotMakeCallCaption);
		}

		protected virtual string NoDefaultPhoneContactDialsCaption
		{
			get { return NoPhoneNumberCaption; }
		}

		protected virtual string NoPhoneContactDetailsCaption
		{
			get { return NoPhoneNumberCaption; }
		}

		static string NoPhoneNumberCaption
		{
			get { return ResString.GetMultilingualString("6faa9ce1-b908-4513-8364-270731dda513", "No phone number available."); }
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

		#endregion

		public class ImageButtonWithoutTextInternalPadding : ZImageButton
		{
			public string TextIgnoringInternalPadding
			{
				get { return textIgnoringInternalPadding; }
				set
				{
					textIgnoringInternalPadding = value;
					Invalidate(false);
				}
			}
			string textIgnoringInternalPadding;

			protected override void OnPaint(PaintEventArgs pevent)
			{
#if !WINZOR
				base.OnPaint(pevent);

				if (!string.IsNullOrEmpty(TextIgnoringInternalPadding))
				{
					// manually paint text to bypass button internal text padding
					var stringFormat = new StringFormat();
					stringFormat.Alignment = StringAlignment.Center;
					stringFormat.LineAlignment = StringAlignment.Center;

					using (var brush = new SolidBrush(ForeColor))
					{
						TextRendererHelper.DrawText(pevent.Graphics, TextIgnoringInternalPadding, Font, ControlDpiScalingHelper.NewScaledRectangle(Point.Empty, ControlDpiScalingHelper.NewScaledSize(Width + Padding.Left - Padding.Right, Height + Padding.Top - Padding.Bottom, false), false), brush, stringFormat);
					}
				}
#else
				if (!string.IsNullOrEmpty(textIgnoringInternalPadding))
				{
					Text = textIgnoringInternalPadding;
					TextAlign = ContentAlignment.MiddleCenter;
					InnerDivStyleString = $" top: {Top}px; left: {Left}px; width: {Width}px; height: {Height}px;";
					Padding = ControlDpiScalingHelper.NewScaledPadding(Padding.Left, 0, 0, 0, true);
				}
#endif
			}
		}
	}
}
