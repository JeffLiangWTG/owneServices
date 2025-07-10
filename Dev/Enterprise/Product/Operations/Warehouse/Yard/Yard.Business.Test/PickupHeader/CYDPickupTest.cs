using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDPickup))]
	public class CYDPickupTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDPickup>();
		}

		#region TestPickupHeader

		public void TestPickupHeader()
		{
			var cydPickupHeader = Factory.NewWithValidTestData<CYDPickupHeader>();
			var cydPickup = (CYDPickup)GetNewBusinessObject();
			cydPickup.YPL_YPH_PickupHeader = cydPickupHeader.PK;
			AssertEquals(cydPickupHeader, cydPickup.PickupHeader);
		}

		#endregion

		#region TestYPLType

		public void TestYPLType()
		{
			var cydPickup = (CYDPickup)GetNewBusinessObject();
			AssertEquals(CYDPickupType.Codes.Container, cydPickup.UnitLineItem.YLI_Type);
		}

		#endregion

		public void TestReleaseAdviceLineProperty()
		{
			var pickup = Factory.NewWithValidTestData<CYDPickup>();
			AssertEquals(null, pickup.ReleaseAdviceLine);

			var releaseAdviceLine = Factory.NewWithValidTestData<CYDReleaseAdviceLine>();
			pickup.YPL_YEL_ReleaseAdviceLine = releaseAdviceLine.PK;
			AssertEquals(releaseAdviceLine, pickup.ReleaseAdviceLine);
		}

		public void TestLinkedYardUnitProperty()
		{
			var pickup = Factory.NewWithValidTestData<CYDPickup>();
			AssertEquals(null, pickup.LinkedYardUnit);

			var yardUnit = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnit.YUS_YPL_Pickup = pickup.PK;
			AssertEquals(yardUnit, pickup.LinkedYardUnit);
		}
	}
}
