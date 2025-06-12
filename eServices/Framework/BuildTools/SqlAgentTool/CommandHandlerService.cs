using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using KellermanSoftware.CompareNetObjects;

namespace eServices.BuildTools.SqlAgentTool;
internal partial class CommandHandlerService(ServerJobsService serverJobsService, SourceJobsService sourceJobsService)
{
	private readonly ServerJobsService serverJobsService = serverJobsService;
	private readonly SourceJobsService sourceJobsService = sourceJobsService;

	internal async Task JobsXml()
	{
		await serverJobsService.SaveXml();
	}

	internal async Task Export(string environment, bool ignoreNew)
	{
		var serverJobs = serverJobsService.Load().ToBlockingEnumerable().ToList();
		var sourceJobs = sourceJobsService.Load().ToBlockingEnumerable().ToList();
		var comparer = new CompareLogic();
		comparer.Config.CustomPropertyComparer<SqlJobCommand>(command => command.Content, new StringReaderComparer(RootComparerFactory.GetRootComparer()));

		foreach (var job in serverJobs)
		{
			var source = sourceJobs.FirstOrDefault(s => s.Name == job.Name);
			if (ignoreNew && source == null)
			{
				continue;
			}
			if (source != null)
			{
				job.EnvironmentConfigs = source.EnvironmentConfigs;
				if (environment != "Default")
				{
					if (comparer.Compare(source.DefaultConfig, job.DefaultConfig).AreEqual)
					{
						job.EnvironmentConfigs.Remove(environment);
					}
					else
					{
						var matchedFiles = job.DefaultConfig!.Steps.Select(s => s.Command)
							.Join(source.DefaultConfig!.Steps.Select(s => s.Command),
								o => o.FileName,
								i => i.FileName,
								(o, i) => (server: o, source: i)).ToList();
						foreach (var match in matchedFiles.Where(m => !StringReaderComparer.AreEqual(m.server.Content, m.source.Content)))
						{
							match.server.Name = $"{match.server.Name}.{environment}";
						}
						var sourceFiles = source.DefaultConfig!.Steps.Select(s => s.Command).ToList();
						job.EnvironmentConfigs[environment] = job.DefaultConfig;
						job.DefaultConfig = source.DefaultConfig;
					}
				}
			}
			await sourceJobsService.Save(job);
		}
	}

	internal async Task Script(string environment, FileInfo outputFile, bool ignoreExtra)
	{
		using var writer = outputFile.CreateText();
		writer.WriteLine($"-- Environment: {environment}");
		writer.WriteLine(serverJobsService.FormatConnect());
		writer.WriteLine();
		writer.WriteLine(await GenerateSql(environment, ignoreExtra));
	}

	internal async Task Publish(string environment, bool ignoreExtra)
	{
		var sql = await GenerateSql(environment, ignoreExtra);
		await Console.Out.WriteLineAsync($"{Environment.NewLine}Executing SQL:{Environment.NewLine}");
		await Console.Out.WriteLineAsync(sql);
		await serverJobsService.ExecuteNonQueryAsync(sql);
	}

