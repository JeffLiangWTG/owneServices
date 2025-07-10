using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RatingDocumentsChargeGroupingOrRollup))]
	public class RatingDocumentsChargeGroupingOrRollupTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetBusinessObjectForTest();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetBusinessObjectForTest();

		#region implementation

		RatingDocumentsChargeGroupingOrRollup GetBusinessObjectForTest()
		{
			var result = Factory.NewWithValidTestData<RatingDocumentsChargeGroupingOrRollup>();
			result.RCG_JobType = "ALL";
			result.RCG_TransportMode = "ALL";
			result.RCG_Display = "DEF";
			result.RCG_Style = "DEF";
			return result;
		}
		#endregion
	}
}
