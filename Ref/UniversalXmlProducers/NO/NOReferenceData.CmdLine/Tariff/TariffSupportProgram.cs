using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.NOReferenceData.Business.Nomenclature;
using CargoWise.RefDbRepo.NOReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Customstariffstructure;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tolltariffstruktur;
using CargoWise.RefDbRepo.NOReferenceData.Services.RefCusConditions;
using CargoWise.RefDbRepo.NOReferenceData.Business.RefCusConditions;

namespace CargoWise.RefDbRepo.NOReferenceData.CmdLine.Tariff
{
	static class TariffSupportProgram
	{
		public static void Run()
		{
			var innfoerselsavgifter = new AvgiftListe();
			var utfoerselsavgifter = new AvgiftListe();
			var tariffStructureXmlNO = string.Empty;
			var tariffStructureXmlEN = string.Empty;

			var tariffStructureENModified = DateTime.MinValue;
			var tariffStructureNOModified = DateTime.MinValue;
			var innfoerselsavgifterModified = DateTime.MinValue;
			var utfoerselsavgifterModified = DateTime.MinValue;

			var tariffStructureENDownloadError = false;
			var tariffStructureNODownloadError = false;
			var innfoerselsavgifterDownloadError = false;
			var utfoerselsavgifterDownloadError = false;

			Parallel.Invoke(
				() => (tariffStructureENDownloadError, tariffStructureENModified, _, tariffStructureXmlEN) = DownloadData.TryDownloadData<CustomsTariffStructure>("TariffStructure", ApplicationConfig.ResourceCustomstariffstructureFilename, "CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.customstariffstructure.xsd"),
				() => (tariffStructureNODownloadError, tariffStructureNOModified, _, tariffStructureXmlNO) = DownloadData.TryDownloadData<TolltariffStruktur>("TariffStructure", ApplicationConfig.ResourceTolltariffStrukturFilename, "CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.tolltariffstruktur.xsd"),
				() => (innfoerselsavgifterDownloadError, innfoerselsavgifterModified, innfoerselsavgifter, _) = DownloadData.TryDownloadData<AvgiftListe>("Innfoerselsavgift", ApplicationConfig.ResourceInnfoerselsavgiftFilename, "CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.avgiftliste.xsd"),
				() => (utfoerselsavgifterDownloadError, utfoerselsavgifterModified, utfoerselsavgifter, _) = DownloadData.TryDownloadData<AvgiftListe>("Utfoerselsavgift", ApplicationConfig.ResourceUtfoerselsavgiftFilename, "CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.avgiftliste.xsd")
				);

			if (!tariffStructureENDownloadError && !tariffStructureNODownloadError && !innfoerselsavgifterDownloadError && !utfoerselsavgifterDownloadError)
			{
				var refCusConditions = CusConditionCodes.GetCusConditionData();

				Program.PrintErrorMessage(RefCusConditionTypeParser.ConvertToXMLFile("NO RefCusConditionType", refCusConditions, innfoerselsavgifter,
					innfoerselsavgifterModified, Path.Combine(ApplicationConfig.OutputDirectory, "RefCusConditionType_NO.xml")));

				var tariffStructureModified = new[] { tariffStructureENModified, tariffStructureNOModified }.Max();
				Program.PrintErrorMessage(NomenclatureParser.ConvertToXMLFile("NO Customs Nomenclature", tariffStructureXmlEN, tariffStructureXmlNO,
					tariffStructureModified, Path.Combine(ApplicationConfig.OutputDirectory, "Nomenclature_NO.xml")));

				var avgifterModified = new[] { innfoerselsavgifterModified, utfoerselsavgifterModified }.Max();
				Program.PrintErrorMessage(TariffCodeParser.ConvertToXMLFile("NO RateCodes", innfoerselsavgifter, utfoerselsavgifter,
					avgifterModified, Path.Combine(ApplicationConfig.OutputDirectory, "TariffRateCodes_NO.xml")));
			}
		}
	}
}
