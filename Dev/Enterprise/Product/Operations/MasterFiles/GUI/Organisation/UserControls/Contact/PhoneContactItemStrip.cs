using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public partial class PhoneContactItemStrip : ContactItemStrip
	{
		public PhoneContactItemStrip()
		{
			InitializeComponent();
		}

		#region CurrentDataItem

		internal new PhoneContactItem CurrentDataItem
		{
			get { return (PhoneContactItem)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			DescriptionDropDownList.CodeBox.Validated += DescriptionDropDownList_TextChanged;
			DescriptionDropDownList.CodeBox.TextChanged += DescriptionDropDownList_TextChanged;
			RefreshContactPhoneItemDiallerVisibility();
		}

		#endregion

		#region DeleteButton

		protected override void deleteButton_Click(object sender, EventArgs e)
		{
			OrgContact contact = null;
			if (!CurrentDataItem.IsDeleted)
			{
				contact = CurrentDataItem.Contact;
			}

			if (contact != null)
			{
				contact.PhoneContactItems.RemoveAndDelete(CurrentDataItem);
			}
			else
			{
				base.deleteButton_Click(sender, e);
			}
		}

		#endregion

		#region Refresh Binding

		protected void DescriptionDropDownList_TextChanged(object sender, EventArgs e)
		{
			RefreshContactPhoneItemDiallerVisibility();

			if (CurrentDataItem != null)
			{
				if (CurrentDataItem.OI_Description == PhoneContactItemDescriptionList.Codes.Extension
					|| CurrentDataItem.OI_Description == PhoneContactItemDescriptionList.Codes.Skype
					|| CurrentDataItem.OI_Description == PhoneContactItemDescriptionList.Codes.Skype2)
				{
					PhoneNumberControl.NumberTextBoxMaxLength = CurrentDataItem.OI_Address_MaxLength;
				}
				else
				{
					PhoneNumberControl.NumberTextBoxMaxLength = 0;
				}
			}
		}

		void RefreshContactPhoneItemDiallerVisibility()
		{
			var isCallable = CurrentDataItem != null && CurrentDataItem.IsCallable;
			PhoneNumberControl.ShowDiallerControl = isCallable;
			PhoneNumberControl.ShowToolTip = isCallable;
		}

		#endregion

		#region ReadOnly

		protected override void RefreshReadOnlyControls()
		{
			base.RefreshReadOnlyControls();
			PhoneNumberControl.SetReadOnly(ReadOnly);
		}

		#endregion
	}
}
