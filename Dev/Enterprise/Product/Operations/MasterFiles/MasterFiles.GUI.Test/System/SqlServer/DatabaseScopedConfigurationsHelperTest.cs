using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	sealed partial class DatabaseScopedConfigurationsHelperTest : TestCase
	{
		[DatCapabilityRequirementLatestAvailableSqlServer]
		public void TestLoadConfigurations()
		{
			// Arrange
			const string MaxDopName = "MAXDOP";
			const string ElevateOnlineName = "ELEVATE_ONLINE";
			const string AsyncStatsUpdateWaitAtLowPriorityName = "ASYNC_STATS_UPDATE_WAIT_AT_LOW_PRIORITY";
			DatabaseScopedConfigurationExtensions.WriteConfiguration(adminConnection, MaxDopName, "10");
			DatabaseScopedConfigurationExtensions.WriteConfiguration(adminConnection, ElevateOnlineName, "WHEN_SUPPORTED");
			DatabaseScopedConfigurationExtensions.WriteConfiguration(adminConnection, AsyncStatsUpdateWaitAtLowPriorityName, "ON");

			// Act
			var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);

			// Assert
			AssertEquals(true, configurations.Select(x => x.ConfigurationId).All(x => x > 0));
			AssertContainsExactElementsInAnyOrder(
				configurations.Select(x => x.ConfigurationId).ToHashSet(),
				configurations.Select(x => x.ConfigurationId));
			AssertEquals(true, configurations.Select(x => x.Name).All(x => !x.IsEmpty));
			AssertContainsExactElementsInAnyOrder(
				configurations.Select(x => x.Name).ToHashSet(),
				configurations.Select(x => x.Name));
			AssertEquals(true, configurations.Select(x => x.CurrentValue).All(x => x != null));
			AssertEquals(true, configurations.Select(x => x.CurrentValueText).All(x => !x.IsEmpty));
			AssertEquals(true, configurations.Select(x => x.CurrentValueForSecondary).All(x => x == null));
			AssertEquals(true, configurations.Select(x => x.CurrentValueForSecondaryText).All(x => x.IsEmpty));

			var namesHavingRecommendedValue = PresetDatabaseScopedConfigurationTest.RecommendedItemSet.Select(x => x.Name).ToHashSet();
			AssertContainsExactElementsInAnyOrder(
				configurations.Where(x  => !namesHavingRecommendedValue.Contains(x.Name))
					.Select(x => x.RecommendedValue).ToHashSet(),
				new[] { ZString.Empty });

			var maxDop = configurations.Cast<DatabaseScopedConfiguration>()
				.Single(x => x.Name == MaxDopName);
			AssertEquals(10, maxDop.CurrentValue);
			AssertEquals("10", maxDop.CurrentValueText);
			AssertEquals(false, maxDop.IsValueDefault);

			var elevateOnline = configurations.Cast<DatabaseScopedConfiguration>()
				.Single(x => x.Name == ElevateOnlineName);
			AssertEquals("WHEN_SUPPORTED", elevateOnline.CurrentValue);
			AssertEquals("WHEN_SUPPORTED", elevateOnline.CurrentValueText);
			AssertEquals(false, elevateOnline.IsValueDefault);

			var asyncStatsUpdateWaitAtLowPriority = configurations.Cast<DatabaseScopedConfiguration>()
				.Single(x => x.Name == AsyncStatsUpdateWaitAtLowPriorityName);
			AssertEquals(true, asyncStatsUpdateWaitAtLowPriority.CurrentValue);
			AssertEquals("ON", asyncStatsUpdateWaitAtLowPriority.CurrentValueText);
			AssertEquals("ON", asyncStatsUpdateWaitAtLowPriority.RecommendedValue);
			AssertEquals(false, asyncStatsUpdateWaitAtLowPriority.IsValueDefault);
		}

		public void TestLoadConfigurations_DbIsNotReadOnly()
		{
			// Arrange
			// Act
			var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);

			// Assert
			AssertContainsExactElementsInExactOrder(
				"None is read only",
				Array.Empty<DatabaseScopedConfiguration>(),
				configurations.Where(x => x.IsDbReadOnly).ToArray());
		}

		public void TestLoadConfigurations_DbIsReadOnly()
		{
			// Arrange
			adminConnection.AlterDbWriteableState(adminConnection.CurrentDatabase, false);

			// Act
			var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);

			// Assert
			AssertContainsExactElementsInExactOrder(
				"None is writable",
				Array.Empty<DatabaseScopedConfiguration>(),
				configurations.Where(x => !x.IsDbReadOnly).ToArray());
		}

		public void TestReload()
		{
			// Arrange
			const string MaxDop = "MAXDOP";
			const string ElevateOnline = "ELEVATE_ONLINE";

			var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
			AssertNotEquals(10, DatabaseScopedConfigurationExtensions.FindByName(configurations, MaxDop).CurrentValue);
			AssertNotEquals("WHEN_SUPPORTED", DatabaseScopedConfigurationExtensions.FindByName(configurations, ElevateOnline).CurrentValue);

			DatabaseScopedConfigurationExtensions.WriteConfiguration(adminConnection, MaxDop, "10"); // int
			DatabaseScopedConfigurationExtensions.WriteConfiguration(adminConnection, ElevateOnline, "WHEN_SUPPORTED"); // string

			var configurationsToReload = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);

			// Act
			DatabaseScopedConfigurationsHelper.Reload(adminConnection, configurationsToReload);

			// Assert
			AssertContainsExactElementsInAnyOrder(
				new DatabaseScopedConfigurationComparer(),
				configurations.Where(x => x.Name != MaxDop && x.Name != ElevateOnline),
				configurationsToReload.Where(x => x.Name != MaxDop && x.Name != ElevateOnline));

			var maxDop = DatabaseScopedConfigurationExtensions.FindByName(configurationsToReload, MaxDop);
			AssertEquals(10, maxDop.CurrentValue);
			AssertEquals(false, maxDop.IsValueDefault);

			var elevateOnline = DatabaseScopedConfigurationExtensions.FindByName(configurationsToReload, ElevateOnline);
			AssertEquals("WHEN_SUPPORTED", elevateOnline.CurrentValue);
			AssertEquals(false, elevateOnline.IsValueDefault);
		}

		public void TestReload_DbIsNotReadOnly()
		{
			// Arrange
			var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);

			// Act
			DatabaseScopedConfigurationsHelper.Reload(adminConnection, configurations);

			// Assert
			AssertContainsExactElementsInExactOrder(
				"None is read only",
				Array.Empty<DatabaseScopedConfiguration>(),
				configurations.Where(x => x.IsDbReadOnly).ToArray());
		}

		public void TestReload_DbIsReadOnly()
		{
			// Arrange
			var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
			adminConnection.AlterDbWriteableState(adminConnection.CurrentDatabase, false);

			// Act
			DatabaseScopedConfigurationsHelper.Reload(adminConnection, configurations);

			// Assert
			AssertContainsExactElementsInExactOrder(
				"None is writable",
				Array.Empty<DatabaseScopedConfiguration>(),
				configurations.Where(x => !x.IsDbReadOnly).ToArray());
		}

		public void TestReload_NoChange()
		{
			// Arrange
			var configurations = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);
			var configurationsToReload = DatabaseScopedConfigurationsHelper.LoadConfigurations(adminConnection);

			// Act
			DatabaseScopedConfigurationsHelper.Reload(adminConnection, configurationsToReload);

			// Assert
			AssertContainsExactElementsInAnyOrder(new DatabaseScopedConfigurationComparer(), configurations, configurationsToReload);
		}

		AdminConnection adminConnection;
		const string NewDbName = nameof(DatabaseScopedConfigurationsHelperTest);
		protected override void SetUp()
		{
			base.SetUp();

			adminConnection = Db.NewAdminConnection(Db.SqlMasterDb);
			AdoTestUtils.CreateDbDropExisting(adminConnection, NewDbName);
			((ICurrentDbControl)adminConnection).UseDatabase(NewDbName);
		}

		protected override void TearDown()
		{
			((ICurrentDbControl)adminConnection).UseDatabase(Db.SqlMasterDb);
			AdoTestUtils.DropDbIfExists(adminConnection, NewDbName);
			adminConnection.Dispose();
			base.TearDown();
		}

		sealed class DatabaseScopedConfigurationComparer : IEqualityComparer<DatabaseScopedConfiguration>
		{
			public bool Equals(DatabaseScopedConfiguration x, DatabaseScopedConfiguration y)
			{
				return EqualityComparer<ZInt>.Default.Equals(x.ConfigurationId, y.ConfigurationId) &&
					EqualityComparer<ZString>.Default.Equals(x.Name, y.Name) &&
					EqualityComparer<ZString>.Default.Equals(x.RecommendedValue, y.RecommendedValue) &&
					EqualityComparer<object>.Default.Equals(x.CurrentValue, y.CurrentValue) &&
					EqualityComparer<object>.Default.Equals(x.CurrentValueForSecondary, y.CurrentValueForSecondary) &&
					EqualityComparer<ZBool>.Default.Equals(x.IsValueDefault, y.IsValueDefault);
			}

			public int GetHashCode(DatabaseScopedConfiguration obj)
			{
				unchecked
				{
					var hashCode = 1660409415;
					hashCode = hashCode * -1521134295 + base.GetHashCode();
					hashCode = hashCode * -1521134295 + obj.ConfigurationId.GetHashCode();
					hashCode = hashCode * -1521134295 + obj.Name.GetHashCode();
					hashCode = hashCode * -1521134295 + obj.RecommendedValue.GetHashCode();
					hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(obj.CurrentValue);
					hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(obj.CurrentValueForSecondary);
					hashCode = hashCode * -1521134295 + obj.IsValueDefault.GetHashCode();
					return hashCode;
				}
			}
		}
	}
}
