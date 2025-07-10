using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.ApplicationConfigurationGenerator
{
	public class ApplicationConfigurationGenerator
	{
		const string XMLDefaultName = "ApplicationPathsAndDefaultConfigurations.xml";
		const string AppPathSearchPattern = "CargoWise.RefDbRepo.*.exe";
		const string jobXmlPathSearchPattern = "*RunnerJobs.xml";
		readonly string _rootFolder;
		readonly string _saveTo;

		public List<ApplicationConfiguration> ApplicationConfigurations => _applicationConfigurations ?? (_applicationConfigurations = new List<ApplicationConfiguration>());
		List<ApplicationConfiguration> _applicationConfigurations;

		public ApplicationConfigurationGenerator(string rootFolder, string saveTo)
		{
			_rootFolder = rootFolder;
			_saveTo = Path.Combine(saveTo, XMLDefaultName);
		}

		public async Task Run()
		{
			await SetFilesToRun();
			SaveFile();
		}

		public void SaveFile()
		{
			var settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = true
			};
			var serializer = new XmlSerializer(typeof(List<ApplicationConfiguration>));
			using (var xw = XmlWriter.Create(_saveTo, settings))
			{
				serializer.Serialize(xw, ApplicationConfigurations);
			}
		}

		async Task SetFilesToRun()
		{
			var jobDetailDictionary = await ConstructJobDetailDictionary();

			var directories = Directory.GetDirectories(_rootFolder);
			foreach (var directory in directories)
			{
				var files = Directory.GetFiles(directory, AppPathSearchPattern, SearchOption.AllDirectories);
				foreach (var file in files)
				{
					var appConfiguration = GetAndTransformConfigurationFile(file);
					if (appConfiguration == null)
					{
						continue;
					}
					var programPath = file.Replace(_rootFolder, string.Empty).TrimStart('\\');
					if (programPath.StartsWith("Staging\\", StringComparison.OrdinalIgnoreCase))
					{
						programPath = programPath.Replace("Staging\\", string.Empty);
					}
					else
					{
						programPath = @"..\" + programPath;
					}
					appConfiguration.ApplicationPath = programPath;
					appConfiguration.ApplicationJobGroup = jobDetailDictionary.ContainsKey(Path.GetFileName(programPath)) ? jobDetailDictionary[Path.GetFileName(programPath)] : string.Empty;
					ApplicationConfigurations.Add(appConfiguration);

				}
			}
		}

		async Task<Dictionary<string, string>> ConstructJobDetailDictionary()
		{
			var directories = Directory.GetDirectories(_rootFolder);
			List<string> jobFiles = new List<string>();
			foreach (var directory in directories)
			{
				var files = Directory.GetFiles(directory, jobXmlPathSearchPattern, SearchOption.AllDirectories);
				jobFiles.AddRange(files);
			}

			var jobDetailFileReader = new JobDetailFileReader(jobFiles);
			var jobDetails = await jobDetailFileReader.GetJobDetails();

			var jobDetailDictionary = new Dictionary<string, string>();
			foreach (var jobDetail in jobDetails)
			{
				var jobDataEntryArr = jobDetail.jobdatamap.entry;
				var programExePathEntry = jobDataEntryArr.FirstOrDefault(x => x.key.Equals("ProgramExePath", StringComparison.OrdinalIgnoreCase));
				if (programExePathEntry != null)
				{
					var programExePath = programExePathEntry.value;
					jobDetailDictionary[Path.GetFileName(programExePath)] = jobDetail.group;
				}
			}
			return jobDetailDictionary;
		}

		static ApplicationConfiguration GetAndTransformConfigurationFile(string file)
		{
			var appConfigFile = $"{file}.config";
			if (File.Exists(appConfigFile))
			{
				return TransformAppConfig(appConfigFile);
			}
			var configJsonFile = $"{file.Replace(".exe", "")}.config.json";
			if (File.Exists(configJsonFile))
			{
				return TransformConfigJson(configJsonFile);
			}
			return null;
		}

		static ApplicationConfiguration TransformConfigJson(string configJsonFile)
		{
			var configJsonFileName = Path.GetFileName(configJsonFile);
			var result = new ApplicationConfiguration { ApplicationConfigFileName = configJsonFileName };
			var jsonContent = File.ReadAllText(configJsonFile);
			var jsonDocument = JsonDocument.Parse(jsonContent);
			foreach (var property in jsonDocument.RootElement.EnumerateObject())
			{
				if (property.Value.ValueKind == JsonValueKind.Object)
				{
					continue;
				}
				result.ConfigData.Add(new KeyData { Name = property.Name, Value = property.Value.ToString() });
			}
			AddIssueReportingExitCodeIfNotExist(result);
			return result;
		}

		static ApplicationConfiguration TransformAppConfig(string appConfigPath)
		{
			var appConfigFileName = Path.GetFileName(appConfigPath);
			var result = new ApplicationConfiguration { ApplicationConfigFileName = appConfigFileName };
			var appConfigXDocument = XDocument.Load(appConfigPath);
			var appSettings = appConfigXDocument.Descendants(XName.Get("appSettings"));
			if (appSettings.Any())
			{
				foreach (var key in appSettings.First().Descendants())
				{
					var name = key.Attribute(XName.Get("key"))?.Value;
					if (string.IsNullOrEmpty(name))
					{
						continue;
					}
					var value = key.Attribute(XName.Get("value"))?.Value;
					result.ConfigData.Add(new KeyData { Name = name, Value = value });
				}
			}
			AddIssueReportingExitCodeIfNotExist(result);
			return result;
		}

		static void AddIssueReportingExitCodeIfNotExist(ApplicationConfiguration applicationConfiguration)
		{
			if (!applicationConfiguration.ConfigData.Select(x => x.Name).Contains("IssueReportingExitCode"))
			{
				applicationConfiguration.ConfigData.Add(new KeyData { Name = "IssueReportingExitCode", Value = UXMLProducerHelper.IssueReportingExitCodeDefault });
			}
		}
	}
}
