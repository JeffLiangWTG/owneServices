using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.RefDbRepo.CNReferenceData.Services;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public interface ISetting : IEChinaAPIConfig
	{
		string CIQOfficeCodeInputFile { get; }
		string DecTpAccessInputFile { get; }
		string ExcelSourceFileFolder { get; }
		string OutputFileFolderPath { get; }
		string OutputFolderForLog { get; }
		string OutputFolderForResponse { get; }
		string GetDataSource(string codeType);

		string ExchangeRateSourceURL { get; }
		IDictionary<string, string> CurrencyInChineseMap { get; }

		string SmtpClientHost { get; }
		string EmailAddress { get; }
		string EmailPassword { get; }
		string RecipientAddress { get; }
	}

	public class Setting : ISetting
	{
		public Setting(string jsonPath)
		{
			var configurationBuilder = new ConfigurationBuilder();
			config = configurationBuilder.AddJsonFile(jsonPath, true).Build();
		}
		IConfigurationRoot config;

		public string CIQOfficeCodeInputFile => config["CIQOfficeCodeInputFile"];
		public string DecTpAccessInputFile => config["DecTpAccessInputFile"];
		public string ExcelSourceFileFolder => config["TariffSourceFileFolder"];
		public string OutputFileFolderPath => config["OutputFolder"];
		public string OutputFolderForLog => config["OutputFolderForLog"];
		public string OutputFolderForResponse => config["OutputFolderForResponse"];

		public string GetDataSource(string codeType) => config[codeType];

		public string SmtpClientHost => config["SmtpClientHost"];
		public string EmailAddress => config["EmailAddress"];
		public string EmailPassword => config["EmailPassword"];
		public string RecipientAddress => config["RecipientAddress"];

		public string EChinaCheckForUpdateUrl => config["EChinaCheckForUpdateUrl"];
		public string EChinaAppCode => config["EChinaAppCode"];
		public string EChinaGetUpdatesUrl => config["EChinaGetUpdatesUrl"];

		public string ExchangeRateSourceURL => config["ExchangeRateSourceURL"];

		public IDictionary<string, string> CurrencyInChineseMap => config["Currencies"]
			.Split(',').Select(x => x.Split(':')).Where(x => x.Length == 2)
			.ToDictionary(x => x[1].Trim(), x => x[0].Trim()).ToImmutableDictionary();
	}
}
