using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.ProcessManagement.DataTransfer
{
	static class ActivityOrganizationHelper
	{
		internal static OrganizationAddress GetAddress(OrgHeader organization, IDataWritingManager writeManager, ActivityOrganizationAddressType addressType, OrgContact contact = null)
		{
			var address = organization?.MainAddress;

			return GetAddress(address, writeManager, addressType, contact);
		}

		internal static OrganizationAddress GetAddress(OrgAddress address, IDataWritingManager writeManager, ActivityOrganizationAddressType addressType, OrgContact contact = null)
		{
			var addressTypeString = addressType == ActivityOrganizationAddressType.None ? string.Empty : addressType.ToString();
			return OrganizationAddressHelper.GetAddressDataObject(address, writeManager, addressTypeString, contact);
		}
	}
}
