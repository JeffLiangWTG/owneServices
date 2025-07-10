using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class DbConnectionStringManagerFixture
	{
		[Test]
		public void GetConnectionString()
		{
			Assert.That(DbConnectionStringManager.SafeConnectionString, Is.Not.Null.Or.Empty);
			Assert.That(DbConnectionStringManager.StagingConnectionString, Is.Not.Null.Or.Empty);
			Assert.That(DbConnectionStringManager.SafeReadOnlyConnectionString, Is.Not.Null.Or.Empty);
		}
	}
}
