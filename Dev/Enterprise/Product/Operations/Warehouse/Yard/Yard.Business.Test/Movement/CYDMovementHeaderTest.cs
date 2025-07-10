using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDMovementHeader))]
	public class CYDMovementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDMovementHeader>();
		}

		#region TestYard

		public void TestYard()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var cydMovementHeader = (CYDMovementHeader)GetNewBusinessObject();
			cydMovementHeader.YMH_WW_Yard = warehouse.PK;
			AssertEquals(warehouse, cydMovementHeader.Yard);
		}

		#endregion
	}
}
