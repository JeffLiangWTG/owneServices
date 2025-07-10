using System;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class PreProcessCheckerTest
	{
		[Test]
		public void TestAnythingNeedToProcess()
		{
			var dateTime = DateTime.Now;
			var checker = new PreProcessChecker(Constants.ProgramFunctions.CATariff);
			checker.MarkAsProcessRequired();
			Assert.IsTrue(checker.UpdateLastPublishDate(dateTime));
			Assert.IsTrue(!checker.UpdateLastPublishDate(dateTime));
			Assert.IsTrue(!checker.UpdateLastPublishDate(dateTime.AddDays(-1)));
			checker.MarkAsProcessRequired();
			Assert.IsTrue(checker.UpdateLastPublishDate(dateTime.AddDays(-1)));
			Assert.IsTrue(checker.UpdateLastPublishDate(dateTime));
			Assert.IsTrue(checker.UpdateLastPublishDate(dateTime.AddDays(1)));
		}
	}
}
