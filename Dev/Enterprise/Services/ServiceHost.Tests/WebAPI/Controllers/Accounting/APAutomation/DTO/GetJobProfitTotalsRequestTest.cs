using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Services.ServiceHost.Tests
{
	class GetJobProfitTotalsRequestTest : TestCaseWithFactory
	{
		public void TestGetJobProfitTotalsRequestProperties()
		{
			var jobParentInfo = new JobParentInfo { ParentId = Guid.NewGuid(), ParentTableCode = "JS" };
			var companyPK = Guid.NewGuid();
			var request = new GetJobProfitTotalsRequest
			{
				JobParentInfo = jobParentInfo,
				CompanyPK = companyPK
			};

			AssertEquals(jobParentInfo, request.JobParentInfo);
			AssertEquals(companyPK, request.CompanyPK);
		}

		public void TestGetJobProfitTotalsRequestDefaultValues()
		{
			var request = new GetJobProfitTotalsRequest();
			AssertNull(request.JobParentInfo);
			AssertEquals(Guid.Empty, request.CompanyPK);
		}
	}
}
