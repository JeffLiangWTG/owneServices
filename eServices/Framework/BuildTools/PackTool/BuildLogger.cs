using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;

namespace eServices.BuildTools.PackTool;

public partial class PackCommandService
{
	public class BuildLogger(Microsoft.Extensions.Logging.ILogger logger) : Microsoft.Build.Utilities.Logger
	{
		public override void Initialize(IEventSource eventSource)
		{
			eventSource.MessageRaised += (sender, args) =>
			{
				switch (args.Importance)
				{
					case MessageImportance.High when IsVerbosityAtLeast(LoggerVerbosity.Minimal):
						logger.LogInformation("{Message}", args.Message);
						break;
					case MessageImportance.Normal when IsVerbosityAtLeast(LoggerVerbosity.Normal):
						logger.LogDebug("{Message}", args.Message);
						break;
					case MessageImportance.Low when IsVerbosityAtLeast(LoggerVerbosity.Detailed):
						logger.LogTrace("{Message}", args.Message);
						break;
				}
			};
		}
	}
}
