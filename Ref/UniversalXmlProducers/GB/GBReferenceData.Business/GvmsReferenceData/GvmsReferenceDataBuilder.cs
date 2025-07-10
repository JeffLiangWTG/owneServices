using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData;
using CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Business;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.GvmsReferenceData
{
	public class GvmsReferenceDataBuilder : IRefXmlBuilder
	{
		public void BuildXml(string outputPath, ReferenceData referenceData, DateTime publicationDate)
		{
			GenerateRefCarrierCodeXML(referenceData, outputPath, publicationDate);
			GenerateRefCusCodeListRoutesAndErrorsXML(referenceData, outputPath, publicationDate);
			GenerateRefCusCodeListPortsXML(referenceData, outputPath, publicationDate);
			GenerateRefLocoMapXML(referenceData, outputPath, publicationDate);
			GenerateRefCusCodeListInspectionLocationsXML(referenceData, outputPath, publicationDate);
			GenerateRefCusCodeListInspectionTypesXML(referenceData, outputPath, publicationDate);
		}

		static void GenerateRefCarrierCodeXML(ReferenceData referenceData, string outputPath, DateTime publicationDate)
		{
			var refCarrierCodes = referenceData.Carriers.Select(carrier => GvmsReferenceDataConverter.ConvertCarrier(carrier)).ToList();
			Helper.ExportToXMLFile(Constants.GvmsDefaults.XmlWriterDataSourceGvmsCarrier, Path.Combine(outputPath, $"GVMSRefCarrierCode_{publicationDate:yyyyMMdd}.xml"), Helper.GetRefCarrierCodeWriterConfiguration("GB"), publicationDate, UpdateType.Partial, refCarrierCodes);
		}

		static void GenerateRefCusCodeListRoutesAndErrorsXML(ReferenceData referenceData, string outputPath, DateTime publicationDate)
		{
			var refCusCodeLists = referenceData.Routes.Select(route => GvmsReferenceDataConverter.ConvertRoute(route, referenceData.Ports, referenceData.Carriers)).ToList();
			refCusCodeLists.AddRange(referenceData.RuleFailures.Select(ruleFailure => GvmsReferenceDataConverter.ConvertRuleFailure(ruleFailure)));
			Helper.ExportToXMLFile(Constants.GvmsDefaults.XmlWriterDataSourceGvmsCusCodeRoutesAndErrors, Path.Combine(outputPath, $"GVMSRefCusCodeListRoutesAndErrors_{publicationDate:yyyyMMdd}.xml"), Helper.GetRefCusCodeListWriterConfigurationWithAttributes("GB", defaultDataGrouping: false), publicationDate, UpdateType.Partial, refCusCodeLists);
		}

		static void GenerateRefCusCodeListPortsXML(ReferenceData referenceData, string outputPath, DateTime publicationDate)
		{
			var refCusCodeLists = referenceData.Ports.Where(port => GvmsReferenceDataConverter.IsCdsPort(port)).Select(port => GvmsReferenceDataConverter.ConvertCDSPort(port)).ToList();
			refCusCodeLists.AddRange(referenceData.Ports.Where(port => GvmsReferenceDataConverter.IsChiefPort(port)).Select(port => GvmsReferenceDataConverter.ConvertChiefPort(port)));
			Helper.ExportToXMLFile(Constants.GvmsDefaults.XmlWriterDataSourceGvmsCusCodePorts, Path.Combine(outputPath, $"GVMSRefCusCodeListPorts_{publicationDate:yyyyMMdd}.xml"), Helper.GetRefCusCodeListWriterConfigurationWithAttributes("GB", defaultDataGrouping: false, isPort: true), publicationDate, UpdateType.Partial, refCusCodeLists);
		}

		static void GenerateRefLocoMapXML(ReferenceData referenceData, string outputPath, DateTime publicationDate)
		{
			List<RefDataRepoModelEntityType> refLocoMaps = new List<RefDataRepoModelEntityType>();
			var ports = referenceData.Ports.Where(port => GvmsReferenceDataConverter.IsForeignPort(port));
			foreach (var port in ports)
			{
				refLocoMaps.AddRange(GvmsReferenceDataConverter.ConvertForeignPort(port));
			}
			Helper.ExportToXMLFile(Constants.GvmsDefaults.XmlWriterDataSourceGvmsLocoMap, Path.Combine(outputPath, $"GVMSRefLocoMap_{publicationDate:yyyyMMdd}.xml"), Helper.GetRefLocoMapWriterConfiguration(Constants.GvmsDefaults.LocoMap.LocalCountryGB, Constants.GvmsDefaults.LocoMap.GvmsSystemUsage), publicationDate, UpdateType.Partial, refLocoMaps);
		}

		static void GenerateRefCusCodeListInspectionLocationsXML(ReferenceData referenceData, string outputPath, DateTime publicationDate)
		{
			var refInspectionLocations = referenceData.InspectionLocations.Select(location => GvmsReferenceDataConverter.ConvertInspectionLocation(location)).ToList();
			Helper.ExportToXMLFile(Constants.GvmsDefaults.XmlWriterDataSourceGvmsInspectionLocations, Path.Combine(outputPath, $"GVMSRefCusCodeListInspectionLocations_{publicationDate:yyyyMMdd}.xml"), Helper.GetRefCusCodeListWriterConfigurationWithAttributes(Constants.DefaultValues.GBDataGrouping), publicationDate, UpdateType.Partial, refInspectionLocations);
		}

		static void GenerateRefCusCodeListInspectionTypesXML(ReferenceData referenceData, string outputPath, DateTime publicationDate)
		{
			var refInspectionTypes = referenceData.InspectionTypes.Select(inspectionType => GvmsReferenceDataConverter.ConvertInspectionType(inspectionType)).ToList();
			Helper.ExportToXMLFile(Constants.GvmsDefaults.XmlWriterDataSourceGvmsInspectionTypes, Path.Combine(outputPath, $"GVMSRefCusCodeListInspectionTypes_{publicationDate:yyyyMMdd}.xml"), Helper.GetRefCusCodeListWriterConfigurationWithAttributes(Constants.DefaultValues.GBDataGrouping), publicationDate, UpdateType.Partial, refInspectionTypes);
		}
	}
}
