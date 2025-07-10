using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public abstract class OrgRelatedPartySubsetCollection : OrgRelatedPartySubsetCollection<OrgRelatedParty>
	{
		public OrgRelatedPartySubsetCollection(OrgRelatedPartyDependentCollection completeCollection)
			: base(completeCollection)
		{
		}
	}

	public abstract class OrgRelatedPartySubsetCollection<T> : SubsetBusinessObjectCollection<T>
		where T : OrgRelatedParty
	{
		public OrgRelatedPartySubsetCollection(OrgRelatedPartyDependentCollection completeCollection)
			: base(completeCollection)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((T)child).PR_OH_Parent = ((OrgRelatedPartyDependentCollection)CollectionToFilter).Master.PK;
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}
	}
}
