namespace eServices.BuildTools.SqlAgentTool;
public class SqlJobSchedule
{
	public required string Name { get; set; }
	public bool Enabled { get; set; }
	public ScheduleType Type { get; set; }
	public int Interval { get; set; }
	public ScheduleSubDayType? SubDayType { get; set; }
	public int SubDayInterval { get; set; }
	public RelativeInterval? RelativeInterval { get; set; }
	public int RecurrenceFactor { get; set; }
	public int StartDate { get; set; }
	public int StartTime { get; set; }
	public int EndTime { get; set; }
}

public enum ScheduleType
{
	Once = 1,
	Daily = 4,
	Weekly = 8,
	Monthly = 16,
	MonthlyRelative = 32,
	OnSqlAgentStart = 64,
	OnIdle = 128
}

public enum ScheduleSubDayType
{
	AtSpecifiedTime = 1,
	Seconds = 2,
	Minutes = 4,
	Hours = 8
}

public enum RelativeInterval
{
	First = 1,
	Second = 2,
	Third = 4,
	Fourth = 8,
	Last = 16
}
