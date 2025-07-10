using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocAddressZone))]
	sealed class JobDocAddressZoneTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var jobDocAddressZone = Factory.New<JobDocAddressZone>();
			jobDocAddressZone.E2Z_ZoneIdentifierType = "CRZ";
			jobDocAddressZone.E2Z_E2_Address = jobDocAddress.PK;
			jobDocAddressZone.E2Z_ZoneName = "TESTZONE";
			return jobDocAddressZone;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	}
}
