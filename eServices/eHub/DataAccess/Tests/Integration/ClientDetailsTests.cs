using System;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.Integration
{
	public class ClientDetailsTests
	{
		[Test]
		public void ClientDetails_ConvertsToAndFromSharedTypes()
		{
			Assert.Multiple(() =>
			{
				Assert.DoesNotThrow(() =>
					_ = (CargoWise.eHub.DataAccess.Integration.ClientDetails)
						new eServices.eHubDataAccess.Integration.ClientDetails(
							"fullName",
							Guid.NewGuid(),
							"email"
						));

				Assert.DoesNotThrow(() =>
					_ = (CargoWise.eHub.DataAccess.Integration.ClientDetails)
						new eServices.eHubDataAccess.Integration.ClientDetails(
							null,
							Guid.Empty,
							null
						));

				Assert.DoesNotThrow(() =>
					_ = (eServices.eHubDataAccess.Integration.ClientDetails)
						new CargoWise.eHub.DataAccess.Integration.ClientDetails(
							"fullName",
							Guid.NewGuid(),
							"email"
						));

				Assert.DoesNotThrow(() =>
					_ = (eServices.eHubDataAccess.Integration.ClientDetails)
						new CargoWise.eHub.DataAccess.Integration.ClientDetails(
							null,
							Guid.Empty,
							null
						));
			});
		}
	}
}
