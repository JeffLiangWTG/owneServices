using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class OrgRelatedPartySubsetCollectionTest<TCollection> : OrgRelatedPartySubsetCollectionTest<TCollection, OrgRelatedParty>
		where TCollection : OrgRelatedPartySubsetCollection
	{
	}

	public abstract class OrgRelatedPartySubsetCollectionTest<TCollection, TOrgRelatedParty> : SubsetBusinessObjectCollectionTestCase<TCollection, TOrgRelatedParty>
		where TCollection : OrgRelatedPartySubsetCollection<TOrgRelatedParty>
		where TOrgRelatedParty : OrgRelatedParty
	{
		public void TestSetDefaultsForNewChild()
		{
			var collection = GetCollectionToTest();
			var relatedParty = collection.AddNew();
			AssertEquals(Organisation.PK, relatedParty.PR_OH_Parent);
			AssertSpecificDefaults(relatedParty);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TOrgRelatedParty>();
		}

		protected OrgHeader Organisation
		{
			get { return organisation ?? (organisation = Factory.New<OrgHeader>()); }
		}
		OrgHeader organisation;

		protected abstract void AssertSpecificDefaults(TOrgRelatedParty relatedParty);
	}
}
