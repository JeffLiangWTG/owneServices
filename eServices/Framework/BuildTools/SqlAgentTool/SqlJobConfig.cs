namespace eServices.BuildTools.SqlAgentTool;

public class SqlJobConfig
{
	public string? DisplayName { get; set; }
	public bool Enabled { get; set; }
	public string? Description { get; set; }
	public string? CategoryName { get; set; }
	public string? OwnerLoginName { get; set; }
	public NotifyLevel NotifyLevelEmail { get; set; }
	public string? NotifyEmailOperatorName { get; set; }
	public List<SqlJobStep> Steps { get; set; } = [];
	public List<SqlJobSchedule> Schedules { get; set; } = [];
	public List<SqlJobAlert> Alerts { get; set; } = [];
}

public enum NotifyLevel
{
	Never,
	OnSuccess,
	OnFailure,
	Always
}
