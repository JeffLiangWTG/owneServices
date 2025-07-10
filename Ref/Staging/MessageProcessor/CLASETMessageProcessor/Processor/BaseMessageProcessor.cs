using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Staging.Schema_New;
using RefCusCodeList = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusCodeList;
using RefCusCodeListAttribute = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusCodeListAttribute;
using RefCusProcedure = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusProcedure;
using RefCusProcedureAttribute = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusProcedureAttribute;
using RefCusTariff = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusTariff;
using RefCusTariffAttribute = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusTariffAttribute;
using RefCusTariffUOM = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusTariffUOM;

namespace CargoWise.RefDbRepo.Staging.CLASETMessageProcessor
{
	public abstract class BaseMessageProcessor
	{
		protected BaseMessageProcessor(string outputPath)
		{
			this.outputPath = outputPath;

			ProcedureList = new List<RefCusProcedure>();
			CommodityList = new List<RefCusTariff>();
			FacilityList = new List<RefCusCodeList>();
			PortList = new List<RefCusCodeList>();
			TotalProcessed = 0;
		}

		readonly string outputPath;

		public int Process(SourceData sourceData, string messageText)
		{
			ProcedureList.Clear();
			CommodityList.Clear();
			FacilityList.Clear();
			PortList.Clear();

			TotalProcessed = 0;

			return ProcessCore(sourceData, messageText);
		}

		protected abstract int ProcessCore(SourceData sourceData, string messageText);

		#region XmlWriterConfiguration

		static protected XmlWriterConfiguration GetCommodityWriterConfiguration()
		{
			(var writerConfiguration, var tariffCfg) = Helper.GetRefCusTariffWriterConfiguration(Constants.RefCusTariffTypes.Commodity, DataSourceConstants.Country.Singapore);
			Helper.AddRefCusTariffAttributeConfiguration(writerConfiguration, tariffCfg);
			Helper.AddRefCusTariffRelationshipConfiguration(writerConfiguration, tariffCfg, Constants.RefCusTariffTypes.HarmonizedCode, DataSourceConstants.Country.Singapore);
			return writerConfiguration;
		}

		static protected XmlWriterConfiguration GetPortWriterConfiguration()
		{
			return Helper.GetRefCusCodeListWriterConfiguration(Constants.RefCusCodeListTypes.Port, DataSourceConstants.Country.Singapore).writerConfiguration;
		}

		static protected XmlWriterConfiguration GetFacilityWriterConfiguration()
		{
			(var writerConfiguration, var codeListConfiguration) = Helper.GetRefCusCodeListWriterConfiguration(Constants.RefCusCodeListTypes.Facility, DataSourceConstants.Country.Singapore);
			Helper.AddRefCusCodeListAttributeConfiguration(writerConfiguration, codeListConfiguration);
			return writerConfiguration;
		}

		static protected XmlWriterConfiguration GetProcedureWriterConfiguration()
		{
			(var writerConfiguration, var procedureConfiguration) = Helper.GetRefCusProcedureWriterConfiguration(DataSourceConstants.Country.Singapore);
			Helper.AddRefCusProcedureAttributeConfiguration(writerConfiguration, procedureConfiguration);

			return writerConfiguration;
		}

		protected void ExportToXMLFile()
		{
			ExportToXMLFileCore("Procedure Codes", GetProcedureWriterConfiguration(), RefDbRepo.Common.UniversalXmlWriter.UpdateType.Partial, ProcedureList);
			ExportToXMLFileCore("Facility Codes", GetFacilityWriterConfiguration(), RefDbRepo.Common.UniversalXmlWriter.UpdateType.Partial, FacilityList);
			ExportToXMLFileCore("Port Codes", GetPortWriterConfiguration(), RefDbRepo.Common.UniversalXmlWriter.UpdateType.Partial, PortList);
			ExportToXMLFileCore("Commodity Codes", GetCommodityWriterConfiguration(), RefDbRepo.Common.UniversalXmlWriter.UpdateType.Partial, CommodityList);
		}

		void ExportToXMLFileCore<T>(string sourceType, XmlWriterConfiguration xmlWriterConfig, UpdateType updateType, IEnumerable<T> dataList) where T : RefDataRepoModelEntityType
		{
			if (dataList != null && dataList.Any())
			{
				var fileName = Path.Combine(outputPath, $"{typeof(T).Name}ZZ_{DataSourceConstants.Country.Singapore}_{sourceType}_{ReleaseNumber}_{ReleaseDate.ToString(DateFormat, CultureInfo.InvariantCulture)}_{DateTime.UtcNow.Ticks}.xml");
				Helper.ExportToXMLFile($"{DataSourceConstants.Country.Singapore} CLASET {sourceType}", fileName, xmlWriterConfig, ReleaseDate, updateType, dataList);
			}
		}

