using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace CargoWise.Billing.CollectorService.Plugin.Tests
{
	[TestFixture]
	public class ServiceStateTest
	{
		[Test]
		public void TestSerialization()
		{
			var tempFileName = Path.GetTempFileName();
			try
			{
				StateExample.WriteToFile(tempFileName);

				// Need to remove these attributes as the ordering is non-deterministic.
				var output = File.ReadAllText(tempFileName).Replace(@"xmlns:xsd=""http://www.w3.org/2001/XMLSchema""", "").Replace(@"xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "");
				var expected = StateStringExample.Replace(@"xmlns:xsd=""http://www.w3.org/2001/XMLSchema""", "").Replace(@"xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "");

				Assert.That(output, Is.EqualTo(expected));
			}
			finally
			{
				File.Delete(tempFileName);
			}
		}

		[Test]
		public void TestDeserialization()
		{
			var tempFileName = Path.GetTempFileName();
			try
			{
				File.WriteAllText(tempFileName, StateStringExample);
				var serviceState = ServiceState.ReadFromFile(tempFileName);
				Assert.That(serviceState, Is.EqualTo(StateExample));
			}
			finally
			{
				File.Delete(tempFileName);
			}
		}

		ServiceState StateExample
		{
			get
			{
				return new ServiceState
				{
					Plugins = new List<PluginControllerState>
					{
						new PluginControllerState
						{
							Key = "TestPlugin1.Key",
							LastSuccessfulRun = DateTime.Parse("2014-10-23T21:07:25.0355728Z").ToUniversalTime(),
							LastTransactionTimestamp = DateTime.Parse("2014-10-23T21:06:25.0355728Z").ToUniversalTime(),
						},
						new PluginControllerState
						{
							Key = "TestPlugin2.Key",
							LastSuccessfulRun = new DateTime(2014, 5, 5, 9, 30, 0, 0, DateTimeKind.Utc),
							LastTransactionTimestamp = new DateTime(2014, 5, 5, 9, 25, 0, 0, DateTimeKind.Utc),
						},
					}
				};
			}
		}
#if NETFRAMEWORK
		const string StateStringExample = @"<?xml version=""1.0""?>
<Plugins xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Plugin>
    <Key>TestPlugin1.Key</Key>
    <LastSuccessfulRun>2014-10-23T21:07:25.0355728Z</LastSuccessfulRun>
    <LastTransactionTimestamp>2014-10-23T21:06:25.0355728Z</LastTransactionTimestamp>
  </Plugin>
  <Plugin>
    <Key>TestPlugin2.Key</Key>
    <LastSuccessfulRun>2014-05-05T09:30:00Z</LastSuccessfulRun>
    <LastTransactionTimestamp>2014-05-05T09:25:00Z</LastTransactionTimestamp>
  </Plugin>
</Plugins>";
#elif NET8_0
		const string StateStringExample = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Plugins xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Plugin>
    <Key>TestPlugin1.Key</Key>
    <LastSuccessfulRun>2014-10-23T21:07:25.0355728Z</LastSuccessfulRun>
    <LastTransactionTimestamp>2014-10-23T21:06:25.0355728Z</LastTransactionTimestamp>
  </Plugin>
  <Plugin>
    <Key>TestPlugin2.Key</Key>
    <LastSuccessfulRun>2014-05-05T09:30:00Z</LastSuccessfulRun>
    <LastTransactionTimestamp>2014-05-05T09:25:00Z</LastTransactionTimestamp>
  </Plugin>
</Plugins>";
#endif
	}
}
