using System.ServiceProcess;

namespace CargoWise.eServices.Authentication.WindowsService
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] 
            { 
                new AuthenticationService() 
            };
			ServiceBase.Run(ServicesToRun);
		}
	}
}
