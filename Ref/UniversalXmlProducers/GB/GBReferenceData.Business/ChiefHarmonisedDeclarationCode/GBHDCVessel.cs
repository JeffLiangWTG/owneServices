using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.GBReferenceData.Services.GBHarmonisedDeclarationCode;
using CargoWise.RefDbRepo.GBReferenceData.Services.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.GBHarmonisedDeclarationCode
{
	public class GBHDCVessel
	{
		IEnumerable<GBHDCData> hdcList;
		IXmlWriter hdcXMLWriter;

		public IXmlWriter CreateXMLWriter()
		{
			var writer = new XmlWriter(Helper.GetRefCusCodeListWriterConfigurationWithAttributes("GB"));
			writer.SetDataSource(GBHDCConfig.DataSource);
			writer.SetPublicationTime(GBHDCConfig.PublicationTime);
			writer.SetUpdateType(UpdateType.Full);
			return writer;
		}

		public void Process()
		{
			GBHDCDownloader.Download(GBHDCConfig.DownloadSourceURL, GBHDCConfig.URLRegex, GBHDCConfig.DownloadOutputPath, GBHDCConfig.ZipOutputFileName);
			CheckDownload(GBHDCConfig.ZipOutputFileName);
			hdcList = GBHDCExtractor.Extract(GBHDCConfig.ZipOutputFileName);
			CheckExtract(hdcList);
			GBHDCConfig.SetPublicationTime(GBHDCDownloader.GetPublicationTime(GBHDCConfig.DownloadSourceURL));
			hdcXMLWriter = CreateXMLWriter();
			GBHDCXMLGenerator.Generate(hdcList, hdcXMLWriter, GBHDCConfig.XMLOutputFileName, GBHDCConfig.ZipOutputFileName);
		}

		public void CheckDownload(string zipOutputFileName)
		{
			if (File.Exists(zipOutputFileName))
			{
				var info = new FileInfo(zipOutputFileName);
				if (info.Length == 0)
				{
					throw new ApplicationException("Downloaded File is Empty!");
				}
			}
			else
			{
				throw new ApplicationException("No Downloaded File!");
			}
		}
		public void CheckExtract(IEnumerable<GBHDCData> list)
		{
			if (list == null || ((List<GBHDCData>)list).Count == 0)
			{
				throw new ApplicationException("No Data was Extracted!");
			}
		}
	}
}
