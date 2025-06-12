using System;
using CargoWise.eHub.Products.JPCustoms.PullService.Common;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	public class TestPullRunnerConfiguration : IPullRunnerConfiguration
	{
		public TestPullRunnerConfiguration(int pullIntervalInSecond)
		{
			this.PullInterval = new TimeSpan(0, 0, pullIntervalInSecond);
		}

		public TimeSpan PullInterval { get; set; }
	}
}