namespace eServices.BuildTools.SqlAgentTool;
public class SqlJobStep
{
	public required string Name { get; set; }
	public required SqlJobCommand Command { get; set; }
	public string? Database { get; set; }
	public string? OutputFile { get; set; }
	public StepResultAction? OnSuccess { get; set; }
	public int? OnSuccessStepId { get; set; }
	public StepResultAction? OnFail { get; set; }
	public int? OnFailStepId { get; set; }
	public int RetryAttempts { get; set; }
	public int RetryInterval { get; set; }
}

public enum StepResultAction
{
	QuitReportingSuccess = 1,
	QuitReportingFailure = 2,
	GoToNextStep = 3,
	GoToStep = 4
}
