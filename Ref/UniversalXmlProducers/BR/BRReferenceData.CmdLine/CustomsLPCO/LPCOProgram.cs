using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class LPCOProgram : BaseProgram
	{
		protected override void RunCore()
		{
			var models = DownloadSource();
			models.ForEach(model =>
			{
				byte[] bytes = Encoding.UTF8.GetBytes(model.content);
				File.WriteAllBytes(GetOutputFilePath(model.codigo + ".json"), bytes);
				Console.WriteLine($"Json model {model.codigo} exported.");
			});
		}

		static List<ModelDTO> DownloadSource()
		{
			using (var handler = new HttpClientHandler
			{
				ClientCertificateOptions = ClientCertificateOption.Manual,
				SslProtocols = SslProtocols.None
			})
			{
				handler.ClientCertificates.Add(new X509Certificate2(CertificateLoader.LoadCertificate(), ConfigurationProvider.Configuration.GetSection("PORTAL_UNICO_CERTIFICATE_PASSWORD").Value));
				using (var client = HttpClientUtils.New(handler))
				{
					var token = PortalUnicoAuthenticator.DoAuthentication(client, false);
					var dtos = LPCOConsentingBodyDownloader.DownloadModelsFromConsentingBody(client, token);
					dtos.ForEach(dto => dto.content = LPCODownloader.DownloadModel(dto.codigo, client, token));
					return dtos;
				}
			}
		}
	}
}
