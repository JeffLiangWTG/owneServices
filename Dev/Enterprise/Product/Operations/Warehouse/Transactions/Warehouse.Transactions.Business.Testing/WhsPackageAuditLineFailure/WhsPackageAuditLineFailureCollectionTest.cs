using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPackageAuditLineFailureCollection))]
	class WhsPackageAuditLineFailureCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsPackageAuditLineFailureCollection>
	{
		#region TestConstructor

		#region TestWhsPackageAuditConstructor

		public void TestWhsPackageAuditConstructor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var audit = Helper.CreateWhsPackageAudit(order);
			var collection = new WhsPackageAuditLineFailureCollection(audit);

			AssertEquals(0, collection.Count);

			var auditLineFailure1 = collection.AddNew();
			AssertEquals("New auditLineFailure has correct relationship.", audit.PK, auditLineFailure1.WPF_WPA_WhsPackageAudit);

			var auditLineFailure2 = Factory.New<WhsPackageAuditLineFailure>();
			auditLineFailure2.WPF_WPA_WhsPackageAudit = audit.PK;
			AssertContainsExactElementsInAnyOrder(new[] { auditLineFailure1, auditLineFailure2 }, collection);
		}

		#endregion

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#region GetCollectionToTest

		protected override WhsPackageAuditLineFailureCollection GetCollectionToTest()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var audit = Helper.CreateWhsPackageAudit(order);
			return new WhsPackageAuditLineFailureCollection(audit);
		}

		#endregion

		#endregion
	}
}
