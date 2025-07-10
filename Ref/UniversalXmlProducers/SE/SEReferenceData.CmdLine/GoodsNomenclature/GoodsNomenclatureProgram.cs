using System.IO;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.SEReferenceData.CmdLine
{
	public sealed class GoodsNomenclatureProgram
	{
		public static void Run(string outputPath, DownloadExportXml downloader)
		{
			ProcessNomenclature<goodsNomenclature>(outputPath, downloader, "GoodsNomenclature");
			ProcessNomenclature<declarableGoodsNomenclature>(outputPath, downloader, "DeclarableGoodsNomenclature");
			ProcessNomenclature<goodsNomenclatureGroup>(outputPath, downloader, "GoodsNomenclatureGroup");
		}

		static void ProcessNomenclature<TXmlItem>(string outputPath, DownloadExportXml downloader, string objectName) where TXmlItem : class
		{
			var goodsNomenclatureRepositoryUrl = ApplicationConfig.CompleteMonthlyRepositoryUrl;
			var latestNomenclatureFile = downloader.FindLatestFile(goodsNomenclatureRepositoryUrl, objectName);
			if (!string.IsNullOrEmpty(latestNomenclatureFile))
			{
				var url = Path.Combine(goodsNomenclatureRepositoryUrl, latestNomenclatureFile);
				var nomenclatureItems = downloader.DownloadLatestFullAndExtract<TXmlItem>(url, Path.GetTempPath());
				//var dateTimeProvider = new DateTimeProvider();
				foreach (var nomenclatureItem in nomenclatureItems)
				{
					// TODO: the parsing step
					//var outputFile = Path.Combine(outputPath, $"Ref{objectName}ZZ_SE_{nomenclatureItem.SID}.xml");
					//Program.PrintErrorMessage(new Parser(nomenclatureItem).ConvertToXMLFile(outputFile, dateTimeProvider));
				}
			}
			else
			{
				Program.PrintErrorMessage($"Unable to find any full files for the {objectName} going back two months.");
			}
		}
	}
}
