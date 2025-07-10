using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	static class PreferenceRuleSchemesParser
	{
		internal static IEnumerable<PreferenceRuleScheme> DownloadAndParse(string schemesFilePrefix, string indexUri)
		{
			var clientHelper = new HttpClientHelper();
			var (content, _) = CMRReferenceFileDownloader.Download(clientHelper, schemesFilePrefix, indexUri);

			return Parse(content);
		}

		internal static IEnumerable<PreferenceRuleScheme> Parse(string content)
		{
			var codeListConverter = new LineToEntityConverter<PreferenceRuleScheme>(CodeListMappings, 1);
			return content.NonEmptyLines().Select(codeListConverter.Convert);
		}

		static PropertyMapping<PreferenceRuleScheme>[] CodeListMappings => new[]
		{
			new PropertyMapping<PreferenceRuleScheme>(entity => entity.Value, 1, 4),
			new PropertyMapping<PreferenceRuleScheme>(entity => entity.Code, 32, 4),
		};
	}

	public class PreferenceRuleScheme : RefDataRepoModelEntityType
	{
		public string Code { get; set; }
		public string Value { get; set; }
	}
}
