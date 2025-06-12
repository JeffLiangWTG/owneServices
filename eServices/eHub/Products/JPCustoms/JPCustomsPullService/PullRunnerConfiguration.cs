using System;
using System.Configuration;
using CargoWise.eHub.Products.JPCustoms.PullService.Common;

namespace CargoWise.eHub.Products.JPCustoms.PullService
{
	public class PullRunnerConfiguration : IPullRunnerConfiguration
	{
		public TimeSpan PullInterval
		{
			get
			{
				return new TimeSpan(0, Convert.ToInt32(ConfigurationManager.AppSettings["PullIntervalInMinutes"]), 0);
			}
		}
	}
}