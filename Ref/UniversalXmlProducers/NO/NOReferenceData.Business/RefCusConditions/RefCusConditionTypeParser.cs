using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Services.RefCusConditions;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.RefCusConditions
{
	public static class RefCusConditionTypeParser
	{
		public static string ConvertToXMLFile(string dataSource, RefCusCodeConditionItems cusCodeCondition, AvgiftListe importFees, DateTime lastModified, string outputFileWithPath)
		{
			_ = cusCodeCondition ?? throw new ArgumentNullException(nameof(cusCodeCondition));
			_ = importFees ?? throw new ArgumentNullException(nameof(importFees));
			ErrorBuilder.Clear();
			var writerConfiguration = GetRefCusConditionsConfiguration();

			var resultCusConditions = new List<RefCusConditionType>();

			if (cusCodeCondition.RefCusCodeConditionItem == null || cusCodeCondition.RefCusCodeConditionItem.Length == 0)
			{
				ErrorBuilder.AppendLine($"Failed to parse CusConditionTypeParser, no conditions in file");
				return ErrorBuilder.ToString();
			}

			foreach (var conditions in cusCodeCondition.RefCusCodeConditionItem)
			{
				var (validCode, condition) = ConvertRefCusConditionType(conditions.CusCode, importFees);
				if (validCode)
				{
					resultCusConditions.Add(condition);
				}
			}

			if (resultCusConditions.Any())
			{
				FileHelper.ExportToXMLFile(dataSource, outputFileWithPath, writerConfiguration, lastModified, resultCusConditions);
			}

			return ErrorBuilder.ToString();
		}

		static (bool validCode, RefCusConditionType condition) ConvertRefCusConditionType (string cusCode, AvgiftListe importFees)
		{
			if (string.IsNullOrEmpty(cusCode))
			{
				return (false, null);
			}
			var feeType = cusCode.SubstringSafe(0, 2);
			var feeCode = cusCode.SubstringSafe(cusCode.Length - 3, 3);

			var description = (from goods in importFees?.Goods ?? Array.Empty<Vare>()
							   from dutyRate in goods.DutyRates
							   from dutyTypes in dutyRate.DutyTypes
							   where dutyTypes.DutyType == feeType
							   from dutyType in dutyTypes.DutyType
							   from dutyGroup in dutyTypes.DutyGroups
							   where dutyGroup.DutyGroup.ToString() == feeCode
							   select new
							   {
								   dutyGroup.DutyGroupDescription
							   }).Take(1);
			var fullDescr = description.FirstOrDefault()?.DutyGroupDescription;

			if (!string.IsNullOrEmpty(fullDescr))
			{
				return (true, new RefCusConditionType
				{
					ZX2_ConditionType = cusCode,
					ZX2_Description = fullDescr
				});
			}
			return (false, null);
		}

		static XmlWriterConfiguration GetRefCusConditionsConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var refCusConditionTypeConfiguration = new EntityTypeConfiguration<RefCusConditionType>(true);
			refCusConditionTypeConfiguration.IncludeColumnWithConstantValue(x => x.ZX2_ConditionClass, isKeyColumn: false, "RATE");
			refCusConditionTypeConfiguration.IncludeColumnWithConstantValue(x => x.ZX2_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCodes.Norway);
			refCusConditionTypeConfiguration.IncludeColumn(x => x.ZX2_ConditionType, isKeyColumn: true);
			refCusConditionTypeConfiguration.IncludeColumn(x => x.ZX2_Description, isKeyColumn: false);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionTypeConfiguration);

			return writerConfiguration;
		}

		static StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		static StringBuilder errorBuilder;
	}
}
