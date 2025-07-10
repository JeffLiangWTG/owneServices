using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(BranchManagementCodeDescriptionBoolCollection))]
	class BranchManagementCodeDescriptionBoolCollectionTest : CodeDescriptionBoolCollectionAbstractTest<BranchManagementCodeDescriptionBoolCollection>
	{
		public override void TestDefaultBoolForNewChild()
		{
			var collectionType = Collection.GetType();
			var collection = new BranchManagementCodeDescriptionBoolCollection();
			AssertEquals("collection.AddNew().Bool", true, collection.AddNew().Bool);
			var clone = (BranchManagementCodeDescriptionBoolCollection)collection.Clone(null, Factory);
			AssertEquals("clone.AddNew().Bool", true, clone.AddNew().Bool);
		}

		#region Implementation

		protected override BranchManagementCodeDescriptionBoolCollection GetCollectionToTest()
		{
			return new BranchManagementCodeDescriptionBoolCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BranchManagementCodeDescriptionBool();
		}

		protected new BranchManagementCodeDescriptionBoolCollection Collection
		{
			get { return new BranchManagementCodeDescriptionBoolCollection(); }
		}

		#endregion
	}
}
