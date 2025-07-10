using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class OrganizationContactDataObjectWriter : DataObjectWriter<OrgContact, OrganizationContact>
	{
		public OrganizationContactDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override OrganizationContact PopulateDataObject(OrgContact orgContactBO)
		{
			var contactData = new OrganizationContact { FullName = orgContactBO.OC_ContactName, Email = orgContactBO.OC_Email };

			if (!orgContactBO.OC_Phone.IsEmpty)
			{
				contactData.Phone = orgContactBO.OC_Phone;
			}
			else if (!orgContactBO.OC_Mobile.IsEmpty)
			{
				contactData.Phone = orgContactBO.OC_Mobile;
			}
			else if (!orgContactBO.PhoneFallbackToOrganisation.IsEmpty)
			{
				contactData.Phone = orgContactBO.PhoneFallbackToOrganisation;
			}
			else if (orgContactBO.ParentOrg != null && !orgContactBO.ParentOrg.MainAddress.OA_Mobile.IsEmpty)
			{
				contactData.Phone = orgContactBO.ParentOrg.MainAddress.OA_Mobile;
			}
			else
			{
				contactData.Phone = ZString.Empty;
			}

			return contactData;
		}
	}
}

