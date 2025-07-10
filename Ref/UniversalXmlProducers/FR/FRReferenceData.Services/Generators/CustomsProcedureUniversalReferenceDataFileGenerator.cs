using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public abstract class CustomsProcedureUniversalReferenceDataFileGenerator : BaseUniversalReferenceDataFileGenerator<RefCusProcedure>
	{
		public override IEnumerable<string> InputFiles
		{
			get
			{
				yield return ApplicationConfig.Instance.CustomsProcedureFileName;
				yield return ApplicationConfig.Instance.ProcedureFileName;
				yield return ApplicationConfig.Instance.PreviousProcedureFileName;
				yield return ApplicationConfig.Instance.ConcessionFileName;
			}
		}
		public override string OutputFile => ApplicationConfig.Instance.FRCustomsProcedureOutputFile;

		protected override List<RefCusProcedure> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var result = new List<RefCusProcedure>();

			var procedureDictionary = GetDictionary(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.ProcedureFileName));
			var previousProcedureDictionary = GetDictionary(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.PreviousProcedureFileName));
			var concessionDictionary = GetDictionary(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.ConcessionFileName));

			XmlDocument sourceXmlDocument = new XmlDocument();
			sourceXmlDocument.Load(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.CustomsProcedureFileName));
			XmlNodeList nodeList = sourceXmlDocument.GetElementsByTagName("ligne");

			foreach (XmlNode node in nodeList)
			{
				var procedure = UniversalDataHelper.GetTagValue(node, "CHAMP1");
				if (procedure == NullProcedure)
				{
					continue;
				}
				var previousProcedure = UniversalDataHelper.GetTagValue(node, "CHAMP2");
				var concession = UniversalDataHelper.GetTagValue(node, "CHAMP3");
				procedureDictionary.TryGetValue(procedure, out var procedureDescription);
				previousProcedureDictionary.TryGetValue(previousProcedure, out var previousProcedureDescription);
				concessionDictionary.TryGetValue(concession, out var concessionDescription);
				var description = $"{procedureDescription} {previousProcedureDescription} {concessionDescription}";
				var startDate = UniversalDataHelper.GetStartDateFromTag(node, "CHAMP5");
				var endDate = UniversalDataHelper.GetEndDateFromTag(node, "CHAMP6");
				var category = GetCategory(procedure);
				var shipmentType = $"{category}P";
				var calculateDuty = GetCalculateDuties(procedure);
				var calculateVAT = GetCalculateVAT(procedure, previousProcedure, concession);
				var intoWarehouse = IsIntoWarehouseProcedure(procedure) ? UniversalDataHelper.Constants.Yes : UniversalDataHelper.Constants.No;
				var outOfWarehouse = IsIntoWarehouseProcedure(previousProcedure) ? UniversalDataHelper.Constants.Yes : UniversalDataHelper.Constants.No;
				var intoInwardProcessing = IsIntoInwardProcessing(procedure) ? UniversalDataHelper.Constants.Yes : UniversalDataHelper.Constants.No;
				var outofInwardProcessing = IsIntoInwardProcessing(previousProcedure) ? UniversalDataHelper.Constants.Yes : UniversalDataHelper.Constants.No;
				var intoOutwardProcessing = IsIntoOutwardProcessing(procedure) ? UniversalDataHelper.Constants.Yes : UniversalDataHelper.Constants.No;
				var outofOutwardProcessing = IsIntoOutwardProcessing(previousProcedure) ? UniversalDataHelper.Constants.Yes : UniversalDataHelper.Constants.No;
				var intoTemporaryImport = IsIntoTemporaryImport(procedure) ? UniversalDataHelper.Constants.Yes : UniversalDataHelper.Constants.No;
				var outOfTemporaryImport = IsIntoTemporaryImport(previousProcedure) ? UniversalDataHelper.Constants.Yes : UniversalDataHelper.Constants.No;
				var intoTemporaryExport = IsIntoTemporaryExport(procedure) ? UniversalDataHelper.Constants.Yes : UniversalDataHelper.Constants.No;
				var outOfTemporaryExport = IsIntoTemporaryExport(previousProcedure) ? UniversalDataHelper.Constants.Yes : UniversalDataHelper.Constants.No;
				var isGuaranteeConsumed = IsGuaranteeConsumed(procedure) ? UniversalDataHelper.Constants.Yes : UniversalDataHelper.Constants.No;
				var isGuaranteeReleased = IsGuaranteeReleased(previousProcedure) ? UniversalDataHelper.Constants.Yes : UniversalDataHelper.Constants.No;
				var group = GetProcedureGroup(procedure, previousProcedure);
				var isTransit = IsTransit(procedure) ? UniversalDataHelper.Constants.Yes : UniversalDataHelper.Constants.No;

				if (!UniversalDataHelper.CheckDatesAreValid(startDate, endDate) || string.IsNullOrEmpty(group))
				{
					continue;
				}

				result.Add(new RefCusProcedure()
				{
					ZZ6_ProcedureCode = procedure,
					ZZ6_PreviousProcedureCode = previousProcedure,
					ZZ6_Concession = concession,
					ZZ6_Description = description,
					ZZ6_StartDate = startDate,
					ZZ6_EndDate = endDate,
					ZZ6_Category = category,
					ZZ6_ShipmentType = shipmentType,
					ZZ6_CalculateDuty = calculateDuty,
					ZZ6_CalculateVAT = calculateVAT,
					ZZ6_IntoWarehouse = intoWarehouse,
					ZZ6_OutOfWarehouse = outOfWarehouse,
					ZZ6_IntoInwardProcessing = intoInwardProcessing,
					ZZ6_OutOfInwardProcessing = outofInwardProcessing,
					ZZ6_IntoOutwardProcessing = intoOutwardProcessing,
					ZZ6_OutofOutwardProcessing = outofOutwardProcessing,
					ZZ6_IntoTemporaryImport = intoTemporaryImport,
					ZZ6_OutOfTemporaryImport = outOfTemporaryImport,
					ZZ6_IntoTemporaryExport = intoTemporaryExport,
					ZZ6_OutOfTemporaryExport = outOfTemporaryExport,
					ZZ6_IsGuaranteeConsumed = isGuaranteeConsumed,
					ZZ6_IsGuaranteeReleased = isGuaranteeReleased,
					ZZ6_Group = group,
					ZZ6_IsTransit = isTransit,
					RefCusProcedureAttributes = GetRefCusProcedureAttributes(procedure, concession, group)
				});
			}

			return result.ToList();
		}

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var cusProcedure = new EntityTypeConfiguration<RefCusProcedure>(true);
			cusProcedure.IncludeColumn(x => x.ZZ6_ProcedureCode, true);
			cusProcedure.IncludeColumn(x => x.ZZ6_PreviousProcedureCode, true);
			cusProcedure.IncludeColumn(x => x.ZZ6_Concession, true);
			cusProcedure.IncludeColumn(x => x.ZZ6_Description, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_StartDate, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_EndDate, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_Category, true);
			cusProcedure.IncludeColumn(x => x.ZZ6_ShipmentType, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_CalculateDuty, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_CalculateVAT, false);
			cusProcedure.IncludeColumnWithConstantValue(x => x.ZZ6_LandedCost, false, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_IntoWarehouse, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_OutOfWarehouse, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_IntoInwardProcessing, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_OutOfInwardProcessing, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_IntoOutwardProcessing, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_OutofOutwardProcessing, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_IntoTemporaryImport, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_OutOfTemporaryImport, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_IntoTemporaryExport, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_OutOfTemporaryExport, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_IsGuaranteeConsumed, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_IsGuaranteeReleased, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_Group, false);
			cusProcedure.IncludeColumn(x => x.ZZ6_IsTransit, false);
			cusProcedure.IncludeColumnWithConstantValue(x => x.ZZ6_ZZZ_NKDataGrouping, true, GetDefaultDataGrouping());
			cusProcedure.IncludeColumn(x => x.RefCusProcedureAttributes, false);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(cusProcedure);

			var cusProcedureAttribute = new EntityTypeConfiguration<RefCusProcedureAttribute>(true);
			cusProcedureAttribute.IncludeColumn(x => x.ZXB_Name, true);
			cusProcedureAttribute.IncludeColumn(x => x.ZXB_Value, false);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(cusProcedureAttribute);

			return xmlWriterConfiguration;
		}

		protected abstract string GetDefaultDataGrouping();

		string GetProcedureGroup(string procedureCode, string previousProcedureCode) => GetProcedureGroupCore(procedureCode, previousProcedureCode);
		protected abstract string GetProcedureGroupCore(string procedureCode, string previousProcedureCode);


		public static string GetCategory(string procedure) => IsImportProcedure(procedure) ? "IM" : "EX";

		public static bool IsImportProcedure(string procedureCode)
		{
			categoryDictionary.TryGetValue(procedureCode, out var procedureCategory);
			return procedureCategory == "0" ? true : false;
		}

		public static bool IsExemptedOfVATNumber(string procedureCode) => noVATNumberProcedureCodes.Contains(procedureCode);
		static string[] noVATNumberProcedureCodes => new string[] { "42", "51", "53", "63", "71", "78" };

		public static bool GetCalculateDuties(string procedure) => IsImportProcedure(procedure) && !IsExemptedOfDutiesCalculation(procedure);
		static bool IsExemptedOfDutiesCalculation(string procedureCode) => noDutiesCalculationProcedureCodes.Contains(procedureCode);
		static string[] noDutiesCalculationProcedureCodes = new string[] { "63" };

		public static bool GetCalculateVAT(string procedure, string previousProcedureCode, string concession) => IsImportProcedure(procedure) && !IsExemptedOfVATCalculation(procedure) && !IsSpecificallyExemptedOfVATCalculation(procedure, previousProcedureCode, concession);
		static bool IsExemptedOfVATCalculation(string procedureCode) => noVATCalculationProcedureCodes.Contains(procedureCode);
		static bool IsSpecificallyExemptedOfVATCalculation(string procedureCode, string previousProcedureCode, string concession) => procedureCode == "61" && previousProcedureCode == "22" && concession == "B02";

		static string[] noVATCalculationProcedureCodes => new string[] { "42", "45", "49", "63" };

		static bool IsIntoWarehouseProcedure(string procedureCode) => intoWarehouseProcedureCodes.Contains(procedureCode);
		static string[] intoWarehouseProcedureCodes => new string[] { "71", "76", "77", "78" };

		static bool IsIntoInwardProcessing(string procedureCode) => intoInwardProcessingProcedureCodes.Contains(procedureCode);
		static string[] intoInwardProcessingProcedureCodes => new string[] { "41", "51" };

		static bool IsIntoOutwardProcessing(string procedureCode) => intoOutwardProcessingProcedureCodes.Contains(procedureCode);
		static string[] intoOutwardProcessingProcedureCodes => new string[] { "21", "22" };

		static bool IsIntoTemporaryImport(string procedureCode) => intoTemporaryImportProcedureCodes.Contains(procedureCode);
		static string[] intoTemporaryImportProcedureCodes => new string[] { "53" };

		static bool IsIntoTemporaryExport(string procedureCode) => intoTemporaryExportProcedureCodes.Contains(procedureCode);
		static string[] intoTemporaryExportProcedureCodes => new string[] { "23" };


		public static bool IsGuaranteeConsumed(string procedureCode) => hasGuarantee.Contains(procedureCode);
		public static bool IsGuaranteeReleased(string previousProcedureCode) => hasGuarantee.Contains(previousProcedureCode);

		static string[] hasGuarantee => new string[] { "07", "48", "51", "53", "71", "78", "91" };

		static bool IsTransit(string procedureCode) => false;

		static bool IsIntoWarehouseOfAnyType(string procedureCode) => IsIntoWarehouseProcedure(procedureCode)
																|| IsIntoInwardProcessing(procedureCode)
																|| IsIntoOutwardProcessing(procedureCode);


		static string[] procedureCodesWithEconomicImpactButNotWarehoused = new string[] { "31", "02", "54", "11" };

		public static bool HasEconomicImpact(string procedureCode) => IsIntoWarehouseOfAnyType(procedureCode) || IsIntoTemporaryExport(procedureCode) || IsIntoTemporaryImport(procedureCode) || procedureCodesWithEconomicImpactButNotWarehoused.Contains(procedureCode);

		static RefCusProcedureAttribute[] GetRefCusProcedureAttributes(string procedureCode, string concessionCode, string group)
		{
			var result = new List<RefCusProcedureAttribute>();
			if (RequiresVATNumberExemptionAttribute(procedureCode, concessionCode, group))
			{
				result.Add(UniversalDataHelper.CreateRefCusProcedureAttribute("VATNumberExempt", UniversalDataHelper.Constants.Yes));
			}
			if (RequiresIntoEndUseAttribute(procedureCode))
			{
				result.Add(UniversalDataHelper.CreateRefCusProcedureAttribute("IntoEndUse", UniversalDataHelper.Constants.Yes));
			}
			return result.ToArray();
		}

		public static bool RequiresVATNumberExemptionAttribute(string procedureCode, string concessionCode, string group)
		{
			return concessionCode == "F48" || (IsExemptedOfVATNumber(procedureCode) && !(concessionCode == "D51" && group.Contains("53P")));
		}

		public static bool RequiresIntoEndUseAttribute(string procedureCode) => procedureCode.StartsWith("44", StringComparison.InvariantCulture);

		static Dictionary<string, string> GetDictionary(string xmlSource)
		{
			var result = new Dictionary<string, string>();

			XmlDocument sourceXmlDocument = new XmlDocument();
			sourceXmlDocument.Load(xmlSource);
			XmlNodeList nodeList = sourceXmlDocument.GetElementsByTagName("ligne");

			foreach (XmlNode node in nodeList)
			{
				var code = UniversalDataHelper.GetTagValue(node, "CHAMP1");
				if (!result.ContainsKey(code))
				{
					result.Add(code, HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(node, "CHAMP2")));
				}
			}

			return result;
		}

		static Dictionary<string, string> categoryDictionary
		{
			get
			{
				if (categories == null)
				{
					categories = new Dictionary<string, string>();

					XmlDocument sourceXmlDocument = new XmlDocument();
					sourceXmlDocument.Load(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.ProcedureFileName));
					XmlNodeList nodeList = sourceXmlDocument.GetElementsByTagName("ligne");

					foreach (XmlNode node in nodeList)
					{
						var code = UniversalDataHelper.GetTagValue(node, "CHAMP1");
						if (!categories.ContainsKey(code))
						{
							var category = HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(node, "CHAMP3"));
							categories.Add(code, category);
						}
					}
				}

				return categories;
			}

		}
		static Dictionary<string, string> categories;

		const string NullProcedure = "00";
	}
}
