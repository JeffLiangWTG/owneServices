using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.NOReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Customstariffstructure;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tollsats;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Varenummer;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.LandGruppe;
using CargoWise.RefDbRepo.NOReferenceData.Services.RefCusConditions;

namespace CargoWise.RefDbRepo.NOReferenceData.CmdLine.Tariff
{
	static class TariffProgram
	{
		public static void Run()
		{
			var tollsatsListe = new vareListe();
			var raavareTollsatsListe = new vareListe();
			var tariffStructureEN = new CustomsTariffStructure();
			var innfoerselsavgifter = new AvgiftListe();
			var utfoerselsavgifter = new AvgiftListe();
			var vareNummer = new VarenummerListe();
			var landgruppe = new LandgruppeListe();
			var tariffStructureXmlEN = string.Empty;

			var tollsatsListeModified = DateTime.MinValue;
			var raavareTollsatsListeModified = DateTime.MinValue;
			var tariffStructureENModified = DateTime.MinValue;
			var innfoerselsavgifterModified = DateTime.MinValue;
			var utfoerselsavgifterModified = DateTime.MinValue;
			var vareNummerModified = DateTime.MinValue;
			var landgruppeModified = DateTime.MinValue;

			var tollsatsListeDownloadError = false;
			var raavareTollsatsListeDownloadError = false;
			var tariffStructureENDownloadError = false;
			var innfoerselsavgifterDownloadError = false;
			var utfoerselsavgifterDownloadError = false;
			var vareNummerDownloadError = false;
			var landgruppeDownloadError = false;

			Parallel.Invoke(
				() => (tollsatsListeDownloadError, tollsatsListeModified, tollsatsListe, _) = DownloadData.TryDownloadData<vareListe>("Tollsats", ApplicationConfig.ResourceTollsatsFilename, "CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.tollavgiftssats.xsd"),
				() => (raavareTollsatsListeDownloadError, raavareTollsatsListeModified, raavareTollsatsListe, _) = DownloadData.TryDownloadData<vareListe>("RaaVareTollsats", ApplicationConfig.ResourceRaavaretollsatsFilename, "CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.tollavgiftssats.xsd"),
				() => (tariffStructureENDownloadError, tariffStructureENModified, tariffStructureEN, tariffStructureXmlEN) = DownloadData.TryDownloadData<CustomsTariffStructure>("TariffStructure", ApplicationConfig.ResourceCustomstariffstructureFilename, "CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.customstariffstructure.xsd"),
				() => (innfoerselsavgifterDownloadError, innfoerselsavgifterModified, innfoerselsavgifter, _) = DownloadData.TryDownloadData<AvgiftListe>("Innfoerselsavgift", ApplicationConfig.ResourceInnfoerselsavgiftFilename, "CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.avgiftliste.xsd"),
				() => (utfoerselsavgifterDownloadError, utfoerselsavgifterModified, utfoerselsavgifter, _) = DownloadData.TryDownloadData<AvgiftListe>("Utfoerselsavgift", ApplicationConfig.ResourceUtfoerselsavgiftFilename, "CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.avgiftliste.xsd"),
				() => (vareNummerDownloadError, vareNummerModified, vareNummer, _) = DownloadData.TryDownloadData<VarenummerListe>("VarenummerListe", ApplicationConfig.ResourceVarenummerFilename, "CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.varenummer.xsd"),
				() => (landgruppeDownloadError, landgruppeModified, landgruppe, _) = DownloadData.TryDownloadData<LandgruppeListe>("LandGruppe", ApplicationConfig.ResourceLandgruppeFilename, "CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.landgruppe.xsd")
				);

			if (!tollsatsListeDownloadError && !raavareTollsatsListeDownloadError && !tariffStructureENDownloadError && !innfoerselsavgifterDownloadError && !utfoerselsavgifterDownloadError && !vareNummerDownloadError && !landgruppeDownloadError)
			{
				var refCusConditions = CusConditionCodes.GetCusConditionData();

				var lastModified = new[] { tollsatsListeModified, raavareTollsatsListeModified, tariffStructureENModified, innfoerselsavgifterModified, utfoerselsavgifterModified, vareNummerModified, landgruppeModified }.Max();
				Program.PrintErrorMessage(TariffParser.ConvertToXMLFile("NO Customs Tariff", tollsatsListe, raavareTollsatsListe, tariffStructureEN, innfoerselsavgifter, utfoerselsavgifter, vareNummer, landgruppe, refCusConditions, tariffStructureXmlEN,
					lastModified, Path.Combine(ApplicationConfig.OutputDirectory, "RefCusTariff_NO.xml")));
			}
		}
	}
}
