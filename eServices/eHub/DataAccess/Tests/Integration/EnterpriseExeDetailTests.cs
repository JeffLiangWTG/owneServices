using System;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.Integration
{
	public class EnterpriseExeDetailTests
	{
		[Test]
		public void EnterpriseExeDetail_ConvertsFromSharedTypes()
		{
			Assert.Multiple(() =>
			{
				Assert.DoesNotThrow(() =>
					_ = (CargoWise.eHub.DataAccess.Integration.EnterpriseExeDetail)
						new eServices.eHubDataAccess.Integration.EnterpriseExeDetail(
							"releaseStatus",
							DateTime.Now,
							"licenceType"
						));

				Assert.DoesNotThrow(() =>
					_ = (CargoWise.eHub.DataAccess.Integration.EnterpriseExeDetail)
						new eServices.eHubDataAccess.Integration.EnterpriseExeDetail(
							null,
							DateTime.MinValue,
							null
						));
			});
		}
	}
}
