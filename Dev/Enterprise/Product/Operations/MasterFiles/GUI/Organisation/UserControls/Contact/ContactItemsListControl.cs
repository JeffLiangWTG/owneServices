using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ContactItemsListControl : ZUserControl, IReadOnlyToggleControl
	{
		public ContactItemsListControl()
		{
			InitializeComponent();

			SetupAddContactItemButtons();
			AddMandatoryContactItems();
		}

		#region CurrentDataItem

		new OrgContact CurrentDataItem
		{
			get { return (OrgContact)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			var previousContact = CurrentDataItem;
			if (previousContact != null)
			{
				UnhookContactItemHandlers(previousContact);
			}

			RemoveAllContactItemStrips(EmailItemsPanel, MandatoryEmailItemStrips.Values);
			RemoveAllContactItemStrips(PhoneItemsPanel, MandatoryPhoneItemStrips.Values);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var newContact = CurrentDataItem;
			if (newContact != null)
			{
				SuspendLayout();

				AddContactItemStrips(EmailItemsPanel, CurrentDataItem.EmailContactItems.Items, GetNewEmailContactItemStrip, MandatoryEmailItemStrips);
				AddContactItemStrips(PhoneItemsPanel, CurrentDataItem.PhoneContactItems.Items, GetNewPhoneContactItemStrip, MandatoryPhoneItemStrips);
				AdduncommittedForUnusedMandatoryStrips(CurrentDataItem.EmailContactItems, MandatoryEmailItemStrips);
				AdduncommittedForUnusedMandatoryStrips(CurrentDataItem.PhoneContactItems, MandatoryPhoneItemStrips);
				HookContactItemHandlers(newContact);

				UpdateDeliveryStatusControls();

				SortContactItemStrips();
				RefreshControlReadOnly();

				ResumeLayout();
			}
		}

		#endregion

		#region Properties

		#region ReadOnly

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get
			{
				if (readOnly)
				{
					return true;
				}

				var currentDataItem = CurrentDataItem;
				if (currentDataItem != null)
				{
					if (currentDataItem.EmailContactItems.ReadOnly && currentDataItem.PhoneContactItems.ReadOnly)
					{
						return true;
					}
				}

				return false;
			}
			set
			{
				if (readOnly != value)
				{
					readOnly = value;
					RefreshControlReadOnly();
				}
			}
		}
		bool readOnly;

		void RefreshControlReadOnly()
		{
			var readOnly = ReadOnly;
			AddEmailItemButton.Visible = !readOnly;
			AddPhoneItemButton.Visible = !readOnly;
		}

		#endregion

		#endregion

		#region Delivery Status Indicator

		string NonDeliveryReceiptText
		{
			get { return Res.GetString("e1ebd217-3c92-4c58-b3b5-68e929a7ec99", "Non-Delivery Receipt: "); }
		}
		string VerifiedDeliveryReceiptText
		{
			get { return Res.GetString("5c29af8c-1874-4db8-ab41-02a9c2e86301", "Verified"); }
		}
		string UnverifiedDeliveryReceiptText
		{
			get { return Res.GetString("907c8fb0-4baa-4307-9017-e48a49dee91b", "Unverified"); }
		}

		void UpdateDeliveryStatusControls()
		{
			var emailAddress = CurrentDataItem.EmailAddress;
			if (emailAddress.GI_DeliveryStatus == EmailDeliveryReportStatus.Codes.NonDeliveryReport)
			{
				DeliveryStatusLabel.Text = NonDeliveryReceiptText + CurrentDataItem.DeliveryReportTimeUtc.ToLocalBranchTime().ToLongTimeString();
				DeliveryStatusLabel.ForeColor = ZArchitecture.GUI.Notifications.NotificationColorScheme.GetFontColor(NotificationType.Warning);

				DeliveryStatusButton.Visible = true;
				DeliveryStatusButton.NormalBackgroundImage = WarningImageRest;
				DeliveryStatusButton.HotBackgroundImage = WarningImageHot;
			}
			else if (emailAddress.GI_DeliveryStatus == EmailDeliveryReportStatus.Codes.ValidReport)
			{
				DeliveryStatusLabel.Text = VerifiedDeliveryReceiptText;
				DeliveryStatusLabel.ForeColor = System.Drawing.Color.Black;

				DeliveryStatusButton.Visible = true;
				DeliveryStatusButton.NormalBackgroundImage = Properties.Resources.Verified;
				DeliveryStatusButton.HotBackgroundImage = Properties.Resources.Verified;
			}
			else if (EmailAddressValidation.IsEmailAddressValidAndNotEmpty(CurrentDataItem.OC_Email))
			{
				DeliveryStatusLabel.Text = UnverifiedDeliveryReceiptText;
				DeliveryStatusLabel.ForeColor = System.Drawing.Color.Black;

				DeliveryStatusButton.Visible = true;
				DeliveryStatusButton.NormalBackgroundImage = Properties.Resources.Unverified;
				DeliveryStatusButton.HotBackgroundImage = Properties.Resources.Unverified;
			}
			else
			{
				DeliveryStatusLabel.Text = string.Empty;
				DeliveryStatusButton.Visible = false;
			}
		}

		Bitmap WarningImageRest
		{
			get { return warningImageRest ?? (warningImageRest = Icons.GetIcon(IconTypes.Warning).ToBitmap()); }
		}
		Bitmap warningImageRest;

		Bitmap WarningImageHot
		{
			get
			{
				if (warningImageHot == null)
				{
					warningImageHot = new Bitmap(WarningImageRest.Width, WarningImageRest.Height);
					using (var g = Graphics.FromImage(warningImageHot))
					{
						// create a copy bumped up by 1 pixel
						var dst = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(0, 0, WarningImageRest.Width, WarningImageRest.Height - 1, false);
						var src = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(0, 1, WarningImageRest.Width, WarningImageRest.Height - 1, false);
						g.DrawImage(WarningImageRest, dst, src, GraphicsUnit.Pixel);
					}
				}
				return warningImageHot;
			}
		}
		Bitmap warningImageHot;

		void IsNDR_Changed(object sender, EventArgs e)
		{
			UpdateDeliveryStatusControls();
		}

		#endregion

		#region DeliveryStatusButton

		void DeliveryStatusButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem.EmailAddress.GI_DeliveryStatus == EmailDeliveryReportStatus.Codes.NonDeliveryReport)
			{
				var deliveryDetails = ObjectFactory.Get<IEmailDeliveryDetailsProvider>("IEmailDeliveryDetailsProvider", CurrentDataItem);
				if (!string.IsNullOrEmpty(deliveryDetails.BounceBackEmail))
				{
					IDeliveryDetailsPopupFormHelper trackingControl = ObjectFactory.Get<IDeliveryDetailsPopupFormHelper>();
					trackingControl.ShowDeliveryDetailsPopupForm(deliveryDetails, ParentForm);
				}
			}
		}

		#endregion

		#region AddContactItemStrips

		void AddContactItemStrips<TItem, TStrip>(ZPanel panel, IEnumerable<TItem> contactItems, Func<TItem, ContactItemStrip> newStripFunc, Dictionary<string, TStrip> mandatoryStrips)
			where TItem : ContactItemProxy
			where TStrip : ContactItemStrip
		{
			foreach (var contactItem in contactItems)
			{
				TStrip mandatoryStripToUse;
				if (!mandatoryStrips.TryGetValue(contactItem.OI_Description, out mandatoryStripToUse))
				{
					mandatoryStripToUse = null;
				}
				else if (mandatoryStripToUse.CurrentDataItem != null)
				{
					if (mandatoryStripToUse.CurrentDataItem.OI_Address.IsEmpty)
					{
						mandatoryStripToUse.CurrentDataItem.Delete();
					}
					else
					{
						mandatoryStripToUse = null;
					}
				}

				if (mandatoryStripToUse != null)
				{
					mandatoryStripToUse.SetDataBinding(contactItem, "");
				}
				else
				{
					var itemStrip = newStripFunc(contactItem);
					panel.Controls.Add(itemStrip);
				}
			}
		}

		#endregion

		#region RemoveContactItemStrip

		void RemoveContactItemStrip<TStrip>(ZPanel panel, TStrip strip, IEnumerable<TStrip> mandatoryStrips)
			where TStrip : ContactItemStrip
		{
			if (!mandatoryStrips.Contains(strip))
			{
				panel.Controls.Remove(strip);
				strip.Dispose();
			}
			else
			{
				strip.SetDataBinding(null, "");
			}
		}

		void RemoveAllContactItemStrips<TStrip>(ZPanel panel, IEnumerable<TStrip> mandatoryStrips)
			where TStrip : ContactItemStrip
		{
			panel.SuspendLayout();
			{
				foreach (var strip in panel.Controls.OfType<ContactItemStrip>().ToArray())
				{
					RemoveContactItemStrip(panel, strip, mandatoryStrips);
				}
			}
			panel.ResumeLayout();
		}

		#endregion

		#region UpdateContactItemStrips

		void UpdatePhoneItemStrips()
		{
			UpdateContactItemStrips(PhoneItemsPanel, CurrentDataItem.PhoneContactItems, MandatoryPhoneItemStrips);
		}

		void UpdateContactItemStrips<TItem, TStrip>(ZPanel panel, ContactItemProxyCollection<TItem> contactItems, Dictionary<string, TStrip> mandatoryStrips)
		where TItem : ContactItemProxy
		where TStrip : ContactItemStrip
		{
			panel.SuspendLayout();
			{
				var contactItemsToAdd = new HashSet<TItem>(contactItems.Items);
				foreach (var contactItem in contactItemsToAdd)
				{
					TStrip mandatoryStripToUse;
					if (mandatoryStrips.TryGetValue(contactItem.OI_Description, out mandatoryStripToUse))
					{
						mandatoryStripToUse.SetDataBinding(null, "");
						mandatoryStripToUse.SetDataBinding(contactItem, "");
					}
				}
			}
			panel.ResumeLayout();
		}

		#endregion

		#region RefreshContactItemStrips

		#region RefreshEmailContactItemStrips

		void RefreshEmailContactItemStrips(bool sort)
		{
			if (CurrentDataItem != null)
			{
				RefreshContactItemStrips(EmailItemsPanel, CurrentDataItem.EmailContactItems, GetNewEmailContactItemStrip, MandatoryEmailItemStrips, EmailContactItemComparer, sort);
			}
		}

		static EmailContactItemStrip GetNewEmailContactItemStrip(EmailContactItem emailItem)
		{
			var itemStrip = new EmailContactItemStrip();
			itemStrip.Dock = DockStyle.Top;
			itemStrip.SetDataBinding(emailItem, "");

			return itemStrip;
		}

		#endregion

		#region RefreshPhoneContactItemStrips

		void RefreshPhoneContactItemStrips(bool sort)
		{
			if (CurrentDataItem != null)
			{
				RefreshContactItemStrips(PhoneItemsPanel, CurrentDataItem.PhoneContactItems, GetNewPhoneContactItemStrip, MandatoryPhoneItemStrips, PhoneContactItemComparer, sort);
			}
		}

		static PhoneContactItemStrip GetNewPhoneContactItemStrip(PhoneContactItem phoneItem)
		{
			var itemStrip = new PhoneContactItemStrip();
			itemStrip.Dock = DockStyle.Top;
			itemStrip.SetDataBinding(phoneItem, "");

			return itemStrip;
		}

		#endregion

		void RefreshContactItemStrips<TItem, TStrip>(ZPanel panel, ContactItemProxyCollection<TItem> contactItems, Func<TItem, ContactItemStrip> newStripFunc, Dictionary<string, TStrip> mandatoryStrips, Comparer<ContactItemStrip> comparer, bool sort)
			where TItem : ContactItemProxy
			where TStrip : ContactItemStrip
		{
			panel.SuspendLayout();
			{
				var contactItemsToAdd = new HashSet<TItem>(contactItems.Items);
				foreach (var itemStrip in panel.Controls.OfType<TStrip>().ToArray())
				{
					var contactItem = itemStrip.CurrentDataItem as TItem;
					if (contactItemsToAdd.Contains(contactItem))
					{
						contactItemsToAdd.Remove(contactItem);
					}
					else
					{
						RemoveContactItemStrip(panel, itemStrip, mandatoryStrips.Values);
					}
				}

				if (contactItemsToAdd.Count > 0)
				{
					AddContactItemStrips(panel, contactItemsToAdd, newStripFunc, mandatoryStrips);

					if (sort)
					{
						panel.SuspendLayout();
						SortContactItemStrips(panel, comparer);
						panel.ResumeLayout();
					}
				}
			}
			panel.ResumeLayout();
		}

		void EmailContactItems_CountChanged(object sender, EventArgs e)
		{
			RefreshEmailContactItemStrips(true);
		}

		void PhoneContactItems_CountChanged(object sender, EventArgs e)
		{
			RefreshPhoneContactItemStrips(true);
		}

		void PhoneContactItems_ValueChanged(object sender, EventArgs e)
		{
			UpdatePhoneItemStrips();
		}

		#region Comparers

		readonly EmailContactItemStripComparerForListControl EmailContactItemComparer = new EmailContactItemStripComparerForListControl();
		readonly PhoneContactItemStripComparerForListControl PhoneContactItemComparer = new PhoneContactItemStripComparerForListControl();

		class EmailContactItemStripComparerForListControl : ContactItemStripComparerForListControl
		{
			public EmailContactItemStripComparerForListControl()
				: base(new EmailContactItemDescriptionList(), MandatoryEmailItemDescriptions.Select(pair => pair.Description).ToArray())
			{
			}
		}

		class PhoneContactItemStripComparerForListControl : ContactItemStripComparerForListControl
		{
			public PhoneContactItemStripComparerForListControl()
				: base(new PhoneContactItemDescriptionList(), MandatoryPhoneItemDescriptions.Select(pair => pair.Description).ToArray())
			{
			}
		}

		internal class ContactItemStripComparerForListControl : Comparer<ContactItemStrip>
		{
			protected ContactItemStripComparerForListControl(CodeDescriptionPairList orderedDescriptionList, string[] mandatoryDescriptions)
			{
				descriptionPriorityLookup = new Dictionary<string, int>(orderedDescriptionList.Count);

				var priority = orderedDescriptionList.Count;
				foreach (var mandatoryDescription in mandatoryDescriptions)
				{
					descriptionPriorityLookup[mandatoryDescription] = priority;
					priority--;
				}

				foreach (ICodeDescription description in orderedDescriptionList)
				{
					if (!descriptionPriorityLookup.ContainsKey(description.Description))
					{
						descriptionPriorityLookup[description.Description] = priority;
						priority--;
					}
				}
			}

			readonly Dictionary<string, int> descriptionPriorityLookup;

			public override int Compare(ContactItemStrip x, ContactItemStrip y)
			{
				int xDescriptionPriority;
				if (!descriptionPriorityLookup.TryGetValue(x.DescriptionDropDownList.Text, out xDescriptionPriority))
				{
					xDescriptionPriority = -1;
				}

				int yDescriptionPriority;
				if (!descriptionPriorityLookup.TryGetValue(y.DescriptionDropDownList.Text, out yDescriptionPriority))
				{
					yDescriptionPriority = -1;
				}

				if (xDescriptionPriority == yDescriptionPriority)
				{
					return x.DescriptionDropDownList.Text.CompareTo(y.DescriptionDropDownList.Text);
				}
				else
				{
					return -(xDescriptionPriority.CompareTo(yDescriptionPriority));
				}
			}
		}

		#endregion

		#endregion

		#region SortContactItemStrips

		void SortContactItemStrips()
		{
			SortContactItemStrips(EmailItemsPanel, EmailContactItemComparer);
			SortContactItemStrips(PhoneItemsPanel, PhoneContactItemComparer);
		}

		static void SortContactItemStrips(ZPanel panel, Comparer<ContactItemStrip> comparer)
		{
			foreach (var itemStrip in panel.Controls.OfType<ContactItemStrip>().OrderBy(x => x, comparer))
			{
				itemStrip.BringToFront();
			}
		}

		#endregion

		#region AddContactItemButtons

		void SetupAddContactItemButtons()
		{
			AddEmailItemButton.FlatStyle = FlatStyle.Flat;
			AddPhoneItemButton.FlatStyle = FlatStyle.Flat;

			var addButtonImage = Icons.GetImage(IconTypes.AddButtonRest);
			AddEmailItemButton.BackgroundImage = addButtonImage;
			AddPhoneItemButton.BackgroundImage = addButtonImage;
		}

		void AddEmailItemButton_Click(object sender, EventArgs e)
		{
			var contact = CurrentDataItem;
			if (contact != null)
			{
				var descriptions = new EmailContactItemDescriptionList();
				var descriptionWithoutAnItem = descriptions.Cast<CodeDescriptionPair>().FirstOrDefault(pair => !contact.EmailContactItems.GetItemsWithDescription(pair.Code).Any());
				if (descriptionWithoutAnItem != null)
				{
					var newItem = contact.EmailContactItems.AddNew();
					var newItemStrip = GetStrip(EmailItemsPanel, newItem);
					if (newItemStrip != null)
					{
						newItemStrip.BringToFront();
						newItemStrip.FocusDescriptionDropDownList();
					}

					newItem.OI_Description = descriptionWithoutAnItem.Code;
				}
				else
				{
					Globals.Message.ShowInformation(ResString.GetMultilingualString("6f0c42e0-5908-45a4-98ed-631bc4330005", "No more emails allowed."));
				}
			}
		}

		void AddPhoneItemButton_Click(object sender, EventArgs e)
		{
			var contact = CurrentDataItem;
			if (contact != null)
			{
				var descriptions = new PhoneContactItemDescriptionList();
				var descriptionWithoutAnItem = descriptions.Cast<CodeDescriptionPair>().FirstOrDefault(pair => !contact.PhoneContactItems.GetItemsWithDescription(pair.Code).Any());
				if (descriptionWithoutAnItem != null)
				{
					var newItem = contact.PhoneContactItems.AddNew();
					var newItemStrip = GetStrip(PhoneItemsPanel, newItem);
					if (newItemStrip != null)
					{
						newItemStrip.BringToFront();
						newItemStrip.FocusDescriptionDropDownList();
					}

					newItem.OI_Description = descriptionWithoutAnItem.Code;
				}
				else
				{
					Globals.Message.ShowInformation(ResString.GetMultilingualString("88a61463-5549-4b36-840e-4647123b528a", "No more phone numbers allowed."));
				}
			}
		}

		static ContactItemStrip GetStrip(ZPanel panel, ContactItemProxy contactItem)
		{
			return panel.Controls.OfType<ContactItemStrip>().First(itemStrip => itemStrip.CurrentDataItem == contactItem);
		}

		#endregion

		#region MandatoryContactItems

		EmailContactItemStrip AddMandatoryEmailContactItemStrip()
		{
			var itemStrip = new EmailContactItemStrip();
			itemStrip.Dock = DockStyle.Top;
			itemStrip.IsMandatory = true;
			BindingSource.SetBindingMember(itemStrip, "");
			EmailItemsPanel.Controls.Add(itemStrip);

			return itemStrip;
		}

		PhoneContactItemStrip AddMandatoryPhoneContactItemStrip()
		{
			var itemStrip = new PhoneContactItemStrip();
			itemStrip.Dock = DockStyle.Top;
			itemStrip.IsMandatory = true;
			BindingSource.SetBindingMember(itemStrip, "");
			PhoneItemsPanel.Controls.Add(itemStrip);

			return itemStrip;
		}

		void AddMandatoryContactItems()
		{
			MandatoryEmailItemStrips = new Dictionary<string, EmailContactItemStrip>();
			foreach (var description in MandatoryEmailItemDescriptions.Select(pair => pair.Code))
			{
				MandatoryEmailItemStrips.Add(description, AddMandatoryEmailContactItemStrip());
			}

			MandatoryPhoneItemStrips = new Dictionary<string, PhoneContactItemStrip>();
			foreach (var description in MandatoryPhoneItemDescriptions.Select(pair => pair.Code))
			{
				MandatoryPhoneItemStrips.Add(description, AddMandatoryPhoneContactItemStrip());
			}
		}

		void AdduncommittedForUnusedMandatoryStrips<TItem, TStrip>(ContactItemProxyCollection<TItem> contactItems, Dictionary<string, TStrip> mandatoryStrips)
			where TItem : ContactItemProxy
			where TStrip : ContactItemStrip
		{
			foreach (var strip in mandatoryStrips)
			{
				if (strip.Value.CurrentDataItem == null)
				{
					var uncommittedItem = contactItems.AddNew(strip.Key);
					strip.Value.SetDataBinding(uncommittedItem, "");
				}
			}
		}

		Dictionary<string, EmailContactItemStrip> MandatoryEmailItemStrips;

		static CodeDescriptionPair[] MandatoryEmailItemDescriptions
		{
			get
			{
				return new[]
				{
					new CodeDescriptionPair(EmailContactItemDescriptionList.Codes.Main, EmailContactItemDescriptionList.Descriptions.Main)
				};
			}
		}

		Dictionary<string, PhoneContactItemStrip> MandatoryPhoneItemStrips;

		static CodeDescriptionPair[] MandatoryPhoneItemDescriptions
		{
			get
			{
				return new[]
				{
					new CodeDescriptionPair(PhoneContactItemDescriptionList.Codes.Work, PhoneContactItemDescriptionList.Descriptions.Work),
					new CodeDescriptionPair(PhoneContactItemDescriptionList.Codes.Mobile, PhoneContactItemDescriptionList.Descriptions.Mobile),
					new CodeDescriptionPair(PhoneContactItemDescriptionList.Codes.Home, PhoneContactItemDescriptionList.Descriptions.Home)
				};
			}
		}

		#endregion

		#region Event Handlers

		void HookContactItemHandlers(OrgContact contact)
		{
			contact.EmailContactItems.CountChanged += EmailContactItems_CountChanged;
			contact.PhoneContactItems.CountChanged += PhoneContactItems_CountChanged;
			contact.PhoneContactItems.HasChangesChanged += PhoneContactItems_ValueChanged;
			contact.IsNDRInfo.ValueChanged += IsNDR_Changed;
		}

		void UnhookContactItemHandlers(OrgContact contact)
		{
			contact.EmailContactItems.CountChanged -= EmailContactItems_CountChanged;
			contact.PhoneContactItems.CountChanged -= PhoneContactItems_CountChanged;
			contact.PhoneContactItems.HasChangesChanged -= PhoneContactItems_ValueChanged;
			contact.IsNDRInfo.ValueChanged -= IsNDR_Changed;
		}

		#endregion

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (warningImageRest != null)
			{
				warningImageRest.Dispose();
				warningImageRest = null;
			}

			if (warningImageHot != null)
			{
				warningImageHot.Dispose();
				warningImageHot = null;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
