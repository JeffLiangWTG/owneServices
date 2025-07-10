using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public class ApplicationConfig : IApplicationConfig
	{
		public static ApplicationConfig Instance => instance.Value;

		static readonly Lazy<ApplicationConfig> instance = new Lazy<ApplicationConfig>(() => new ApplicationConfig());

		const string JsonConfigFileName = "CargoWise.RefDbRepo.IEReferenceData.CmdLine.config.json";
		IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					config = configurationBuilder.AddJsonFile(JsonConfigFileName).Build();
				}
				return config;
			}
		}
		IConfiguration config;

		public string OutputDirectory => OutputPath;
		string OutputPath => Config[nameof(OutputPath)];
		public string ExchangeRatesStartURL => Config[nameof(ExchangeRatesStartURL)];
		public string ROSErrorsListUrl => Config[nameof(ROSErrorsListUrl)];
		public string BasePageAES => Config[nameof(BasePageAES)];
		public string CodeListTextAES => Config[nameof(CodeListTextAES)];
		public string RevenueBaseUrl => Config[nameof(RevenueBaseUrl)];
		public string BasePageAIS => Config[nameof(BasePageAIS)];
		public string CodeListTextAIS => Config[nameof(CodeListTextAIS)];
		public string BasePageNCTS => Config[nameof(BasePageNCTS)];
		public string CodeListTextNCTS => Config[nameof(CodeListTextNCTS)];

		public string AISCodeListUCC5 => Config[nameof(AISCodeListUCC5)];

		public string RefDbServiceURI => Config[nameof(RefDbServiceURI)];
		public bool IsRefDbServiceSecure => bool.Parse(Config[nameof(IsRefDbServiceSecure)]);

		public string ExciseDuty_OutputFileName => Config[nameof(ExciseDuty_OutputFileName)];
		public string RateType_OutputFileName => Config[nameof(RateType_OutputFileName)];
		public string ExciseDuty_Rate_Mapping => Config[nameof(ExciseDuty_Rate_Mapping)];
		public string ExciseDuty_PublishDateXpath => Config[nameof(ExciseDuty_PublishDateXpath)];
		public string ExciseDuty_Url_Mineral_Oil => Config[nameof(ExciseDuty_Url_Mineral_Oil)];
		public string ExciseDuty_Url_Alcohol_Products => Config[nameof(ExciseDuty_Url_Alcohol_Products)];
		public string ExciseDuty_Url_Tobacco_Products => Config[nameof(ExciseDuty_Url_Tobacco_Products)];
		public string[] ExciseDuty_EmailNotificationRecipients => Config[nameof(ExciseDuty_EmailNotificationRecipients)].Split(",");

		public string EmailSender => Config[nameof(EmailSender)];
		public string EmailSmtpServer => Config[nameof(EmailSmtpServer)];
		public string EmailCredentialsUserName => Config[nameof(EmailCredentialsUserName)];
		public string EmailCredentialsPassword => Config[nameof(EmailCredentialsPassword)];
		public int EmailSmtpPort => int.Parse(Config[nameof(EmailSmtpPort)], CultureInfo.InvariantCulture);
		public int EmailSendingTries => int.Parse(Config[nameof(EmailSendingTries)], CultureInfo.InvariantCulture);
		public int EmailSendingRetryDelaySeconds => int.Parse(Config[nameof(EmailSendingRetryDelaySeconds)], CultureInfo.InvariantCulture);
		public string DefaultHttpUserAgent => Config[nameof(DefaultHttpUserAgent)];
	}

	public interface IApplicationConfig
	{
		string OutputDirectory { get; }
		string ExchangeRatesStartURL { get; }
		string ROSErrorsListUrl { get; }
		string BasePageAES { get; }
		string CodeListTextAES { get; }
		string RevenueBaseUrl { get; }
		string BasePageAIS { get; }
		string CodeListTextAIS { get; }
		string BasePageNCTS { get; }
		string CodeListTextNCTS { get; }

		string RefDbServiceURI { get; }
		bool IsRefDbServiceSecure { get; }

		string ExciseDuty_OutputFileName { get; }
		string RateType_OutputFileName { get; }
		string ExciseDuty_Rate_Mapping { get; }
		string ExciseDuty_PublishDateXpath { get; }
		string ExciseDuty_Url_Mineral_Oil { get; }
		string ExciseDuty_Url_Alcohol_Products { get; }
		string ExciseDuty_Url_Tobacco_Products { get; }
		string[] ExciseDuty_EmailNotificationRecipients { get; }

		string DefaultHttpUserAgent { get; }
	}
}
