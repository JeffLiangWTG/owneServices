using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace eServices.BuildTools.SqlAgentTool;

public partial class SourceJobsService(DirectoryInfo sourceFiles, List<string> jobs)
{
	private readonly DirectoryInfo sourceFiles = sourceFiles;
	private readonly List<string> jobs = jobs;

	public async IAsyncEnumerable<SqlJob> Load()
	{
		await Task.Yield();

		foreach (var jobDir in sourceFiles.EnumerateDirectories())
		{
			var files = jobDir.EnumerateFiles().Select(f => new { file = f, match = RegexFileName().Match(f.Name) }).ToList();

			var commands = files.Where(f => !f.match.Groups["config"].Success)
				.Select(f => new
				{
					f.file,
					f.match,
					type = f.match.Groups["extn"].Value.ToLower() switch
					{
						".sql" => SqlJobCommandType.SQL,
						".cmd" => SqlJobCommandType.CmdExec,
						".ps1" => SqlJobCommandType.Powershell,
						_ => (SqlJobCommandType?)null
					}
				})
				.Where(f => f.type.HasValue)
				.Select(async f => new SqlJobCommand
				{
					Name = f.match.Groups["command"].Value,
					Type = f.type!.Value,
					Content = await ReadFileText(f.file)
				}).Select(t => t.Result).ToList();

			var configs = files.Where(f => f.match.Groups["config"].Success)
				.Select(f => new { f.file, f.match, config = DeserializeConfigXml(f.file, commands).Result })
				.Where(c => jobs.Count == 0 || jobs.Contains(jobDir.Name) || (c.config?.DisplayName != null && jobs.Contains(c.config.DisplayName)))
				.ToArray();
			if (configs.Length == 0) continue;

			var job = new SqlJob
			{
				Name = jobDir.Name,
				DefaultConfig = configs.FirstOrDefault(c => !c.match.Groups["environment"].Success)?.config,
				EnvironmentConfigs = configs.Where(c => c.match.Groups["environment"].Success)
					.ToDictionary(c => c.match.Groups["environment"].Value, c => c.config)
			};

			yield return job;
		}
	}

	public async Task Save(SqlJob job)
	{
		var jobDir = sourceFiles.CreateSubdirectory(job.Name);
		if (job.DefaultConfig != null)
		{
			File.WriteAllText(Path.Combine(jobDir.FullName, "SqlJob.xml"), await SerializeConfigXml(job.DefaultConfig));
			foreach (var command in job.DefaultConfig.Steps.Select(s => s.Command).Distinct())
			{
				await WriteFileText(Path.Combine(jobDir.FullName, command.FileName), command.Content);
			}
		}
		foreach (var config in job.EnvironmentConfigs)
		{
			File.WriteAllText(Path.Combine(jobDir.FullName, $"SqlJob.{config.Key}.xml"), await SerializeConfigXml(config.Value));
			if (config.Value != null)
			{
				foreach (var command in config.Value.Steps.Select(s => s.Command).Distinct())
				{
					await WriteFileText(Path.Combine(jobDir.FullName, command.FileName), command.Content);
				}
			}
		}
	}

