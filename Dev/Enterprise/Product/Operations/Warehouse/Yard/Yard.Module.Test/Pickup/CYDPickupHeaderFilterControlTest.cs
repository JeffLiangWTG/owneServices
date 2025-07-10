using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDPickupHeaderFilterControl))]
	public class CYDPickupHeaderFilterControlTest : TestCaseWithFactory
	{
		#region FilterControlFileds

		public void TestFilterControlFileds()
		{
			using (var workOrderFilterControl = new CYDPickupHeaderFilterControl(GetNewGridCollection(), (CYDPickupHeaderFilterBusinessObject)GetNewFilterBusinessObject()))
			{
				AssertEquals("CYDPickupHeaderFilterControl", workOrderFilterControl.Name);
				AssertEquals(10, workOrderFilterControl.Grid.ColumnStyles.Count);
			}
		}

		#endregion

		#region GetSomeThing

		protected FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CYDPickupHeaderFilterBusinessObject();
		}

		protected IBusinessObjectCollection GetNewGridCollection()
		{
			return new CYDPickupHeaderCollection(Factory);
		}

		#endregion
	}
}
