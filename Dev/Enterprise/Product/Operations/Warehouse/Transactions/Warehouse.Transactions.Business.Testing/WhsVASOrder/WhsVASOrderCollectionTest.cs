using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsVASOrderCollection))]
	public class WhsVASOrderCollectionTest : WhsActiveBusinessObjectCollectionTestCase<WhsVASOrderCollection>
	{
		#region TestModuleIdAttribute

		public void TestModuleIdAttribute()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			var moduleIDAttribute = (ModuleIDAttribute)GetCollectionToTest().GetType().GetCustomAttributes(typeof(ModuleIDAttribute), false)[0];
			AssertEquals(ModuleId.WhsVASOrder, moduleIDAttribute.ModuleId);
		}

		#endregion

		#region TestGetFetchStrategy

		public void TestGetFetchStrategy()
		{
			var collection = new WhsVASOrderCollection(Factory);
			AssertType<WhsVASOrderCollectionFetchStrategy>(((IBusinessObjectCollection)collection).FetchStrategy);
		}

		#endregion

		#region GetCollectionToTest

		protected override WhsVASOrderCollection GetCollectionToTest()
		{
			return new WhsVASOrderCollection(Factory);
		}

		#endregion
	}
}
