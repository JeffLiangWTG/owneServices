using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.TransportBooking;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CommonCartageLoaderTest : TestCaseWithFactory
	{
		public void TestGetRelatedCommonCartagesForLocatingEDocs()
		{
			var parentBooking = Factory.New<IDtbBooking>();
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_ParentTableCode = ((BusinessObject)parentBooking).TablePrefix;
			cartage1.JJ_ParentID = parentBooking.PK;
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_ParentTableCode = ((BusinessObject)parentBooking).TablePrefix;
			cartage2.JJ_ParentID = parentBooking.PK;

			var unrelatedBooking = Factory.New<IDtbBooking>();
			var cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_ParentTableCode = ((BusinessObject)unrelatedBooking).TablePrefix;
			cartage3.JJ_ParentID = unrelatedBooking.PK;

			var loader = new CommonCartageLoader();
			var relatedCartages = loader.GetRelatedCommonCartagesForLocatingEDocs(parentBooking);

			AssertEquals("Only loads the cartages related to parentBooking", 2, relatedCartages.Count());
			AssertContainsExactElementsInAnyOrder("Only loads the cartages related to parentBooking", new BusinessObject[] { cartage1, cartage2 }, relatedCartages);
		}
	}
}
