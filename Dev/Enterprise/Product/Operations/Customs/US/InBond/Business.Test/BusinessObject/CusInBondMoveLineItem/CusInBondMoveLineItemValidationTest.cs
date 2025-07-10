using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondMoveLineItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBI_WeightUnit()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var moveLine = moveDetail.CBP7512Lines.AddNew();
			moveLine.BI_WeightUnit = Core.Constants.Weight.Ounces;
			AssertHasWarningContaining(moveLine.BI_WeightUnitInfo, ListValidation.InvalidCodeMessage);
			moveLine.BI_WeightUnit = WeightUnitList.Codes.Pounds;
			AssertNoWarningContaining(moveLine.BI_WeightUnitInfo, ListValidation.InvalidCodeMessage);
		}
	}
}
