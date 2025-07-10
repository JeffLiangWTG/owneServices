using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class SqlCommentAdderFixture
	{
		[Test]
		public void TestSqlCommentAdder()
		{
			var sqlCommentAdder = SqlCommentAdder.Create()
				.WithSql("SELECT * FROM test")
				.WithId("1")
				.Build();

			Assert.That(sqlCommentAdder.ToString(), Is.EqualTo(@"-- Measure Performance metrics : 1 --
SELECT * FROM test"));
		}
	}
}
