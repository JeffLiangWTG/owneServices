using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RecipientSelectionForm))]
	sealed class RecipientSelectionFormCase : ZFormBasherTest
	{
		public void TestAddressBookRecipientsGridContextMenu()
		{
			var addressBook = new AddressBookSelection();
			var recipient1 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), null, "", "", "", "", "", "1@email.com");
			var recipient2 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), null, "", "", "", "", "", "2@email.com");
			var recipient3 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), null, "", "", "", "", "", "3@email.com");
			addressBook.AddRecipient(recipient1);
			addressBook.AddRecipient(recipient2);
			addressBook.AddRecipient(recipient3);

			var recipientSelection = new RecipientSelection(addressBook);
			using (var form = new RecipientSelectionFormForTest(recipientSelection))
			{
				form.Show();
				Application.DoEvents();

				var removeFromToAddressMenu = form.AddressBookRecipientsGrid_Exposed.ContextMenu.MenuItems.FindByText("Remove from To Address");
				var removeFromCcAddressMenu = form.AddressBookRecipientsGrid_Exposed.ContextMenu.MenuItems.FindByText("Remove from Cc Address");
				var removeFromBccAddressMenu = form.AddressBookRecipientsGrid_Exposed.ContextMenu.MenuItems.FindByText("Remove from Bcc Address");

				form.AddressBookRecipientsGrid_Exposed.ContextMenu.DoPopup();
				AssertEquals("Remove from To Address should be disabled", false, removeFromToAddressMenu.Enabled);
				AssertEquals("Remove from Cc Address should be disabled", false, removeFromCcAddressMenu.Enabled);
				AssertEquals("Remove from Bcc Address should be disabled", false, removeFromBccAddressMenu.Enabled);

				recipientSelection.ToEmailAddress = "1@email.com";
				recipientSelection.Cc = "2@email.com";
				recipientSelection.Bcc = "3@email.com";

				form.AddressBookRecipientsGrid_Exposed.Select(0);
				form.AddressBookRecipientsGrid_Exposed.Select(1);
				form.AddressBookRecipientsGrid_Exposed.Select(2);
				form.AddressBookRecipientsGrid_Exposed.ContextMenu.DoPopup();

				AssertEquals("Remove from To Address should be enabled", true, removeFromToAddressMenu.Enabled);
				AssertEquals("Remove from Cc Address should be enabled", true, removeFromCcAddressMenu.Enabled);
				AssertEquals("Remove from Bcc Address should be enabled", true, removeFromBccAddressMenu.Enabled);

				removeFromToAddressMenu.PerformClick();
				AssertEquals("", recipientSelection.ToEmailAddress);
				AssertEquals("2@email.com", recipientSelection.Cc);
				AssertEquals("3@email.com", recipientSelection.Bcc);

				removeFromCcAddressMenu.PerformClick();
				AssertEquals("", recipientSelection.ToEmailAddress);
				AssertEquals("", recipientSelection.Cc);
				AssertEquals("3@email.com", recipientSelection.Bcc);

				removeFromBccAddressMenu.PerformClick();
				AssertEquals("", recipientSelection.ToEmailAddress);
				AssertEquals("", recipientSelection.Cc);
				AssertEquals("", recipientSelection.Bcc);
			}
		}

		public void TestEmailAddressTextBoxShouldBeManuallyEntered()
		{
			using (var form = new RecipientSelectionFormForTest(new RecipientSelection(new AddressBookSelection())))
			{
				AssertEquals(true, form.ToTextBox_Exposed.Enabled);
				AssertEquals(true, form.CcTextBox_Exposed.Enabled);
				AssertEquals(true, form.BccTextBox_Exposed.Enabled);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new RecipientSelectionForm(new RecipientSelection(new AddressBookSelection()));
		}

		#endregion

		class RecipientSelectionFormForTest : RecipientSelectionForm
		{
			public RecipientSelectionFormForTest(RecipientSelection recipientSelection)
				: base(recipientSelection)
			{
			}

			public ZGrid AddressBookRecipientsGrid_Exposed => AddressBookRecipientsGrid;
			public ZTextBox ToTextBox_Exposed => ToTextBox;
			public ZTextBox CcTextBox_Exposed => CcTextBox;
			public ZTextBox BccTextBox_Exposed => BccTextBox;
		}
	}
}
