using CargoWise.RefDbRepo.AUReferenceData.Services;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class PreferenceRulesTestParser : PreferenceRulesParser
	{
		public PreferenceRulesTestParser(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider)
		{
		}

		protected override string FileNamePrefix => ApplicationConfig.PreferenceRulesTestFilePrefix;
		protected override string IndexUri => ApplicationConfig.AUReferenceTestFilesDirectory;
		protected override string OutputXMLName => "RefCusCodeList_AU_PreferenceRulesTest.xml";
		protected override string DataSource => "AU CMR Preference Rules Test";
		protected override string DataGrouping => Constants.DataGroupingTest;
		protected override string SchemesFilePrefix => ApplicationConfig.PreferenceRuleSchemesTestFilePrefix;
	}
}
