using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	class RefDbRepoStagingPluralizerFixture
	{
		[TestCase("AutoSchema", "AutoSchemas")]
		[TestCase("RefAccTaxRate", "RefAccTaxRates")]
		[TestCase("RefCountryStates", "RefCountryStates")]
		[TestCase("RefCusQuota", "RefCusQuota")]
		[TestCase("RefUNLOCO", "RefUNLOCOs")]
		[TestCase("SystemData", "SystemData")]
		[TestCase("DataSetChangeHistory", "DataSetChangeHistories")]
		[TestCase("RefCusCodeListAttributeUserView", "RefCusCodeListAttributeUserViews")]
		[TestCase("QRTZ_CALENDARS", "QRTZ_CALENDARS")]
		[TestCase("QRTZ_LOCKS", "QRTZ_LOCKS")]
		[TestCase("QRTZ_PAUSED_TRIGGER_GRPS", "QRTZ_PAUSED_TRIGGER_GRPS")]
		[TestCase("QRTZ_SCHEDULER_STATE", "QRTZ_SCHEDULER_STATE")]
		[TestCase("QRTZ_TRIGGERS", "QRTZ_TRIGGERS")]
		public void Pluralize(string identifier, string expectedResult)
		{
			var pluralizer = new RefDbRepoStagingPluralizer();
			var result = pluralizer.Pluralize(identifier);
			Assert.AreEqual(expectedResult, result);
		}

		[TestCase("AutoSchemas", "AutoSchema")]
		[TestCase("RefAccTaxRates", "RefAccTaxRate")]
		[TestCase("RefCountryStates", "RefCountryStates")]
		[TestCase("RefCusQuota", "RefCusQuota")]
		[TestCase("RefUNLOCOs", "RefUNLOCO")]
		[TestCase("SystemData", "SystemData")]
		[TestCase("DataSetChangeHistories", "DataSetChangeHistory")]
		[TestCase("RefCusCodeListAttributeUserViews", "RefCusCodeListAttributeUserView")]
		[TestCase("QRTZ_CALENDARS", "QRTZ_CALENDARS")]
		[TestCase("QRTZ_LOCKS", "QRTZ_LOCKS")]
		[TestCase("QRTZ_PAUSED_TRIGGER_GRPS", "QRTZ_PAUSED_TRIGGER_GRPS")]
		[TestCase("QRTZ_SCHEDULER_STATE", "QRTZ_SCHEDULER_STATE")]
		[TestCase("QRTZ_TRIGGERS", "QRTZ_TRIGGERS")]
		public void Singularize(string identifier, string expectedResult)
		{
			var pluralizer = new RefDbRepoStagingPluralizer();
			var result = pluralizer.Singularize(identifier);
			Assert.AreEqual(expectedResult, result);
		}
	}
}