	private static Task<string> SerializeConfigXml(SqlJobConfig? config)
	{
		if (config is null)
			return Task.FromResult("<SqlJob/>");

		var configContents = new List<XElement>()
		{
			new(nameof(SqlJobConfig.DisplayName), config.DisplayName),
			new(nameof(SqlJobConfig.Enabled), config.Enabled),
			new(nameof(SqlJobConfig.OwnerLoginName), config.OwnerLoginName),
		};
		if (!string.IsNullOrWhiteSpace(config.Description))
			configContents.Add(new(nameof(SqlJobConfig.Description), config.Description));
		if (!string.IsNullOrWhiteSpace(config.CategoryName) && config.CategoryName != "[Uncategorized (Local)]")
			configContents.Add(new(nameof(SqlJobConfig.CategoryName), config.CategoryName));
		if (config.NotifyLevelEmail != NotifyLevel.OnFailure)
			configContents.Add(new(nameof(SqlJobConfig.NotifyLevelEmail), config.NotifyLevelEmail));
		if (config.NotifyLevelEmail != NotifyLevel.Never && !string.IsNullOrWhiteSpace(config.NotifyEmailOperatorName))
			configContents.Add(new(nameof(SqlJobConfig.NotifyEmailOperatorName), config.NotifyEmailOperatorName));

		var steps = config.Steps.Select(step =>
		{
			var stepContents = new List<XAttribute>
			{
				new(nameof(SqlJobStep.Name), step.Name),
				new(nameof(SqlJobStep.Command), step.Command.FileName),
				new(nameof(SqlJobCommand.Type), step.Command.Type),
			};
			if (!string.IsNullOrWhiteSpace(step.Database))
				stepContents.Add(new(nameof(SqlJobStep.Database), step.Database));
			if (!string.IsNullOrWhiteSpace(step.OutputFile))
				stepContents.Add(new(nameof(SqlJobStep.OutputFile), step.OutputFile));
			if (step.RetryAttempts > 0)
			{
				stepContents.Add(new(nameof(SqlJobStep.RetryAttempts), step.RetryAttempts));
				stepContents.Add(new(nameof(SqlJobStep.RetryInterval), step.RetryInterval));
			}
			var isLastStep = step == config.Steps.Last();
			if ((isLastStep && step.OnSuccess != StepResultAction.QuitReportingSuccess)
				|| (!isLastStep && step.OnSuccess != StepResultAction.GoToNextStep))
			{
				stepContents.Add(new(nameof(SqlJobStep.OnSuccess), step.OnSuccess!));
				if (step.OnSuccess == StepResultAction.GoToNextStep)
					stepContents.Add(new(nameof(SqlJobStep.OnSuccessStepId), step.OnSuccessStepId!));
			}
			if (step.OnFail != StepResultAction.QuitReportingFailure)
			{
				stepContents.Add(new(nameof(SqlJobStep.OnFail), step.OnFail!));
				if (step.OnFail == StepResultAction.GoToStep)
					stepContents.Add(new(nameof(SqlJobStep.OnFailStepId), step.OnFailStepId!));
			}

			return new XElement("Step", stepContents);
		});
		configContents.Add(new XElement(nameof(SqlJobConfig.Steps), steps));

		var schedules = config.Schedules.Select(sched =>
		{
			var schedContents = new List<XAttribute>
			{
				new(nameof(SqlJobSchedule.Name), sched.Name),
				new(nameof(SqlJobSchedule.Enabled), sched.Enabled),
				new(nameof(SqlJobSchedule.Type), sched.Type)
			};

			if (sched.Type is ScheduleType.Daily or ScheduleType.Weekly or ScheduleType.Monthly or ScheduleType.MonthlyRelative)
			{
				schedContents.Add(new(nameof(SqlJobSchedule.Interval), sched.Interval));
				schedContents.Add(new(nameof(SqlJobSchedule.SubDayType), sched.SubDayType!));
				if (sched.SubDayType != ScheduleSubDayType.AtSpecifiedTime)
					schedContents.Add(new(nameof(SqlJobSchedule.SubDayInterval), sched.SubDayInterval));
				if (sched.Type == ScheduleType.MonthlyRelative)
					schedContents.Add(new(nameof(SqlJobSchedule.RelativeInterval), sched.RelativeInterval!));
				if (sched.Type is ScheduleType.Weekly or ScheduleType.Monthly or ScheduleType.MonthlyRelative)
					schedContents.Add(new(nameof(SqlJobSchedule.RecurrenceFactor), sched.RecurrenceFactor));
			}
			if (sched.Type == ScheduleType.Once)
				schedContents.Add(new(nameof(SqlJobSchedule.StartDate), sched.StartDate.ToString("D8")));
			if (sched.StartTime > 0)
				schedContents.Add(new(nameof(SqlJobSchedule.StartTime), sched.StartTime.ToString("D6")));
			if (sched.EndTime != 235959)
				schedContents.Add(new(nameof(SqlJobSchedule.EndTime), sched.EndTime.ToString("D6")));
			return new XElement("Schedule", schedContents);
		});
		configContents.Add(new XElement("Schedules", schedules));

		if (config.Alerts.Count > 0)
		{
			var alerts = config.Alerts.Select(alert =>
			{
				var alertContents = new List<XAttribute>
				{
					new(nameof(SqlJobAlert.Name), alert.Name),
					new(nameof(SqlJobAlert.Enabled), alert.Enabled)
				};
				if (!string.IsNullOrWhiteSpace(alert.WmiNamespace))
					alertContents.Add(new(nameof(SqlJobAlert.WmiNamespace), alert.WmiNamespace));
				if (!string.IsNullOrWhiteSpace(alert.WmiQuery))
					alertContents.Add(new(nameof(SqlJobAlert.WmiQuery), alert.WmiQuery));
				if (alert.DelayBetweenResponses is not 0)
					alertContents.Add(new(nameof(SqlJobAlert.DelayBetweenResponses), alert.DelayBetweenResponses));
				return new XElement("Alert", alertContents);
			});
			configContents.Add(new XElement("Alerts", alerts));
		}

		var xml = new XElement("SqlJob", configContents);
		return Task.FromResult(xml.ToString());
	}

