using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	class ContactBuilder
	{
		internal Contact Build(OrgContact contactBO)
		{
			if (contactBO == null)
			{
				return null;
			}

			var contact = new Contact();
			contact.FullName = contactBO.OC_ContactName;
			contact.Phone = contactBO.OC_Phone;
			contact.Email = contactBO.OC_Email;

			return contact;
		}
	}
}
