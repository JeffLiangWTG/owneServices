namespace eServices.BuildTools.SqlAgentTool;

public class SqlJobAlert
{
	public required string Name { get; set; }
	public bool Enabled { get; set; }
	public string? WmiNamespace { get; set; }
	public string? WmiQuery { get; set; }
	public int DelayBetweenResponses { get; set; }
}