	private static async Task<SqlJobConfig?> DeserializeConfigXml(FileInfo file, List<SqlJobCommand> commands)
	{
		var xml = XElement.Parse(await ReadFileText(file));
		if (!xml.HasElements)
			return null;
		var steps = xml.Element("Steps")!.Elements().ToList();
		int i = 0;
		var config = new SqlJobConfig
		{
			DisplayName = xml.Element(nameof(SqlJobConfig.DisplayName))?.Value,
			Enabled = bool.Parse(xml.Element(nameof(SqlJobConfig.Enabled))!.Value),
			Description = xml.Element(nameof(SqlJobConfig.Description))?.Value,
			CategoryName = xml.Element(nameof(SqlJobConfig.CategoryName))?.Value ?? "[Uncategorized (Local)]",
			OwnerLoginName = xml.Element(nameof(SqlJobConfig.OwnerLoginName))?.Value,
			NotifyLevelEmail = Enum.TryParse<NotifyLevel>(xml.Element(nameof(SqlJobConfig.NotifyLevelEmail))?.Value, out var nl) ? nl : NotifyLevel.OnFailure,
			NotifyEmailOperatorName = xml.Element(nameof(SqlJobConfig.NotifyEmailOperatorName))?.Value,
			Steps = steps.Select(step =>
			{
				var command = commands.First(f => f.FileName == step.Attribute(nameof(SqlJobStep.Command))!.Value);
				return new SqlJobStep
				{
					Name = step.Attribute(nameof(SqlJobStep.Name))!.Value,
					Command = command,
					Database = step.Attribute(nameof(SqlJobStep.Database))?.Value,
					OutputFile = step.Attribute(nameof(SqlJobStep.OutputFile))?.Value,
					OnSuccess = Enum.TryParse<StepResultAction>(step.Attribute(nameof(SqlJobStep.OnSuccess))?.Value, out var onSuccess) ? onSuccess : step == steps.Last() ? StepResultAction.QuitReportingSuccess : StepResultAction.GoToNextStep,
					OnSuccessStepId = int.TryParse(step.Attribute(nameof(SqlJobStep.OnSuccessStepId))?.Value, out i) ? i : null,
					OnFail = Enum.TryParse<StepResultAction>(step.Attribute(nameof(SqlJobStep.OnFail))?.Value, out var onFail) ? onFail : StepResultAction.QuitReportingFailure,
					OnFailStepId = int.TryParse(step.Attribute(nameof(SqlJobStep.OnFailStepId))?.Value, out i) ? i : null,
					RetryAttempts = int.TryParse(step.Attribute(nameof(SqlJobStep.RetryAttempts))?.Value, out i) ? i : 0,
					RetryInterval = int.TryParse(step.Attribute(nameof(SqlJobStep.RetryInterval))?.Value, out i) ? i : 0,
				};
			}).ToList(),
			Schedules = xml.Element("Schedules")!.Elements().Select(sched => new SqlJobSchedule
			{
				Name = sched.Attribute(nameof(SqlJobSchedule.Name))!.Value,
				Enabled = bool.Parse(sched.Attribute(nameof(SqlJobSchedule.Enabled))!.Value),
				Type = Enum.TryParse<ScheduleType>(sched.Attribute(nameof(SqlJobSchedule.Type))?.Value, out var type) ? type : ScheduleType.Once,
				Interval = int.TryParse(sched.Attribute(nameof(SqlJobSchedule.Interval))?.Value, out i) ? i : 0,
				SubDayType = Enum.TryParse<ScheduleSubDayType>(sched.Attribute(nameof(SqlJobSchedule.SubDayType))?.Value, out var subDayType) ? subDayType : null,
				SubDayInterval = int.TryParse(sched.Attribute(nameof(SqlJobSchedule.SubDayInterval))?.Value, out i) ? i : 0,
				RelativeInterval = Enum.TryParse<RelativeInterval>(sched.Attribute(nameof(SqlJobSchedule.RelativeInterval))?.Value, out var relativeInterval) ? relativeInterval : null,
				RecurrenceFactor = int.TryParse(sched.Attribute(nameof(SqlJobSchedule.RecurrenceFactor))?.Value, out i) ? i : 0,
				StartDate = int.TryParse(sched.Attribute(nameof(SqlJobSchedule.StartDate))?.Value, out i) ? i : 0,
				StartTime = int.TryParse(sched.Attribute(nameof(SqlJobSchedule.StartTime))?.Value, out i) ? i : 0,
				EndTime = int.TryParse(sched.Attribute(nameof(SqlJobSchedule.EndTime))?.Value, out i) ? i : 235959
			}).ToList(),
			Alerts = xml.Element("Alerts") is not null ? xml.Element("Alerts")!.Elements().Select(alert => new SqlJobAlert
			{
				Name = alert.Attribute(nameof(SqlJobAlert.Name))!.Value,
				Enabled = bool.Parse(alert.Attribute(nameof(SqlJobAlert.Enabled))!.Value),
				WmiNamespace = alert.Attribute(nameof(SqlJobAlert.WmiNamespace))?.Value,
				WmiQuery = alert.Attribute(nameof(SqlJobAlert.WmiQuery))?.Value,
				DelayBetweenResponses = int.TryParse(alert.Attribute(nameof(SqlJobAlert.DelayBetweenResponses))?.Value, out i) ? i : 0
			}).ToList() : []
		};
		return config;
	}

	static async Task<string> ReadFileText(FileInfo file)
	{
		using var rdr = file.OpenText();
		return await rdr.ReadToEndAsync();
	}

	static async Task WriteFileText(string fileName, string content)
	{
		using var reader = new StringReader(content);
		using var filestrm = File.OpenWrite(fileName);
		using var filewrtr = new StreamWriter(filestrm);
		while (await reader.ReadLineAsync() is string line)
		{
			await filewrtr.WriteLineAsync(line);
		}
	}

	[GeneratedRegex(@"^((?<config>SqlJob(\.(?<environment>\w+))?)|(?<command>.+))(?<extn>\.\w+)$")]
	private static partial Regex RegexFileName();
}
