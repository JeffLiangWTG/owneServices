using System;
using CsvHelper.Configuration;
using FsisEstNumbersCrawler.Models;
using FsisEstNumbersCrawler.Services;
using NUnit.Framework;

namespace FsisEstNumbersCrawler.Test.Services
{
	[TestFixture]
	public class CsvParserTest
	{
		static readonly ICsvParser Parser = new CsvParser();

		[Test]
		public void ParseNoExceptionThrow()
		{
			var filePath = Utilities.CurrentFolder() + "\\Resources\\MPI_Directory_by_Establishment_Name.csv";
			var establishments = Parser.Parse<Establishment>(filePath);
			Assert.AreEqual(6991, establishments.Count);
		}

		[Test]
		public void Parse23072024_ByNumber()
		{
			var filePath = Utilities.CurrentFolder() + "\\Resources\\MPI_2307_byNumber.csv";
			var establishments = Parser.Parse<Establishment>(filePath);
			Assert.AreEqual(7049, establishments.Count);
		}

		[Test]
		public void ParseShouldWork()
		{
			var config = new Configuration
			{
				Delimiter = ",",
				HasHeaderRecord = true,
			};
			var filePath = Utilities.CurrentFolder() + "\\Resources\\articles.csv";
			var articles = Parser.Parse<Article>(filePath, config);

			Assert.AreEqual(4, articles.Count);

			Assert.AreEqual(new Article
			{
				Id = 1,
				Author = "Tom",
				Title = "First Article",
				Content = "This is the first article.",
				Time = new DateTime(2001, 1, 1),
				UnformattedTime = new DateTime(2002, 1, 1)
			}, articles[0]);

			Assert.AreEqual(new Article
			{
				Id = 2,
				Author = "Jerry",
				Title = "Second Article",
				Content = "This is the second article.",
				Time = new DateTime(2001, 1, 2),
				UnformattedTime = new DateTime(2002, 1, 2)
			}, articles[1]);

			Assert.AreEqual(new Article
			{
				Id = 3,
				Author = "Mickey",
				Title = "Third Article",
				Content = "This is the third article.",
				Time = new DateTime(2001, 1, 3),
				UnformattedTime = new DateTime(2002, 1, 3)
			}, articles[2]);
		}
	}
}
