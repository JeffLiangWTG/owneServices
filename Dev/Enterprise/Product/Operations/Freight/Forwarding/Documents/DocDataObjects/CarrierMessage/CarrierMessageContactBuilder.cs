using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This class is used in CarrierMessageContactBuilderTest, will be removed if no use in the future")]
	class CarrierMessageContactBuilder
	{
		internal Contact Build(OrgContact contactBO)
		{
			if (contactBO == null)
			{
				return null;
			}

			var contact = new Contact();
			contact.FullName = contactBO.OC_ContactName;
			contact.Email = contactBO.OC_Email;

			if (!contactBO.OC_Phone.IsEmpty)
			{
				contact.Phone = contactBO.OC_Phone;
			}
			else if (!contactBO.OC_Mobile.IsEmpty)
			{
				contact.Phone = contactBO.OC_Mobile;
			}
			else if (!contactBO.PhoneFallbackToOrganisation.IsEmpty)
			{
				contact.Phone = contactBO.PhoneFallbackToOrganisation;
			}
			else if (contactBO.ParentOrg != null && !contactBO.ParentOrg.MainAddress.OA_Mobile.IsEmpty)
			{
				contact.Phone = contactBO.ParentOrg.MainAddress.OA_Mobile;
			}
			else
			{
				contact.Phone = ZString.Empty;
			}

			return contact;
		}
	}
}
