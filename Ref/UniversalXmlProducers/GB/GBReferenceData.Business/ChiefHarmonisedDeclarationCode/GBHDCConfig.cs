using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.GBHarmonisedDeclarationCode
{
	public static class GBHDCConfig
	{
		public static string DataSource = @"GB Harmonised Declaration Code";
		public static string URLRegex = @"\<a.*href=""(.*certificate_authorisation_codes_for_harmonised_declarations.*ods)";
		public static DateTime PublicationTime { get; private set; }
		public static string DownloadSourceURL { get; private set; }
		public static string DownloadOutputPath { get; private set; }
		public static string OutputFilePrefix { get; private set; }
		public static string ZipOutputFileName { get; private set; }
		public static string XMLOutputFileName { get; private set; }

		public static void ConfigSetup()
		{
			DownloadSourceURL = ConfigurationManager.AppSettings["DownloadSourceURL"];
			DownloadOutputPath = Path.Combine(Assembly.GetExecutingAssembly().Location, ConfigurationManager.AppSettings["OutputPath"]);
			OutputFilePrefix = ConfigurationManager.AppSettings["OutputFileNamePrefix"];
			ZipOutputFileName = Path.Combine(Assembly.GetExecutingAssembly().Location, Path.Combine(DownloadOutputPath, OutputFilePrefix + ".zip"));
			XMLOutputFileName = Path.Combine(Assembly.GetExecutingAssembly().Location, Path.Combine(DownloadOutputPath, OutputFilePrefix + ".xml"));
		}

		public static void SetPublicationTime(DateTime publicationTime)
		{
			PublicationTime = publicationTime;
		}
	}
}
