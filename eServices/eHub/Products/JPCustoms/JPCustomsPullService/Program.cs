using System.ServiceProcess;
using CargoWise.eHub.Products.JPCustoms.Client;
using CargoWise.eHub.Products.JPCustoms.Common;
using CargoWise.eHub.Products.JPCustoms.PullService.Common;
using CargoWise.eHub.Shared.IssueManager;
using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.PullService
{
	static class Program
	{
		static void Main()
		{
			var logger = LogManager.GetLogger(typeof(Program));

			IAuditLoggerConfiguration auditLoggerConfiguration = new AuditLoggerConfiguration(new DateTimeProvider());
			var auditLogger = new AuditLogger(auditLoggerConfiguration);

			IPullRunnerConfiguration pullRunnerConfiguration = new PullRunnerConfiguration();

			IPop3MailClientConfiguration mailClientConfiguration = new Pop3MailClientConfiguration(logger);

			IJPCustomsPullClient pullClient = new JPCustomsPullClient(mailClientConfiguration);
			
			IEHubClientConfiguration ehubConfiguration = new EHubClientConfiguration();
			IEHubClient eHubClient = new EHubClient(ehubConfiguration);
			var issueManager= new IssueManager();
			IJPCustomsPullManager manager = new JPCustomsPullManager(logger, auditLogger, pullClient, eHubClient, issueManager);

			var servicesToRun = new ServiceBase[] 
			{ 
				new Service(logger, pullRunnerConfiguration, manager) 
			};

#if DEBUG
			logger.Info("Run JPCustoms PullService as Console application.");
			new JPCustomsPullRunner(logger, pullRunnerConfiguration, manager).Start();
			System.Threading.Thread.Sleep(10000000);
#else 
			logger.Info("Run JPCustoms PullService as Windows service.");
			ServiceBase.Run(servicesToRun);
#endif
		}

	}
}
