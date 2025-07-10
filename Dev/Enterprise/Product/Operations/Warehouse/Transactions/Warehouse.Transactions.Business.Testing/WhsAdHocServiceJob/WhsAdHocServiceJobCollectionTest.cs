using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdHocServiceJobCollection))]
	public class WhsAdHocServiceJobCollectionTest : WhsActiveBusinessObjectCollectionTestCase<WhsAdHocServiceJobCollection>
	{
		#region TestModuleIdAttribute

		public void TestModuleIdAttribute()
		{
			var adhocServiceJob = Factory.New<WhsAdHocServiceJob>();
			var moduleIDAttribute = (ModuleIDAttribute)GetCollectionToTest().GetType().GetCustomAttributes(typeof(ModuleIDAttribute), false)[0];
			AssertEquals(ModuleId.WhsAdHocServiceJob, moduleIDAttribute.ModuleId);
		}

		#endregion

		#region GetCollectionToTest

		protected override WhsAdHocServiceJobCollection GetCollectionToTest()
		{
			return new WhsAdHocServiceJobCollection(Factory);
		}

		#endregion

	}
}
