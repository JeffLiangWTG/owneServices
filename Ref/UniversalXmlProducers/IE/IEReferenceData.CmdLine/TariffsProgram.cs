using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tariffs.Business;

namespace CargoWise.RefDbRepo.IEReferenceData.Tariffs.CmdLine
{
	public static class TariffsProgram
	{
		public static void Run(IApplicationConfig config, ILoggerWithErrors logger)
		{
			DownloadExciseDutyRates(config, logger);
		}

		static void DownloadExciseDutyRates(IApplicationConfig config, ILoggerWithErrors logger)
		{
			ExciseDutyRateProducer.ExtractAndWriteToXml(config, logger);

			if (logger.HasErrors)
			{
				const string subject = "Excise Duty Rate: Unrecognizable item detected";
				var messageBody = $@"{subject} in the revenue page.
Please check the following information and update the Excise Duty Rate mapping csv accordingly:
{logger.GetErrors()}";

				var emailMessage = new EmailMessage
				{
					Subject = subject,
					Body = messageBody,
					To = config.ExciseDuty_EmailNotificationRecipients
				};

				EmailNotificationHelper.SendWithRetries(emailMessage).GetAwaiter().GetResult();
			}
		}
	}
}
