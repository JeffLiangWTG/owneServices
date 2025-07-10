using System.Globalization;
using System.Net;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification
{
	public class Program
	{
		static void Main(string[] args)
		{
			var smtpServer = ApplicationConfig.SmtpServer;
			var username = ApplicationConfig.NetworkUsername;
			var password = ApplicationConfig.NetworkCredentialsPassword;
			NetworkCredential credentials = null;
			if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
			{
				credentials = new NetworkCredential
				{
					UserName = username,
					Password = password
				};
			}
			var smtpValue = ApplicationConfig.SmtpPort;
			var smtpPort = int.Parse(smtpValue, NumberStyles.Integer, CultureInfo.InvariantCulture);

			var processorStatusEmail = new ProcessorStatusEmail(string.Empty, smtpServer, credentials, smtpPort);
			var sourceDataEmail = new SourceDataEmail(string.Empty, smtpServer, credentials, smtpPort);

			using (var scanner = new Scanner(sourceDataEmail, processorStatusEmail, new StagingRepository(ApplicationConfig.ConnectionStrings)))
			{
				scanner.Run();
			}
		}
	}
}
