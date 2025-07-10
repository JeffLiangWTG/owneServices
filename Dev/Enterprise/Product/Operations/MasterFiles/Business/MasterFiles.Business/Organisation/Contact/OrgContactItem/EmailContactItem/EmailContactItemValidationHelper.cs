using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class EmailContactItemValidationHelper
	{
		public EmailContactItemValidationHelper(OrgContact contact)
		{
			this.contact = contact;
		}

		public EmailContactItemValidationHelper(EmailContactItem emailItem)
		{
			this.emailItem = emailItem;
		}

		readonly OrgContact contact;
		readonly EmailContactItem emailItem;

		OrgContact Contact
		{
			get { return contact ?? emailItem.Contact; }
		}

		public void ValidateEmailAddress(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Value.IsEmpty)
			{
				EmailAddressValidation.ValidateEmailAddress(propertyInfo);
				CheckAddressIsUniqueInCollection(propertyInfo);
			}
		}

		protected void CheckAddressIsUniqueInCollection(ZPropertyInfo propertyInfo)
		{
			var contact = Contact;
			if (contact != null)
			{
				var propertyValue = (ZString)propertyInfo.Value;
				var propertyValueCount = 0;
				foreach (var emailItem in contact.EmailContactItems.Items)
				{
					if (emailItem.OI_Address.EqualsIgnoringCase(propertyValue))
					{
						propertyValueCount++;
						if (propertyValueCount > 1)
						{
							propertyInfo.AddError(PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(propertyInfo.HumanReadableName));
						}
					}
				}
			}
		}
	}
}
