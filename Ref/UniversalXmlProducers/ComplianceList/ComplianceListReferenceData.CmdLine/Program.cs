using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.ComplianceListReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;

[assembly: InternalsVisibleTo("CargoWise.RefDbRepo.ComplianceListReferenceData.Test")]
namespace CargoWise.RefDbRepo.ComplianceListReferenceData.CmdLine
{
	class Program
	{
		static async Task<int> Main(string[] args)
		{
			using (var tokenProvider = new TokenProvider())
			{
				await ProduceXmlAsync(args, tokenProvider);
			}
			return (int)ProducerStatus.Success;
		}

		internal static async Task ProduceXmlAsync(string[] args, ITokenProvider tokenProvider)
		{
			var outputPath = AppConfigurationProvider.AppConfiguration.OutputPath;
			var exportFilePath = Path.Combine(outputPath, "RefComplianceList.xml");
			var refComplianceList = await GetRefComplianceListAsync(tokenProvider);
			var parser = new ComplianceListParser(refComplianceList, exportFilePath, DateTime.Today);
			parser.ExportXml();
		}

		internal static async Task<IEnumerable<RefComplianceListResponse>> GetRefComplianceListAsync(ITokenProvider tokenProvider)
		{
			var appConfig = AppConfigurationProvider.AppConfiguration;
			var baseUrl = appConfig.DpsWebServiceBaseUrl;
			var token = await tokenProvider.GetAuthorizationTokenAsync(appConfig.TenantId, appConfig.ClientId, appConfig.ServiceId, appConfig.PrivateKeyFileName, appConfig.CertificateFileName);
			var refComplianceList = HttpDpsWebHelper.GetRefComplianceList(baseUrl, token);
			return refComplianceList;
		}
	}
}
