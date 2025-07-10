using CargoWise.Definitions;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Winzor.Architecture;

public class WinzorCargoWiseLoginTask : IApplicationStartupTask
{
	readonly IWinzorCargoWiseLoginHandler loginHandler;

	public WinzorCargoWiseLoginTask(IWinzorCargoWiseLoginHandler winzorCargoWiseLoginHandler)
	{
		loginHandler = winzorCargoWiseLoginHandler;
	}
	public int FailureExitCode => ExitCodes.WinzorCargoWiseLoginTaskError;
	public string TaskDescription => (NoResString)"Performing Login";
	public bool Execute(CommandLineArguments arguments) => loginHandler.Login();
	public bool ShouldExecute(CommandLineArguments arguments) => true;
}
