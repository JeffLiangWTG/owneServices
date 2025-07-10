using Enterprise.Registry.Business;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class SqlSystemConfigurationTest : TestCase
	{
		public void TestHasComputedValue()
		{
			var config = new SqlSystemConfiguration(1);

			Assert(!config.HasRecommendedValue);

			config.RecommendedValue = "101";
			Assert(config.HasRecommendedValue);
		}

		public void TestProposedValue()
		{
			var config = new SqlSystemConfiguration(505)
			{
				Name = "network packet size (B)	4096",
				MinValue = 512,
				MaxValue = 32767,
				ConfiguredValue = 512
			};

			using (config.SuspendValidationTesting())
			{
				var proposedValueHasChanged = false;
				config.PropertyChanged += (sender, args) =>
				{
					if (args.PropertyName == nameof(SqlSystemConfiguration.ProposedValue))
					{
						proposedValueHasChanged = true;
					}
				};

				var expectedValue = (config.MinValue + config.MaxValue) / 2;
				config.ProposedValueText = Invariant($"{expectedValue}");

				Assert(!config.HasErrors);
				Assert(config.HasProposedChange);
				Assert(proposedValueHasChanged);
				AssertEquals(expectedValue, config.ProposedValue);
			}
		}

		public void TestSetPersistedConfig()
		{
			var config = new SqlSystemConfiguration(102)
			{
				Name = "allow updates",
				MinValue = 0,
				MaxValue = 1,
				ConfiguredValue = 1
			};

			var persistedConfig = new PersistedSqlConfiguration { ConfigurationId = 102, ProposedValue = 0 };
			Assert(!config.HasProposedChange);

			config.SetPersistedConfig(persistedConfig);
			AssertEquals(persistedConfig.ProposedValue, config.ProposedValue);
			Assert(config.HasProposedChange);
		}
	}
}
