using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDReleaseAdviceLine))]
	public class CYDReleaseAdviceLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDReleaseAdviceLine>();
		}

		#region Properties

		public void TestTotalAvailablePickupQuantity_ShouldReturnCorrectAmount()
		{
			var businessObject = (CYDReleaseAdviceLine)GetNewBusinessObject();
			businessObject.UnitLineItem.YLI_Quantity = 10;

			var pickup = Factory.NewWithValidTestData<CYDPickup>();
			pickup.YPL_YEL_ReleaseAdviceLine = businessObject.PK;
			pickup.UnitLineItem.YLI_Quantity = 6;

			AssertEquals(4, businessObject.TotalAvailablePickupQuantity);
		}

		#endregion

		public void TestIsAvailableToPickup()
		{
			var releaseAdviceLine = Factory.NewWithValidTestData<CYDReleaseAdviceLine>();
			releaseAdviceLine.UnitLineItem.YLI_Quantity = 5;

			var pickup = Factory.NewWithValidTestData<CYDPickup>();
			pickup.YPL_YEL_ReleaseAdviceLine = releaseAdviceLine.PK;
			pickup.UnitLineItem.YLI_Quantity = 4;

			var pickup2 = Factory.NewWithValidTestData<CYDPickup>();
			pickup2.UnitLineItem.YLI_Quantity = 1;

			AssertEquals(4, releaseAdviceLine.TotalPickupQuantity);
			AssertEquals(true, releaseAdviceLine.IsAvailableToPickup);

			var pickup3 = Factory.NewWithValidTestData<CYDPickup>();
			pickup3.YPL_YEL_ReleaseAdviceLine = releaseAdviceLine.PK;
			pickup3.UnitLineItem.YLI_Quantity = 1;

			AssertEquals(5, releaseAdviceLine.TotalPickupQuantity);
			AssertEquals(false, releaseAdviceLine.IsAvailableToPickup);
		}
	}
}
