using System;

namespace CargoWise.eHub.Products.JPCustoms.PullService.Common
{
	public interface IPullRunnerConfiguration
	{
		TimeSpan PullInterval { get; }
	}
}
