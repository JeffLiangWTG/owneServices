using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.ApplicationConfig;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.XmlProducer.Common;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.ProcessorRunner
{
	public class IssueReportingConfigProvider
	{
		const string AttributeName = "IssueReportingExitCode";

		public IssueReportingConfigProvider() : this(null)
		{
		}

		public IssueReportingConfigProvider(IStagingRepository repo)
		{
			if (!IsRunningTests)
			{
				if (repo == null)
				{
					repo = StagingRepositoryFactory.GetStagingRepository();
				}
				Argument.NotNull(repo, nameof(repo));
			}
			this.repo = repo;
		}

		readonly IStagingRepository repo;

#if DEBUG
		public
#endif
			bool IsRunningTests
		{
			get => isRunningTests;
			set => isRunningTests = value;
		}

		bool isRunningTests = UnitTestDetector.IsRunningTests.Value;

		public string GetIssueReportingExitCodeValue(string appPath)
		{
#if DEBUG
			if (!IsRunningTests && repo.DatabaseExists)
#endif
			{
				var attributeFromStaging = GetApplicationAttribute(AttributeName, appPath);
				if (!string.IsNullOrEmpty(attributeFromStaging?.RAA_Value))
				{
					return attributeFromStaging.RAA_Value;
				}
			}
			var config = new ConfigurationBuilder().AddJsonFile(Path.ChangeExtension(appPath, "config.json")).Build();
			var configValue = config[AttributeName];
			return string.IsNullOrEmpty(configValue) ? UXMLProducerHelper.IssueReportingExitCodeDefault : configValue;
		}

		RefApplicationAttribute GetApplicationAttribute(string attributeName, string programPath)
		{
			var configFilePath = Path.GetFileNameWithoutExtension(programPath) + ".config.json";
			var attribute = repo.Get<RefApplicationAttribute>().Where(x => x.RAA_AttributeName == attributeName && x.RAA_ConfigFilePath == configFilePath).FirstOrDefault();
			return attribute;
		}
	}
}
