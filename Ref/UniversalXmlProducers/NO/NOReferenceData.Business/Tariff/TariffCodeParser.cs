using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff;

public static class TariffCodeParser
{
	public static string ConvertToXMLFile(string dataSource, AvgiftListe importFees, AvgiftListe exportFees, DateTime lastModified, string outputFileWithPath)
	{
		ErrorBuilder.Clear();
		var cusRateTypeList = new List<RefCusRateType>();

		var importAndExportFees = new Dictionary<int, AvgiftListe>()
		{
			// This to be able to implement separate rateCode for
			// Import and Exports later, when WI00604494 is implemented.
			{ 0, importFees },
			{ 1, exportFees }
		};
		foreach (var kvp in importAndExportFees)
		{
			var isExport = kvp.Key;
			var feeList = kvp.Value;

			var fees = (from goods in feeList?.Goods ?? Array.Empty<Vare>()
				from dutyRate in goods.DutyRates
				from dutyTypes in dutyRate.DutyTypes
				where dutyTypes.avgiftstype != "MV"
				from dutyType in dutyTypes.DutyType
				from dutyGroup in dutyTypes.DutyGroups
				from dutyGroupDescription in dutyGroup.DutyGroupDescription
				orderby dutyTypes.DutyType, dutyGroup.DutyGroup
				select new
				{
					type = string.Concat(dutyTypes.DutyType, dutyGroup.DutyGroup),
					description = string.Concat(dutyTypes.DutyTypeDescription, " # ", dutyGroup.DutyGroupDescription)
				});

			var outputCodeList = new List<RefCusRateCode>();

			foreach (var fee in fees.Distinct())
			{
				var cusRateCode = ConvertRefCusRateCode(fee.type, fee.description);
				if (cusRateCode != null)
				{
					outputCodeList.Add(cusRateCode);
				}
			}

			if (outputCodeList.Any())
			{
				var cusRateType = new RefCusRateType
				{
					ZZR_RateType = isExport == 1 ? Constants.Types.ExciseExport : Constants.Types.ExciseImport,
					ZZR_Description = isExport == 1 ? "Excise, export" : "Excise",
					RefCusRateCodes = outputCodeList.ToArray()
				};
				cusRateTypeList.Add(cusRateType);
			}
		}

			var dutyRateCodes = new Dictionary<string, string>()
			{
				{ Constants.RateTypes.RawMaterialDutiesRate, "RåvaretollSats" },
				{ Constants.RateTypes.DutyInPercent, "Tollsats i prosent"}
			};
			var outputDutyCodeList = dutyRateCodes.Select(dutyRateCode => ConvertRefCusRateCode(dutyRateCode.Key, dutyRateCode.Value)).ToList();

			var cusRefRateType = new RefCusRateType
			{
				ZZR_RateType = Constants.Types.Duty,
				ZZR_Description = "Duty",
				RefCusRateCodes = outputDutyCodeList.ToArray()
			};
			cusRateTypeList.Add(cusRefRateType);

		var refCusTariffConfiguration = GetRefCusRateTypeWriterConfiguration();
		FileHelper.ExportToXMLFile(dataSource, outputFileWithPath, refCusTariffConfiguration, lastModified, cusRateTypeList);

		return ErrorBuilder.ToString();
	}

	public static RefCusRateCode ConvertRefCusRateCode(string feeType, string description)
	{
		if (!string.IsNullOrEmpty(feeType) && !string.IsNullOrEmpty(description))
		{
			return new RefCusRateCode
			{
				ZY1_RateCode = feeType,
				ZY1_Description = DataHelpers.CleanHtmlStringIfApplicable(description)
			};
		}

		ErrorBuilder.AppendLine("Unable to parse CusRateCode due to empty code, or empty description.");
		ErrorBuilder.AppendLine("DETAILS:");
		ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "FeeType    : {0}", feeType).AppendLine();
		ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Description: {0}", description).AppendLine();
		return null;
	}

	static XmlWriterConfiguration GetRefCusRateTypeWriterConfiguration()
	{
		var writerConfiguration = new XmlWriterConfiguration();

		var refCusCodeTypeConfiguration = new EntityTypeConfiguration<RefCusRateType>(true);
		refCusCodeTypeConfiguration.IncludeColumn(x => x.ZZR_RateType, true);
		refCusCodeTypeConfiguration.IncludeColumnWithConstantValue(x => x.ZZR_ZZZ_NKDataGrouping, true, Constants.CountryCodes.Norway);
		refCusCodeTypeConfiguration.IncludeColumnWithConstantValue(x => x.ZZR_IsPayable, false, true);
		refCusCodeTypeConfiguration.IncludeColumn(x => x.ZZR_Description, false);
		refCusCodeTypeConfiguration.IncludeColumn(x => x.RefCusRateCodes);
		writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeTypeConfiguration);

		var refCusRateCodeConfiguration = new EntityTypeConfiguration<RefCusRateCode>(true);
		refCusRateCodeConfiguration.IncludeColumn(x => x.ZY1_RateCode, true);
		refCusRateCodeConfiguration.IncludeColumn(x => x.ZY1_Description);
		refCusRateCodeConfiguration.IncludeColumnWithConstantValue(x => x.ZY1_ZZZ_NKDataGrouping, false, Constants.CountryCodes.Norway);
		writerConfiguration.IncludeEntityTypeConfiguration(refCusRateCodeConfiguration);

		return writerConfiguration;
	}

	public static StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
	static StringBuilder errorBuilder;
}
