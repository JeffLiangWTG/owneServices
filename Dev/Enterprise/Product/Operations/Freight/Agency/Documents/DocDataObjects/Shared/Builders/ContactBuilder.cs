using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class ContactBuilder
	{
		public Contact Build(OrgContact contactBO)
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
