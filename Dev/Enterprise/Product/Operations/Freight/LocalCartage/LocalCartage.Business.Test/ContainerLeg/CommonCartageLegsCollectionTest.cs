using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonCartageLegCollection))]
	public class CommonCartageLegsCollectionTest : ActiveBusinessObjectCollectionTestCase<CommonCartageLegCollection>
	{
		public void TestGetBusinessObjectFromCode_WithoutSlashInConsignmentID()
		{
			CommonCartageLeg leg = Factory.NewWithValidTestData<CommonCartageLeg>();
			leg.JU_SplitDeliverySuffix = "A";
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ConsignmentID = "T00001111";
			var move = Factory.NewWithValidTestData<CommonBookedCtgMove>();
			move.EW_JJ = cartage.PK;
			leg.JU_EW = move.PK;
			var runSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg);
			CommonCartageLegCollection collection = new CommonCartageLegCollection(runSheet);
			Factory.Save();
			AssertEquals("Should find correct leg", leg, ((IFindBoxListProvider)collection).GetBusinessObjectFromCode(leg.UniqueIDWithJobNumber));
		}

		public void TestGetBusinessObjectFromCode_WithSlashInConsignmentID()
		{
			CommonCartageLeg leg = Factory.NewWithValidTestData<CommonCartageLeg>();
			leg.JU_SplitDeliverySuffix = "A";
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ConsignmentID = "S00001111/I";
			var move = Factory.NewWithValidTestData<CommonBookedCtgMove>();
			move.EW_JJ = cartage.PK;
			leg.JU_EW = move.PK;
			var runSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg);
			CommonCartageLegCollection collection = new CommonCartageLegCollection(runSheet);
			Factory.Save();
			AssertEquals("Should find correct leg", leg, ((IFindBoxListProvider)collection).GetBusinessObjectFromCode(leg.UniqueIDWithJobNumber));
		}
	}
}