	internal async Task<string> GenerateSql(string environment, bool ignoreExtra)
	{
		var serverJobs = serverJobsService.Load().ToBlockingEnumerable().ToList();
		var sourceJobs = sourceJobsService.Load().ToBlockingEnumerable().ToList();

		var script = new StringBuilder();
		script.AppendLine($"USE [msdb]");
		script.AppendLine($"SET XACT_ABORT ON");
		script.AppendLine();

		if (environment is not null or "Default")
		{
			foreach (var job in sourceJobs.Where(s => s.EnvironmentConfigs.ContainsKey(environment)))
			{
				job.DefaultConfig = job.EnvironmentConfigs[environment];
			}
		}
		sourceJobs.RemoveAll(j => j.DefaultConfig is null);

		if (!ignoreExtra)
		{
			var deletes = serverJobs.ExceptBy(sourceJobs.Select(s => s.Name), j => j.Name).ToList();
			foreach (var job in deletes)
			{
				await Console.Out.WriteLineAsync($"Extra job: {job.Name}");
				script.AppendLine(SeparatorLine);
				foreach (var alert in job.DefaultConfig!.Alerts)
				{
					script.AppendLine($"PRINT 'Deleting alert [{alert.Name}]'").AppendLine();
					script.AppendLine($"EXEC msdb.dbo.sp_delete_alert @name=N'{EscapeSqlText(alert.Name)}'").AppendLine();
				}
				script.AppendLine($"PRINT 'Deleting [{job.Name}]'").AppendLine();
				script.AppendLine($"EXEC msdb.dbo.sp_delete_job @job_name=N'{EscapeSqlText(job.DefaultConfig!.DisplayName)}', @delete_unused_schedule=1").AppendLine();
			}
		}

		var newCategories = sourceJobs.Select(j => j.DefaultConfig!.CategoryName).Except(serverJobs.Select(s => s.DefaultConfig!.CategoryName)).Distinct().ToList();
		if (newCategories.Count > 0)
		{
			script.AppendLine(SeparatorLine);
			script.AppendLine($"PRINT 'Adding Categories'").AppendLine();
			foreach (var category in newCategories)
			{
				script.AppendLine($"IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'{EscapeSqlText(category)}' AND category_class=1)");
				script.AppendLine($"    EXEC msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'{EscapeSqlText(category)}'");
			}
			script.AppendLine();
		}

		var adds = sourceJobs.ExceptBy(serverJobs.Select(s => s.Name), j => j.Name).ToList();
		foreach (var job in adds)
		{
			await Console.Out.WriteLineAsync($"New job: {job.Name}");
			script.AppendLine(SeparatorLine);
			script.AppendLine($"PRINT 'Adding [{job.Name}]'").AppendLine();
			script.AppendLine($"BEGIN TRANSACTION").AppendLine();
			FormatAddJob(job, script);
			script.AppendLine().AppendLine($"COMMIT TRANSACTION").AppendLine();
		}

		var comparer = new CompareLogic(new ComparisonConfig { });
		comparer.Config.MaxDifferences = 20;
		comparer.Config.IgnoreProperty<SqlJobCommand>(x => x.Name);
		comparer.Config.IgnoreProperty<SqlJobCommand>(x => x.FileName);
		comparer.Config.IgnoreProperty<SqlJob>(x => x.EnvironmentConfigs);
		comparer.Config.CustomPropertyComparer<SqlJobCommand>(command => command.Content, new StringReaderComparer(RootComparerFactory.GetRootComparer()));
		var updates = sourceJobs.Join(serverJobs, o => o.Name, i => i.Name, (o, i) => new { o, i, c = comparer.Compare(o, i) })
			.Where(u => !u.c.AreEqual).Select(u => new { job = u.o, compare = u.c }).ToList();
		foreach (var update in updates)
		{
			await Console.Out.WriteLineAsync($"Changed job: {update.job.Name}");
			await Console.Out.WriteLineAsync(update.compare.DifferencesString);
			await Console.Out.WriteLineAsync();
			script.AppendLine(SeparatorLine);
			script.AppendLine($"PRINT 'Updating [{update.job.Name}]'").AppendLine();
			script.AppendLine($"BEGIN TRANSACTION").AppendLine();
			script.AppendLine($"EXEC msdb.dbo.sp_delete_job @job_name=N'{EscapeSqlText(update.job.DefaultConfig!.DisplayName)}', @delete_unused_schedule=1").AppendLine();
			FormatAddJob(update.job, script);
			script.AppendLine().AppendLine($"COMMIT TRANSACTION").AppendLine();
		}

		return script.ToString();
	}

