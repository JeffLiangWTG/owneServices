using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgRelatedPartyFetchStrategy : TestCaseWithFactory
	{
		public void TestFetchForViewFromCollectionSort()
		{
			AssertFetchForViewFromCollectionSort(OrgRelatedPartySchema.PR_OH_Parent, nameof(OrgRelatedParty.ParentName));
			AssertFetchForViewFromCollectionSort(OrgRelatedPartySchema.PR_OH_RelatedParty, nameof(OrgRelatedParty.RelatedPartyName));
		}

		void AssertFetchForViewFromCollectionSort(SchemaGuidColumn dataColumn, string sortProperty)
		{
			var collection = new OrgRelatedPartyCollection(Factory, new ZQuery());
			for (int i = 0; i < 5; i++)
			{
				((INeedRow)collection.AddNew()).Row[dataColumn.Name] = Guid.NewGuid();
			}

			var initialHitCount = Factory.GetTableHitCount(OrgHeaderSchema.Constants.TableName);

			var descriptorCollection = ZCustomTypeDescriptor.GetProperties(typeof(OrgRelatedParty));
			var propertyDescriptor = descriptorCollection[sortProperty];
			var sortDescription = new ListSortDescription(propertyDescriptor, ListSortDirection.Ascending);
			var sorts = new ListSortDescriptionCollection(new[] { sortDescription });
			((IBindingListView)collection).ApplySort(sorts);

			var hitCount = Factory.GetTableHitCount(OrgHeaderSchema.Constants.TableName);

			AssertEquals("Should use fetch hints and hit db only once.", 1, hitCount - initialHitCount);
		}
	}
}
