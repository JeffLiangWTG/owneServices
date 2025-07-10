using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessagePurposeCollection))]
	class EDIMessagePurposeCollectionTest : ActiveBusinessObjectCollectionTestCase<EDIMessagePurposeCollection>
	{
		#region Setup

		protected override EDIMessagePurposeCollection GetCollectionToTest()
		{
			return new EDIMessagePurposeCollection(Factory, new ZQuery());
		}

		#endregion

		#region AdhockCollectionRelationship

		public void TestAdhocCollection_AddToCollection()
		{
			var collection = new EDIMessagePurposeCollection(Factory, System.Array.Empty<EDIMessagePurpose>());
			var purpose = collection.AddNew();
			AssertCollectionContains(purpose, collection);
		}

		public void TestAdhocCollection_InitCollection()
		{
			var purpose1 = Factory.New<EDIMessagePurpose>();
			var purpose2 = Factory.New<EDIMessagePurpose>();
			var collection = new EDIMessagePurposeCollection(Factory, new[] { purpose1, purpose2 });
			AssertEquals(2, collection.Count);
			AssertCollectionContains(purpose1, collection);
			AssertCollectionContains(purpose2, collection);
		}

		public void TestAdhocCollection_DeleteFrom()
		{
			var purpose = Factory.New<EDIMessagePurpose>();
			var collection = new EDIMessagePurposeCollection(Factory, new[] { purpose });
			collection.DeleteAll();
			AssertEquals(0, collection.Count);
			AssertEquals(true, purpose.IsDeleted);
		}

		#endregion
	}
}
