using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.Integration
{
	public class PasswordDetailTests
	{
		[Test]
		public void PasswordDetail_ConvertsFromSharedTypes()
		{
			Assert.Multiple(() =>
			{
				Assert.DoesNotThrow(() =>
					_ = (CargoWise.eHub.DataAccess.Integration.PasswordDetail)
						new eServices.eHubDataAccess.Integration.PasswordDetail(
							"password",
							"email"
						));

				Assert.DoesNotThrow(() =>
					_ = (CargoWise.eHub.DataAccess.Integration.PasswordDetail)
						new eServices.eHubDataAccess.Integration.PasswordDetail(
							null,
							null
						));
			});
		}
	}
}
