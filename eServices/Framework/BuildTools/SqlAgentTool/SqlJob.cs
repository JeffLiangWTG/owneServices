namespace eServices.BuildTools.SqlAgentTool;

public class SqlJob
{
	public required string Name { get; set; }
	public SqlJobConfig? DefaultConfig { get; set; } = null;
	public Dictionary<string, SqlJobConfig?> EnvironmentConfigs { get; set; } = [];
}
