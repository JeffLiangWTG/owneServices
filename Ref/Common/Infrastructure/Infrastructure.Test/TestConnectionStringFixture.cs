using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[TestFixture]
	class TestConnectionStringFixture
	{
		[Test]
		public void GetAdmin()
		{
			var dbName = string.Empty;
			var connectionString = TestConnectionString.GetAdmin(dbName);

			Assert.AreEqual("Data Source=localhost;Integrated Security=True;Multiple Active Result Sets=True;Trust Server Certificate=True;Application Name=\"RefDbRepo Testing\"", connectionString);
			dbName = "Test";
			connectionString = TestConnectionString.GetAdmin(dbName);
			Assert.AreEqual("Data Source=localhost;Initial Catalog=Test;Integrated Security=True;Multiple Active Result Sets=True;Trust Server Certificate=True;Application Name=\"RefDbRepo Testing\"", connectionString);
		}

		[Test]
		public void GetWriter()
		{
			var dbName = string.Empty;
			var connectionString = TestConnectionString.GetWriter(dbName);

			Assert.AreEqual($"Data Source=localhost;User ID=refdbrepowriterfortest;Password={TestConnectionString.WriterPassword};Multiple Active Result Sets=True;Trust Server Certificate=True;Application Name=\"RefDbRepo Testing\"", connectionString);
			dbName = "Test";
			connectionString = TestConnectionString.GetWriter(dbName);
			Assert.AreEqual($"Data Source=localhost;Initial Catalog=Test;User ID=refdbrepowriterfortest;Password={TestConnectionString.WriterPassword};Multiple Active Result Sets=True;Trust Server Certificate=True;Application Name=\"RefDbRepo Testing\"", connectionString);
		}
	}
}
