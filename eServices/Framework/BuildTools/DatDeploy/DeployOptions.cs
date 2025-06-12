namespace eServices.BuildTools.DatDeploy
{
	internal class DeployOptions
	{
		public ErrorAction MissingEnvironmentErrorAction { get; set; } = ErrorAction.Stop;
	}

	internal enum ErrorAction
	{
		/// <summary>
		/// Fail the deployment with error.
		/// </summary>
		Break,
		/// <summary>
		/// Exit the deployment without failure.
		/// </summary>
		Stop,
		/// <summary>
		/// Conitnue the deployment.
		/// </summary>
		Continue
	}
}
