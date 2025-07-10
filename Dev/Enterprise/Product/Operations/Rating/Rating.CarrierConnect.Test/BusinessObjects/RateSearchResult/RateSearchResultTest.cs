using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.CarrierConnect.Test
{
	[TestedType(typeof(RateSearchResult))]
	public class RateSearchResultTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = Factory.New<RateSearchResult>();
			bizo.RR_JsonContent = ZBlob.FromUTF8("{}");
			return bizo;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var bizo = factory.NewWithValidTestData<RateSearchResult>();
			bizo.RR_JsonContent = ZBlob.FromUTF8("{}");
			return bizo;
		}

		public void Test_StoringDto()
		{
			var bizo = Factory.New<RateSearchResult>();
			bizo.StoreDto(new SimpleDto { Name = "Simple" });
			Assert(bizo.TryGetDto<SimpleDto>(out var dto));
			AssertEquals("Simple", dto.Name);
		}

		public void TestCleanupOrphanedRecords()
		{
			var oldBizo = Factory.New<RateSearchResult>();
			oldBizo.RR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-2);

			_ = Factory.New<RateSearchResult>();
			Factory.Save();

			var baselineBizos = Factory.Load<RateSearchResult>(new ZQuery());
			AssertEquals(2, baselineBizos.Length);

			RateSearchResult.DeleteExpiredRecords(Factory);

			var postCleanupBizos = Factory.Load<RateSearchResult>(new ZQuery());
			AssertEquals(1, postCleanupBizos.Length);
		}

		#region Implementation

		class SimpleDto
		{
			public string Name { get; set; }
		}

		#endregion
	}
}
