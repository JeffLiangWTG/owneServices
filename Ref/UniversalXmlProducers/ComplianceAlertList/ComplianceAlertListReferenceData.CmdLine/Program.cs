using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using CargoWise.RefDbRepo.ComplianceAlertListReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;

[assembly: InternalsVisibleTo("CargoWise.RefDbRepo.ComplianceAlertListReferenceData.Test")]
namespace CargoWise.RefDbRepo.ComplianceAlertListReferenceData.CmdLine
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml(args);
			return (int)ProducerStatus.Success;
		}

		internal static void ProduceXml(string[] args)
		{
			var outputPath = AppConfigurationProvider.AppConfiguration.OutputPath;
			var exportFilePath = Path.Combine(outputPath, "RefComplianceCommodityAlertList.xml");
			var refComplianceCommodityAlertList = GetRefComplianceCommodityAlertList();
			var parser = new ComplianceCommodityAlertListParser(refComplianceCommodityAlertList, exportFilePath, DateTime.UtcNow);
			parser.ExportXml();
		}

		internal static IEnumerable<ComplianceCountryAlertDetailsResponseModel> GetRefComplianceCommodityAlertList()
		{
			var baseUrl = AppConfigurationProvider.AppConfiguration.BWBaseUrl;
			var refComplianceCommodityAlertList = HttpBWHelper.GetRefComplianceCommodityAlertList(baseUrl);
			return refComplianceCommodityAlertList;
		}
	}
}
