using System.Linq;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.RemoteDbManager;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class DbUpgraderHelperFixture
	{
		[Test]
		public void TestGetDeltaScriptByVersion()
		{
			provider.Setup(x => x.GetAvailableVersionsAfterVersion(It.IsAny<int>())).Returns(new int[] { 5 });
			provider.Setup(x => x.GetUpgradeWrapperByVersion(5)).Returns(new UpgradeWrapper(5, "DeltaScriptVersion5", "Description of Version5"));
			var result = helper.GetDeltaScriptsAfterVersion(2).ToArray();
			Assert.AreEqual(1, result.Length);
			Assert.AreEqual(5, result.First().UpgradeVersion);
			Assert.AreEqual("DeltaScriptVersion5", result.First().UpgradeScript);
			Assert.AreEqual("Description of Version5", result.First().Description);
		}

		[Test]
		public void TestLatestVersion()
		{
			provider.SetupGet(x => x.LatestVersion).Returns(100);
			Assert.AreEqual(100, helper.LatestVersion);
		}

		Mock<IUpgradeScriptProvider> provider;
		DbUpgraderHelper helper;

		[SetUp]
		public void SetUp()
		{
			provider = new Mock<IUpgradeScriptProvider>();
			helper = new DbUpgraderHelper(provider.Object);
		}
	}
}
