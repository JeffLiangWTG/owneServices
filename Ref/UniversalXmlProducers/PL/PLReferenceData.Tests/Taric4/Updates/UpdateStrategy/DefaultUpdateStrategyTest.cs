using System;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates.UpdateStrategy;
using CargoWise.RefDbRepo.PLReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4.Updates.UpdateStrategy;

[TestFixture]
sealed class DefaultUpdateStrategyTest
{
	readonly DateTime testDate = new(2023, 10, 1, 0, 0, 0);

	[Test]
	public void TestGetDate() => Assert.That(testUpdateStrategy.FromDate, Is.EqualTo(testDate), "From date.");

	[Test]
	public void TestGetUpdateStrategy() => Assert.That(testUpdateStrategy.ToDate, Is.EqualTo(DateTime.Today), "To date.");

	[Test]
	public void TestShouldWaitForResponses() => Assert.That(testUpdateStrategy.ShouldWaitForResponses, Is.True, "Should wait for responses.");

	[Test]
	public void TestSaveRequestsFilePath()
	{
		const string testPath = "testPLpath";
		const string testFileName = "testFileName";
		settingIndexerMock.SetupGet(x => x["DownloadsPLPath"]).Returns(testPath);
		settingIndexerMock.SetupGet(x => x["UpdateRequestsFileName"]).Returns(testFileName);
		var expectedPath = System.IO.Path.Combine(testPath, testFileName);
		Assert.That(testUpdateStrategy.SaveRequestsFilePath, Is.EqualTo(expectedPath), "Save requests file path.");
	}


	[SetUp]
	public void SetUp()
	{
		settingIndexerMock = new Mock<ISettingsIndexer>();
		testUpdateStrategy = new DefaultUpdateStrategy(testDate, settingIndexerMock.Object);
	}

	Mock<ISettingsIndexer> settingIndexerMock;
	DefaultUpdateStrategy testUpdateStrategy;
}