	private static void FormatAddJob(SqlJob job, StringBuilder script)
	{
		var config = job.DefaultConfig!;
		script.AppendLine($"EXEC msdb.dbo.sp_add_job @job_name=N'{EscapeSqlText(config.DisplayName)}',");
		if (!string.IsNullOrWhiteSpace(EscapeSqlText(config.Description)))
			script.AppendLine($"    @description=N'{EscapeSqlText(config.Description)}',");
		if (!string.IsNullOrWhiteSpace(config.OwnerLoginName))
			script.AppendLine($"    @owner_login_name=N'{config.OwnerLoginName}',");
		if (!string.IsNullOrWhiteSpace(config.CategoryName))
			script.AppendLine($"    @category_name=N'{config.CategoryName}',");
		if (config.NotifyLevelEmail != NotifyLevel.Never)
		{
			script.AppendLine($"    @notify_level_email={(int?)config.NotifyLevelEmail},");
			if (!string.IsNullOrWhiteSpace(config.NotifyEmailOperatorName))
				script.AppendLine($"    @notify_email_operator_name=N'{config.NotifyEmailOperatorName}',");
		}
		script.AppendLine($"    @enabled={(config.Enabled ? 1 : 0)}");
		script.AppendLine();

		foreach (var step in config.Steps)
		{
			string outputFile = "";
			if (!string.IsNullOrWhiteSpace(step.OutputFile))
			{
				var outputFileVarName = $"@{job.Name}_{config.Steps.IndexOf(step)}_OutputFile";
				script.AppendLine($"DECLARE {outputFileVarName} nvarchar(max) = CONCAT(N'{EscapeSqlText(step.OutputFile)!.Replace("$(", "', '$', '(")}')");
				outputFile = $"    @output_file_name={outputFileVarName},";
			}
			string command = "";
			if (step.Command.Content.Contains("$("))
			{
				var commandVarName = $"@{job.Name}_{config.Steps.IndexOf(step)}_Command";
				AppendTextLines(script, $"DECLARE {commandVarName} nvarchar(max) = CONCAT(N'{EscapeSqlText(step.Command.Content)!.Replace("$(", "', '$', '(")}')");
				command = $"    @command={commandVarName}";
			}
			else
			{
				command = $"    @command=N'{EscapeSqlText(step.Command.Content)}'";
			}
			script.AppendLine($"EXEC msdb.dbo.sp_add_jobstep @step_name=N'{EscapeSqlText(step.Name)}',");
			script.AppendLine($"    @job_name=N'{EscapeSqlText(config.DisplayName)}',");
			script.AppendLine($"    @on_success_action={(int?)step.OnSuccess},");
			if (step.OnSuccess == StepResultAction.GoToStep)
				script.AppendLine($"    @on_success_step_id={step.OnSuccessStepId},");
			script.AppendLine($"    @on_fail_action={(int?)step.OnFail},");
			if (step.OnFail == StepResultAction.GoToStep)
				script.AppendLine($"    @on_fail_step_id={step.OnFailStepId},");
			if (step.RetryAttempts > 0)
			{
				script.AppendLine($"    @retry_attempts={step.RetryAttempts},");
				script.AppendLine($"    @retry_interval={step.RetryInterval},");
			}
			if (!string.IsNullOrWhiteSpace(outputFile))
				script.AppendLine(outputFile);
			script.AppendLine($"    @subsystem=N'{step.Command.Type switch
			{
				SqlJobCommandType.SQL => "TSQL",
				SqlJobCommandType.CmdExec => "CmdExec",
				SqlJobCommandType.Powershell => "PowerShell",
				_ => throw new NotImplementedException(),
			}}',");
			if (!string.IsNullOrWhiteSpace(step.Database))
				script.AppendLine($"    @database_name=N'{step.Database}',");
			AppendTextLines(script, command);
			script.AppendLine();
		}

		foreach (var sched in config.Schedules)
		{
			script.AppendLine($"EXEC msdb.dbo.sp_add_jobschedule @name=N'{EscapeSqlText(sched.Name)}',");
			script.AppendLine($"    @job_name=N'{EscapeSqlText(config.DisplayName)}',");
			script.AppendLine($"    @freq_type='{(int?)sched.Type}',");
			if (sched.Type is ScheduleType.Daily or ScheduleType.Weekly or ScheduleType.Monthly or ScheduleType.MonthlyRelative)
			{
				script.AppendLine($"    @freq_interval='{sched.Interval}',");
				script.AppendLine($"    @freq_subday_type='{(int?)sched.SubDayType}',");
				if (sched.SubDayType != ScheduleSubDayType.AtSpecifiedTime)
					script.AppendLine($"    @freq_subday_interval='{sched.SubDayInterval}',");
				if (sched.Type == ScheduleType.MonthlyRelative)
					script.AppendLine($"    @freq_relative_interval='{(int?)sched.RelativeInterval}',");
				if (sched.Type is ScheduleType.Weekly or ScheduleType.Monthly or ScheduleType.MonthlyRelative)
					script.AppendLine($"    @freq_recurrence_factor='{sched.RecurrenceFactor}',");
			}
			if (sched.Type == ScheduleType.Once)
				script.AppendLine($"    @active_start_date='{sched.StartDate:D8}',");
			if (sched.StartTime > 0)
				script.AppendLine($"    @active_start_time='{sched.StartTime:D6}',");
			if (sched.EndTime != 235959)
				script.AppendLine($"    @active_end_time='{sched.EndTime:D6}',");

			script.AppendLine($"    @enabled={(sched.Enabled ? 1 : 0)}");
			script.AppendLine();
		}

		foreach (var alert in job.DefaultConfig!.Alerts)
		{
			script.AppendLine($"EXEC msdb.dbo.sp_add_alert @name=N'{EscapeSqlText(alert.Name)}',");
			script.AppendLine($"    @job_name=N'{EscapeSqlText(config.DisplayName)}',");
			script.AppendLine($"    @wmi_namespace='{EscapeSqlText(alert.WmiNamespace)}',");
			script.AppendLine($"    @wmi_query='{EscapeSqlText(alert.WmiQuery)}',");
			script.AppendLine($"    @delay_between_responses='{(int?)alert.DelayBetweenResponses}',");
			script.AppendLine($"    @enabled={(alert.Enabled ? 1 : 0)}");
			script.AppendLine();
		}

		script.AppendLine($"EXEC msdb.dbo.sp_add_jobserver @job_name=N'{EscapeSqlText(config.DisplayName)}', @server_name=N'(local)'");
	}

	private static string? EscapeSqlText(string? text) => text?.Replace("'", "''");

	private static void AppendTextLines(StringBuilder sb, string text)
	{
		using var reader = new StringReader(text);
		while (reader.ReadLine() is string line)
		{
			sb.AppendLine(line);
		}
	}

	private const string SeparatorLine = "------------------------------------------------------------------------";

	internal static string TrimSpecialChars(string s) => RegexSpecialChars().Replace(s, "");

	[GeneratedRegex(@"[^a-zA-Z0-9_]")]
	private static partial Regex RegexSpecialChars();
}
