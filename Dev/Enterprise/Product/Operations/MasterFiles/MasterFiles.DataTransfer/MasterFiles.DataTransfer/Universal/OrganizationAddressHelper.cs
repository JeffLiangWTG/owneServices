using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer
{
	public static class OrganizationAddressHelper
	{
		public static OrganizationAddress GetAddressDataObject(OrgAddress address, IDataWritingManager writeManager, ZString addressType, OrgContact contact = null, bool populateGeolocation = false, bool populateValidationStatus = false)
		{
			return new OrganizationDataObjectWriter(writeManager, addressType, contact)
			{
				PopulateGeoLocation = populateGeolocation,
				PopulateValidationStatus = populateValidationStatus
			}.GetDataObject(address);
		}
	}
}
