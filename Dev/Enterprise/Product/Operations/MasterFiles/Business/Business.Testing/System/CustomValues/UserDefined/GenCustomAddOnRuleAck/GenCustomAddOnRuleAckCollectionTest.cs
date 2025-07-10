using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[TestedType(typeof(GenCustomAddOnRuleAckCollection))]
	sealed class GenCustomAddOnRuleAckCollectionTest : ActiveBusinessObjectCollectionTestCase<GenCustomAddOnRuleAckCollection>
	{
		public void TestSetDefaultsForNewElementCore()
		{
			var collection = new GenCustomAddOnRuleAckCollection(dummyParent);
			var newAck = collection.AddNew();
			AssertEquals(dummyParent.PK, newAck.XK_ParentID);
			AssertEquals(dummyParent.TablePrefix, newAck.XK_ParentTableCode);
		}

		#region Implementation

		protected override GenCustomAddOnRuleAckCollection GetCollectionToTest()
		{
			return new GenCustomAddOnRuleAckCollection(dummyParent ?? Factory.New<DummyBusinessObject>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			dummyParent = Factory.New<DummyBusinessObject>();
		}

		DummyBusinessObject dummyParent;

		#endregion
	}
}
