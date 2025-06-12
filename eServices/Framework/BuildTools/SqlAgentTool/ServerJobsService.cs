using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;

namespace eServices.BuildTools.SqlAgentTool;

internal class ServerJobsService(SqlConnection? sqlConnection, FileInfo sqlJobsXml, List<string>? jobs = null)
{
	private readonly SqlConnection? sqlConnection = sqlConnection;
	private readonly FileInfo sqlJobsXml = sqlJobsXml;
	private readonly List<string> jobs = jobs ?? [];
	private readonly string? target;

	public ServerJobsService(SqlConnection? sqlConnection, FileInfo sqlJobsXml, List<string>? jobs, string? target) : this(sqlConnection, sqlJobsXml, jobs)
	{
		this.target = target;
	}

	internal async Task SaveXml()
	{
		var sqlXmlText = await GetSqlXml();
		using var writer = sqlJobsXml.CreateText();
		await writer.WriteAsync(sqlXmlText);
	}

	internal async IAsyncEnumerable<SqlJob> Load()
	{
		var sqlXmlText = await GetSqlXml();
		if (sqlConnection is not null && sqlJobsXml is not null)
		{
			using var writer = sqlJobsXml.CreateText();
			await writer.WriteAsync(sqlXmlText);
		}
		var sqlXml = XElement.Parse(sqlXmlText);

		foreach (var sqlJob in sqlXml.Elements())
		{
			var fullName = sqlJob.Element("name")!.Value;
			var jobName = CommandHandlerService.TrimSpecialChars(fullName);
			if (jobs.Count > 0 && !jobs.Contains(fullName) && !jobs.Contains(jobName))
				continue;

			var job = new SqlJob
			{
				Name = jobName,
				DefaultConfig = new SqlJobConfig
				{
					DisplayName = fullName,
					Enabled = sqlJob.Element("enabled")!.Value == "1",
					Description = sqlJob.Element("description")?.Value,
					CategoryName = sqlJob.Element("category_name")?.Value,
					OwnerLoginName = sqlJob.Element("owner_login_name")?.Value,
					NotifyLevelEmail = int.Parse(sqlJob.Element("notify_level_email")!.Value) switch
					{
						0 => NotifyLevel.Never,
						1 => NotifyLevel.OnSuccess,
						2 => NotifyLevel.OnFailure,
						3 => NotifyLevel.Always,
						_ => throw new NotImplementedException(),
					},
					NotifyEmailOperatorName = sqlJob.Element("notify_email_operator_name")?.Value,
				}
			};
			foreach (var jobStep in sqlJob.Elements("sysjobstep"))
			{
				var stepName = CommandHandlerService.TrimSpecialChars(jobStep.Element("step_name")!.Value);
				var command = new SqlJobCommand
				{
					Name = stepName,
					Type = jobStep.Element("subsystem")!.Value.ToUpper() switch
					{
						"TSQL" => SqlJobCommandType.SQL,
						"CMDEXEC" => SqlJobCommandType.CmdExec,
						"POWERSHELL" => SqlJobCommandType.Powershell,
						_ => throw new NotImplementedException(),
					},
					Content = jobStep.Element("command")!.Value
				};
				var step = new SqlJobStep
				{
					Name = stepName,
					Command = command,
					Database = jobStep.Element("database_name")?.Value,
					OutputFile = jobStep.Element("output_file_name")?.Value,
					OnSuccess = Enum.Parse<StepResultAction>(jobStep.Element("on_success_action")!.Value),
					OnFail = Enum.Parse<StepResultAction>(jobStep.Element("on_fail_action")!.Value),
					RetryAttempts = int.Parse(jobStep.Element("retry_attempts")!.Value)
				};
				if (step.RetryAttempts > 0)
					step.RetryInterval = int.Parse(jobStep.Element("retry_interval")!.Value);
				if (step.OnSuccess == StepResultAction.GoToStep)
					step.OnSuccessStepId = int.Parse(jobStep.Element("on_success_step_id")!.Value);
				if (step.OnFail == StepResultAction.GoToStep)
					step.OnFailStepId = int.Parse(jobStep.Element("on_fail_step_id")!.Value);
				job.DefaultConfig.Steps.Add(step);
			};
			foreach (var jobSched in sqlJob.Elements("sysjobschedule"))
			{
				var sched = new SqlJobSchedule()
				{
					Name = jobSched.Element("name")!.Value,
					Enabled = jobSched.Element("enabled")!.Value == "1",
					Type = Enum.Parse<ScheduleType>(jobSched.Element("freq_type")!.Value),
					Interval = int.TryParse(jobSched.Element("freq_interval")?.Value, out int i) ? i : 0,
					SubDayType = Enum.TryParse<ScheduleSubDayType>(jobSched.Element("freq_subday_type")!.Value, out var sdt) && sdt > 0 ? sdt : null,
					RecurrenceFactor = int.TryParse(jobSched.Element("freq_recurrence_factor")?.Value, out i) ? i : 0,
					StartTime = int.TryParse(jobSched.Element("active_start_time")?.Value, out i) ? i : 0,
					EndTime = int.TryParse(jobSched.Element("active_end_time")?.Value, out i) ? i : 0,
				};
				if (sched.Type == ScheduleType.Once)
					sched.StartDate = int.Parse(jobSched.Element("active_start_date")!.Value);
				if (sched.SubDayType is > 0 and not ScheduleSubDayType.AtSpecifiedTime)
					sched.SubDayInterval = int.Parse(jobSched.Element("freq_subday_interval")!.Value);
				if (sched.Type == ScheduleType.MonthlyRelative)
					sched.RelativeInterval = Enum.Parse<RelativeInterval>(jobSched.Element("freq_relative_interval")!.Value);

				job.DefaultConfig.Schedules.Add(sched);
			}
			foreach (var jobAlert in sqlJob.Elements("sysalert"))
			{
				var alert = new SqlJobAlert()
				{
					Name = jobAlert.Element("name")!.Value,
					Enabled = jobAlert.Element("enabled")!.Value == "1",
					WmiNamespace = jobAlert.Element("database_name")?.Value,
					WmiQuery = jobAlert.Element("performance_condition")?.Value,
					DelayBetweenResponses = int.Parse(jobAlert.Element("delay_between_responses")!.Value)
				};

				job.DefaultConfig.Alerts.Add(alert);
			}
			yield return job;
		}
	}

	[ExcludeFromCodeCoverage]
	internal virtual async Task<string> GetSqlXml()
	{
		if (sqlConnection is null)
		{
			using var reader = sqlJobsXml.OpenText();
			return await reader.ReadToEndAsync();
		}

		var assembly = Assembly.GetExecutingAssembly();
		using var stm = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.SqlJobs.sql")!;
		using var rdr = new StreamReader(stm);
		using var cmd = new SqlCommand(await rdr.ReadToEndAsync(), sqlConnection);
		return (string)(await cmd.ExecuteScalarAsync())!;
	}

	[ExcludeFromCodeCoverage]
	internal virtual async Task<int> ExecuteNonQueryAsync(string sql)
	{
		using var cmd = new SqlCommand(sql, sqlConnection);
		return await cmd.ExecuteNonQueryAsync();
	}

	internal virtual string FormatConnect()
	{
		if (sqlConnection != null)
			return $":connect {sqlConnection.DataSource}";
		else if (!string.IsNullOrWhiteSpace(target))
			return $":connect {target}";
		else if (sqlJobsXml != null)
			return $"-- Generated from XML file '{sqlJobsXml.FullName}'";
		else
			return string.Empty;
	}
}
