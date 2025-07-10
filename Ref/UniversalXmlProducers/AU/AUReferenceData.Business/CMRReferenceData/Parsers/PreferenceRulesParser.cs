using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class PreferenceRulesParser : BaseCMRReferenceDataParser
	{
		public PreferenceRulesParser(IDateTimeProvider dateTimeProvider)
		{
			this.dateTimeProvider = dateTimeProvider;
		}
		readonly IDateTimeProvider dateTimeProvider;

		protected override string FileNamePrefix => ApplicationConfig.PreferenceRulesFilePrefix;
		protected override string OutputXMLName => "RefCusCodeList_AU_PreferenceRules.xml";
		protected override string DataSource => "AU CMR Preference Rules";
		protected virtual string DataGrouping => Constants.DataGrouping;
		protected virtual string SchemesFilePrefix => ApplicationConfig.PreferenceRuleSchemesFilePrefix;
		const int YearsExpiredCodeKept = 5;

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate)
		{
			return CMRXMLWriterConfigurationBuilder.BuildRefCusCodeListConfiguration(CMRConstants.CodeTypes.CMRRP, DataGrouping, CMRConstants.CodeListAttributeNames.ApplicablePreferenceScheme);
		}

		static PropertyMapping<PreferenceRule>[] PreferenceRuleMappings => new[]
		{
			new PropertyMapping<PreferenceRule>(entity => entity.Code, 1, 4),
			new PropertyMapping<PreferenceRule>(entity => entity.StartDate, 37, 8),
			new PropertyMapping<PreferenceRule>(entity => entity.EndDate, 46, 8, Constants.RefData_Common.MaximumDateTime),
			new PropertyMapping<PreferenceRule>(entity => entity.Description, 96, 250)
		};

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			xmlWriter.SetPublicationTime(dateTimeProvider.CurrentLocalDateTime);

			var preferenceRuleDataConverter = new LineToEntityConverter<PreferenceRule>(PreferenceRuleMappings, 1);
			var preferenceRules = new Dictionary<string, PreferenceRule>();
			foreach (var line in content.NonEmptyLines())
			{
				var rule = preferenceRuleDataConverter.Convert(line);
				if (rule.StartDate <= dateTimeProvider.CurrentLocalDateTime && rule.EndDate >= dateTimeProvider.CurrentLocalDateTime.AddYears(-YearsExpiredCodeKept))
				{
					if (!preferenceRules.TryGetValue(rule.Code, out var existing) || rule.StartDate > existing.StartDate)
					{
						preferenceRules[rule.Code] = rule;
					}
				}
			}

			var schemes = PreferenceRuleSchemes.ToLookup(scheme => scheme.Code, scheme => scheme.Value);
			var cusCodes = preferenceRules.Values.Select(rule => new RefCusCodeList
			{
				ZZD_Code = rule.Code,
				ZZD_Description = rule.Description,
				RefCusCodeListAttributes = schemes[rule.Code].Distinct().Order().Select(scheme => new RefCusCodeListAttribute { ZZE_Value = scheme }).ToArray()
			});

			foreach (var code in cusCodes)
			{
				xmlWriter.PopulateData(code);
			}
		}

		protected virtual IEnumerable<PreferenceRuleScheme> PreferenceRuleSchemes => PreferenceRuleSchemesParser.DownloadAndParse(SchemesFilePrefix, IndexUri);
	}

	class PreferenceRule: RefDataRepoModelEntityType
	{
		public string Code { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Description { get; set; }
	}
}
