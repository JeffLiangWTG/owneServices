using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public sealed class ApplicationConfig
	{
		public static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile(JsonConfigFileName)
						.Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		public static ApplicationConfig Instance => instance.Value;
		static readonly Lazy<ApplicationConfig> instance = new Lazy<ApplicationConfig>(() => new ApplicationConfig());


		public static void ReLoad()
		{
			Instance.RecieverID = Convert.ToInt32(Configuration["RecieverID"], ilCultureInfo);
			Instance.SenderID = Convert.ToInt32(Configuration["SenderID"], ilCultureInfo);

			Instance.ConsumerId = Configuration["ConsumerId"];
			Instance.MsgClientConnect = Configuration["MsgClient.Connect"];
			Instance.MsgClientCaBundle = Configuration["MsgClient.CaBundle"];
			Instance.MsgClientURI = Configuration["MsgClient.URI"];
			Instance.MsgClientApplicationIdentification = Configuration["MsgClient.ApplicationIdentification"];
			Instance.MsgClientTimeoutInSeconds = Convert.ToInt32(Configuration["MsgClient.TimeoutInSeconds"], ilCultureInfo);
			Instance.OutputDirectory = Configuration["OutputDirectory"];
			Instance.DownloadsDirectory = Configuration["DownloadsDirectory"];
		}

		ApplicationConfig()
		{
			RecieverID = Convert.ToInt32(Configuration["RecieverID"], ilCultureInfo);
			SenderID = Convert.ToInt32(Configuration["SenderID"], ilCultureInfo);

			ConsumerId = Configuration["ConsumerId"];
			MsgClientConnect = Configuration["MsgClient.Connect"];
			MsgClientCaBundle = Configuration["MsgClient.CaBundle"];
			MsgClientURI = Configuration["MsgClient.URI"];
			MsgClientApplicationIdentification = Configuration["MsgClient.ApplicationIdentification"];
			MsgClientTimeoutInSeconds = Convert.ToInt32(Configuration["MsgClient.TimeoutInSeconds"], ilCultureInfo);
			OutputDirectory = Configuration["OutputDirectory"];
			DownloadsDirectory = Configuration["DownloadsDirectory"];
		}

		public int RecieverID { get; set; }
		public int SenderID { get; set; }
		public string ConsumerId { get; set; }
		public string MsgClientConnect { get; set; }
		public string MsgClientCaBundle { get; set; }
		public string MsgClientURI { get; set; }
		public string MsgClientApplicationIdentification { get; set; }
		public int MsgClientTimeoutInSeconds { get; set; }
		public string OutputDirectory { get; set; }
		public string DownloadsDirectory { get; set; }

		static CultureInfo ilCultureInfo => CultureInfo.GetCultureInfo("he-IL");

		const string JsonConfigFileName = "CargoWise.RefDbRepo.ILReferenceData.CmdLine.config.json";
	}
}
