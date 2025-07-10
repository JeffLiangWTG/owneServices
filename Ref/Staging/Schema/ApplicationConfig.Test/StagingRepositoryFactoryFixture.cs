using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.ApplicationConfig.Test
{
	[TestFixture]
	public class StagingRepositoryFactoryFixture
	{
		[Test]
		public void GetStagingRepository()
		{
			using (var repo = StagingRepositoryFactory.GetStagingRepository())
			{
				Assert.NotNull(repo);
			}
		}
	}
}
