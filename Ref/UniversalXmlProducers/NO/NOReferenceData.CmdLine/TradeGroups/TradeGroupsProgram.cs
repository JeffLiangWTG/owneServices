using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.NOReferenceData.Business.TradeGroups;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.LandGruppe;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.MedlemsLand;

namespace CargoWise.RefDbRepo.NOReferenceData.CmdLine.TradeGroups
{
	static class TradeGroupsProgram
	{
		internal static void Run()
		{
			var landgruppeListe = new LandgruppeListe();
			var medlemLandListe = new MedlemLandListe();
			var landgruppeLastModified = string.Empty;
			var medlemLandLastModified = string.Empty;
			var landgruppeDownloadError = false;
			var medlemLandDownloadError = false;

			Parallel.Invoke(
				() =>
				{
					var client = new HttpClient();
					var downloadUrl = new Uri(ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceLandgruppeFilename);
					var downloadContent = DownloadContent.Download<LandgruppeListe>(client, downloadUrl, "CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.landgruppe.xsd");
					if (downloadContent.Errors.Length == 0)
					{
						landgruppeListe = downloadContent.XmlData;
						landgruppeLastModified = downloadContent.ResourceData.LastModified;
					}
					landgruppeDownloadError = Program.PrintErrorMessage("Error downloading LandgruppeListe: ", downloadContent.Errors);
				},
				() =>
				{
					var client = new HttpClient();
					var downloadUrl = new Uri(ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceMedlemslandFilename);
					var downloadContent = DownloadContent.Download<MedlemLandListe>(client, downloadUrl, "CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.medlemsland.xsd");
					if (downloadContent.Errors.Length == 0)
					{
						medlemLandListe = downloadContent.XmlData;
						medlemLandLastModified = downloadContent.ResourceData.LastModified;
					}
					medlemLandDownloadError = Program.PrintErrorMessage("Error downloading MedlemLandListe: ", downloadContent.Errors);
				});

			if (!landgruppeDownloadError && !medlemLandDownloadError)
			{
				Program.PrintErrorMessage(TradeGroupsParser.ConvertToXMLFile(landgruppeListe, medlemLandListe, landgruppeLastModified, medlemLandLastModified, Path.Combine(ApplicationConfig.OutputDirectory, "RefCusTradeGroup_NO.xml")));
			}
		}
	}
}
