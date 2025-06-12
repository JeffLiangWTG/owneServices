namespace eServices.BuildTools.SqlAgentTool;

public class SqlJobCommand
{
	public required string Name { get; set; }
	public required SqlJobCommandType Type { get; set; }
	public string Extension => Type switch
	{
		SqlJobCommandType.SQL => ".sql",
		SqlJobCommandType.CmdExec => ".cmd",
		SqlJobCommandType.Powershell => ".ps1",
		_ => throw new NotImplementedException()
	};
	public string FileName => $"{Name}{Extension}";
	public required string Content { get; set; }
}

public enum SqlJobCommandType
{
	SQL,
	CmdExec,
	Powershell
}
