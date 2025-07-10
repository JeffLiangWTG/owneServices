//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgDocumentCopyRecipientLookups
//
//    This class should be used for overriding collections in AutoOrgDocumentCopyRecipientLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDocumentCopyRecipientLookups : AutoOrgDocumentCopyRecipientLookups
	{
		public OrgDocumentCopyRecipientLookups(AutoOrgDocumentCopyRecipient parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ODR_AvailableEmailAddress_List
		{
			get
			{
				var result = new CodeDescriptionPairList();
				OrgDocumentCopyRecipient copyRecipient = (OrgDocumentCopyRecipient)Parent;
				if (copyRecipient != null)
				{
					var organization = copyRecipient.Organization;
					if (organization != null)
					{
						foreach (var contact in organization.ContactsActive.Cast<OrgContact>())
						{
							result.Add(new CodeDescriptionPair(contact.Email.ToString(), contact.OC_ContactName.ToString()));
						}
					}
				}
				return result;
			}
		}
	}
}