using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(MNRWorkOrderFilterControl))]
	public class MNRWorkOrderFilterControlTest : TestCaseWithFactory
	{
		#region FilterControlFileds

		public void TestFilterControlFileds()
		{
			using (var workOrderFilterControl = new MNRWorkOrderFilterControl(GetNewGridCollection(), (MNRWorkOrderFilterBusinessObject)GetNewFilterBusinessObject()))
			{
				AssertEquals("MNRWorkOrderFilterControl", workOrderFilterControl.Name);
				AssertEquals(14, workOrderFilterControl.Grid.ColumnStyles.Count);
			}
		}

		#endregion

		#region GetSomeThing

		protected FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new MNRWorkOrderFilterBusinessObject();
		}

		protected IBusinessObjectCollection GetNewGridCollection()
		{
			return new MNRWorkOrderHeaderCollection(Factory);
		}

		#endregion
	}
}
