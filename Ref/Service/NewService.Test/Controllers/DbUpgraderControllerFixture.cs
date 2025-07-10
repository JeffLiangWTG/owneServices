using System;
using System.Linq;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.NewService.Controllers;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test.Controllers
{
	[TestFixture]
	class DbUpgraderControllerFixture
	{
		[Test]
		public void TestGetDeltaScriptByVersion()
		{
			helper.Setup(x => x.GetDeltaScriptsAfterVersion(2)).Returns(new UpgradeWrapper[] { new UpgradeWrapper(5, "DeltaScriptVersion5", "Description of Version5") });
			var result = controller.GetDeltaScriptsAfterVersion("2").ToArray();
			Assert.AreEqual(1, result.Length);
			Assert.AreEqual(5, result.First().UpgradeVersion);
			Assert.AreEqual("DeltaScriptVersion5", result.First().UpgradeScript);
			Assert.AreEqual("Description of Version5", result.First().Description);
		}

		[TestCase("-1")]
		[TestCase("31")]
		public void TestGetDeltaScriptByVersion_Exception(string version)
		{
			Assert.That(() => controller.GetDeltaScriptsAfterVersion(version), Throws.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void TestLatestVersion()
		{
			Assert.AreEqual(30, controller.GetLatestVersion());
		}

		[Test]
		public void TestRequiredVersion()
		{
			Assert.AreEqual(10, controller.GetRequiredVersion());
		}

		Mock<IDbUpgraderHelper> helper;
		DbUpgraderController controller;

		[SetUp]
		public void SetUp()
		{
			helper = new Mock<IDbUpgraderHelper>();
			controller = new DbUpgraderController(helper.Object);
			helper.SetupGet(x => x.LatestVersion).Returns(30);
			helper.SetupGet(x => x.RequiredVersion).Returns(10);
		}
	}
}
