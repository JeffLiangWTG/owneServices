using System.Linq;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefAirlineEFreightRule))]
	sealed class RefAirlineEFreightRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHasDuplicateRules()
		{
			var airLine = Factory.NewWithValidTestData<RefAirline>();

			var rule1 = airLine.EFreightStatusCollection.AddNew();
			rule1.RME_OriginLocation = "AUSYD";
			rule1.RME_DestinationLocation = "SGSIN";

			var rule2 = airLine.EFreightStatusCollection.AddNew();
			rule2.RME_OriginLocation = "AUSYD";
			rule2.RME_DestinationLocation = "SGSIN";

			var errorMessage = "You cannot have duplicate rules. Please differentiate the rule origin and destination location.";

			airLine.RunPreSaveValidation();

			Assert(rule1.RowErrors.Any(c => c.Message == errorMessage));
			Assert(rule2.RowErrors.Any(c => c.Message == errorMessage));

			rule2.RME_DestinationLocation = "NZAKL";
			airLine.RunPreSaveValidation();

			Assert(rule1.RowErrors.All(c => c.Message != errorMessage));
			Assert(rule2.RowErrors.All(c => c.Message != errorMessage));
		}
	}
}
