using System;
using System.Collections;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using Common.Logging;

namespace CargoWise.eHub.Nudge
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			if (Environment.UserInteractive)
			{
				if (args.Length == 1)
				{
					switch (args[0])
					{
						case "-install":
							ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
							break;
						case "-uninstall":
							ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
							break;
					}
				}
				else
				{
					Logger.Info("executing program.Main without any arguments.");
				}
			}
			else
			{
				ServiceBase[] servicesToRun = new ServiceBase[] 
					{ 
						new NudgeService() 
					};
				ServiceBase.Run(servicesToRun);
			}
		}

		static readonly ILog Logger = LogManager.GetLogger(typeof(Program));
	}
}
