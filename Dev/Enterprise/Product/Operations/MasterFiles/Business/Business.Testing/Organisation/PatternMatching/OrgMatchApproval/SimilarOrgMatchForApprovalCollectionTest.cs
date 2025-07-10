using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SimilarOrgMatchForApprovalCollection))]
	sealed class SimilarOrgMatchForApprovalCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SimilarOrgMatchForApprovalCollection>
	{
		public void TestReadOnly()
		{
			OrgMatchApproval matchApproval = Factory.New<OrgMatchApproval>();
			SimilarOrgMatchForApprovalCollection collection = matchApproval.SimilarOrgMatchesSortedByRank;
			AssertEquals("Collection is always read only", true, collection.ReadOnly);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestAddNewNotSupported()
		{
			OrgMatchApproval matchApproval = Factory.New<OrgMatchApproval>();
			SimilarOrgMatchForApprovalCollection collection = matchApproval.SimilarOrgMatchesSortedByRank;
			collection.AddNew();
		}

		public void TestAddNewOnIBindingListWorksForAddNewCancelForBinding()
		{
			OrgMatchApproval matchApproval = Factory.New<OrgMatchApproval>();
			SimilarOrgMatchForApprovalCollection collection = matchApproval.SimilarOrgMatchesSortedByRank;

			SimilarOrgMatchForApproval dummyOrgMatchForApproval = (SimilarOrgMatchForApproval)((IBindingList)collection).AddNew();
			AssertNotNull("Should be able to AddNew on the interface so that binding can find the PropertyDescriptors when there are no elements in the collection", dummyOrgMatchForApproval);
		}

		protected override SimilarOrgMatchForApprovalCollection GetCollectionToTest()
		{
			OrgMatchApproval matchApproval = Factory.New<OrgMatchApproval>();
			return matchApproval.SimilarOrgMatchesSortedByRank;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			DummyBusinessObject dummyParent = Factory.New<DummyBusinessObject>();
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			DummyOrgMatchApproval dummyMatchApproval = (DummyOrgMatchApproval)loader.LoadOrCreate(dummyParent.PK, OrgMatchApprovalType.DummyType);

			return SimilarOrgMatchForApproval.New(dummyMatchApproval, Factory.New<OrgPatternMatch>());
		}
	}
}
