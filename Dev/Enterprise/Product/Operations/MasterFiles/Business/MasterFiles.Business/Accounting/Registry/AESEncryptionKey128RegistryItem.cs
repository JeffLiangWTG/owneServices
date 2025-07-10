using System;
using System.Text.RegularExpressions;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class AESEncryptionKey128RegistryItem : StringRegistryItem
	{
		public AESEncryptionKey128RegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new RegistryItemImpl(name, category, caption, hint, new AESEncryptionKey128DataType(), storage, option))
		{
		}
	}

	public class AESEncryptionKey128DataType : StringRegistryDataType
	{
		public AESEncryptionKey128DataType()
			: base(32, 32)
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (registryItem is AESEncryptionKey128RegistryItem)
			{
				var regex = new Regex("[0-9a-fA-F]{32}");
				if (!regex.IsMatch(proposedValue))
				{
					throw new RegistryValidationException(Res.GetString("ED9AB760-2B60-4E89-BFD1-20A13216DC15", "AES Encryption Key should contain 32 digit hex number."));
				}
			}
		}
	}
}
