using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(RatingDateConfig))]
	public class RatingDateConfigTest : EnterpriseBusinessObjectTestCase
	{
		[TestDateIncremental(seconds: 1)]
		public void TestUpdate()
		{
			var ratingDateConfig = (RatingDateConfig)GetNewBusinessObject();
			Factory.Save();

			ratingDateConfig.RDT_Direction = Core.Constants.FreightShipmentDirection.Code.Import;
			Factory.Save();

			AssertEquals
			(
				expected: Core.Constants.FreightShipmentDirection.Code.Import,
				actual: ratingDateConfig.RDT_Direction
			);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject() => Factory.NewWithValidTestData<RatingDateConfig>();

		#endregion
	}
}
