using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EmailContactItemStrip : ContactItemStrip
	{
		public EmailContactItemStrip()
		{
			InitializeComponent();
		}

		#region CurrentDataItem

		internal new EmailContactItem CurrentDataItem
		{
			get { return (EmailContactItem)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem == null)
			{
				AddressTextBox.ResetText();
			}
		}

		#endregion

		#region DeleteButton

		protected override void deleteButton_Click(object sender, EventArgs e)
		{
			var contact = CurrentDataItem.Contact;
			if (contact != null)
			{
				contact.EmailContactItems.RemoveAndDelete(CurrentDataItem);
			}
			else
			{
				base.deleteButton_Click(sender, e);
			}
		}

		#endregion

		#region ReadOnly

		protected override void RefreshReadOnlyControls()
		{
			base.RefreshReadOnlyControls();
			AddressTextBox.ReadOnly = ReadOnly;
			AddressTextBox.UpdateHotkeysForBindingChanged();
		}

		#endregion
	}
}