		#endregion

		#region Mapping

		static protected string GetShipmentCode(string commonAccessReference)
		{
			switch (commonAccessReference)
			{
				case Constants.InPayment.CommonAccessReference:
					return Constants.InPayment.ShipmentCode;
				case Constants.InNonPayment.CommonAccessReference:
					return Constants.InNonPayment.ShipmentCode;
				case Constants.Out.CommonAccessReference.WithoutCertificateOfOrigin:
					return Constants.Out.ShipmentCode;
				case Constants.Out.CommonAccessReference.WithCertificateOfOrigin:
					return Constants.Out.ShipmentCode;
				case Constants.TranshipmentMovement.CommonAccessReference:
					return Constants.TranshipmentMovement.ShipmentCode;
				default:
					return commonAccessReference;
			}
		}

		static protected string GetGroupCode(string shipmentCode, string declarationTypeReference)
		{
			switch (shipmentCode)
			{
				case Constants.InPayment.ShipmentCode:
					return GetInPaymentGroupCode(declarationTypeReference);
				case Constants.InNonPayment.ShipmentCode:
					return GetInNonPaymentGroupCode(declarationTypeReference);
				case Constants.Out.ShipmentCode:
					return GetOutGroupCode(declarationTypeReference);
				case Constants.TranshipmentMovement.ShipmentCode:
					return GetTranshipmentMovementGroupCode(declarationTypeReference);
				default:
					return declarationTypeReference;
			}
		}

		static protected string GetInPaymentGroupCode(string declarationTypeReference)
		{
			switch (declarationTypeReference)
			{
				case Constants.InPayment.DeclarationTypeRefs.GstIncludingDutyExemption:
					return Constants.DeclarationTypeCodes.GstIncludingDutyExemption;
				case Constants.InPayment.DeclarationTypeRefs.Duty:
					return Constants.DeclarationTypeCodes.Duty;
				case Constants.InPayment.DeclarationTypeRefs.DutyAndGst:
					return Constants.DeclarationTypeCodes.DutyAndGst;
				case Constants.InPayment.DeclarationTypeRefs.BlanketIncludingBlanketGstPaymentAndDutyExemption:
					return Constants.DeclarationTypeCodes.BlanketIncludingBlanketGstPaymentAndDutyExemption;
				default:
					return declarationTypeReference;
			}
		}

		static protected string GetInNonPaymentGroupCode(string declarationTypeReference)
		{
			switch (declarationTypeReference)
			{
				case Constants.InNonPayment.DeclarationTypeRefs.ApprovedPremisesSchemes:
					return Constants.DeclarationTypeCodes.ApprovedPremisesSchemes;
				case Constants.InNonPayment.DeclarationTypeRefs.GstReliefAndDutyExemption:
					return Constants.DeclarationTypeCodes.GstReliefAndDutyExemption;
				case Constants.InNonPayment.DeclarationTypeRefs.ShutOut:
					return Constants.DeclarationTypeCodes.ShutOut;
				case Constants.InNonPayment.DeclarationTypeRefs.Destruction:
					return Constants.DeclarationTypeCodes.Destruction;
				case Constants.InNonPayment.DeclarationTypeRefs.ForReExport:
					return Constants.DeclarationTypeCodes.ForReExport;
				case Constants.InNonPayment.DeclarationTypeRefs.StorageInFtz:
					return Constants.DeclarationTypeCodes.StorageInFtz;
				case Constants.InNonPayment.DeclarationTypeRefs.BlanketIncludingBlanketGstReliefAndDutyExemption:
					return Constants.DeclarationTypeCodes.BlanketIncludingBlanketGstReliefAndDutyExemption;
				case Constants.InNonPayment.DeclarationTypeRefs.TemporaryImportForExhibitionAuctionsWithSales:
					return Constants.DeclarationTypeCodes.TemporaryImportForExhibitionAuctionsWithSales;
				case Constants.InNonPayment.DeclarationTypeRefs.TemporaryImportForRepairs:
					return Constants.DeclarationTypeCodes.TemporaryImportForRepairs;
				case Constants.InNonPayment.DeclarationTypeRefs.TemporaryImportForExhibitionAuctionsWithoutSales:
					return Constants.DeclarationTypeCodes.TemporaryImportForExhibitionAuctionsWithoutSales;
				case Constants.InNonPayment.DeclarationTypeRefs.TemporaryImportForOtherPurposes:
					return Constants.DeclarationTypeCodes.TemporaryImportForOtherPurposes;
				case Constants.InNonPayment.DeclarationTypeRefs.TemporaryExportReImportedGoods:
					return Constants.DeclarationTypeCodes.TemporaryExportReImportedGoods;
				default:
					return declarationTypeReference;
			}
		}

