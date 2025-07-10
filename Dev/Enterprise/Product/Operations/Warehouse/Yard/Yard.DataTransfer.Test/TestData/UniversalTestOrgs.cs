using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	public class UniversalTestOrgs
	{
		readonly TestErrorLogger Logger;
		readonly UniversalObjectFactory Factory;
		OrgHeader org1;

		public UniversalTestOrgs(UniversalObjectFactory factory, TestErrorLogger logger)
		{
			this.Logger = logger;
			this.Factory = factory;
		}

		public OrgHeader Org1
		{
			get { return org1 ?? (org1 = CreateOrgInDB(Warehouse_WUFSHIJNB)); }
		}

		OrgHeader CreateOrgInDB(OrganizationAddress addressDataObject)
		{
			var address = new OrganisationDataObjectReader(addressDataObject, Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			return address.Header;
		}

		public OrganizationAddress Warehouse_WUFSHIJNB
		{
			get
			{
				var result = OrganizationAddressTestHelper.GetNewAddressData_WUFSHIJNB(DocAddressType.LocalCartageYard);
				result.Port = new UNLOCO() { Code = "ZAJNB" };
				result.AddressShortCode = "SC1";
				return result;
			}
		}
	}
}
