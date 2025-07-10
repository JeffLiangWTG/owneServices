using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class JobComInvLineRefsCollectionTest<T> : BusinessObjectCollectionTestCase where T : JobComInvLineRefs
	{
		public void TestSetDefaultValuesForNewChild()
		{
			var coll = GetJobComInvLineRefsCollection();
			var element = coll.AddNew();
			AssertEquals(coll.JG_ReferenceType, element.JG_ReferenceType);
			AssertEquals(coll.Master.PK, element.InvoiceLine.PK);
		}

		public void TestCreateRelationshipFilter()
		{
			var coll = GetJobComInvLineRefsCollection();
			coll.RemoveAll();
			var element1 = (JobComInvLineRefs)GetNewElementToAddToTheCollection();
			var element2 = (JobComInvLineRefs)GetNewElementToAddToTheCollection();
			coll.Add(element1);
			coll.Add(element2);
			element2.JG_ReferenceType = "~~~";
			AssertNotEquals("PreCondition:JG_ReferenceType passed into the collection is not the same as element2's", element2.JG_ReferenceType, coll.JG_ReferenceType);
			coll.Load();
			AssertEquals("contains element1", true, coll.Contains(element1));
			AssertEquals("should not contain element2", false, coll.Contains(element2));
		}

		protected abstract JobComInvLineRefsCollection<T> GetJobComInvLineRefsCollection();
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return GetJobComInvLineRefsCollection();
		}
	}
}
