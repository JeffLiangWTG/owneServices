using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	internal class RequestGenerationProviderHelper
	{
		public static (ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfo(JobDocAddress docAddress)
		{
			if (docAddress == null || docAddress.E2_AddressOverride || docAddress.Address == null)
			{
				return (ZGuid.Empty, ZGuid.Empty);
			}

			return (docAddress.OrganisationPK, string.IsNullOrEmpty(docAddress.E2_Contact) ? ZGuid.Empty : docAddress.ContactPK);
		}

		public static (ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfo(BusinessObjectFactory factory, ZGuid orgPK, ZString contactName)
		{
			if (orgPK.IsEmpty)
			{
				return (ZGuid.Empty, ZGuid.Empty);
			}

			return (orgPK, factory.LoadTop1<OrgContact>(new ZQuery(OrgContactSchema.OC_OH, orgPK).AddToFilter(OrgContactSchema.OC_ContactName, contactName))?.PK ?? ZGuid.Empty);
		}

		public static (ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfo(BusinessObjectFactory factory, string script, string orgColumnName, string contactNameOrPKColumnName, bool contactName = true)
		{
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(script);

			if (collection.Count != 1)
			{
				return (ZGuid.Empty, ZGuid.Empty);
			}

			if (contactName)
			{
				return RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(factory, (ZGuid)collection[0][orgColumnName], (ZString)collection[0][contactNameOrPKColumnName]);
			}
			else
			{
				return ((ZGuid)collection[0][orgColumnName], (ZGuid)collection[0][contactNameOrPKColumnName]);
			}
		}
	}
}
