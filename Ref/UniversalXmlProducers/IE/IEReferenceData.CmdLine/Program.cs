using System;
using System.Linq;
using System.Net.Http;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.CmdLine;
using CargoWise.RefDbRepo.IEReferenceData.ExchangeRates.CmdLine;
using CargoWise.RefDbRepo.IEReferenceData.ROSErrors.CmdLine;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tariffs.CmdLine;
using CargoWise.RefDbRepo.XmlProducer.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.IEReferenceData.CmdLine
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml(args);
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml(string[] args)
		{
			if (args.Length != 0)
			{
				var functionToRun = args[0].ToUpperInvariant();
				switch (functionToRun)
				{
					case Constants.ProgramFunctions.ExchangeRates:
						ExchangeRatesProgram.Run(ApplicationConfig.Instance.OutputDirectory);
						break;
					case Constants.ProgramFunctions.CodeLists:
						var codeListArgs = args.Skip(1).ToArray();
						(_, var validArgs) = CodeListProgram.ValidateArguments(codeListArgs);
						if (validArgs)
						{
							CodeListProgram.Run(
								codeListArgs,
								ApplicationConfig.Instance.OutputDirectory,
								new[] {
									(ApplicationType.AIS, GetUriToCodeList(ApplicationType.AIS)),
									(ApplicationType.AISUCC5, GetUriToCodeList(ApplicationType.AISUCC5)),
									(ApplicationType.AES, GetUriToCodeList(ApplicationType.AES)),
									(ApplicationType.NCTS, GetUriToCodeList(ApplicationType.NCTS))
								}
							);
						}
						break;
					case Constants.ProgramFunctions.ROSErrorsList:
						ROSErrorsProgram.Run(ApplicationConfig.Instance.OutputDirectory);
						break;
					case Constants.ProgramFunctions.ExciseTaxes:
						TariffsProgram.Run(ApplicationConfig.Instance, new Logger());
						break;
					default:
						throw new ArgumentException($"Invalid argument entered: {functionToRun}");
				}
			}
			else
			{
				throw new ArgumentException("No arguments entered.");
			}
		}

		static string GetUriToCodeList(ApplicationType applicationType)
		{
			string basePageUrl, linkTitle;
			switch (applicationType)
			{
				case ApplicationType.AES:
					basePageUrl = ApplicationConfig.Instance.BasePageAES;
					linkTitle = ApplicationConfig.Instance.CodeListTextAES.ToUpperInvariant();
					break;
				case ApplicationType.AIS:
					basePageUrl = ApplicationConfig.Instance.BasePageAIS;
					linkTitle = ApplicationConfig.Instance.CodeListTextAIS.ToUpperInvariant();
					break;
				case ApplicationType.AISUCC5:
					return ApplicationConfig.Instance.AISCodeListUCC5;
				case ApplicationType.NCTS:
					basePageUrl = ApplicationConfig.Instance.BasePageNCTS;
					linkTitle = ApplicationConfig.Instance.CodeListTextNCTS.ToUpperInvariant();
					break;
				default:
					throw new ArgumentException($"Unsupported application type: {applicationType}");
			}
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("User-Agent", ApplicationConfig.Instance.DefaultHttpUserAgent);
				var uri = new Uri(basePageUrl);
				var pageContent = client.GetAsync(uri).Result;
				var htmlContent = pageContent.Content.ReadAsStringAsync().Result;
				var htmlDoc = new HtmlDocument();
				htmlDoc.LoadHtml(htmlContent);
				var linkNode = htmlDoc.DocumentNode.SelectSingleNode($"//a[translate(@title, 'abcdefghijklmnopqrstuvwxyz', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ')='{linkTitle}']");
				if (linkNode != null)
				{
					var codeListLink = linkNode.GetAttributeValue("href", string.Empty);
					if (!codeListLink.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
					{
						codeListLink = ApplicationConfig.Instance.RevenueBaseUrl + codeListLink;
					}
					return codeListLink;
				}
				return null;
			}
		}
	}
}
