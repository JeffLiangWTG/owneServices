using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketCollectionAdHoc))]
	class WhsDocketCollectionAdHocTest : ActiveBusinessObjectCollectionTestCase<WhsDocketCollectionAdHoc>
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)new WhsDocketCollectionAdHoc(Factory)).AllowNew);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			return Helper.CreateWhsOrder(data.Org1, data.Whs1, "");
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