		static protected string GetOutGroupCode(string declarationTypeReference)
		{
			switch (declarationTypeReference)
			{
				case Constants.Out.DeclarationTypeRefs.ApprovedPremisesSchemes:
					return Constants.DeclarationTypeCodes.ApprovedPremisesSchemes;
				case Constants.Out.DeclarationTypeRefs.DirectIncludingStorageInFtz:
					return Constants.DeclarationTypeCodes.DirectIncludingStorageInFtz;
				case Constants.Out.DeclarationTypeRefs.Blanket:
					return Constants.DeclarationTypeCodes.Blanket;
				case Constants.Out.DeclarationTypeRefs.TemporaryImportForExhibitionAuctionsWithSales:
					return Constants.DeclarationTypeCodes.TemporaryImportForExhibitionAuctionsWithSales;
				case Constants.Out.DeclarationTypeRefs.TemporaryImportForRepairs:
					return Constants.DeclarationTypeCodes.TemporaryImportForRepairs;
				case Constants.Out.DeclarationTypeRefs.TemporaryImportForExhibitionAuctionsWithoutSales:
					return Constants.DeclarationTypeCodes.TemporaryImportForExhibitionAuctionsWithoutSales;
				case Constants.Out.DeclarationTypeRefs.TemporaryImportForOtherPurposes:
					return Constants.DeclarationTypeCodes.TemporaryImportForOtherPurposes;
				case Constants.Out.DeclarationTypeRefs.TemporaryExportReImportedGoods:
					return Constants.DeclarationTypeCodes.TemporaryExportReImportedGoods;
				default:
					return declarationTypeReference;
			}
		}

		static protected string GetTranshipmentMovementGroupCode(string declarationTypeReference)
		{
			switch (declarationTypeReference)
			{
				case Constants.TranshipmentMovement.DeclarationTypeRefs.ThruTranshipmentWithInterGatewayMovement:
					return Constants.DeclarationTypeCodes.ThruTranshipmentWithInterGatewayMovement;
				case Constants.TranshipmentMovement.DeclarationTypeRefs.ThruTranshipmentWithinSameFtz:
					return Constants.DeclarationTypeCodes.ThruTranshipmentWithinSameFtz;
				case Constants.TranshipmentMovement.DeclarationTypeRefs.InterGatewayMovement:
					return Constants.DeclarationTypeCodes.InterGatewayMovement;
				case Constants.TranshipmentMovement.DeclarationTypeRefs.Removal:
					return Constants.DeclarationTypeCodes.Removal;
				case Constants.TranshipmentMovement.DeclarationTypeRefs.BlanketRemoval:
					return Constants.DeclarationTypeCodes.BlanketRemoval;
				default:
					return declarationTypeReference;
			}
		}

		#endregion

		#region Commondity Code

		protected int AddCommondityCode(string code, string description, string tariffCode, DateTime startDate, DateTime endDate, string measurement, string controlledFor)
		{
			var totalProcessed = 0;

			var commodityCode = CommodityList.FirstOrDefault(x => x.ZZ1_TariffCode.Equals(code, StringComparison.OrdinalIgnoreCase) && x.ZZ1_StartDate == startDate);

			if (commodityCode == null)
			{
				commodityCode = new RefCusTariff()
				{
					ZZ1_TariffCode = code,
					ZZ1_StartDate = Helper.GetValidValue(startDate)
				};

				CommodityList.Add(commodityCode);
				totalProcessed = 1;
			}

			commodityCode.ZZ1_Description = description;
			commodityCode.ZZ1_EndDate = Helper.GetValidValue(endDate);
			commodityCode.RefCusTariffAttributes = GetCommodityAttributes(controlledFor.Contains(Constants.ControlTypes.Import), controlledFor.Contains(Constants.ControlTypes.Export), controlledFor.Contains(Constants.ControlTypes.Transhipment));
			commodityCode.RefCusTariffUOMs = string.IsNullOrWhiteSpace(measurement) ? null : new[] { new RefCusTariffUOM() { ZZ8_UOM = measurement } };

			var tariffRelationships = new List<RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusTariffRelationship>();

			if (commodityCode.RefCusTariffRelationships != null)
			{
				tariffRelationships.AddRange(commodityCode.RefCusTariffRelationships);
			}

			if (!tariffRelationships.Any(x => x.ZZH_TariffCode.Equals(tariffCode, StringComparison.OrdinalIgnoreCase)))
			{
				tariffRelationships.Add(new RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusTariffRelationship() { ZZH_TariffCode = tariffCode });
				totalProcessed++;
			}

			commodityCode.RefCusTariffRelationships = tariffRelationships.ToArray();

			return totalProcessed;
		}

