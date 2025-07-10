using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDMovement))]
	public class CYDMovementTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDMovement>();
		}

		#region TestMovementHeader

		public void TestMovementHeader()
		{
			var cydMovementHeader = Factory.NewWithValidTestData<CYDMovementHeader>();
			var cydMovement = (CYDMovement)GetNewBusinessObject();
			cydMovement.YML_YMH_MovementHeader = cydMovementHeader.PK;
			AssertEquals(cydMovementHeader, cydMovement.MovementHeader);
		}

		#endregion

		#region TestYardUnitState

		public void TestYardUnitState()
		{
			var cydYardUnitState = Factory.NewWithValidTestData<CYDYardUnitState>();
			var cydMovement = (CYDMovement)GetNewBusinessObject();
			cydMovement.YML_YUS_YardUnitState = cydYardUnitState.PK;
			AssertEquals(cydYardUnitState, cydMovement.YardUnitState);
		}

		#endregion
	}
}
