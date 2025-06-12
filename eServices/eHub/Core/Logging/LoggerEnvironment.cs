using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;

namespace CargoWise.eHub.Core.Logging
{
	class LoggerEnvironment
	{
		public virtual string GetHostName()
		{
			var commandLine = Environment.GetCommandLineArgs();
			return ValidateHostAndGetName(commandLine);
		}

		internal static string ValidateHostAndGetName(string[] commandLine)
		{
			var exeName = Path.GetFileName(commandLine[0]);
			if (exeName.Equals("BTSNTSvc.exe", StringComparison.InvariantCultureIgnoreCase) ||
			    exeName.Equals("BTSNTSvc64.exe", StringComparison.InvariantCultureIgnoreCase))
			{
				if (commandLine.Length < 5)
					throw new InvalidOperationException(string.Format("Incorrect command line argurments for BizTalk host: {0}", Environment.CommandLine));

				return commandLine[4];
			}

			if (exeName.Equals("w3wp.exe", StringComparison.InvariantCultureIgnoreCase) && commandLine.Length >= 3)
			{
				return Regex.Replace(commandLine[2], @"\s*\W*", string.Empty);
			}

			throw new InvalidOperationException(string.Format("Operation not valid for non-BizTalk host: {0}", Environment.CommandLine));
		}

		public virtual Configuration GetExeConfig()
		{
			return ConfigurationManager.OpenExeConfiguration(Environment.GetCommandLineArgs()[0]);
		}

		public virtual AppenderSkeleton GetRollingFileAppender(string logName, string logFile, PatternLayout layout, LoggerConfigItem settings, Level logLevel)
		{
			return new RollingFileAppender()
			{
				Name = logName,
				File = logFile,
				Layout = layout,
				RollingStyle = RollingFileAppender.RollingMode.Size,
				MaximumFileSize = settings.MaximumFileSize,
				MaxSizeRollBackups = settings.MaxSizeRollBackups,
				Threshold = logLevel,
				Encoding = Encoding.UTF8
			};
		}
	}
}
