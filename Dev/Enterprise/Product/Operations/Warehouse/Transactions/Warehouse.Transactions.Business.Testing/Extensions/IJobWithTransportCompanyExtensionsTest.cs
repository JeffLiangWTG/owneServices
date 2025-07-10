using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class IJobWithTransportCompanyExtensionsTest : WhsTestCaseWithFactory
	{
		#region TestGetTransportCo

		public void TestGetTransportCo()
		{
			var job = new TestJob();
			job.TransportCoDocAddress = Factory.New<JobDocAddress>();
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TRANSPORT CO";

			job.TransportCoDocAddress.OrganisationPK = org.PK;
			AssertEquals(org, job.GetTransportCo());

			job.TransportCoDocAddress.E2_AddressOverride = true;
			job.TransportCoDocAddress.E2_CompanyName = "OVERRIDEN TRANSPORT CO";
			AssertNull(job.GetTransportCo());
		}

		#endregion

		#region Implementation

		class TestJob : IJobWithTransportCompany
		{
			public JobDocAddress TransportCoDocAddress { get; set; }

			public ZString TransportCoName { get; set; }

			public ZGuid TransportCoPK { get; set; }

			public ZString TransportCoFieldType { get; set; }

			public ZString TransportCoNameOrPK { get; set; }

			public ZPropertyInfo TransportCoNameOrPKInfo { get; set; }

			public bool GetTransportCoDocAddressReadOnly() => TransportCoDocAddressReadOnly;

			public bool TransportCoDocAddressReadOnly { get; set; }
		}

		#endregion
	}
}