		static RefCusTariffAttribute[] GetCommodityAttributes(bool isImportControl, bool isExportControl, bool isTranshipmentControl)
		{
			var result = new List<RefCusTariffAttribute>();
			if (isImportControl)
			{
				result.Add(CreateRefCusTariffAttribute(Constants.ControlTypes.AttributeNames.Import));
			}
			if (isExportControl)
			{
				result.Add(CreateRefCusTariffAttribute(Constants.ControlTypes.AttributeNames.Export));
			}
			if (isTranshipmentControl)
			{
				result.Add(CreateRefCusTariffAttribute(Constants.ControlTypes.AttributeNames.Transhipment));
			}
			return result.ToArray();
		}

		static RefCusTariffAttribute CreateRefCusTariffAttribute(string name)
		{
			return new RefCusTariffAttribute() { ZZ3_Name = name, ZZ3_Value = Constants.ControlTypes.IsControlled };
		}

		#endregion

		#region Customs Procedure Code

		protected int AddCustomsProcedureCode(string code, string description, string cpCode, DateTime startDate, DateTime endDate)
		{
			var totalProcessed = 0;
			var concession = string.Empty;
			var procedureCode = string.Empty;

			if (cpCode.Length > 3)
			{
				procedureCode = cpCode.Substring(0, 3);
				concession = cpCode.Substring(3);
			}
			else
			{
				procedureCode = cpCode;
				concession = code;
			}

			var customsProcedureCode = ProcedureList.FirstOrDefault(x => x.ZZ6_ProcedureCode.Equals(procedureCode, StringComparison.OrdinalIgnoreCase) && x.ZZ6_Concession.Equals(concession, StringComparison.OrdinalIgnoreCase) && x.ZZ6_StartDate == startDate);
			if (customsProcedureCode == null)
			{
				customsProcedureCode = new RefCusProcedure()
				{
					ZZ6_ProcedureCode = procedureCode,
					ZZ6_Concession = concession,
					ZZ6_StartDate = Helper.GetValidValue(startDate)
				};

				ProcedureList.Add(customsProcedureCode);
				totalProcessed = 1;
			}

			var commonAccessReference = string.Empty;
			var declarationTypeReference = string.Empty;

			if (procedureCode.Length > 0)
			{
				commonAccessReference = procedureCode.Substring(0, 1);
			}
			if (procedureCode.Length > 1)
			{
				declarationTypeReference = procedureCode.Substring(1);
			}

			var shipmentCode = GetShipmentCode(commonAccessReference);
			var groupCode = GetGroupCode(shipmentCode, declarationTypeReference);

			customsProcedureCode.ZZ6_ShipmentType = shipmentCode;
			customsProcedureCode.ZZ6_Group = groupCode;
			customsProcedureCode.ZZ6_Description = $"{description} ({shipmentCode} {groupCode})";

			switch (shipmentCode)
			{
				case Constants.InPayment.ShipmentCode:
					customsProcedureCode.ZZ6_CalculateDuty = true;
					customsProcedureCode.ZZ6_LandedCost = true;
					break;

				case Constants.InNonPayment.ShipmentCode when groupCode.StartsWith("T", StringComparison.OrdinalIgnoreCase):
				case Constants.Out.ShipmentCode when groupCode.StartsWith("T", StringComparison.OrdinalIgnoreCase):
					break;
			}

			var attributes = GetProcedureAttributes(commonAccessReference, concession);
			if (attributes.Any())
			{
				customsProcedureCode.RefCusProcedureAttributes = attributes.ToArray();
			}

			customsProcedureCode.ZZ6_EndDate = Helper.GetValidValue(endDate);
			return totalProcessed;
		}

