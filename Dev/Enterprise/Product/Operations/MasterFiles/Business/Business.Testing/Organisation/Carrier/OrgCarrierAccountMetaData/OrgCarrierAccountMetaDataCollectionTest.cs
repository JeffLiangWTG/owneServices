using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCarrierAccountMetaDataCollection))]
	sealed class OrgCarrierAccountMetaDataCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgCarrierAccountMetaDataCollection>
	{
		public void TestAllowNew()
		{
			var result = new OrgCarrierAccountMetaDataCollection(Factory);
			result.AddNew();
			AssertEquals(1, result.Count);
		}

		public void TestOrgCarrierAccountMetaDataCollection()
		{
			var carrierAccount = Factory.NewWithValidTestData<OrgCarrierAccount>();

			var metaData1 = Factory.New<OrgCarrierAccountMetaData>();
			var metaData2 = Factory.New<OrgCarrierAccountMetaData>();

			metaData1.OAM_OAN_CarrierAccount = carrierAccount.PK;
			metaData2.OAM_OAN_CarrierAccount = carrierAccount.PK;

			var carrierAccounts = new OrgCarrierAccountMetaDataCollection(carrierAccount);
			AssertEquals(2, carrierAccounts.Count);
		}
	}
}
