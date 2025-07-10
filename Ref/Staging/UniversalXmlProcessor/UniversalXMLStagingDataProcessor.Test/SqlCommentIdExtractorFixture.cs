using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class SqlCommentIdExtractorFixture
	{
		[Test]
		public void TestSqlCommentIdExtractor()
		{
			const string commandText = "-- Measure Performance metrics : testMethod --";
			var extractId = SqlCommentIdExtractor.ExtractId(commandText);
			Assert.That(extractId, Is.EqualTo("testMethod"));
		}

		[Test]
		public void TestSqlCommentIdExtractor_InvalidCommand()
		{
			const string commandText = "-- Measure Performance metrics : --";
			Assert.Throws<InvalidOperationException>(() => SqlCommentIdExtractor.ExtractId(commandText));
		}

		[Test]
		public void TestSqlCommentIdExtractor_EmptyCommand()
		{
			Assert.Throws<ArgumentNullException>(() => SqlCommentIdExtractor.ExtractId(string.Empty));
		}
	}
}
