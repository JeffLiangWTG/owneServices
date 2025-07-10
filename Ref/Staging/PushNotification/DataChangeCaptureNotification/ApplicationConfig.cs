using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification
{
	public static class ApplicationConfig
	{
		public static string Email => Config["email"];
		public static string SmtpServer => Config["smtpServer"];
		public static string NetworkUsername => Config["networkUsername"];
		public static string NetworkCredentialsPassword => Config["networkCredentialsPassword"];
		public static string SmtpPort => Config["smtpPort"];
		public static string System => Config["system"];
		public static string ConnectionStrings => Config["ConnectionStrings:RefDbRepoStaging"];

		static IConfiguration Config =>
			new ConfigurationBuilder().AddJsonFile("CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification.config.json").Build();
	}
}
