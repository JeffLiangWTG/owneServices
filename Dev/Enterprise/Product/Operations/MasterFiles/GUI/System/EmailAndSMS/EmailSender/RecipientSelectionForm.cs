using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RecipientSelectionForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public RecipientSelectionForm()
		{
			InitializeComponent();
		}

		public RecipientSelectionForm(RecipientSelection recipientSelection)
			: base(recipientSelection)
		{
			InitializeComponent();

			SetupAddressBookRecipientsGridContextMenu();
		}

		ZMenuItem removeFromToAddressMenu;
		ZMenuItem RemoveFromToAddressMenu => removeFromToAddressMenu ?? (removeFromToAddressMenu = new ZMenuItem(ResString.GetMultilingualString("007B361A-7514-470D-B914-ED62CC34788B", "Remove from To Address"), RemoveFromToAddress_Click));

		ZMenuItem removeFromCcAddressMenu;
		ZMenuItem RemoveFromCcAddressMenu => removeFromCcAddressMenu ?? (removeFromCcAddressMenu = new ZMenuItem(ResString.GetMultilingualString("6C9B237E-A24D-45B6-87CC-B8CE40B5C3DD", "Remove from Cc Address"), RemoveFromCcAddress_Click));

		ZMenuItem removeFromBccAddressMenu;
		ZMenuItem RemoveFromBccAddressMenu => removeFromBccAddressMenu ?? (removeFromBccAddressMenu = new ZMenuItem(ResString.GetMultilingualString("A4EB2474-9519-4AE9-8976-00C21DB153BB", "Remove from Bcc Address"), RemoveFromBccAddress_Click));

		void SetupAddressBookRecipientsGridContextMenu()
		{
			AddressBookRecipientsGrid.ContextMenu.Popup -= AddressBookRecipientsGridContextMenu_Popup;
			AddressBookRecipientsGrid.ContextMenu.Popup += AddressBookRecipientsGridContextMenu_Popup;

			AddressBookRecipientsGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			AddressBookRecipientsGrid.ContextMenu.MenuItems.Add(RemoveFromToAddressMenu);
			AddressBookRecipientsGrid.ContextMenu.MenuItems.Add(RemoveFromCcAddressMenu);
			AddressBookRecipientsGrid.ContextMenu.MenuItems.Add(RemoveFromBccAddressMenu);
			AddressBookRecipientsGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
		}

		void AddressBookRecipientsGridContextMenu_Popup(object sender, EventArgs e)
		{
			var selectedRecipients = AddressBookRecipientsGrid.SelectedElements;
			if (selectedRecipients.Length > 0)
			{
				var emails = selectedRecipients.Cast<AddressBookRecipient>().Select(r => r.Email).Where(r => !r.IsEmpty).ToArray();
				RemoveFromToAddressMenu.Enabled = emails.Any(RecipientSelection.ToEmailAddress.Contains);
				RemoveFromCcAddressMenu.Enabled = emails.Any(RecipientSelection.Cc.Contains);
				RemoveFromBccAddressMenu.Enabled = emails.Any(RecipientSelection.Bcc.Contains);
			}
			else
			{
				RemoveFromToAddressMenu.Enabled = false;
				RemoveFromCcAddressMenu.Enabled = false;
				RemoveFromBccAddressMenu.Enabled = false;
			}
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		RecipientSelection RecipientSelection
		{
			get { return (RecipientSelection)base.BusinessEntity; }
		}

		void NewCancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void GoButton_Click(object sender, EventArgs e)
		{
			RecipientSelection.Search();
		}

		void AddressBookRecipientsGrid_DoubleClick(object sender, EventArgs e)
		{
			ToButton_Click(sender, e);
		}

		void ToButton_Click(object sender, EventArgs e)
		{
			AppendSelectedRecipients(RecipientSelection.AppendRecipientsEmailToEmailAddress);
		}

		void CcButton_Click(object sender, EventArgs e)
		{
			AppendSelectedRecipients(RecipientSelection.AppendRecipientsEmailToCc);
		}

		void BccButton_Click(object sender, EventArgs e)
		{
			AppendSelectedRecipients(RecipientSelection.AppendRecipientsEmailToBcc);
		}

		void AppendSelectedRecipients(Action<AddressBookRecipient[]> appendMethod)
		{
			if (AddressBookRecipientsGrid.SelectedElements.Length > 0)
			{
				appendMethod(AddressBookRecipientsGrid.SelectedElements.Cast<AddressBookRecipient>().ToArray());
			}
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		void AdvancedSearchLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			OpenAdvancedSearchForm();
		}

		void OpenAdvancedSearchForm()
		{
			RecipientSelection.RetainPreviousAdvancedSearchQueries();
			var form = new RecipientSelectionAdvancedSearchForm(RecipientSelection);
			ZFormModaliser.Show(form, ParentForm);
			form.FormClosed += delegate
			{
				if (form.DialogResult == DialogResult.OK)
				{
					RecipientSelection.AdvancedSearch();
				}
				else
				{
					RecipientSelection.RestorePreviousAdvancedSearchQueriesIfCancelled();
				}
			};
		}

		void RemoveFromToAddress_Click(object sender, EventArgs e)
		{
			if (AddressBookRecipientsGrid.SelectedElements.Length > 0)
			{
				RecipientSelection.RemoveRecipientsEmailFromEmailAddress(AddressBookRecipientsGrid.SelectedElements.Cast<AddressBookRecipient>().ToArray());
			}
		}

		void RemoveFromCcAddress_Click(object sender, EventArgs e)
		{
			if (AddressBookRecipientsGrid.SelectedElements.Length > 0)
			{
				RecipientSelection.RemoveRecipientsEmailFromCc(AddressBookRecipientsGrid.SelectedElements.Cast<AddressBookRecipient>().ToArray());
			}
		}

		void RemoveFromBccAddress_Click(object sender, EventArgs e)
		{
			if (AddressBookRecipientsGrid.SelectedElements.Length > 0)
			{
				RecipientSelection.RemoveRecipientsEmailFromBcc(AddressBookRecipientsGrid.SelectedElements.Cast<AddressBookRecipient>().ToArray());
			}
		}
	}
}
