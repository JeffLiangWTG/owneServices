using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDAdHocServiceOrderFilterControl))]
	public class CYDAdHocServiceOrderFilterControlTest : TestCaseWithFactory
	{
		#region FilterControlFileds

		public void TestFilterControlFileds()
		{
			using (var adHocServiceOrderFilterControl = new CYDAdHocServiceOrderFilterControl(GetNewGridCollection(), (CYDAdHocServiceOrderFilterBusinessObject)GetNewFilterBusinessObject()))
			{
				AssertEquals("CYDAdHocServiceOrderFilterControl", adHocServiceOrderFilterControl.Name);
			}
		}

		#endregion

		#region GetSomeThing

		protected FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CYDAdHocServiceOrderFilterBusinessObject();
		}

		protected IBusinessObjectCollection GetNewGridCollection()
		{
			return new CYDAdHocServiceOrderCollection(Factory);
		}

		#endregion
	}
}
