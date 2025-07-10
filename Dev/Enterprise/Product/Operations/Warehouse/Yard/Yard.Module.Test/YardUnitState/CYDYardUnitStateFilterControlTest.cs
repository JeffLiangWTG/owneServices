using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDYardUnitStateFilterControl))]
	public class CYDYardUnitStateFilterControlTest : TestCaseWithFactory
	{
		#region FilterControlFileds

		public void TestFilterControlFileds()
		{
			using (var yardUnitStateFilterControl = new CYDYardUnitStateFilterControl(GetNewGridCollection(), (CYDYardUnitStateFilterBusinessObject)GetNewFilterBusinessObject()))
			{
				AssertEquals("CYDYardUnitStateFilterControl", yardUnitStateFilterControl.Name);
				AssertEquals(14, yardUnitStateFilterControl.Grid.ColumnStyles.Count);
			}
		}

		#endregion

		#region GetSomeThing

		protected FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CYDYardUnitStateFilterBusinessObject();
		}

		protected IBusinessObjectCollection GetNewGridCollection()
		{
			return new CYDYardUnitStateCollection(Factory);
		}

		#endregion
	}
}
