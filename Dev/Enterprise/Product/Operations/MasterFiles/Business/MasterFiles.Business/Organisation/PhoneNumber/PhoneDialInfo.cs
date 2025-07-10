using System;

namespace Enterprise.MasterFiles.Business
{
	public class PhoneDialInfo
	{
		public PhoneDialInfo(PhoneContactItem phoneItem)
		{
			ContactItem = phoneItem;
			Number = phoneItem.OI_Address;
			Description = phoneItem.DisplayDescription;
		}

		public PhoneDialInfo(PhoneContactItem phoneItem, PhoneContactItem phoneItemExtension)
		{
			ContactItem = phoneItem;
			Number = phoneItem.OI_Address;
			Description = phoneItem.DisplayDescription;
			Extension = ResString.GetMultilingualString("a0fa41d6-c724-4aa3-827d-73bee2989ce1", "{0} {1}", phoneItemExtension.DisplayDescription, phoneItemExtension.Number);
		}

		public PhoneDialInfo(string number, string description)
		{
			ContactItem = null;
			Description = description;
			Number = number;
		}

		public readonly PhoneContactItem ContactItem;
		public readonly string Number;
		public readonly string Description;
		public readonly string Extension;
	}

	public class PhoneDiallingEventArgs : EventArgs
	{
		public PhoneDiallingEventArgs(PhoneDialInfo dialInfo)
		{
			DialInfo = dialInfo;
			CreateRelatedCommunication = false;
		}

		public readonly PhoneDialInfo DialInfo;
		public bool CreateRelatedCommunication;
	}
}