		static List<RefCusProcedureAttribute> GetProcedureAttributes(string commonAccessReference, string concession)
		{
			var result = new List<RefCusProcedureAttribute>();

			if (commonAccessReference == Constants.Out.CommonAccessReference.WithCertificateOfOrigin)
			{
				result.Add(new RefCusProcedureAttribute { ZXB_Name = Constants.ProcedureAttributeNames.ISCOO, ZXB_Value = Constants.ProcedureAttributeValues.ISCOO });
			}

			if (concession == Constants.ProcedureConcessionTypes.AEO)
			{
				result.Add(new RefCusProcedureAttribute { ZXB_Name = Constants.ProcedureAttributeNames.ISAEO, ZXB_Value = Constants.ProcedureAttributeValues.ISAEO });
				result.Add(new RefCusProcedureAttribute { ZXB_Name = Constants.ProcedureAttributeNames.PC1, ZXB_Value = Constants.ProcedureAttributeValues.AEOPC1 });
				result.Add(new RefCusProcedureAttribute { ZXB_Name = Constants.ProcedureAttributeNames.PC2, ZXB_Value = Constants.ProcedureAttributeValues.AEOPC2 });
			}
			else if (concession == Constants.ProcedureConcessionTypes.SEASTORE)
			{
				result.Add(new RefCusProcedureAttribute { ZXB_Name = Constants.ProcedureAttributeNames.ISSEASTORE, ZXB_Value = Constants.ProcedureAttributeValues.ISSEASTORE });
				result.Add(new RefCusProcedureAttribute { ZXB_Name = Constants.ProcedureAttributeNames.PC1, ZXB_Value = Constants.ProcedureAttributeValues.SEASTOREPC1 });
				result.Add(new RefCusProcedureAttribute { ZXB_Name = Constants.ProcedureAttributeNames.PC2, ZXB_Value = Constants.ProcedureAttributeValues.SEASTOREPC2 });
			}

			return result;
		}

		#endregion

		#region Facility Code

		protected int AddFacilityCode(string code, string description, DateTime startDate, DateTime endDate)
		{
			var totalProcessed = 0;

			var facilityCode = FacilityList.FirstOrDefault(x => x.ZZD_Code.Equals(code, StringComparison.OrdinalIgnoreCase) && x.ZZD_StartDate == startDate);

			if (facilityCode == null)
			{
				facilityCode = new RefCusCodeList()
				{
					ZZD_Code = code,
					ZZD_StartDate = Helper.GetValidValue(startDate)
				};

				FacilityList.Add(facilityCode);
				totalProcessed = 2; // plus 1 for attribute
			}

			facilityCode.ZZD_Description = description;
			facilityCode.ZZD_EndDate = Helper.GetValidValue(endDate);

			facilityCode.RefCusCodeListAttributes = new[]
			{
				new RefCusCodeListAttribute()
				{
					ZZE_ZXE_NKName = Constants.FacilityAttributeName,
					ZZE_Value = code
				}
			};

			return totalProcessed;
		}

		#endregion

		#region Port Code

		protected int AddPortCode(string code, string description, DateTime startDate, DateTime endDate)
		{
			var totalProcessed = 0;

			var portCodeCusCodeList = PortList.FirstOrDefault(x => x.ZZD_Code.Equals(code, StringComparison.OrdinalIgnoreCase) && x.ZZD_StartDate == startDate);
			if (portCodeCusCodeList == null)
			{
				portCodeCusCodeList = new RefCusCodeList()
				{
					ZZD_Code = code,
					ZZD_StartDate = Helper.GetValidValue(startDate)
				};

				PortList.Add(portCodeCusCodeList);
				totalProcessed = 1;
			}

			portCodeCusCodeList.ZZD_Description = description;
			portCodeCusCodeList.ZZD_EndDate = Helper.GetValidValue(endDate);

			return totalProcessed;
		}

		#endregion

		#region End Date

		protected DateTime GetValidEndDate(DateTime startDate)
		{
			var result = ReleaseDate;

			if (result < startDate)
			{
				result = startDate;
			}

			return result;
		}

		#endregion

		#region Implement

		protected const string DateFormat = "yyyyMMdd";

		protected DateTime ReleaseDate { get; set; }

		protected string ReleaseNumber { get; set; }

		protected List<RefCusProcedure> ProcedureList { get; }

		protected List<RefCusTariff> CommodityList { get; }

		protected List<RefCusCodeList> FacilityList { get; }

		protected List<RefCusCodeList> PortList { get; }

		protected int TotalProcessed { get; set; }

		#endregion
	}
}
