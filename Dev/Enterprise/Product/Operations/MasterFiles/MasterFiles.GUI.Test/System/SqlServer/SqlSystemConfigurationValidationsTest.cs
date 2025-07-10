using System.Linq;
using NUnit.Framework;
using static System.FormattableString;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class SqlSystemConfigurationValidationsTest : TestCase
	{
		public void TestProposedValueRange()
		{
			var config = new SqlSystemConfiguration(1562)
			{
				Name = "clr enabled",
				MinValue = 0,
				MaxValue = 1,
				ConfiguredValue = 0
			};

			using (config.SuspendValidationTesting())
			{
				config.ProposedValueText = Invariant($"{config.MaxValue + 1}");
				Assert(config.HasNotifications(NotificationType.Error));
				Assert(config.Notifications.Any(x =>
					x.Message == "Error - ProposedValueText: The entered value (2) for 'clr enabled' is outside allowed range of (min:0, max:1)."));
				Assert(!config.HasProposedChange);
				AssertEquals(config.ConfiguredValue, config.ProposedValue);

				config.ClearAllNotifications();
				config.ProposedValueText = Invariant($"{config.MinValue - 1}");
				Assert(config.HasNotifications(NotificationType.Error));
				Assert(config.Notifications.Any(x =>
					x.Message == "Error - ProposedValueText: The entered value (-1) for 'clr enabled' is outside allowed range of (min:0, max:1)."));
				Assert(!config.HasProposedChange);
				AssertEquals(config.ConfiguredValue, config.ProposedValue);
			}
		}
	}
}
