using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class UniversalTestData
	{
		public UniversalTestData(UniversalObjectFactory factory)
		{
			Factory = factory;
		}

		readonly UniversalObjectFactory Factory;

		public void CreateConsigneeAddressCRAHOLSYDInDB()
		{
			consigneeOrgCRAHOLSYD = new OrganisationDataObjectReader(ConsigneeAddressCRAHOLSYDDataObject, new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;
			Factory.SaveForTesting();
		}

		public OrgHeader ConsigneeOrgCRAHOLSYD
		{
			get
			{
				if (consigneeOrgCRAHOLSYD == null)
				{
					CreateConsigneeAddressCRAHOLSYDInDB();
				}

				return consigneeOrgCRAHOLSYD;
			}
		}

		OrgHeader consigneeOrgCRAHOLSYD;

		public OrganizationAddress ConsigneeAddressCRAHOLSYDDataObject
		{
			get { return OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress); }
		}
	}
}
