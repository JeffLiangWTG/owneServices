using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using OcmPoc.Tests.Scenario.Configuration;
using OcmPoc.Tests.Scenario.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace OcmPoc.Tests.Scenario
{
	public class ThroughputTests
    {
		readonly ITestOutputHelper output;

		public ThroughputTests(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Theory]
		[InlineData(16, 10, 120, "CW1:Provider-B")]
		[InlineData(16, 20, 120, "Provider-A:Provider-B")]
		[InlineData(16, 10, 200, "Provider-A:Provider-B", "Provider-B:Provider-A")]
		[InlineData(1 << 10, 300, 120, "Provider-A:Provider-B", "Provider-B:Provider-A", "CW1:Provider-B")]
		[InlineData(1 << 15, 300, 300, "Provider-A:Provider-B", "Provider-B:Provider-A", "CW1:Provider-B")]
		[InlineData(1 << 20, 300, 500, "Provider-A:Provider-B", "Provider-B:Provider-A", "CW1:Provider-B")]
		[InlineData(1 << 25, 300, 1000, "Provider-A:Provider-B", "Provider-B:Provider-A", "CW1:Provider-B")]
		public async Task MessagesAreDeliveredAsExpected(int messageSize, int messageCount, int timeout, params string[] flows)
		{
			output.WriteLine(TestConfig.ToJson());
			var testDriver = new TestDriver(messageSize, messageCount, flows);
			var sw = new Stopwatch();

			sw.Start();
			await testDriver.RunTestAsync(TimeSpan.FromSeconds(timeout));
			sw.Stop();

			var elapsed = sw.Elapsed;
			elapsed.TotalSeconds.Should().BeLessThan(timeout);

			output.WriteLine($"Messages/Minute : {testDriver.MessageCount / elapsed.TotalMinutes}");
			output.WriteLine($"Bytes/Minute : {testDriver.TotalData / elapsed.TotalMinutes}");
		}
	}
}
