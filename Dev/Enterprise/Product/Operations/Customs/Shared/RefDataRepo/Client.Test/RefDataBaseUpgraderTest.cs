using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public class RefDataBaseUpgraderForTesting : RefDataBaseUpgrader
	{
		public RefDataBaseUpgraderForTesting(ILogger logger, IDbConnection connection, IServerProxy proxy)
			: base(logger, proxy, new DBUpgradeHelper(connection))
		{
		}
	}

	public class RefDataBaseUpgraderTest : TestCase
	{
		public void TestShouldUpgrade()
		{
			proxyMock.Setup(f => f.GetRemoteDbLatestVersion()).Returns(Task.FromResult("10"));
			AssertEquals(true, upgrader.ShouldUpgrade("1"));
			dBUpgHelper.SaveDbExtendedProperty(RefDataBaseUpgrader.RefDbVersionExtendedPropertyName, "11", null);
			AssertEquals(false, upgrader.ShouldUpgrade("11"));
		}

		public void TestDoUpgrade()
		{
			var currentVersion = dBUpgHelper.LoadDbExtendedProperty(RefDataBaseUpgrader.RefDbVersionExtendedPropertyName, null);
			AssertEquals("0", currentVersion);

			proxyMock.Setup(f => f.GetRemoteDbScriptsAfterVersion(It.IsAny<string>())).Returns(Task.FromResult(new[] { new UpgradeWrapper(20, "SELECT * FROM sys.extended_properties", "Description") }.AsEnumerable()));
			proxyMock.Setup(f => f.GetRemoteDbLatestVersion()).Returns(Task.FromResult("10"));
			upgrader.DoUpgrade(null);
			currentVersion = dBUpgHelper.LoadDbExtendedProperty(RefDataBaseUpgrader.RefDbVersionExtendedPropertyName, null);
			AssertEquals("20", currentVersion);
		}

		protected override void TearDown()
		{
			base.TearDown();

			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, TestDbName, Db.DatabaseName);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var upgradeConnection = Db.NewAdminConnection();
			upgradeConnection.CreateDatabase(TestDbName, logInitialSizeMb: RefDbLogFileIntialSizeMb, logGrowthMb: RefDbLogFileGrowthMb, physicalFileSuffix: DateTime.UtcNow.ToString("yyyyMMddhhmmss", System.Globalization.CultureInfo.InvariantCulture));

			connection = ((IDbConnectionInternals)Db.NewAdminConnection(TestDbName)).ADOConnection;
			dBUpgHelper = new DBUpgradeHelper(connection);
			dBUpgHelper.SaveDbExtendedProperty(RefDataBaseUpgrader.RefDbVersionExtendedPropertyName, "0", null);
			upgrader = new RefDataBaseUpgraderForTesting(loggerMock.Object, connection, proxyMock.Object);
		}
		readonly Mock<ILogger> loggerMock = new Mock<ILogger>();
		IDbConnection connection;
		IDBUpgradeHelper dBUpgHelper;
		RefDataBaseUpgraderForTesting upgrader;
		readonly Mock<IServerProxy> proxyMock = new Mock<IServerProxy>();
		const int RefDbLogFileIntialSizeMb = 100;
		const int RefDbLogFileGrowthMb = 100;
		const string TestDbName = "CW-RefDatabase_RefDatabaseUpgraderTestRandom";
	}
}
