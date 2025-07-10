using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsLocationTypeCollection))]
	class WhsLocationTypeCollectionTest : WhsActiveBusinessObjectCollectionTestCase<WhsLocationTypeCollection>
	{
		#region TestModuleIdAttribute

		public void TestModuleIdAttribute()
		{
			var moduleIDAttribute = (ModuleIDAttribute)GetCollectionToTest().GetType().GetCustomAttributes(typeof(ModuleIDAttribute), false)[0];
			AssertEquals(ModuleId.WhsConfigLocationType, moduleIDAttribute.ModuleId);
		}

		#endregion

		#region GetCollectionToTest

		protected override WhsLocationTypeCollection GetCollectionToTest()
		{
			return new WhsLocationTypeCollection(Factory);
		}

		#endregion
	}
}
