using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsLoadCollection))]
	class WhsLoadCollectionTest : WhsActiveBusinessObjectCollectionTestCase<WhsLoadCollection>
	{
		#region TestModuleIdAttribute

		public void TestModuleIdAttribute()
		{
			var moduleIDAttribute = (ModuleIDAttribute)GetCollectionToTest().GetType().GetCustomAttributes(typeof(ModuleIDAttribute), false)[0];
			AssertEquals(ModuleId.WhsLoad, moduleIDAttribute.ModuleId);
		}

		#endregion

		#region GetCollectionToTest

		protected override WhsLoadCollection GetCollectionToTest() => new WhsLoadCollection(Factory);

		#endregion
	}
}
